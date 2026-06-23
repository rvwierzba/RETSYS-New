using Microsoft.AspNetCore.Mvc;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Interfaces;

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

        // 1. Abre a tela de Auto-Cadastro (GET)
        [HttpGet("/cadastro")]
        public IActionResult CriarConta()
        {
            return Inertia.Render("Auth/Cadastro");
        }

        // 2. Processa o registro do novo dono da ótica (POST)
        [HttpPost("/cadastro")]
        public async Task<IActionResult> Registrar([FromBody] DtoRegistro requisicao)
        {
            if (string.IsNullOrWhiteSpace(requisicao.Nome) || string.IsNullOrWhiteSpace(requisicao.Email))
            {
                Inertia.Share("erro", "Preencha todos os campos obrigatórios.");
                return RedirectToAction(nameof(CriarConta));
            }

            // Evita duplicidade de e-mail corporativo
            var emailExiste = _context.Usuarios.Any(u => u.Email == requisicao.Email);
            if (emailExiste)
            {
                Inertia.Share("erro", "Este e-mail já está registrado no RETSYS.");
                return RedirectToAction(nameof(CriarConta));
            }

            // Instancia a entidade Usuario mapeando como Dono/Admin
            var novoAdmin = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.Nome,
                Email = requisicao.Email,
                FilialLoja = requisicao.NomeDaOtica,
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                // Passa a senha limpa pelo método de hash da sua interface de criptografia
                SenhaHash = _criptografia.CriptografarSenha(requisicao.Senha) 
            };

            _context.Usuarios.Add(novoAdmin);
            await _context.SaveChangesAsync();

            // Conta criada! Redireciona direto para o login com feedback positivo
            return RedirectToAction("Login", "Autenticacao");
        }
    }

    // DTO para transportar os dados do Vue para o C# de forma limpa
    public record DtoRegistro(string Nome, string Email, string Senha, string NomeDaOtica);
}