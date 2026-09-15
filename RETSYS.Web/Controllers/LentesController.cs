using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RETSYS.Web.Controllers
{
    public class LentesController : TenantController
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

            if (oticaId != Guid.Empty)
            {
                await GarantirLentesIniciais(oticaId);
            }

            var lentes = await _context.Lentes
                .Where(l => l.OticaId == oticaId)
                .OrderBy(l => l.Laboratorio)
                .ThenBy(l => l.Tipo)
                .ToListAsync();

            var precos = await _context.LentesTabelaPrecos
                .Include(p => p.Lente)
                .Where(p => p.Ativo && p.Lente != null && p.Lente.OticaId == oticaId)
                .OrderBy(p => p.Lente!.Laboratorio)
                .ToListAsync();

            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente != null && lp.Lente.OticaId == oticaId)
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            bool isAdmin = true; // Sempre permitir gestão completa da tabela da própria ótica

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
                        Laboratorio = p.Lente!.Laboratorio,
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

        [HttpGet("/api/lentes/calcular-preco")]
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

        [HttpGet("/api/lentes/tratamentos")]
        public async Task<IActionResult> ListarTratamentos()
        {
            var oticaId = ObterOticaId();

            var tratamentos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente != null && lp.Lente.OticaId == oticaId)
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tratamentos);
        }

        [HttpGet("/api/lentes/{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var oticaId = ObterOticaId();

            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo && lp.Lente != null && lp.Lente.OticaId == oticaId)
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao, lp.Tratamento })
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }

        // =========================================================================
        // 2. ENDPOINTS DE ESCRITA (CADASTRO DE LENTE BASE)
        // =========================================================================

        [HttpPost("/lentes")]
        public async Task<IActionResult> CriarLenteBase([FromBody] NovaLenteInput input)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem cadastrar lentes base." });
            }

            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.Laboratorio) || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    return BadRequest(new { mensagem = "Laboratório e Tipo de Bloco são campos obrigatórios." });
                }

                var oticaId = ObterOticaId();

                if (oticaId == Guid.Empty)
                {
                    return BadRequest(new { mensagem = "Não foi possível identificar a ótica do usuário logado. Faça login novamente." });
                }

                var novaLente = new Lente
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
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

                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao salvar lente base.", erro = ex.Message });
            }
        }

        // =========================================================================
        // 3. ENDPOINTS DE ESCRITA (MATRIZ DE PREÇOS - LentePreco)
        // =========================================================================

        [HttpPost("/lentes/precos")]
        public async Task<IActionResult> CriarPreco([FromBody] NovoLentePrecoInput input)
        {
            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    return BadRequest(new { mensagem = "O tipo da variação é obrigatório." });
                }

                var oticaId = ObterOticaId();
                if (oticaId == Guid.Empty)
                {
                    return BadRequest(new { mensagem = "Ótica não identificada. Faça login novamente." });
                }

                Guid targetLenteId = Guid.Empty;

                if (input.LenteId.HasValue && input.LenteId.Value != Guid.Empty)
                {
                    var lenteExiste = await _context.Lentes.AnyAsync(l => l.Id == input.LenteId.Value && l.OticaId == oticaId);
                    if (lenteExiste)
                    {
                        targetLenteId = input.LenteId.Value;
                    }
                }

                if (targetLenteId == Guid.Empty)
                {
                    string lab = string.IsNullOrWhiteSpace(input.Laboratorio) ? "Genérico" : input.Laboratorio.Trim();
                    string bloco = string.IsNullOrWhiteSpace(input.NomeBloco) ? "Padrão" : input.NomeBloco.Trim();

                    var lenteExistente = await _context.Lentes
                        .FirstOrDefaultAsync(l => l.OticaId == oticaId &&
                                                 l.Laboratorio.ToLower() == lab.ToLower() &&
                                                 l.Tipo.ToLower() == bloco.ToLower());

                    if (lenteExistente != null)
                    {
                        targetLenteId = lenteExistente.Id;
                    }
                    else
                    {
                        var novaLente = new Lente
                        {
                            Id = Guid.NewGuid(),
                            OticaId = oticaId,
                            CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                            Laboratorio = lab,
                            Tipo = bloco,
                            Surfacada = input.Surfacada,
                            GraduacaoMin = -20.00m,
                            GraduacaoMax = 20.00m,
                            Ativo = true
                        };

                        _context.Lentes.Add(novaLente);
                        await _context.SaveChangesAsync();
                        targetLenteId = novaLente.Id;
                    }
                }

                var novoPreco = new LentePreco
                {
                    Id = Guid.NewGuid(),
                    LenteId = targetLenteId,
                    Tipo = input.Tipo.Trim(),
                    IndiceRefracao = input.IndiceRefracao,
                    Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim(),
                    PrecoCusto = input.PrecoCusto,
                    PrecoVenda = input.PrecoVenda,
                    Ativo = true
                };

                _context.LentesTabelaPrecos.Add(novoPreco);
                await _context.SaveChangesAsync();

                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao salvar preço na matriz.", erro = ex.Message });
            }
        }

        [HttpDelete("/lentes/precos/{id:guid}")]
        public async Task<IActionResult> RemoverPreco(Guid id)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem remover preços da matriz." });
            }

            try
            {
                var oticaId = ObterOticaId();

                var preco = await _context.LentesTabelaPrecos
                    .Include(p => p.Lente)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Lente != null && p.Lente.OticaId == oticaId);

                if (preco == null)
                {
                    return NotFound(new { mensagem = "Preço não encontrado na matriz." });
                }

                _context.LentesTabelaPrecos.Remove(preco);
                await _context.SaveChangesAsync();

                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao remover preço da matriz.", erro = ex.Message });
            }
        }

        [HttpPut("/lentes/{id:guid}")]
        [HttpPost("/lentes/editar/{id:guid}")]
        public async Task<IActionResult> EditarLenteBase(Guid id, [FromBody] NovaLenteInput input)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem editar lentes base." });
            }

            try
            {
                var oticaId = ObterOticaId();
                var lente = await _context.Lentes.FirstOrDefaultAsync(l => l.Id == id && l.OticaId == oticaId);
                if (lente == null)
                {
                    return NotFound(new { mensagem = "Lente base não encontrada." });
                }

                lente.Laboratorio = input.Laboratorio.Trim();
                lente.Tipo = input.Tipo.Trim();
                lente.Surfacada = input.Surfacada;

                await _context.SaveChangesAsync();
                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao editar lente base.", erro = ex.Message });
            }
        }

        [HttpDelete("/lentes/{id:guid}")]
        [HttpPost("/lentes/excluir/{id:guid}")]
        public async Task<IActionResult> RemoverLenteBase(Guid id)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem remover lentes base." });
            }

            try
            {
                var oticaId = ObterOticaId();
                var lente = await _context.Lentes.FirstOrDefaultAsync(l => l.Id == id && l.OticaId == oticaId);
                if (lente == null)
                {
                    return NotFound(new { mensagem = "Lente base não encontrada." });
                }

                _context.Lentes.Remove(lente);
                await _context.SaveChangesAsync();
                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao remover lente base.", erro = ex.Message });
            }
        }

        [HttpPut("/lentes/precos/{id:guid}")]
        [HttpPost("/lentes/precos/editar/{id:guid}")]
        public async Task<IActionResult> EditarPreco(Guid id, [FromBody] NovoLentePrecoInput input)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem editar preços na matriz." });
            }

            try
            {
                var oticaId = ObterOticaId();
                var preco = await _context.LentesTabelaPrecos
                    .Include(p => p.Lente)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Lente != null && p.Lente.OticaId == oticaId);

                if (preco == null)
                {
                    return NotFound(new { mensagem = "Preço não encontrado na matriz." });
                }

                if (input.LenteId.HasValue && input.LenteId.Value != Guid.Empty)
                {
                    preco.LenteId = input.LenteId.Value;
                }
                preco.Tipo = input.Tipo.Trim();
                preco.IndiceRefracao = input.IndiceRefracao;
                preco.Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim();
                preco.PrecoCusto = input.PrecoCusto;
                preco.PrecoVenda = input.PrecoVenda;

                await _context.SaveChangesAsync();
                return await Index();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao editar preço da matriz.", erro = ex.Message });
            }
        }

        // =========================================================================
        // AUXILIAR — MESMO PADRÃO USADO EM OrdensServicoController E CaixaController
        // =========================================================================

        private bool EhAdministrador()
        {
            return true; // Todos os usuários logados na ótica têm acesso operacional à tabela de preços da sua loja
        }

        private async Task GarantirLentesIniciais(Guid oticaId)
        {
            if (oticaId == Guid.Empty) return;

            bool jaExiste = await _context.Lentes.AnyAsync(l => l.OticaId == oticaId);
            if (jaExiste) return;

            var l1 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ESS-01", Laboratorio = "Essilor", Tipo = "Monofocal Orma", Surfacada = false, GraduacaoMin = -10.00m, GraduacaoMax = 10.00m, Ativo = true };
            var l2 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ESS-02", Laboratorio = "Essilor", Tipo = "Varilux Comfort", Surfacada = true, GraduacaoMin = -12.00m, GraduacaoMax = 12.00m, Ativo = true };
            var l3 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-HOY-01", Laboratorio = "Hoya", Tipo = "Miyosmart BlueControl", Surfacada = false, GraduacaoMin = -10.00m, GraduacaoMax = 10.00m, Ativo = true };
            var l4 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ZEI-01", Laboratorio = "Zeiss", Tipo = "Progressiva Light D", Surfacada = true, GraduacaoMin = -15.00m, GraduacaoMax = 15.00m, Ativo = true };

            _context.Lentes.AddRange(l1, l2, l3, l4);

            _context.LentesTabelaPrecos.AddRange(
                new LentePreco { Id = Guid.NewGuid(), LenteId = l1.Id, Tipo = "MONOFOCAL", IndiceRefracao = 1.50m, Tratamento = "Antirreflexo Crizal", PrecoCusto = 45.00m, PrecoVenda = 135.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l2.Id, Tipo = "PROGRESSIVA", IndiceRefracao = 1.56m, Tratamento = "Antirreflexo Premium", PrecoCusto = 160.00m, PrecoVenda = 480.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l3.Id, Tipo = "MONOFOCAL", IndiceRefracao = 1.60m, Tratamento = "Filtro Azul (BlueCut)", PrecoCusto = 85.00m, PrecoVenda = 270.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l4.Id, Tipo = "PROGRESSIVA", IndiceRefracao = 1.67m, Tratamento = "DuraVision Platinum", PrecoCusto = 230.00m, PrecoVenda = 690.00m, Ativo = true }
            );

            await _context.SaveChangesAsync();
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
        public Guid? LenteId { get; set; }
        public string? Laboratorio { get; set; }
        public string? NomeBloco { get; set; }
        public bool Surfacada { get; set; }
        public string Tipo { get; set; } = "MONOFOCAL";
        public decimal IndiceRefracao { get; set; }
        public string? Tratamento { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
    }
}
