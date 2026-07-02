using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RETSYS.Web.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem, Busca Textual e Filtragem por Período de Compra (GET /clientes)
        [HttpGet("/clientes")]
        public async Task<IActionResult> Index([FromQuery] string? busca, [FromQuery] int? mes, [FromQuery] int? ano)
        {
            var query = _context.Clientes
                .Include(c => c.OrdensServico)
                    .ThenInclude(os => os.Financeiro)
                .AsQueryable();

            // Filtro por termo textual (Nome ou CPF)
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(termo) || c.CPF.Contains(termo));
            }

            // Filtro dinâmico de CRM por Período (Retorna quem comprou no mês/ano indicado)
            if (mes.HasValue && ano.HasValue)
            {
                query = query.Where(c => c.OrdensServico.Any(os => os.DataEntrada.Month == mes.Value && os.DataEntrada.Year == ano.Value));
            }

            var listaClientes = await query
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.CPF,
                    c.Telefone,
                    // Busca a última OS emitida para esse cliente (física ou histórica)
                    UltimaOs = c.OrdensServico.OrderByDescending(os => os.DataEntrada).Select(os => os.NumeroOS).FirstOrDefault() ?? "Nenhuma",
                    // Soma as compras reais finalizadas e as importadas do histórico (status ENTREGUE)
                    TotalGasto = c.OrdensServico
                        .Where(os => os.Status == "ENTREGUE")
                        .Sum(os => os.Financeiro.ValorTotalLiquido)
                })
                .ToListAsync();

            return Inertia.Render("Clientes/Index", new { 
                Clientes = listaClientes,
                FiltroBusca = busca ?? "",
                MesFiltro = mes,
                AnoFiltro = ano
            });
        }

        // 2. Gravação de Novo Cliente e Registro de Compra Histórica (POST com Upload)
        [HttpPost("/clientes")]
        public async Task<IActionResult> Store([FromForm] ClienteCadastroRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.CPF))
            {
                return RedirectToAction(nameof(Index));
            }

            // Instancia o cliente principal no banco de dados
            var novoCliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = model.Nome,
                CPF = model.CPF,
                Telefone = model.Telefone,
                Cep = model.Cep,
                Logradouro = model.Logradouro,
                Numero = model.Numero,
                Bairro = model.Bairro,
                Cidade = model.Cidade,
                Estado = model.Estado,
                Convenio = model.Convenio,
                Email = model.Email
            };

            _context.Clientes.Add(novoCliente);

            // Verifica se o usuário optou por amarrar uma compra antiga a esse cliente
            if (model.RegistrarHistorico && model.HistoricoData.HasValue)
            {
                string? caminhoReceitaSalva = null;

                // Processa o upload físico do prontuário / receita se ele anexou
                if (model.HistoricoFotoReceita != null && model.HistoricoFotoReceita.Length > 0)
                {
                    var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receitas");
                    if (!Directory.Exists(pastaUploads))
                    {
                        Directory.CreateDirectory(pastaUploads);
                    }

                    var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(model.HistoricoFotoReceita.FileName);
                    var caminhoCompleto = Path.Combine(pastaUploads, nomeUnicoArquivo);

                    // CORRIGIDO: Modificado de caminCompleto para caminhoCompleto para sanar o erro CS0103 e CS1503
                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await model.HistoricoFotoReceita.CopyToAsync(stream);
                    }

                    caminhoReceitaSalva = "/uploads/receitas/" + nomeUnicoArquivo;
                }

                // Cria a OS Simplificada e isolada de histórico
                var osHistorica = new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    NumeroOS = "HIST-" + model.HistoricoData.Value.ToString("yyyyMM") + "-" + new Random().Next(100, 999),
                    ClienteId = novoCliente.Id,
                    DataEntrada = model.HistoricoData.Value,
                    DataPrevistaEntrega = model.HistoricoData.Value,
                    Status = "ENTREGUE", // Já nasce entregue para somar corretamente no faturamento do cliente
                    IsRetroativa = true,  // Sinaliza bypass no estoque de armações
                    LenteDescricaoManual = model.HistoricoLente, // Lente de preenchimento textual livre
                    ArmacaoModeloManual = "Peça Histórica Antiga (Cadastro CRM)",
                    Observacoes = "Registro simplificado importado de histórico de fichas de papel.",
                    MedicoTipo = "NAO_ESPECIFICADO"
                };

                // Receita satélite corrigida (Utiliza OsId mapeado no 1:1 compartilhado)
                var receitaSatelite = new OsReceita
                {
                    OsId = osHistorica.Id,
                    OdEsferico = 0, OdCilindrico = 0, OdEixo = 0,
                    OeEsferico = 0, OeCilindrico = 0, OeEixo = 0,
                    ObsReceita = string.IsNullOrEmpty(caminhoReceitaSalva) ? "Importado sem imagem física." : $"Foto de Prontuário Anexa: {caminhoReceitaSalva}"
                };

                // Financeiro satélite corrigido (Utiliza OsId)
                var financeiroSatelite = new OsFinanceiro
                {
                    OsId = osHistorica.Id,
                    ValorTotalBruto = model.HistoricoValor ?? 0,
                    ValorTotalLiquido = model.HistoricoValor ?? 0,
                    FormaPagamento = "DINHEIRO"
                };

                osHistorica.Receita = receitaSatelite;
                osHistorica.Financeiro = financeiroSatelite;

                _context.OrdensServico.Add(osHistorica);
                _context.OsReceitas.Add(receitaSatelite);
                _context.OsFinanceiros.Add(financeiroSatelite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 3. Prontuário Clínico e Linha do Tempo de OS do Cliente (GET)
        [HttpGet("/clientes/{id:guid}/historico")]
        public async Task<IActionResult> Historico(Guid id)
        {
            var cliente = await _context.Clientes
                .Select(c => new { c.Id, c.Nome, c.CPF, c.Telefone, c.Convenio, c.Email, c.Observacoes })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Carrega em tempo real todas as OSs (normais ou históricas) mapeando as receitas
            var historicoOS = await _context.OrdensServico
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id)
                .OrderByDescending(os => os.DataEntrada) 
                .Select(os => new
                {
                    os.NumeroOS,
                    DataVenda = os.DataEntrada, 
                    Medico = os.MedicoNome,      
                    ValorTotal = os.Financeiro.ValorTotalLiquido, 
                    os.Status,
                    IsRetroativa = os.IsRetroativa,
                    LenteManual = os.LenteDescricaoManual, // Exibe o texto livre se for OS do CRM
                    ArmacaoManual = os.ArmacaoModeloManual,
                    ObsReceita = os.Receita.ObsReceita,
                    odEsferico = os.Receita.OdEsferico,
                    odCilindrico = os.Receita.OdCilindrico,
                    odEixo = os.Receita.OdEixo,
                    oeEsferico = os.Receita.OeEsferico,
                    oeCilindrico = os.Receita.OeCilindrico,
                    oeEixo = os.Receita.OeEixo,
                    adicao = os.Receita.Adicao
                })
                .ToListAsync();

            decimal totalGastoHistorico = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id && os.Status == "ENTREGUE")
                .SumAsync(os => os.Financeiro.ValorTotalLiquido);

            return Inertia.Render("Clientes/Historico", new
            {
                Cliente = cliente,
                Historico = historicoOS,
                TotalGasto = totalGastoHistorico 
            });
        }
    }

    public class ClienteCadastroRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Convenio { get; set; }
        public string? Email { get; set; }

        // Dados Históricos do CRM (Lente Livre + Receita em Foto)
        public bool RegistrarHistorico { get; set; }
        public DateTime? HistoricoData { get; set; }
        public decimal? HistoricoValor { get; set; }
        public string? HistoricoLente { get; set; }
        public IFormFile? HistoricoFotoReceita { get; set; }
    }
}