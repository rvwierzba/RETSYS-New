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

        // 1. Busca inteligente do preço de venda com base na matriz de preços e tratamento
        [HttpGet("calcular-preco")]
        public async Task<IActionResult> CalcularPreco(
            [FromQuery] Guid lenteId,
            [FromQuery] string tipo,
            [FromQuery] decimal indiceRefracao,
            [FromQuery] Guid? tratamentoId)
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

                // Busca o preço base na matriz cadastrada para a combinação exata de tipo e índice
                var precoMatriz = await _context.LentesTabelaPrecos
                    .FirstOrDefaultAsync(lp => lp.LenteId == lenteId && 
                                               lp.Tipo == tipo && 
                                               lp.IndiceRefracao == indiceRefracao && 
                                               lp.Ativo);

                if (precoMatriz == null)
                {
                    return BadRequest(new { mensagem = "Não há preço configurado para este Tipo e Índice de refração selecionados." });
                }

                decimal precoVendaFinal = precoMatriz.PrecoVenda;

                // Se houver um tratamento opcional selecionado, soma o valor de acréscimo
                if (tratamentoId.HasValue)
                {
                    var tratamento = await _context.LentesTratamentos.FindAsync(tratamentoId.Value);
                    if (tratamento != null && tratamento.Ativo)
                    {
                        precoVendaFinal += tratamento.AcrescimoValor;
                    }
                }

                return Ok(new
                {
                    surfacada = false,
                    precoVenda = precoVendaFinal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao processar o cálculo da lente.", detalhes = ex.Message });
            }
        }

        // 2. Retorna a lista de tratamentos ativos para preencher os combos do formulário
        [HttpGet("tratamentos")]
        public async Task<IActionResult> ListarTratamentos()
        {
            var tratamentos = await _context.LentesTratamentos
                .Where(t => t.Ativo)
                .OrderBy(t => t.Nome)
                .Select(t => new { t.Id, t.Nome, t.AcrescimoValor })
                .ToListAsync();

            return Ok(tratamentos);
        }

        // 3. Retorna os índices e tipos disponíveis para uma lente específica montar a tela de opções
        [HttpGet("{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo)
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao })
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }
    }
}