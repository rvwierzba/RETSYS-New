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

            // Renderiza Frontend/Pages/Marcas/Index.vue passando a lista como Prop
            return Inertia.Render("Marcas/Index", new { Marcas = marcas });
        }

        // 2. Gravação de Nova Marca (Create)
        [HttpPost("/marcas")]
        public async Task<IActionResult> Store([FromBody] Marca novaMarca)
        {
            if (string.IsNullOrWhiteSpace(novaMarca.Nome))
            {
                return RedirectToAction(nameof(Index)); // Simples validação para o MVP
            }

            _context.Marcas.Add(novaMarca);
            await _context.SaveChangesAsync();

            // Redireciona de volta para a listagem atualizando o SPA instantaneamente
            return RedirectToAction(nameof(Index));
        }
    }
}