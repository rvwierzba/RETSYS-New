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
    public class CarneController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public CarneController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /ordens/{id}/carne
        [HttpGet("/ordens/{id}/carne")]
        public async Task<IActionResult> ImprimirCarne(Guid id)
        {
            var oticaId = ObterOticaId();

            var os = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Vendedor)
                .Include(o => o.Financeiro)
                .Include(o => o.Parcelas.OrderBy(p => p.NumeroParcela))
                .Where(o => o.Id == id && o.OticaId == oticaId)
                .Select(o => new
                {
                    o.Id,
                    o.NumeroOS,
                    DataEntrada = o.DataEntrada.ToString("dd/MM/yyyy"),
                    Cliente = new
                    {
                        o.Cliente.Nome,
                        o.Cliente.CPF,
                        o.Cliente.Telefone,
                        Endereco = $"{o.Cliente.Logradouro}, {o.Cliente.Numero} {o.Cliente.Complemento} - {o.Cliente.Bairro}, {o.Cliente.Cidade}/{o.Cliente.Estado}"
                    },
                    VendedorNome = o.Vendedor != null ? o.Vendedor.Nome : "Atendente",
                    ValorTotal = o.Financeiro.ValorTotalLiquido,
                    ValorEntrada = o.Financeiro.ValorEntrada ?? 0m,
                    ValorRestante = o.Financeiro.ValorRestante,
                    Parcelas = o.Parcelas.Select(p => new
                    {
                        p.Id,
                        p.NumeroParcela,
                        TotalParcelas = o.Parcelas.Count,
                        p.DescricaoParcela,
                        p.Valor,
                        DataVencimento = p.DataVencimento.ToString("dd/MM/yyyy"),
                        DataPagamento = p.DataPagamento.HasValue ? p.DataPagamento.Value.ToString("dd/MM/yyyy") : null,
                        p.Paga,
                        p.NumeroCarne,
                        p.DiasAtraso
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (os == null)
                return NotFound();

            var configLoja = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaId);

            return Inertia.Render("Carne/ImpressaoCarne", new
            {
                Ordem = os,
                Loja = new
                {
                    Nome = configLoja?.NomeLoja ?? "RETSYS ÓTICA",
                    Cnpj = configLoja?.Cnpj ?? "",
                    Telefone = configLoja?.WhatsappNumero ?? ""
                }
            });
        }

        // POST /parcelas/{parcelaId}/quitar
        [HttpPost("/parcelas/{parcelaId}/quitar")]
        public async Task<IActionResult> QuitarParcela(Guid parcelaId, [FromBody] QuitarParcelaDto dto)
        {
            var oticaId = ObterOticaId();

            var parcela = await _context.ParcelasPagamento
                .Include(p => p.OrdemServico)
                .FirstOrDefaultAsync(p => p.Id == parcelaId && p.OrdemServico.OticaId == oticaId);

            if (parcela == null)
                return NotFound();

            parcela.DataPagamento = DateTime.UtcNow;
            parcela.ValorPago = dto.ValorPago > 0 ? dto.ValorPago : parcela.Valor;
            parcela.JurosAtraso = dto.JurosAtraso ?? 0m;
            parcela.MultaAtraso = dto.MultaAtraso ?? 0m;
            parcela.ObservacaoPagamento = dto.Observacao;

            await _context.SaveChangesAsync();

            return Ok(new { Sucesso = true });
        }
    }

    public class QuitarParcelaDto
    {
        public decimal ValorPago { get; set; }
        public decimal? JurosAtraso { get; set; }
        public decimal? MultaAtraso { get; set; }
        public string? Observacao { get; set; }
    }
}

