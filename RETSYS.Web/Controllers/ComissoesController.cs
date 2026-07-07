using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RETSYS.Web.Controllers
{
    public class ComissoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComissoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 👤 PAINEL DA VENDEDORA (EXIGÊNCIA 05/07 - SEÇÃO 2)
        // =========================================================================

        // 1. Renderiza a visão reativa de extrato e histórico individual da vendedora
        [HttpGet("/minhas-comissoes")]
        public async Task<IActionResult> MinhasComissoes([FromQuery] string? periodo)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                return Redirect("/login");
            }

            // Define o período de referência do mês atual (Ex: "2026-07") se nenhum for enviado
            string periodoAlvo = periodo ?? DateTime.UtcNow.ToString("yyyy-MM");

            // Extrato do período: Lista de OS da vendedora com valor bruto e comissão gerada (Seção 2)
            var extratoComissoes = await _context.Comissoes
                .Include(c => c.OrdemServico)
                .Where(c => c.VendedorId == vendedorId && c.PeriodoReferencia == periodoAlvo)
                .OrderByDescending(c => c.DataGeracao)
                .Select(c => new
                {
                    c.Id,
                    NumeroOS = c.OrdemServico.NumeroOS,
                    ValorBrutoVenda = c.ValorBase, // valor_total_bruto mapeado na regra de negócio
                    ComissaoGerada = c.ValorComissao,
                    c.Status,
                    DataLançamento = c.DataGeracao.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            // Histórico de comissões por mês: Consolida os fechamentos já processados da funcionária (Seção 2)
            var historicoFechamentos = await _context.FechamentosComissao
                .Where(f => f.VendedorId == vendedorId)
                .OrderByDescending(f => f.PeriodoReferencia)
                .Select(f => new
                {
                    f.Id,
                    f.PeriodoReferencia,
                    f.TotalVendasBrutas,
                    f.TotalComissao,
                    f.QtdOs,
                    f.Status,
                    DataLiquidacao = f.DataPagamento.HasValue ? f.DataPagamento.Value.ToString("dd/MM/yyyy") : null
                })
                .ToListAsync();

            // Comissão acumulada do mês corrente em tempo real (Status PENDENTE ou PAGO, ignora ESTORNADO)
            decimal comissaoAcumuladaMes = extratoComissoes
                .Where(c => c.Status == "PENDENTE" || c.Status == "PAGO")
                .Sum(c => c.ComissaoGerada);

            return Inertia.Render("Comissoes/MinhaComissao", new
            {
                Extrato = extratoComissoes,
                Historico = historicoFechamentos,
                ComissaoAcumulada = comissaoAcumuladaMes,
                PeriodoFiltro = periodoAlvo
            });
        }

        // =========================================================================
        // ⚙️ PAINEL DO GESTOR / ADMINISTRATIVO (EXIGÊNCIA 05/07 - SEÇÃO 2)
        // =========================================================================

        // 2. Tela Administrativa de Extratos e Fechamentos Consolidados (GET)
        [HttpGet("/admin/comissoes")]
        public async Task<IActionResult> Index()
        {
            var fechamentos = await _context.FechamentosComissao
                .Include(f => f.Vendedor)
                .OrderByDescending(f => f.PeriodoReferencia)
                .Select(f => new
                {
                    f.Id,
                    VendedorNome = f.Vendedor.Nome,
                    f.PeriodoReferencia,
                    f.TotalVendasBrutas,
                    f.TotalComissao,
                    f.QtdOs,
                    f.Status,
                    f.DataFechamento,
                    f.DataPagamento
                })
                .ToListAsync();

            return Inertia.Render("Admin/Comissoes/Index", new { Fechamentos = fechamentos });
        }

        // 3. Processa e consolida o fechamento de mês de um vendedor (POST)
        [HttpPost("/admin/comissoes/fechar-mes")]
        public async Task<IActionResult> FecharMes([FromQuery] Guid vendedorId, [FromQuery] string periodo)
        {
            if (string.IsNullOrEmpty(periodo) || periodo.Length != 7)
            {
                Inertia.Share("erro", "Período de referência inválido.");
                return RedirectToAction(nameof(Index));
            }

            var fechamentoExistente = await _context.FechamentosComissao
                .FirstOrDefaultAsync(f => f.VendedorId == vendedorId && f.PeriodoReferencia == periodo);

            if (fechamentoExistente != null && fechamentoExistente.Status != "ABERTO")
            {
                Inertia.Share("erro", "O período selecionado já está encerrado ou pago para este vendedor.");
                return RedirectToAction(nameof(Index));
            }

            var comissoesDoMes = await _context.Comissoes
                .Where(c => c.VendedorId == vendedorId && c.PeriodoReferencia == periodo && c.Status == "PENDENTE")
                .ToListAsync();

            if (!comissoesDoMes.Any())
            {
                Inertia.Share("erro", "Nenhuma comissão pendente localizada para este vendedor no período informado.");
                return RedirectToAction(nameof(Index));
            }

            decimal totalVendasBrutas = comissoesDoMes.Sum(c => c.ValorBase);
            decimal totalComissaoDevida = comissoesDoMes.Sum(c => c.ValorComissao);
            int quantidadeOs = comissoesDoMes.Count;

            if (fechamentoExistente == null)
            {
                var novoFechamento = new FechamentoComissao
                {
                    Id = Guid.NewGuid(),
                    VendedorId = vendedorId,
                    PeriodoReferencia = periodo,
                    TotalVendasBrutas = totalVendasBrutas,
                    TotalComissao = totalComissaoDevida,
                    QtdOs = quantidadeOs,
                    Status = "FECHADO",
                    DataFechamento = DateTime.UtcNow
                };
                _context.FechamentosComissao.Add(novoFechamento);
            }
            else
            {
                fechamentoExistente.TotalVendasBrutas = totalVendasBrutas;
                fechamentoExistente.TotalComissao = totalComissaoDevida;
                fechamentoExistente.QtdOs = quantidadeOs;
                fechamentoExistente.Status = "FECHADO";
                fechamentoExistente.DataFechamento = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 4. Realiza a baixa financeira e liquida o pagamento das comissões (POST)
        [HttpPost("/admin/comissoes/pagar/{id:guid}")]
        public async Task<IActionResult> PagarVendedor(Guid id)
        {
            var fechamento = await _context.FechamentosComissao.FindAsync(id);
            if (fechamento == null) return NotFound();

            if (fechamento.Status == "PAGO")
            {
                return RedirectToAction(nameof(Index));
            }

            var comissoesVinculadas = await _context.Comissoes
                .Where(c => c.VendedorId == fechamento.VendedorId && 
                            c.PeriodoReferencia == fechamento.PeriodoReferencia && 
                            c.Status == "PENDENTE")
                .ToListAsync();

            foreach (var comissao in comissoesVinculadas)
            {
                comissao.Status = "PAGO";
                comissao.DataPagamento = DateTime.UtcNow;
            }

            // Registra a data de confirmação de pagamento por vendedora no cabeçalho (Seção 2)
            fechamento.Status = "PAGO";
            fechamento.DataPagamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}