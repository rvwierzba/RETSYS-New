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

            // Filtro dinâmico de CRM por Período
            if (mes.HasValue && ano.HasValue)
            {
                query = query.Where(c => 
                    c.OrdensServico.Any(os => os.DataEntrada.Month == mes.Value && os.DataEntrada.Year == ano.Value) ||
                    (c.DataUltimaCompra.HasValue && c.DataUltimaCompra.Value.Month == mes.Value && c.DataUltimaCompra.Value.Year == ano.Value)
                );
            }

            var listaClientes = await query
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.CPF,
                    c.Telefone,
                    UltimaOs = c.OrdensServico.OrderByDescending(os => os.DataEntrada).Select(os => os.NumeroOS).FirstOrDefault() ?? 
                               (c.DataUltimaCompra.HasValue ? "MIGRAÇÃO (CRM)" : "Nenhuma"),
                    
                    TotalGasto = (c.ValorGasto ?? 0) + c.OrdensServico
                        .Where(os => os.Status == "ENTREGUE")
                        .Sum(os => (decimal?)os.Financeiro.ValorTotalLiquido) ?? 0
                })
                .ToListAsync();

            return Inertia.Render("Clientes/Index", new { 
                Clientes = listaClientes,
                FiltroBusca = busca ?? "",
                MesFiltro = mes,
                AñoFiltro = ano
            });
        }

        // API Endpoint: Consulta de CPF para preenchimento automático na emissão de OS
        [HttpGet("/api/clientes/buscar-cpf/{cpf}")]
        public async Task<IActionResult> BuscarPorCpf(string cpf)
        {
            var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CPF == cpfLimpo || c.CPF == cpf);

            if (cliente == null)
            {
                return NotFound(new { mensagem = "Cliente não encontrado." });
            }

            return Ok(new
            {
                nome = cliente.Nome,
                cpf = cliente.CPF,
                telefone = cliente.Telefone,
                dataNascimento = cliente.DataNascimento?.ToString("yyyy-MM-dd"),
                cep = cliente.Cep,
                logradouro = cliente.Logradouro,
                numero = cliente.Numero,
                bairro = cliente.Bairro,
                cidade = cliente.Cidade,
                estado = cliente.Estado,
                email = cliente.Email,
                convenio = cliente.Convenio
            });
        }

        // 2. Gravação de Novo Cliente com Suporte a Campos Nativos de Migração (POST com Upload)
        [HttpPost("/clientes")]
        public async Task<IActionResult> Store([FromForm] ClienteCadastroRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.CPF))
            {
                return RedirectToAction(nameof(Index));
            }

            var novoCliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = model.Nome,
                CPF = model.CPF,
                Telefone = model.Telefone,
                Cep = model.Cep ?? string.Empty,
                Logradouro = model.Logradouro ?? string.Empty,
                Numero = model.Numero ?? string.Empty,
                Bairro = model.Bairro ?? string.Empty,
                Cidade = model.Cidade ?? string.Empty,
                Estado = model.Estado ?? string.Empty,
                Convenio = model.Convenio,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (model.RegistrarHistorico && model.HistoricoData.HasValue)
            {
                novoCliente.ValorGasto = model.HistoricoValor; 
                novoCliente.ProdutoAdquirido = model.HistoricoLente; 
                novoCliente.DataUltimaCompra = DateTime.SpecifyKind(model.HistoricoData.Value, DateTimeKind.Utc); 
                novoCliente.DataReceita = DateTime.SpecifyKind(model.HistoricoData.Value, DateTimeKind.Utc); 

                novoCliente.UltimaOdEsferico = model.UltimaOdEsferico; 
                novoCliente.UltimaOdCilindrico = model.UltimaOdCilindrico; 
                novoCliente.UltimaOdEixo = model.UltimaOdEixo; 
                novoCliente.UltimaOeEsferico = model.UltimaOeEsferico; 
                novoCliente.UltimaOeCilindrico = model.UltimaOeCilindrico; 
                novoCliente.UltimaOeEixo = model.UltimaOeEixo; 
                novoCliente.UltimaAdicao = model.UltimaAdicao; 
                novoCliente.UltimaDnpOd = model.UltimaDnpOd; 
                novoCliente.UltimaDnpOe = model.UltimaDnpOe; 

                if (model.HistoricoFotoReceita != null && model.HistoricoFotoReceita.Length > 0)
                {
                    var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receitas");
                    if (!Directory.Exists(pastaUploads))
                    {
                        Directory.CreateDirectory(pastaUploads);
                    }

                    var nomeUnicoArquivo = Guid.NewGuid().ToString() + Path.GetExtension(model.HistoricoFotoReceita.FileName);
                    var caminhoCompleto = Path.Combine(pastaUploads, nomeUnicoArquivo);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await model.HistoricoFotoReceita.CopyToAsync(stream);
                    }

                    string caminhoReceitaSalva = "/uploads/receitas/" + nomeUnicoArquivo;
                    novoCliente.Observacoes = $"[Foto de Receita Legada Importada: {caminhoReceitaSalva}] " + model.Observacoes;
                }
            }

            _context.Clientes.Add(novoCliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 3. Prontuário Clínico e Visualização Isolada: Linha do Tempo + Bloco de Migração (GET)
        [HttpGet("/clientes/{id:guid}/historico")]
        public async Task<IActionResult> Historico(Guid id)
        {
            var cliente = await _context.Clientes
                .Select(c => new 
                { 
                    c.Id, c.Nome, c.CPF, c.Telefone, c.Convenio, c.Email, c.Observacoes,
                    c.ValorGasto, c.ProdutoAdquirido, c.DataUltimaCompra, c.DataReceita,
                    c.UltimaOdEsferico, c.UltimaOdCilindrico, c.UltimaOdEixo,
                    c.UltimaOeEsferico, c.UltimaOeCilindrico, c.UltimaOeEixo,
                    c.UltimaAdicao, c.UltimaDnpOd, c.UltimaDnpOe
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var historicoOS = await _context.OrdensServico
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id && !os.IsRetroativa)
                .OrderByDescending(os => os.DataEntrada) 
                .Select(os => new
                {
                    os.NumeroOS,
                    DataVenda = os.DataEntrada, 
                    Medico = os.MedicoNome,      
                    ValorTotal = os.Financeiro.ValorTotalLiquido, 
                    os.Status,
                    os.IsRetroativa,
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

            decimal totalOsaSistema = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id && os.Status == "ENTREGUE" && !os.IsRetroativa)
                .SumAsync(os => os.Financeiro.ValorTotalLiquido);

            decimal totalGastoCalculado = (cliente.ValorGasto ?? 0) + totalOsaSistema;

            return Inertia.Render("Clientes/Historico", new
            {
                Cliente = cliente,
                Historico = historicoOS,
                TotalGasto = totalGastoCalculado 
            });
        }

        // 4. Lista de Aniversariantes do Mês
        [HttpGet("/clientes/aniversariantes")]
        public async Task<IActionResult> Aniversariantes([FromQuery] int? mes)
        {
            var mesFiltro = mes ?? DateTime.Today.Month;

            var aniversariantes = await _context.Clientes
                .Where(c => c.DataNascimento.HasValue && c.DataNascimento.Value.Month == mesFiltro)
                .OrderBy(c => c.DataNascimento!.Value.Day)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Telefone,
                    c.Email,
                    DataNascimento = c.DataNascimento,
                    Dia = c.DataNascimento!.Value.Day
                })
                .ToListAsync();

            return Inertia.Render("Clientes/Aniversariantes", new
            {
                Aniversariantes = aniversariantes,
                MesFiltro = mesFiltro
            });
        }

        // 5. Exclusão de Cliente
        [HttpPost("/clientes/excluir/{id:guid}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var possuiOs = await _context.OrdensServico.AnyAsync(o => o.ClienteId == id);
            if (possuiOs)
            {
                Inertia.Share("erro", "Não é possível excluir um cliente que já possui Ordens de Serviço registradas.");
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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
        public string? Observacoes { get; set; }

        public bool RegistrarHistorico { get; set; }
        public DateTime? HistoricoData { get; set; }
        public decimal? HistoricoValor { get; set; }
        public string? HistoricoLente { get; set; }
        public IFormFile? HistoricoFotoReceita { get; set; }

        public decimal? UltimaOdEsferico { get; set; }
        public decimal? UltimaOdCilindrico { get; set; }
        public int? UltimaOdEixo { get; set; }
        public decimal? UltimaOeEsferico { get; set; }
        public decimal? UltimaOeCilindrico { get; set; }
        public int? UltimaOeEixo { get; set; }
        public decimal? UltimaAdicao { get; set; }
        public decimal? UltimaDnpOd { get; set; }
        public decimal? UltimaDnpOe { get; set; }
    }
}