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
        // 👤 PAINEL DA VENDEDORA
        // =========================================================================

        // 1. Renderiza a visão de extrato e histórico individual da vendedora
        [HttpGet("/minhas-comissoes")]
        public async Task<IActionResult> MinhasComissoes([FromQuery] string? periodo)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                return Redirect("/login");
            }

            var vendedor = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == vendedorId);

            // Define o período de referência do mês atual (Ex: "2026-08") se nenhum for enviado
            string periodoAlvo = periodo ?? DateTime.UtcNow.ToString("yyyy-MM");

            // Extrato do período
            var extratoComissoes = await _context.Comissoes
                .Include(c => c.OrdemServico)
                .Where(c => c.VendedorId == vendedorId && c.PeriodoReferencia == periodoAlvo)
                .OrderByDescending(c => c.DataGeracao)
                .Select(c => new
                {
                    c.Id,
                    NumeroOS = c.OrdemServico.NumeroOS,
                    ValorBrutoVenda = c.ValorBase,
                    PercentualAplicado = c.PercentualAplicado,
                    ComissaoGerada = c.ValorComissao,
                    c.Status,
                    DataLancamento = c.DataGeracao.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            // Histórico de fechamentos
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

            decimal comissaoAcumuladaMes = extratoComissoes
                .Where(c => c.Status == "PENDENTE" || c.Status == "PAGO")
                .Sum(c => c.ComissaoGerada);

            return Inertia.Render("Comissoes/MinhaComissao", new
            {
                Extrato = extratoComissoes,
                Historico = historicoFechamentos,
                ComissaoAcumulada = comissaoAcumuladaMes,
                TaxaComissao = vendedor?.PercentualComissao ?? 3.00m, // Retorna a taxa individual da vendedora
                PeriodoFiltro = periodoAlvo
            });
        }

        // =========================================================================
        // ⚙️ PAINEL DO GESTOR / ADMINISTRATIVO
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
                    VendedorId = f.VendedorId,
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

            // Traz a lista de vendedores e suas taxas individuais de comissão para o Admin gerenciar
            var vendedores = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.FilialLoja,
                    u.PercentualComissao,
                    u.ComissaoAtiva
                })
                .ToListAsync();

            return Inertia.Render("Admin/Comissoes/Index", new { Fechamentos = fechamentos, Vendedores = vendedores });
        }

        // 3. Atualização rápida da porcentagem individual de comissão da vendedora (POST Admin)
        [HttpPost("/admin/comissoes/atualizar-taxa/{vendedorId:guid}")]
        public async Task<IActionResult> AtualizarTaxaVendedor(Guid vendedorId, [FromBody] DtoAtualizarTaxa model)
        {
            var vendedor = await _context.Usuarios.FindAsync(vendedorId);
            if (vendedor == null)
            {
                return NotFound(new { message = "Vendedor não localizado." });
            }

            vendedor.PercentualComissao = Math.Max(0, model.PercentualComissao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 4. Processa e consolida o fechamento de mês de um vendedor (POST)
        [HttpPost("/admin/comissoes/fechar-mes")]
        public async Task<IActionResult> FecharMes([FromQuery] Guid vendedorId, [FromQuery] string periodo)
        {
            if (string.IsNullOrEmpty(periodo) || periodo.Length != 7)
            {
                Inertia.Share("erro", "Período de referência inválido.");
                return RedirectToAction(nameof(Index));
            }

            var vendedor = await _context.Usuarios.FindAsync(vendedorId);
            if (vendedor == null)
            {
                Inertia.Share("erro", "Vendedor não encontrado.");
                return RedirectToAction(nameof(Index));
            }

            var fechamentoExistente = await _context.FechamentosComissao
                .FirstOrDefaultAsync(f => f.VendedorId == vendedorId && f.PeriodoReferencia == periodo);

            if (fechamentoExistente != null && fechamentoExistente.Status != "ABERTO")
            {
                Inertia.Share("erro", "O período selecionado já está encerrado ou pago para este vendedor.");
                return RedirectToAction(nameof(Index));
            }

            // Garante que comissões de OSs entregues no mês usem a taxa individual da vendedora
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

        // 5. Realiza a baixa financeira e liquida o pagamento das comissões (POST)
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

            fechamento.Status = "PAGO";
            fechamento.DataPagamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

    public record DtoAtualizarTaxa(decimal PercentualComissao);
}