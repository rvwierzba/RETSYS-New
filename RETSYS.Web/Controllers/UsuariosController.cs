using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Interfaces;
using RETSYS.Domain.Enums;

namespace RETSYS.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia; // 🔥 ADICIONADO: Dependência para tratamento seguro de senhas

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
                    u.Ativo
                })
                .ToListAsync();

            return Inertia.Render("Users/Index", new { Equipe = equipe });
        }

        // 2. Cadastro de Novo Funcionário (POST)
        [HttpPost("/equipe")]
        public async Task<IActionResult> Store([FromBody] DtoNovoColaborador requisicao) // 🔥 CORRIGIDO: Uso de DTO para bloquear Mass Assignment
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

            // Instanciação manual controlada pelo servidor
            var novoFuncionario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.Nome,
                Email = requisicao.Email,
                FilialLoja = requisicao.FilialLoja,
                Perfil = PerfilUsuario.Vendedor, // 🛡️ SEGURANÇA: Todo membro cadastrado aqui nasce estritamente como Vendedor
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                SenhaHash = _criptografia.CriptografarSenha("RETSYS123_PADRAO") // 🔥 CORRIGIDO: Armazenamento em hash seguro para o login
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

    // 🔥 DTO de Segurança: Isola o payload recebido do front-end impedindo fraudes de privilégio (RBAC)
    public record DtoNovoColaborador(string Nome, string Email, string FilialLoja);
}