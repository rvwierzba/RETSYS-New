using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class ArmacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArmacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. LISTAGEM DO ESTOQUE REAL (GET)
        // =========================================================================
        [HttpGet("/estoque")]
        public async Task<IActionResult> Index()
        {
            var estoque = await _context.Armacoes
                .Include(a => a.Marca)
                .OrderBy(a => a.ModeloReferencia)
                .Select(a => new
                {
                    a.Id,
                    a.ModeloReferencia,
                    Codigo = a.CodigoSku,
                    a.Cor,
                    a.Tamanho,
                    a.Material,
                    a.QuantidadeEstoque,
                    PrecoFinal = a.PrecoVenda,
                    MarcaNome = a.Marca.Nome
                })
                .ToListAsync();

            var marcasDisponiveis = await _context.Marcas
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .Select(m => new { m.Id, m.Nome })
                .ToListAsync();

            return Inertia.Render("Estoque/Index", new
            {
                Estoque = estoque,
                Marcas = marcasDisponiveis
            });
        }

        // =========================================================================
        // 2. ENTRADA DE NOVA ARMAÇÃO (POST)
        // =========================================================================
        [HttpPost("/estoque")]
        public async Task<IActionResult> Store([FromBody] Armacao novaArmacao)
        {
            if (string.IsNullOrWhiteSpace(novaArmacao.ModeloReferencia) || novaArmacao.MarcaId == Guid.Empty)
            {
                return RedirectToAction(nameof(Index));
            }

            if (novaArmacao.Id == Guid.Empty)
            {
                novaArmacao.Id = Guid.NewGuid();
            }

            novaArmacao.CriadoEm = DateTime.UtcNow;

            _context.Armacoes.Add(novaArmacao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 3. CADASTRO DE NOVA MARCA DE ARMAÇÃO (POST - RESTRITO ADMIN)
        // =========================================================================
       [HttpPost("/armacoes/marcas")]
        public async Task<IActionResult> CadastrarMarca([FromForm] string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    return BadRequest(new { mensagem = "O nome da marca não pode ser vazio." });
                }

                var novaMarca = new Marca
                {
                    Id = Guid.NewGuid(),
                    Nome = nome.Trim(),
                    Ativo = true
                };

                _context.Marcas.Add(novaMarca);
                await _context.SaveChangesAsync();

                // Retorno limpo em JSON para o Axios processar e atualizar a grid reativamente
                return Ok(new { mensagem = "Marca cadastrada com sucesso!", id = novaMarca.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro interno ao salvar marca no banco: {ex.Message}" });
            }
        }
    }
}