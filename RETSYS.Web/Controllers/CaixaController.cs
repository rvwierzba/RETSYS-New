using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;

namespace RETSYS.Web.Controllers
{
    public class CaixaController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public CaixaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem do Contas a Receber + Geração Dinâmica de PIX (GET)
        [HttpGet("/caixa")]
        public async Task<IActionResult> Index([FromQuery] Guid? gerarPixParaId)
        {
            var oticaId = ObterOticaId();

            var parcelas = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Where(os => os.OticaId == oticaId)
                .SelectMany(os => os.Parcelas.Select(p => new
                {
                    p.Id,
                    p.NumeroParcela,
                    p.DescricaoParcela,
                    p.Valor,
                    p.DataVencimento,
                    p.DataPagamento,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",
                    NumeroOS = os.NumeroOS
                }))
                .OrderBy(p => p.DataPagamento != null)
                .ThenBy(p => p.DataVencimento)
                .ToListAsync();

            Inertia.Share("PixHabilitadoNestaLoja", true);

            if (gerarPixParaId.HasValue)
            {
                var parcelaAlvo = parcelas.FirstOrDefault(p => p.Id == gerarPixParaId.Value);
                if (parcelaAlvo != null && parcelaAlvo.DataPagamento == null)
                {
                    Inertia.Share("DadosPixAtivo", new
                    {
                        qrCodeImagemUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=retsys_mock_pix_emv_{parcelaAlvo.Id}",
                        pixCopiaECola = $"00020101021226870014br.gov.bcb.pix2565retsys{parcelaAlvo.Id}5405{parcelaAlvo.Valor.ToString("F2")}5802BR5909RETSYS_WEB"
                    });
                }
            }

            return Inertia.Render("Caixa/Index", new { Parcelas = parcelas });
        }

