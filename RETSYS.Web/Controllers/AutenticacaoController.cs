using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Interfaces;

namespace RETSYS.Web.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia;

        public AutenticacaoController(ApplicationDbContext context, IServicoCriptografia criptografia)
        {
            _context = context;
            _criptografia = criptografia;
        }

        // 1. Renderiza a Tela de Login (GET)
        [HttpGet("/login")]
        public IActionResult Login()
        {
            // Retorna o componente Vue localizado em Frontend/Pages/Auth/Login.vue
            return Inertia.Render("Auth/Login");
        }

        // 2. Processa a Tentativa de Entrada (POST)
        [HttpPost("/login")]
        public async Task<IActionResult> Autenticar([FromBody] DtoLogin requisicao)
        {
            // Busca o usuário pelo e-mail informado
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == requisicao.Email && u.Ativo);

            // Se não achar ou a senha encriptada não bater, devolve erro
            if (usuario == null || !_criptografia.VerificarSenha(requisicao.Senha, usuario.SenhaHash))
            {
                // Injeta uma mensagem de erro temporária que o Inertia repassa pro Vue
                Inertia.Share("erro", "Credenciais inválidas ou usuário inativo.");
                return RedirectToAction(nameof(Login));
            }

            // Cria os "crachás" (Claims) de identificação do usuário dentro do sistema
            var credenciais = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString()), // Vendedor, Gerente ou Admin
                new Claim("Filial", usuario.FilialLoja)
            };

            var identidade = new ClaimsIdentity(credenciais, "Cookies");
            var principal = new ClaimsPrincipal(identidade);

            // Grava o cookie criptografado no navegador do usuário
            await HttpContext.SignInAsync("Cookies", principal);
            
            return RedirectToRoute(new { controller = "Dashboard", action = "Index" });
        }

        // 3. Efetua a Saída do Sistema (POST)
        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction(nameof(Login));
        }
    }

    // Objeto auxiliar de transferência para receber os dados do Vue
    public record DtoLogin(string Email, string Senha);
}