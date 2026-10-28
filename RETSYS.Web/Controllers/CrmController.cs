using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class CrmController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public CrmController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /crm
        [HttpGet("/crm")]
        public async Task<IActionResult> Index([FromQuery] string? filtro, [FromQuery] string? busca)
        {
            var oticaId = ObterOticaId();
            var hoje = DateTime.UtcNow.Date;
            var dataLimite330Dias = hoje.AddDays(-330);
            var dataLimite450Dias = hoje.AddDays(-450);

            // 1. Clientes para Recall Anual (receita feita entre 11 e 15 meses atrás)
            var queryRecall = _context.Clientes
                .Where(c => c.OticaId == oticaId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                queryRecall = queryRecall.Where(c => c.Nome.ToLower().Contains(termo) 
                    || (c.Telefone != null && c.Telefone.Contains(termo))
                    || (c.CPF != null && c.CPF.Contains(termo)));
            }

            var clientesTodos = await queryRecall
                .Include(c => c.Receitas.OrderByDescending(r => r.DataExame))
                .Include(c => c.OrdensServico.OrderByDescending(o => o.DataEntrada))
                .ToListAsync();

            var listaRecall = new List<object>();
            var listaAniversariantes = new List<object>();
            var listaPosVenda = new List<object>();

            var mesAtual = hoje.Month;

            foreach (var cliente in clientesTodos)
            {
                // Aniversariantes do mês
                if (cliente.DataNascimento.HasValue && cliente.DataNascimento.Value.Month == mesAtual)
                {
                    listaAniversariantes.Add(new
                    {
                        cliente.Id,
                        cliente.Nome,
                        cliente.Telefone,
                        DataNascimento = cliente.DataNascimento.Value.ToString("dd/MM/yyyy"),
                        DiaAniversario = cliente.DataNascimento.Value.Day,
                        EhHoje = cliente.DataNascimento.Value.Day == hoje.Day
                    });
                }

                // Identificar última receita
                var ultimaReceita = cliente.Receitas.FirstOrDefault();
                var dataExame = ultimaReceita?.DataExame ?? cliente.DataReceita ?? cliente.DataUltimaCompra;

                if (dataExame.HasValue)
                {
                    var diasPassados = (hoje - dataExame.Value.Date).TotalDays;
                    if (diasPassados >= 300 && diasPassados <= 480)
                    {
                        var medicoNome = ultimaReceita?.MedicoNome ?? "Oftalmologista";
                        listaRecall.Add(new
                        {
                            cliente.Id,
                            cliente.Nome,
                            cliente.Telefone,
                            cliente.Cidade,
                            DataExame = dataExame.Value.ToString("dd/MM/yyyy"),
                            DiasPassados = (int)diasPassados,
                            StatusRecall = diasPassados >= 365 ? "VENCIDA" : "A_VENCER",
                            MedicoNome = medicoNome,
                            UltimoOdEsferico = ultimaReceita?.OdEsferico ?? cliente.UltimaOdEsferico,
                            UltimoOeEsferico = ultimaReceita?.OeEsferico ?? cliente.UltimaOeEsferico,
                            Adicao = ultimaReceita?.Adicao ?? cliente.UltimaAdicao,
                            MensagemWhatsapp = GerarTextoWhatsappRecall(cliente.Nome, dataExame.Value, medicoNome)
                        });
                    }
                }

                // Pós-venda recente (OS entregue entre 7 e 30 dias atrás)
                var ultimaOs = cliente.OrdensServico.FirstOrDefault(o => o.Status == "ENTREGUE" && o.DataEntregaReal.HasValue);
                if (ultimaOs != null && ultimaOs.DataEntregaReal.HasValue)
                {
                    var diasEntrega = (hoje - ultimaOs.DataEntregaReal.Value.Date).TotalDays;
                    if (diasEntrega >= 7 && diasEntrega <= 45)
                    {
                        listaPosVenda.Add(new
                        {
                            cliente.Id,
                            cliente.Nome,
                            cliente.Telefone,
                            NumeroOS = ultimaOs.NumeroOS,
                            DataEntrega = ultimaOs.DataEntregaReal.Value.ToString("dd/MM/yyyy"),
                            DiasEntrega = (int)diasEntrega,
                            MensagemWhatsapp = GerarTextoWhatsappPosVenda(cliente.Nome, ultimaOs.NumeroOS)
                        });
                    }
                }
            }

            // Ordenar aniversariantes pelo dia
            listaAniversariantes = listaAniversariantes.OrderBy(a => ((dynamic)a).DiaAniversario).ToList();
            listaRecall = listaRecall.OrderByDescending(r => ((dynamic)r).DiasPassados).ToList();

            var configLoja = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaId);

            return Inertia.Render("Crm/Index", new
            {
                ListaRecall = listaRecall,
                ListaAniversariantes = listaAniversariantes,
                ListaPosVenda = listaPosVenda,
                TotalRecall = listaRecall.Count,
                TotalAniversariantes = listaAniversariantes.Count,
                TotalPosVenda = listaPosVenda.Count,
                NomeLoja = configLoja?.NomeLoja ?? "RETSYS Ótica",
                Filtro = filtro ?? "recall",
                Busca = busca ?? ""
            });
        }

        private static string GerarTextoWhatsappRecall(string nomeCliente, DateTime dataExame, string medicoNome)
        {
            var primeiroNome = nomeCliente.Split(' ')[0];
            return $"Olá, {primeiroNome}! Tudo bem? Passando para lembrar que já faz cerca de 1 ano desde sua última consulta de vista com {medicoNome} ({dataExame:dd/MM/yyyy}). Cuidar da sua saúde visual é muito importante! Que tal agendarmos uma nova avaliação?";
        }

        private static string GerarTextoWhatsappPosVenda(string nomeCliente, string numeroOs)
        {
            var primeiroNome = nomeCliente.Split(' ')[0];
            return $"Olá, {primeiroNome}! Tudo bem? Aqui é da Ótica. Como está sendo sua adaptação com seus novos óculos (OS #{numeroOs})? Caso precise de qualquer ajuste nas plaquetas ou hastes, estamos à sua disposição!";
        }
    }
}

