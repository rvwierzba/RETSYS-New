using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    [ApiController]
    [Route("api/lentes")]
    public class LentesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. ENDPOINTS DE CONSULTA (MANTIDOS E OTIMIZADOS)
        // =========================================================================

        [HttpGet("calcular-preco")]
        public async Task<IActionResult> CalcularPreco(
            [FromQuery] Guid lenteId,
            [FromQuery] string tipo,
            [FromQuery] decimal indiceRefracao,
            [FromQuery] string? tratamento)
        {
            try
            {
                var lente = await _context.Lentes.FindAsync(lenteId);
                if (lente == null)
                {
                    return NotFound(new { mensagem = "Lente não cadastrada no sistema." });
                }

                if (lente.Surfacada)
                {
                    return Ok(new
                    {
                        surfacada = true,
                        precoVenda = 0.00m,
                        mensagem = "Lente surfaçada detetada. O preço pode ser editado manualmente."
                    });
                }

                var query = _context.LentesTabelaPrecos
                    .Where(lp => lp.LenteId == lenteId &&
                                 lp.Tipo == tipo &&
                                 lp.IndiceRefracao == indiceRefracao &&
                                 lp.Ativo);

                query = string.IsNullOrEmpty(tratamento)
                    ? query.Where(lp => string.IsNullOrEmpty(lp.Tratamento))
                    : query.Where(lp => lp.Tratamento == tratamento);

                var precoMatriz = await query.FirstOrDefaultAsync();

                if (precoMatriz == null)
                {
                    return BadRequest(new { mensagem = "Não há preço configurado para este Tipo, Índice de refração e Tratamento selecionados." });
                }

                return Ok(new
                {
                    surfacada = false,
                    precoVenda = precoMatriz.PrecoVenda
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao processar o cálculo da lente.", detalhes = ex.Message });
            }
        }

        [HttpGet("tratamentos")]
        public async Task<IActionResult> ListarTratamentos()
        {
            var tratamentos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tratamentos);
        }

        [HttpGet("{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo)
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao, lp.Tratamento })
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }

        // =========================================================================
        // 2. ENDPOINTS DE ESCRITA (APENAS CADASTRO DE LENTE BASE)
        // =========================================================================

        [HttpPost("/lentes")]
        public async Task<IActionResult> CriarLenteBase(
            [FromForm] string laboratorio,
            [FromForm] string tipo,
            [FromForm] bool surfacada)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(laboratorio) || string.IsNullOrWhiteSpace(tipo))
                {
                    return BadRequest("Laboratório e Tipo de Bloco são campos obrigatórios.");
                }

                var novaLente = new Lente
                {
                    Id = Guid.NewGuid(),
                    CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Laboratorio = laboratorio.Trim(),
                    Tipo = tipo.Trim(),
                    Surfacada = surfacada,
                    GraduacaoMin = -20.00m,
                    GraduacaoMax = 20.00m,
                    Ativo = true
                };

                _context.Lentes.Add(novaLente);
                await _context.SaveChangesAsync();

                return Redirect(Request.Headers["Referer"].ToString() ?? "/lentes");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao salvar lente base: {ex.Message}");
            }
        }
    }
}