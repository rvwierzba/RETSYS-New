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

        // 1. Busca inteligente do preço de venda com base na matriz de preços e tratamento (texto livre)
        [HttpGet("calcular-preco")]
        public async Task<IActionResult> CalcularPreco(
            [FromQuery] Guid lenteId,
            [FromQuery] string tipo,
            [FromQuery] decimal indiceRefracao,
            [FromQuery] string? tratamento) // ✅ ALTERADO: era Guid? tratamentoId, agora string opcional
        {
            try
            {
                // Busca a lente para verificar se é surfaçada (preço editável manualmente)
                var lente = await _context.Lentes.FindAsync(lenteId);
                if (lente == null)
                {
                    return NotFound(new { mensagem = "Lente não cadastrada no sistema." });
                }

                // Se for lente surfaçada, o preço de venda é livre e será digitado na tela
                if (lente.Surfacada)
                {
                    return Ok(new
                    {
                        surfacada = true,
                        precoVenda = 0.00m,
                        mensagem = "Lente surfaçada detetada. O preço pode ser editado manualmente."
                    });
                }

                // ✅ ALTERADO: o preço já vem "fechado" na matriz (Tipo + Índice + Tratamento),
                // pois cada combinação agora é uma linha própria na tabela de preços.
                var query = _context.LentesTabelaPrecos
                    .Where(lp => lp.LenteId == lenteId &&
                                 lp.Tipo == tipo &&
                                 lp.IndiceRefracao == indiceRefracao &&
                                 lp.Ativo);

                // Filtra pelo tratamento textual, se informado; caso contrário busca a linha "sem tratamento"
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

        // ❌ REMOVIDO: método ListarTratamentos() — não existe mais tabela lentes_tratamentos.

        // ✅ NOVO: substitui o antigo ListarTratamentos, retornando os valores de texto já cadastrados,
        // para alimentar o autocomplete/combo do formulário no front.
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

        // 3. Retorna os índices, tipos e tratamentos disponíveis para uma lente específica montar a tela de opções
        [HttpGet("{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo)
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao, lp.Tratamento }) // ✅ ALTERADO: incluído Tratamento
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }
    }
}
