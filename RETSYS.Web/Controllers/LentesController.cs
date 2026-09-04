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
        // 0. PÁGINA PRINCIPAL (INERTIA)
        // =========================================================================

        [HttpGet("/lentes")]
        public async Task<IActionResult> Index()
        {
            var oticaId = ObterOticaId();

            var lentes = await _context.Lentes
                .Where(l => l.OticaId == oticaId)
                .OrderBy(l => l.Laboratorio)
                .ThenBy(l => l.Tipo)
                .ToListAsync();

            var precos = await _context.LentesTabelaPrecos
                .Include(p => p.Lente)
                .Where(p => p.Ativo && p.Lente.OticaId == oticaId)
                .OrderBy(p => p.Lente.Laboratorio)
                .ToListAsync();

            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente.OticaId == oticaId)
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var isAdmin = EhAdministrador();

            return Inertia.Render("Lentes/Index", new
            {
                Lentes = lentes.Select(l => new
                {
                    l.Id,
                    l.Laboratorio,
                    l.Tipo,
                    l.Surfacada
                }),
                Precos = precos.Select(p => new
                {
                    p.Id,
                    p.LenteId,
                    Lente = new
                    {
                        Laboratorio = p.Lente.Laboratorio,
                        Tipo = p.Lente.Tipo
                    },
                    p.Tipo,
                    p.IndiceRefracao,
                    p.Tratamento,
                    p.PrecoCusto,
                    p.PrecoVenda
                }),
                TratamentosSugeridos = tratamentosSugeridos,
                IsAdmin = isAdmin
            });
        }

        // =========================================================================
        // 1. ENDPOINTS DE CONSULTA
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
                var oticaId = ObterOticaId();

                var lente = await _context.Lentes
                    .FirstOrDefaultAsync(l => l.Id == lenteId && l.OticaId == oticaId);

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
            var oticaId = ObterOticaId();

            var tratamentos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente.OticaId == oticaId)
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tratamentos);
        }

        [HttpGet("{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var oticaId = ObterOticaId();

            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo && lp.Lente.OticaId == oticaId)
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao, lp.Tratamento })
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }

        // =========================================================================
        // 2. ENDPOINTS DE ESCRITA (CADASTRO DE LENTE BASE COMPATÍVEL COM INERTIA)
        // =========================================================================

        [HttpPost("/lentes")]
        public async Task<IActionResult> CriarLenteBase([FromBody] NovaLenteInput input)
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.Laboratorio) || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    return BadRequest("Laboratório e Tipo de Bloco são campos obrigatórios.");
                }

                var novaLente = new Lente
                {
                    Id = Guid.NewGuid(),
                    OticaId = ObterOticaId(),
                    CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Laboratorio = input.Laboratorio.Trim(),
                    Tipo = input.Tipo.Trim(),
                    Surfacada = input.Surfacada,
                    GraduacaoMin = -20.00m,
                    GraduacaoMax = 20.00m,
                    Ativo = true
                };

                _context.Lentes.Add(novaLente);
                await _context.SaveChangesAsync();

                return RedirecionarSeguro();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao salvar lente base: {ex.Message}");
            }
        }

        // =========================================================================
        // 3. ENDPOINTS DE ESCRITA (MATRIZ DE PREÇOS - LentePreco)
        // =========================================================================

        [HttpPost("/lentes/precos")]
        public async Task<IActionResult> CriarPreco([FromBody] NovoLentePrecoInput input)
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

            try
            {
                if (input == null || input.LenteId == Guid.Empty || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    return BadRequest("Lente base e Tipo são campos obrigatórios.");
                }

                var oticaId = ObterOticaId();

                var lenteExiste = await _context.Lentes.AnyAsync(l => l.Id == input.LenteId && l.OticaId == oticaId);
                if (!lenteExiste)
                {
                    return NotFound("Lente base não encontrada no catálogo.");
                }

                var novoPreco = new LentePreco
                {
                    Id = Guid.NewGuid(),
                    LenteId = input.LenteId,
                    Tipo = input.Tipo.Trim(),
                    IndiceRefracao = input.IndiceRefracao,
                    Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim(),
                    PrecoCusto = input.PrecoCusto,
                    PrecoVenda = input.PrecoVenda,
                    Ativo = true
                };

                _context.LentesTabelaPrecos.Add(novoPreco);
                await _context.SaveChangesAsync();

                return RedirecionarSeguro();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao salvar preço na matriz: {ex.Message}");
            }
        }

        [HttpDelete("/lentes/precos/{id:guid}")]
        public async Task<IActionResult> RemoverPreco(Guid id)
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

            try
            {
                var oticaId = ObterOticaId();

                var preco = await _context.LentesTabelaPrecos
                    .Include(p => p.Lente)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Lente.OticaId == oticaId);

                if (preco == null)
                {
                    return NotFound("Preço não encontrado na matriz.");
                }

                _context.LentesTabelaPrecos.Remove(preco);
                await _context.SaveChangesAsync();

                return RedirecionarSeguro();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao remover preço da matriz: {ex.Message}");
            }
        }

        // =========================================================================
        // AUXILIARES
        // =========================================================================

        private bool EhAdministrador()
        {
            return User.IsInRole("Admin")
                || User.IsInRole("Administrador")
                || User.IsInRole("admin")
                || User.IsInRole("administrador");
        }

        private Guid ObterOticaId()
        {
            var claim = User.FindFirst("OticaId")?.Value;
            return Guid.TryParse(claim, out var oticaId) ? oticaId : Guid.Empty;
        }

        private IActionResult RedirecionarSeguro()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(referer))
            {
                return Redirect("/lentes");
            }
            return Redirect(referer);
        }
    }

    public class NovaLenteInput
    {
        public string Laboratorio { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Surfacada { get; set; }
    }

    public class NovoLentePrecoInput
    {
        public Guid LenteId { get; set; }
        public string Tipo { get; set; } = "MONOFOCAL";
        public decimal IndiceRefracao { get; set; }
        public string? Tratamento { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
    }
}
