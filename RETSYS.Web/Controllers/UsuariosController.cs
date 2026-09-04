using InertiaCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;
using RETSYS.Domain.Interfaces;
using RETSYS.Infrastructure.Data;

namespace RETSYS.Web.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IServicoCriptografia _criptografia;

    public UsuariosController(ApplicationDbContext context, IServicoCriptografia criptografia)
    {
        _context = context;
        _criptografia = criptografia;
    }

    // GET: /equipe
    [HttpGet("/equipe")]
    [HttpGet("/usuarios")]
    public async Task<IActionResult> Index()
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        var oticaId = ObterOticaId();

        var equipe = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.OticaId == oticaId)
            .OrderByDescending(u => u.CriadoEm)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.FilialLoja,
                Perfil = (int)u.Perfil,
                PerfilNome = u.Perfil == PerfilUsuario.Admin ? "Administrador" : "Vendedor",
                u.Ativo,
                u.PercentualComissao,
                u.FotoUrl,
                u.UltimoAcesso,
                u.CriadoEm
            })
            .ToListAsync();

        var lojas = await _context.ConfiguracoesLoja
            .AsNoTracking()
            .Select(l => new { l.Id, Nome = l.NomeLoja })
            .ToListAsync();

        return Inertia.Render("Users/Index", new { Equipe = equipe, Lojas = lojas });
    }

    // POST: /equipe
    [HttpPost("/equipe")]
    public async Task<IActionResult> Store([FromBody] DtoNovoColaborador model)
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Senha))
        {
            Inertia.Share("erro", "Preencha todos os campos obrigatórios (Nome, E-mail e Senha).");
            return RedirectToAction(nameof(Index));
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (emailExiste)
        {
            Inertia.Share("erro", "Este e-mail corporativo já está em uso pela equipe.");
            return RedirectToAction(nameof(Index));
        }

        var hashSenha = _criptografia.CriptografarSenha(model.Senha);

        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            OticaId = ObterOticaId(),
            Nome = model.Nome.Trim(),
            Email = model.Email.Trim().ToLower(),
            SenhaHash = hashSenha,
            FilialLoja = string.IsNullOrWhiteSpace(model.FilialLoja) ? "Matriz" : model.FilialLoja.Trim(),
            Perfil = model.Perfil,
            PercentualComissao = model.PercentualComissao > 0 ? model.PercentualComissao : 3.00m,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /equipe/editar/{id:guid}
    [HttpPost("/equipe/editar/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] DtoEditarColaborador model)
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        var oticaId = ObterOticaId();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.OticaId == oticaId);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.Id != id);

        if (emailExiste)
        {
            Inertia.Share("erro", "Este e-mail já está sendo utilizado por outro usuário.");
            return RedirectToAction(nameof(Index));
        }

        usuario.Nome = model.Nome.Trim();
        usuario.Email = model.Email.Trim().ToLower();
        usuario.FilialLoja = model.FilialLoja;
        usuario.Perfil = model.Perfil;
        usuario.Ativo = model.Ativo;
        usuario.PercentualComissao = model.PercentualComissao;

        if (!string.IsNullOrWhiteSpace(model.NovaSenha))
        {
            usuario.SenhaHash = _criptografia.CriptografarSenha(model.NovaSenha);
        }

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /equipe/alternar-status/{id:guid}
    [HttpPost("/equipe/alternar-status/{id:guid}")]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        var oticaId = ObterOticaId();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.OticaId == oticaId);
        if (usuario != null)
        {
            usuario.Ativo = !usuario.Ativo;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /equipe/excluir/{id:guid}
    [HttpPost("/equipe/excluir/{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        var oticaId = ObterOticaId();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.OticaId == oticaId);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // AUXILIARES
    // =========================================================================

    private bool EhAdministrador()
    {
        return User.IsInRole("Admin")
            || User.IsInRole("Administrador")
            || User.IsInRole("admin")
            || User.IsInRole("administrador");
    }

    private Guid ObterOticaId()
    {
        var claim = User.FindFirst("OticaId")?.Value;
        return Guid.TryParse(claim, out var oticaId) ? oticaId : Guid.Empty;
    }
}

public record DtoNovoColaborador(
    string Nome,
    string Email,
    string FilialLoja,
    string Senha,
    PerfilUsuario Perfil = PerfilUsuario.Vendedor,
    decimal PercentualComissao = 3.00m
);

public record DtoEditarColaborador(
    string Nome,
    string Email,
    string FilialLoja,
    PerfilUsuario Perfil,
    bool Ativo,
    decimal PercentualComissao = 3.00m,
    string? NovaSenha = null
);
