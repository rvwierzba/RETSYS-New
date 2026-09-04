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
            _criptografia = criptografia;
        }

        [HttpGet("/cadastro")]
        public IActionResult CriarConta()
        {
            return Inertia.Render("Auth/Cadastro");
        }

        [HttpPost("/cadastro")]
        public async Task<IActionResult> Registrar([FromBody] DtoRegistro requisicao)
        {
            if (string.IsNullOrWhiteSpace(requisicao.Nome) ||
                string.IsNullOrWhiteSpace(requisicao.Email) ||
                string.IsNullOrWhiteSpace(requisicao.Senha) ||
                string.IsNullOrWhiteSpace(requisicao.NomeDaOtica))
            {
                Inertia.Share("erro", "Preencha todos os campos obrigatórios.");
                return RedirectToAction(nameof(CriarConta));
            }

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == requisicao.Email.ToLower());
            if (emailExiste)
            {
                Inertia.Share("erro", "Este e-mail já está registrado no RETSYS.");
                return RedirectToAction(nameof(CriarConta));
            }

            // Cria a Ótica (Tenant) que será o container de todos os dados desse cliente
            var novaOtica = new Otica
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.NomeDaOtica.Trim(),
                CriadoEm = DateTime.UtcNow
            };

            _context.Oticas.Add(novaOtica);

            // Dono do estabelecimento sempre nasce como Admin, vinculado à Ótica recém-criada
            var novoAdmin = new Usuario
            {
                Id = Guid.NewGuid(),
                OticaId = novaOtica.Id,
                Nome = requisicao.Nome,
                Email = requisicao.Email,
                FilialLoja = "Matriz",
                Perfil = PerfilUsuario.Admin,
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
