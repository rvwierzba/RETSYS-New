using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Interfaces;
using RETSYS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia;

        public UsuariosController(ApplicationDbContext context, IServicoCriptografia criptografia)
        {
            _context = context;
            _criptografia = criptografia;
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
                    Perfil = u.Perfil.ToString(), // Envia o nome do Perfil para o front saber o cargo
                    u.Ativo
                })
                .ToListAsync();

            return Inertia.Render("Users/Index", new { Equipe = equipe });
        }

        // 2. Cadastro de Novo Funcionário (POST)
        [HttpPost("/equipe")]
        public async Task<IActionResult> Store([FromBody] DtoNovoColaborador requisicao)
        {
            if (string.IsNullOrWhiteSpace(requisicao.Nome) || string.IsNullOrWhiteSpace(requisicao.Email))
            {
                return RedirectToAction(nameof(Index));
            }

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == requisicao.Email);
            if (emailExiste)
            {
                Inertia.Share("erro", "Este e-mail corporativo já está em uso pela equipe.");
                return RedirectToAction(nameof(Index));
            }

            var novoFuncionario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.Nome,
                Email = requisicao.Email,
                FilialLoja = requisicao.FilialLoja,
                Perfil = requisicao.Perfil, // Respeita o nível de acesso enviado pelo front-end
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                SenhaHash = _criptografia.CriptografarSenha("RETSYS123_PADRAO")
            };

            _context.Usuarios.Add(novoFuncionario);
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

    // DTO atualizado para capturar a escolha do nível de acesso/perfil enviada pelo formulário
    public record DtoNovoColaborador(string Nome, string Email, string FilialLoja, PerfilUsuario Perfil);
}