        // 2. Confirmação do Recebimento / Baixa da Parcela (POST)
        [HttpPost("/caixa/baixar/{id:guid}")]
        public async Task<IActionResult> BaixarParcela(Guid id)
        {
            var oticaId = ObterOticaId();

            var parcela = await _context.OrdensServico
                .Where(os => os.OticaId == oticaId)
                .SelectMany(os => os.Parcelas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parcela != null)
            {
                parcela.DataPagamento = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Endpoint de Polling Consumido pelo método iniciarMonitoramentoPix do Vue (GET)
        [HttpGet("/caixa/status/{id:guid}")]
        public async Task<IActionResult> ObterStatusPix(Guid id)
        {
            var oticaId = ObterOticaId();

            var parcela = await _context.OrdensServico
                .Where(os => os.OticaId == oticaId)
                .SelectMany(os => os.Parcelas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parcela == null)
            {
                return NotFound();
            }

            return Json(new { pago = parcela.DataPagamento != null });
        }

        // =========================================================================
        // 4. MÓDULO FECHAMENTO DO GERENTE (SEÇÃO 4 DO PDF)
        // =========================================================================
        [HttpGet("/caixa/fechamento")]
        public async Task<IActionResult> Fechamento(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] string? tipoPeriodo)
        {
            var oticaId = ObterOticaId();

            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
            bool isAdminOuGerente = string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

            if (!isAdminOuGerente)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Acesso exclusivo para administradores e gerentes." });
            }

            DateTime hoje = DateTime.UtcNow.Date;
            DateTime inicio = dataInicio?.Date ?? hoje;
            DateTime fim = dataFim?.Date.AddDays(1).AddTicks(-1) ?? hoje.AddDays(1).AddTicks(-1);

            if (!string.IsNullOrEmpty(tipoPeriodo))
            {
                switch (tipoPeriodo.ToLower())
                {
                    case "hoje":
                    case "dia":
                        inicio = hoje;
                        fim = hoje.AddDays(1).AddTicks(-1);
                        break;
                    case "semana":
                        inicio = hoje.AddDays(-(int)hoje.DayOfWeek);
                        fim = inicio.AddDays(7).AddTicks(-1);
                        break;
                    case "mes":
                        inicio = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        fim = inicio.AddMonths(1).AddTicks(-1);
                        break;
                }
            }

            var ordensPeriodo = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Vendedor)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.ConferidoPor)
                .Include(os => os.Receita)
                .Where(os => os.Ativo && os.OticaId == oticaId && os.DataEntrada >= inicio && os.DataEntrada <= fim)
                .ToListAsync();

            var ordensValidas = ordensPeriodo
                .Where(os => os.Status != "CANCELADO" && os.Status != "CANCELADA")
                .ToList();

            var ordensCanceladas = ordensPeriodo
                .Where(os => os.Status == "CANCELADO" || os.Status == "CANCELADA")
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataEntrada,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",
                    VendedorNome = os.Vendedor != null ? os.Vendedor.Nome : "Não informado",
                    ValorTotalLiquido = os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0m
                })
                .ToList();

            decimal totalVendidoLiquido = ordensValidas.Sum(os => os.Financeiro?.ValorTotalLiquido ?? 0m);
            decimal totalEntradasRecebidas = ordensValidas.Sum(os => os.Financeiro?.ValorEntrada ?? 0m);
            decimal totalRetiradasRecebidas = ordensValidas.Sum(os => os.Financeiro?.ValorRecebidoRetirada ?? 0m);
            decimal totalRecebidoCaixa = totalEntradasRecebidas + totalRetiradasRecebidas;
            decimal totalAReceberRestante = ordensValidas.Sum(os => os.Financeiro?.ValorRestante ?? 0m);
            decimal totalDescontosReais = ordensValidas.Sum(os => os.Financeiro?.DescontoReais ?? 0m);
            int qtdOS = ordensValidas.Count;
            decimal ticketMedio = qtdOS > 0 ? Math.Round(totalVendidoLiquido / qtdOS, 2) : 0m;

            var formasPagamento = new[] { "DINHEIRO", "PIX", "CARTAO_CREDITO", "CARTAO_DEBITO", "BOLETO" };
            var resumoFormasPagamento = formasPagamento.Select(forma =>
            {
                decimal valorEntradasForma = ordensValidas
                    .Where(os => os.Financeiro != null && string.Equals(os.Financeiro.FormaPagamento, forma, StringComparison.OrdinalIgnoreCase))
                    .Sum(os => os.Financeiro?.ValorEntrada ?? os.Financeiro?.ValorTotalLiquido ?? 0m);

                decimal valorRetiradasForma = ordensValidas
                    .Where(os => os.Financeiro != null && string.Equals(os.Financeiro.FormaPagamentoRetirada, forma, StringComparison.OrdinalIgnoreCase))
                    .Sum(os => os.Financeiro?.ValorRecebidoRetirada ?? 0m);

                int qtdVendas = ordensValidas.Count(os => os.Financeiro != null && (
                    string.Equals(os.Financeiro.FormaPagamento, forma, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(os.Financeiro.FormaPagamentoRetirada, forma, StringComparison.OrdinalIgnoreCase)
                ));

                return new
                {
                    Forma = forma,
                    Total = valorEntradasForma + valorRetiradasForma,
                    QtdVendas = qtdVendas
                };
            }).ToList();

            var oculosCompleto = ordensValidas.Where(os => (os.Financeiro?.ValorArmacao > 0) && (os.Financeiro?.ValorLente > 0)).ToList();
            var somenteArmacao = ordensValidas.Where(os => (os.Financeiro?.ValorArmacao > 0) && (os.Financeiro?.ValorLente == 0)).ToList();
            var somenteLente = ordensValidas.Where(os => (os.Financeiro?.ValorArmacao == 0) && (os.Financeiro?.ValorLente > 0)).ToList();

            var resumoTiposVenda = new[]
            {
                new { Tipo = "Óculos Completo", Qtd = oculosCompleto.Count, Total = oculosCompleto.Sum(os => os.Financeiro?.ValorTotalLiquido ?? 0m) },
                new { Tipo = "Somente Armação", Qtd = somenteArmacao.Count, Total = somenteArmacao.Sum(os => os.Financeiro?.ValorTotalLiquido ?? 0m) },
                new { Tipo = "Somente Lente", Qtd = somenteLente.Count, Total = somenteLente.Sum(os => os.Financeiro?.ValorTotalLiquido ?? 0m) }
            };

            var comissoesPeriodo = await _context.Comissoes
                .Include(c => c.OrdemServico)
                .Where(c => c.OrdemServico.OticaId == oticaId && c.DataGeracao >= inicio && c.DataGeracao <= fim && c.Status != "CANCELADO")
                .ToListAsync();

            var resumoVendedores = ordensValidas
                .GroupBy(os => os.Vendedor != null ? os.Vendedor.Nome : "Não informado")
                .Select(g =>
                {
                    Guid? vId = g.FirstOrDefault()?.VendedorId;
                    decimal totalVendasVendedor = g.Sum(os => os.Financeiro?.ValorTotalLiquido ?? 0m);
                    decimal totalDescontoVendedor = g.Sum(os => os.Financeiro?.DescontoReais ?? 0m);
                    decimal comissaoGerada = comissoesPeriodo.Where(c => c.VendedorId == vId).Sum(c => c.ValorComissao);

                    return new
                    {
                        VendedorNome = g.Key,
                        QtdOS = g.Count(),
                        TotalVendido = totalVendasVendedor,
                        TotalDesconto = totalDescontoVendedor,
                        ComissaoGerada = comissaoGerada
                    };
                })
                .OrderByDescending(v => v.TotalVendido)
                .ToList();

            var listaVendas = ordensValidas
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataEntrada,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",
                    VendedorNome = os.Vendedor != null ? os.Vendedor.Nome : "Não informado",
                    ValorBruto = os.Financeiro?.ValorTotalBruto ?? 0m,
                    DescontoReais = os.Financeiro?.DescontoReais ?? 0m,
                    DescontoPercentual = os.Financeiro?.DescontoPercentual ?? 0m,
                    ValorLiquido = os.Financeiro?.ValorTotalLiquido ?? 0m,
                    ValorEntrada = os.Financeiro?.ValorEntrada ?? 0m,
                    ValorRestante = os.Financeiro?.ValorRestante ?? 0m,
                    FormaPagamento = os.Financeiro?.FormaPagamento ?? "DINHEIRO",
                    os.Status,
                    PagamentoConferido = os.Financeiro?.PagamentoConferido ?? false,
                    ConferidoPorNome = os.Financeiro?.ConferidoPor != null ? os.Financeiro.ConferidoPor.Nome : null,
                    DataConferencia = os.Financeiro?.DataConferencia
                })
                .OrderByDescending(os => os.DataEntrada)
                .ToList();

            decimal totalConferido = listaVendas.Where(v => v.PagamentoConferido).Sum(v => v.ValorLiquido);
            decimal totalPendenteConferencia = listaVendas.Where(v => !v.PagamentoConferido).Sum(v => v.ValorLiquido);

            return Inertia.Render("Admin/Fechamento/Index", new
            {
                DataInicio = inicio.ToString("yyyy-MM-dd"),
                DataFim = fim.ToString("yyyy-MM-dd"),
                TipoPeriodo = tipoPeriodo ?? "hoje",

                Totais = new
                {
                    TotalVendidoLiquido = totalVendidoLiquido,
                    TotalRecebidoCaixa = totalRecebidoCaixa,
                    TotalEntradasRecebidas = totalEntradasRecebidas,
                    TotalRetiradasRecebidas = totalRetiradasRecebidas,
                    TotalAReceberRestante = totalAReceberRestante,
                    TotalDescontosReais = totalDescontosReais,
                    QtdOS = qtdOS,
                    TicketMedio = ticketMedio,
                    TotalConferido = totalConferido,
                    TotalPendenteConferencia = totalPendenteConferencia
                },

                ResumoFormasPagamento = resumoFormasPagamento,
                ResumoTiposVenda = resumoTiposVenda,
                ResumoVendedores = resumoVendedores,
                ListaVendas = listaVendas,
                OrdensCanceladas = ordensCanceladas
            });
        }

        // --- SEÇÃO 4.3: ALTERAR STATUS DE CONFERÊNCIA DO PAGAMENTO (APENAS ADMIN) ---
        [HttpPost("/caixa/conferir-pagamento/{osId:guid}")]
        public async Task<IActionResult> ConferirPagamento(Guid osId)
        {
            var oticaId = ObterOticaId();

            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
            bool isAdminOuGerente = string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

            if (!isAdminOuGerente)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas gerentes ou administradores podem conferir pagamentos." });
            }

            var financeiro = await _context.OsFinanceiros
                .Include(f => f.OrdemServico)
                .FirstOrDefaultAsync(f => f.OsId == osId && f.OrdemServico.OticaId == oticaId);

            if (financeiro == null)
            {
                return NotFound();
            }

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);

            financeiro.PagamentoConferido = !financeiro.PagamentoConferido;

            if (financeiro.PagamentoConferido)
            {
                financeiro.ConferidoPorId = usuarioLogadoId != Guid.Empty ? usuarioLogadoId : null;
                financeiro.DataConferencia = DateTime.UtcNow;
            }
            else
            {
                financeiro.ConferidoPorId = null;
                financeiro.DataConferencia = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Fechamento));
        }
    }
}
