using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class CaixaController : Controller
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
            // Busca as parcelas trazendo os dados da OS e do Cliente de forma otimizada
            var parcelas = await _context.OrdensServico
                .Include(os => os.Cliente)
                .SelectMany(os => os.Parcelas.Select(p => new
                {
                    p.Id,
                    p.NumeroParcela,
                    p.DescricaoParcela,
                    p.Valor,
                    p.DataVencimento,
                    p.DataPagamento,
                    ClienteNome = os.Cliente.Nome,
                    NumeroOS = os.NumeroOS
                }))
                .OrderBy(p => p.DataPagamento != null) // Pendentes primeiro
                .ThenBy(p => p.DataVencimento)
                .ToListAsync();

            // Ativa a flag para o Vue saber que o gateway está ativo
            Inertia.Share("PixHabilitadoNestaLoja", true);

            // Se o seu Vue disparou o método solicitarPixProducao pedindo um QR Code real
            if (gerarPixParaId.HasValue)
            {
                // CORRIGIDO: alterado de gerarPixParaId.value para gerarPixParaId.Value
                var parcelaAlvo = parcelas.FirstOrDefault(p => p.Id == gerarPixParaId.Value);
                if (parcelaAlvo != null && parcelaAlvo.DataPagamento == null)
                {
                    // Resposta estruturada exigida pelo terminal de checkout do seu front
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
            var parcela = await _context.OrdensServico
                .SelectMany(os => os.Parcelas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parcela != null)
            {
                parcela.DataPagamento = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Endpoint de Polling Consumido pelo método iniciarMonitoramentoPix do seu Vue (GET)
        [HttpGet("/caixa/status/{id:guid}")]
        public async Task<IActionResult> ObterStatusPix(Guid id)
        {
            var parcela = await _context.OrdensServico
                .SelectMany(os => os.Parcelas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parcela == null)
            {
                return NotFound();
            }

            // Retorna se a parcela já foi liquidada
            return Json(new { pago = parcela.DataPagamento != null });
        }
    }
}