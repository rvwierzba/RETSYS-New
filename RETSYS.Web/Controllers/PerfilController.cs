using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Interfaces;
using System.Security.Claims;

namespace RETSYS.Web.Controllers
{
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia;
        private readonly IWebHostEnvironment _environment; // 📸 Necessário para mapear a pasta wwwroot no servidor

        public PerfilController(
            ApplicationDbContext context, 
            IServicoCriptografia criptografia, 
            IWebHostEnvironment environment)
        {
            _context = context;
            _criptografia = criptografia;
            _environment = environment;
        }

        // 1. Renderiza a página de perfil com os dados do usuário logado (GET)
        [HttpGet("/perfil")]
        public async Task<IActionResult> Index()
        {
            // Captura o ID do usuário diretamente dos Claims de autenticação do sistema
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
            {
                return RedirectToAction("Login", "Autenticacao");
            }

            var usuario = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.FotoUrl // Envia o caminho da foto gravado para o Vue montar o preview
                })
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Autenticacao");
            }

            // Entrega o DTO "Colaborador" que a tela Perfil.vue espera receber nas props
            return Inertia.Render("Perfil", new { Colaborador = usuario });
        }

        // 2. Processa as atualizações cadastrais, senha e upload de avatar (POST)
        [HttpPost("/perfil")]
        public async Task<IActionResult> Atualizar([FromForm] DtoAtualizarPerfil payload) // 🔄 FromForm para suportar envio de arquivos físicos
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
            {
                return RedirectToAction("Login", "Autenticacao");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return RedirectToAction("Login", "Autenticacao");
            }

            // Atualização de campos básicos
            if (!string.IsNullOrWhiteSpace(payload.Nome))
            {
                usuario.Nome = payload.Nome.Trim();
            }

            // TRATAMENTO DA FOTO: Se o usuário enviou um novo arquivo de imagem
            if (payload.FotoNova != null && payload.FotoNova.Length > 0)
            {
                // Garante a existência do diretório de uploads dentro da wwwroot
                var pastaUploads = Path.Combine(_environment.WebRootPath, "uploads", "avatares");
                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                // Gera um nome único para o arquivo usando GUID para evitar colisões
                var extensao = Path.GetExtension(payload.FotoNova.FileName);
                var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                // Salva o arquivo fisicamente no disco do servidor
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await payload.FotoNova.CopyToAsync(stream);
                }

                // Se o usuário já tinha uma foto antiga, remove ela do disco para não acumular lixo
                if (!string.IsNullOrEmpty(usuario.FotoUrl))
                {
                    var caminhoFotoAntiga = Path.Combine(_environment.WebRootPath, usuario.FotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoFotoAntiga))
                    {
                        System.IO.File.Delete(caminhoFotoAntiga);
                    }
                }

                // Salva o caminho relativo web no banco de dados
                usuario.FotoUrl = $"/uploads/avatares/{nomeArquivo}";
            }

            // TRATAMENTO DA SENHA: Se o bloco opcional de nova senha foi preenchido
            if (!string.IsNullOrWhiteSpace(payload.NovaSenha))
            {
                usuario.SenhaHash = _criptografia.CriptografarSenha(payload.NovaSenha);
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // Compartilha os novos dados na sessão global do Inertia para atualizar o topo imediatamente
            Inertia.Share("auth", new {
                usuarioNome = usuario.Nome,
                usuarioPerfil = usuario.Perfil.ToString(),
                usuarioFoto = usuario.FotoUrl,
                spotifyTokenAtivo = HttpContext.Session.GetString("SpotifyToken") != null
            });

            return RedirectToAction(nameof(Index));
        }
    }

    // DTO customizado para capturar dados mistos via formulário multipart/form-data
    public record DtoAtualizarPerfil(string Nome, string? NovaSenha, IFormFile? FotoNova);
}