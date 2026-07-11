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
                // Atribui Guid caso não venha preenchido do front
                novaArmacao.Id = Guid.NewGuid();
            }

            novaArmacao.CriadoEm = DateTime.UtcNow;

            _context.Armacoes.Add(novaArmacao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 3. CADASTRO DE NOVA MARCA DE ARMAÇÃO (POST - SEM DTOS)
        // =========================================================================
        [HttpPost("/armacoes/marcas")]
        public async Task<IActionResult> CadastrarMarca([FromForm] string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    return BadRequest("O nome da marca não pode ser vazio.");
                }

                var novaMarca = new Marca
                {
                    Id = Guid.NewGuid(),
                    Nome = nome.Trim(),
                    Ativo = true
                };

                _context.Marcas.Add(novaMarca);
                await _context.SaveChangesAsync();

                // Executa o redirecionamento para o GET index recarregando a grid com a nova marca injetada
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao salvar marca no banco: {ex.Message}");
            }
        }
    }
}