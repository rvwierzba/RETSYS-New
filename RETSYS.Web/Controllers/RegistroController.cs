using Microsoft.AspNetCore.Mvc;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Interfaces;
using RETSYS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace RETSYS.Web.Controllers
{
    public class RegistroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia;

        public RegistroController(ApplicationDbContext context, IServicoCriptografia criptografia)
        {
            _context = context;
            _criptografia =_criptografia = criptografia;
        }

        [HttpGet("/cadastro")]
        public IActionResult CriarConta()
        {
            return Inertia.Render("Auth/Cadastro");
        }

        [HttpPost("/cadastro")]
        public async Task<IActionResult> Registrar([FromBody] DtoRegistro requisicao)
        {
            if (string.IsNullOrWhiteSpace(requisicao.Nome) || string.IsNullOrWhiteSpace(requisicao.Email))
            {
                Inertia.Share("erro", "Preencha todos os campos obrigatórios.");
                return RedirectToAction(nameof(CriarConta));
            }

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == requisicao.Email);
            if (emailExiste)
            {
                Inertia.Share("erro", "Este e-mail já está registrado no RETSYS.");
                return RedirectToAction(nameof(CriarConta));
            }

            // Instancia a entidade Usuario mapeando explicitamente como Dono/Admin
            var novoAdmin = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.Nome,
                Email = requisicao.Email,
                FilialLoja = requisicao.NomeDaOtica,
                Perfil = PerfilUsuario.Admin, // 🔥 CORRIGIDO: Dono da conta agora nasce como Administrador global
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                SenhaHash = _criptografia.CriptografarSenha(requisicao.Senha) 
            };

            _context.Usuarios.Add(novoAdmin);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Autenticacao");
        }
    }

    public record DtoRegistro(string Nome, string Email, string Senha, string NomeDaOtica);
}