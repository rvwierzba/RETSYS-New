using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Domain.Entities;
using RETSYS.Infrastructure.Data;
using System;
using System.Linq;
using System.Security.Claims;
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

        // =========================================================================
        // 👤 PAINEL DA VENDEDORA
        // =========================================================================

        [HttpGet("/minhas-comissoes")]
        public async Task<IActionResult> MinhasComissoes([FromQuery] string? periodo)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                return Redirect("/login");
            }

            string periodoAlvo = string.IsNullOrWhiteSpace(periodo)
                ? DateTime.UtcNow.ToString("yyyy-MM")
                : periodo;

            if (!PeriodoValido(periodoAlvo))
            {
                Inertia.Share("erro", "Período de referência inválido. Use o formato AAAA-MM.");
                return Redirect("/minhas-comissoes");
            }

            var vendedor = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == vendedorId);

            if (vendedor == null)
            {
                return Redirect("/login");
            }

            var comissoes = await _context.Comissoes
                .AsNoTracking()
                .Include(c => c.OrdemServico)
                .Where(c =>
                    c.VendedorId == vendedorId &&
                    c.PeriodoReferencia == periodoAlvo)
                .OrderByDescending(c => c.DataGeracao)
                .ToListAsync();

            var extratoComissoes = comissoes
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
                .ToList();

            var fechamentos = await _context.FechamentosComissao
                .AsNoTracking()
                .Where(f => f.VendedorId == vendedorId)
                .OrderByDescending(f => f.PeriodoReferencia)
                .ToListAsync();

            var historicoFechamentos = fechamentos
                .Select(f => new
                {
                    f.Id,
                    f.PeriodoReferencia,
                    f.TotalVendasBrutas,
                    f.TotalComissao,
                    f.QtdOs,
                    f.Status,
                    DataLiquidacao = f.DataPagamento.HasValue
                        ? f.DataPagamento.Value.ToString("dd/MM/yyyy")
                        : null
                })
                .ToList();

            decimal comissaoAcumuladaMes = extratoComissoes
                .Where(c =>
                    c.Status == "PENDENTE" ||
                    c.Status == "FECHADO" ||
                    c.Status == "PAGO")
                .Sum(c => c.ComissaoGerada);

            return Inertia.Render("Admin/Comissoes/MinhaComissao", new
            {
                Extrato = extratoComissoes,
                Historico = historicoFechamentos,
                ComissaoAcumulada = comissaoAcumuladaMes,
                TaxaComissao = vendedor.PercentualComissao,
                PeriodoFiltro = periodoAlvo
            });
        }

        // =========================================================================
        // ⚙️ PAINEL ADMINISTRATIVO
        // =========================================================================

        [HttpGet("/admin/comissoes")]
        public async Task<IActionResult> Index()
        {
            var fechamentos = await _context.FechamentosComissao
                .AsNoTracking()
                .Include(f => f.Vendedor)
                .OrderByDescending(f => f.PeriodoReferencia)
                .ThenBy(f => f.Vendedor.Nome)
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

            return Inertia.Render("Admin/Comissoes/Index", new
            {
                Fechamentos = fechamentos,
                Vendedores = vendedores
            });
        }

        [HttpPost("/admin/comissoes/atualizar-taxa/{vendedorId:guid}")]
        public async Task<IActionResult> AtualizarTaxaVendedor(
            Guid vendedorId,
            [FromBody] DtoAtualizarTaxa model)
        {
            if (model.PercentualComissao < 0 || model.PercentualComissao > 100)
            {
                Inertia.Share("erro", "A taxa de comissão deve estar entre 0% e 100%.");
                return RedirectToAction(nameof(Index));
            }

            var vendedor = await _context.Usuarios.FindAsync(vendedorId);

            if (vendedor == null)
            {
                Inertia.Share("erro", "Vendedor não localizado.");
                return RedirectToAction(nameof(Index));
            }

            vendedor.PercentualComissao = Math.Round(model.PercentualComissao, 2);

            await _context.SaveChangesAsync();

            Inertia.Share("sucesso", "Taxa de comissão atualizada com sucesso.");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/admin/comissoes/fechar-mes")]
        public async Task<IActionResult> FecharMes(
            [FromQuery] Guid vendedorId,
            [FromQuery] string? periodo)
        {
            if (vendedorId == Guid.Empty)
            {
                Inertia.Share("erro", "Selecione uma vendedora para realizar o fechamento.");
                return RedirectToAction(nameof(Index));
            }

            if (!PeriodoValido(periodo))
            {
                Inertia.Share("erro", "Período de referência inválido. Use o formato AAAA-MM.");
                return RedirectToAction(nameof(Index));
            }

            var vendedor = await _context.Usuarios.FindAsync(vendedorId);

            if (vendedor == null)
            {
                Inertia.Share("erro", "Vendedor não encontrado.");
                return RedirectToAction(nameof(Index));
            }

            if (!vendedor.ComissaoAtiva)
            {
                Inertia.Share("erro", "A comissão desta vendedora está desativada.");
                return RedirectToAction(nameof(Index));
            }

            var fechamentoExistente = await _context.FechamentosComissao
                .FirstOrDefaultAsync(f =>
                    f.VendedorId == vendedorId &&
                    f.PeriodoReferencia == periodo);

            if (fechamentoExistente != null && fechamentoExistente.Status == "PAGO")
            {
                Inertia.Share("erro", "Este período já foi pago e não pode ser alterado.");
                return RedirectToAction(nameof(Index));
            }

            if (fechamentoExistente != null && fechamentoExistente.Status == "FECHADO")
            {
                Inertia.Share("erro", "Este período já está fechado e aguarda confirmação de pagamento.");
                return RedirectToAction(nameof(Index));
            }

            var comissoesPendentes = await _context.Comissoes
                .Where(c =>
                    c.VendedorId == vendedorId &&
                    c.PeriodoReferencia == periodo &&
                    c.Status == "PENDENTE")
                .ToListAsync();

            if (!comissoesPendentes.Any())
            {
                Inertia.Share("erro", "Nenhuma comissão pendente foi localizada para esse período.");
                return RedirectToAction(nameof(Index));
            }

            decimal totalVendasLiquidas = comissoesPendentes.Sum(c => c.ValorBase);
            decimal totalComissaoDevida = comissoesPendentes.Sum(c => c.ValorComissao);
            int quantidadeOs = comissoesPendentes
                .Select(c => c.OrdemServicoId)
                .Distinct()
                .Count();

            DateTime dataFechamento = DateTime.UtcNow;
            Guid? fechadoPorId = ObterUsuarioLogadoId();

            foreach (var comissao in comissoesPendentes)
            {
                comissao.Status = "FECHADO";

                comissao.Observacoes = string.IsNullOrWhiteSpace(comissao.Observacoes)
                    ? $"Comissão incluída no fechamento do período {periodo}."
                    : $"{comissao.Observacoes} Comissão incluída no fechamento do período {periodo}.";
            }

            if (fechamentoExistente == null)
            {
                fechamentoExistente = new FechamentoComissao
                {
                    Id = Guid.NewGuid(),
                    VendedorId = vendedorId,
                    PeriodoReferencia = periodo!,
                    TotalVendasBrutas = totalVendasLiquidas,
                    TotalComissao = totalComissaoDevida,
                    QtdOs = quantidadeOs,
                    Status = "FECHADO",
                    DataFechamento = dataFechamento,
                    FechadoPorId = fechadoPorId
                };

                _context.FechamentosComissao.Add(fechamentoExistente);
            }
            else
            {
                fechamentoExistente.TotalVendasBrutas = totalVendasLiquidas;
                fechamentoExistente.TotalComissao = totalComissaoDevida;
                fechamentoExistente.QtdOs = quantidadeOs;
                fechamentoExistente.Status = "FECHADO";
                fechamentoExistente.DataFechamento = dataFechamento;
                fechamentoExistente.DataPagamento = null;
                fechamentoExistente.FechadoPorId = fechadoPorId;
            }

            await _context.SaveChangesAsync();

            Inertia.Share("sucesso", "Fechamento mensal realizado com sucesso.");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/admin/comissoes/pagar/{id:guid}")]
        public async Task<IActionResult> PagarVendedor(Guid id)
        {
            var fechamento = await _context.FechamentosComissao
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fechamento == null)
            {
                Inertia.Share("erro", "Fechamento não localizado.");
                return RedirectToAction(nameof(Index));
            }

            if (fechamento.Status == "PAGO")
            {
                Inertia.Share("erro", "Este fechamento já foi pago.");
                return RedirectToAction(nameof(Index));
            }

            if (fechamento.Status != "FECHADO")
            {
                Inertia.Share("erro", "Apenas fechamentos com status FECHADO podem ser pagos.");
                return RedirectToAction(nameof(Index));
            }

            var comissoesVinculadas = await _context.Comissoes
                .Where(c =>
                    c.VendedorId == fechamento.VendedorId &&
                    c.PeriodoReferencia == fechamento.PeriodoReferencia &&
                    c.Status == "FECHADO")
                .ToListAsync();

            if (!comissoesVinculadas.Any())
            {
                Inertia.Share("erro", "Não existem comissões fechadas disponíveis para pagamento.");
                return RedirectToAction(nameof(Index));
            }

            DateTime dataPagamento = DateTime.UtcNow;

            foreach (var comissao in comissoesVinculadas)
            {
                comissao.Status = "PAGO";
                comissao.DataPagamento = dataPagamento;

                comissao.Observacoes = string.IsNullOrWhiteSpace(comissao.Observacoes)
                    ? $"Comissão paga em {dataPagamento:dd/MM/yyyy HH:mm} UTC."
                    : $"{comissao.Observacoes} Comissão paga em {dataPagamento:dd/MM/yyyy HH:mm} UTC.";
            }

            fechamento.Status = "PAGO";
            fechamento.DataPagamento = dataPagamento;

            await _context.SaveChangesAsync();

            Inertia.Share("sucesso", "Pagamento de comissão confirmado com sucesso.");

            return RedirectToAction(nameof(Index));
        }

        private Guid? ObterUsuarioLogadoId()
        {
            string? usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(usuarioIdClaim, out Guid usuarioId)
                ? usuarioId
                : null;
        }

        private static bool PeriodoValido(string? periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo) || periodo.Length != 7)
            {
                return false;
            }

            string[] partes = periodo.Split('-');

            if (partes.Length != 2 ||
                !int.TryParse(partes[0], out int ano) ||
                !int.TryParse(partes[1], out int mes))
            {
                return false;
            }

            return ano >= 2000 && mes >= 1 && mes <= 12;
        }
    }

    public record DtoAtualizarTaxa(decimal PercentualComissao);
}
