using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;

namespace RETSYS.Web.Controllers
{
    public class MarcasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarcasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de Marcas (Read)
        [HttpGet("/marcas")]
        public async Task<IActionResult> Index()
        {
            var marcas = await _context.Marcas
                .OrderBy(m => m.Nome)
                .Select(m => new {
                    m.Id,
                    m.Nome,
                    m.Descricao,
                    m.Ativo
                })
                .ToListAsync();

            return Inertia.Render("Marcas/Index", new { Marcas = marcas });
        }

        // 2. Gravação de Nova Marca (Create)
        [HttpPost("/marcas")]
        public async Task<IActionResult> Store([FromForm] string nome, [FromForm] string? descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return RedirectToAction(nameof(Index));
            }

            var novaMarca = new Marca
            {
                Nome = nome.Trim(),
                Descricao = descricao?.Trim(),
                Ativo = true
            };

            _context.Marcas.Add(novaMarca);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}