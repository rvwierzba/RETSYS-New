using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class ComissoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComissoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tela Administrativa de Extratos e Fechamentos (GET)
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

        // 2. Processa e consolida o fechamento do mês de um vendedor (POST)
        [HttpPost("/admin/comissoes/fechar-mes")]
        public async Task<IActionResult> FecharMes([FromQuery] Guid vendedorId, [FromQuery] string periodo)
        {
            // Valida o formato do período enviado pelo front (Ex: "2026-06")
            if (string.IsNullOrEmpty(periodo) || periodo.Length != 7)
            {
                Inertia.Share("erro", "Período de referência inválido.");
                return RedirectToAction(nameof(Index));
            }

            // Verifica se já existe um fechamento concluído ou pago para este vendedor no período
            var fechamentoExistente = await _context.FechamentosComissao
                .FirstOrDefaultAsync(f => f.VendedorId == vendedorId && f.PeriodoReferencia == periodo);

            if (fechamentoExistente != null && fechamentoExistente.Status != "ABERTO")
            {
                Inertia.Share("erro", "O período selecionado já está encerrado ou pago para este vendedor.");
                return RedirectToAction(nameof(Index));
            }

            // Agrupa todas as comissões pendentes do vendedor neste período específico
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

        // 3. Realiza a baixa financeira e quita o pagamento das comissões (POST)
        [HttpPost("/admin/comissoes/pagar/{id:guid}")]
        public async Task<IActionResult> PagarVendedor(Guid id)
        {
            var fechamento = await _context.FechamentosComissao.FindAsync(id);
            if (fechamento == null) return NotFound();

            if (fechamento.Status == "PAGO")
            {
                return RedirectToAction(nameof(Index));
            }

            // Atualiza em lote todas as comissões individuais que faziam parte deste fechamento
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

            // Atualiza o cabeçalho do fechamento gerencial
            fechamento.Status = "PAGO";
            fechamento.DataPagamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}