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
        public async Task<IActionResult> Store([FromBody] MarcaInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Nome))
            {
                return RedirecionarSeguro();
            }

            var novaMarca = new Marca
            {
                Id = Guid.NewGuid(),
                Nome = input.Nome.Trim(),
                Descricao = input.Descricao?.Trim(),
                Ativo = true
            };

            _context.Marcas.Add(novaMarca);
            await _context.SaveChangesAsync();

            return RedirecionarSeguro();
        }

        // 3. Remoção de Marca (Delete)
        [HttpDelete("/marcas/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return NotFound("Marca não localizada.");
            }

            try
            {
                _context.Marcas.Remove(marca);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("mensagem", "Não é possível excluir esta marca pois ela possui armações vinculadas no estoque.");
                return RedirecionarSeguro();
            }

            return RedirecionarSeguro();
        }

        // Garante que o redirecionamento de volta funcione mesmo se o cabeçalho HTTP referer for vazio
        private IActionResult RedirecionarSeguro()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(referer))
            {
                return RedirectToAction(nameof(Index));
            }
            return Redirect(referer);
        }
    }

    public class MarcaInput
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}