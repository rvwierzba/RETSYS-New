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

        // 1. Listagem do Estoque Real (GET)
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
                    Codigo = a.CodigoSku, // Ajustado para ler a propriedade correta do banco
                    a.Cor,
                    a.Tamanho,
                    a.Material,
                    a.QuantidadeEstoque,
                    PrecoFinal = a.PrecoVenda, // Ajustado para ler o preço de venda atualizado
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

        // 2. Entrada de Nova Armação (POST)
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

            // Garante a data correta de inserção no banco
            novaArmacao.CriadoEm = DateTime.UtcNow;

            _context.Armacoes.Add(novaArmacao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}