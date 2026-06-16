using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;

namespace RETSYS.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem dos Colaboradores (GET)
        [HttpGet("/equipe")]
        public async Task<IActionResult> Index()
        {
            var equipe = await _context.Usuarios
                .OrderBy(u => u.Nome)
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.FilialLoja,
                    u.Ativo
                })
                .ToListAsync();

            return Inertia.Render("Users/Index", new { Equipe = equipe });
        }

        // 2. Cadastro de Novo Funcionário (POST)
        [HttpPost("/equipe")]
        public async Task<IActionResult> Store([FromBody] Usuario novoUsuario)
        {
            if (string.IsNullOrWhiteSpace(novoUsuario.Nome) || string.IsNullOrWhiteSpace(novoUsuario.Email))
            {
                return RedirectToAction(nameof(Index));
            }

            if (novoUsuario.Id == Guid.Empty)
            {
                novoUsuario.Id = Guid.NewGuid();
            }

            // Define propriedades padrão de segurança para o MVP
            novoUsuario.Ativo = true;
            novoUsuario.SenhaHash = "RETSYS123_PADRAO"; // Senha provisória para o balcão

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 3. Alternar Status Ativo/Inativo (POST)
        [HttpPost("/equipe/alternar-status/{id:guid}")]
        public async Task<IActionResult> AlternarStatus(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Ativo = !usuario.Ativo;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}