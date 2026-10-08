using System.Security.Claims;
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
public class UsuariosController : TenantController
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
        var idsRede = ObterIdsOticasDaRede(oticaId, _context);

        IQueryable<Usuario> queryUsuarios = _context.Usuarios.AsNoTracking();

        if (EhSistema())
        {
            // Sistema pode ver usuários da ótica ativa ou de todas as lojas
            queryUsuarios = queryUsuarios.Where(u => u.OticaId == oticaId);
        }
        else if (EhDono())
        {
            // Dono vê usuários da sua rede inteira
            queryUsuarios = queryUsuarios.Where(u => idsRede.Contains(u.OticaId));
        }
        else
        {
            // Admin vê usuários da sua própria loja
            queryUsuarios = queryUsuarios.Where(u => u.OticaId == oticaId);
        }

        var equipe = await queryUsuarios
            .OrderByDescending(u => u.CriadoEm)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.FilialLoja,
                u.OticaId,
                OticaNome = u.Otica != null ? u.Otica.Nome : u.FilialLoja,
                Perfil = (int)u.Perfil,
                PerfilNome = u.Perfil == PerfilUsuario.Sistema ? "Sistema" :
                             u.Perfil == PerfilUsuario.Dono ? "Dono" :
                             u.Perfil == PerfilUsuario.Admin ? "Administrador" : "Vendedor",
                u.Ativo,
                u.PercentualComissao,
                u.FotoUrl,
                u.UltimoAcesso,
                u.CriadoEm
            })
            .ToListAsync();

        // Lojas disponíveis para transferência / alocação de funcionários
        IQueryable<Otica> queryLojas = _context.Oticas.AsNoTracking();
        if (!EhSistema())
        {
            queryLojas = queryLojas.Where(o => idsRede.Contains(o.Id));
        }

        var lojas = await queryLojas
            .OrderBy(l => l.Nome)
            .Select(l => new { Id = l.Id, Nome = l.Nome })
            .ToListAsync();

        return Inertia.Render("Users/Index", new 
        { 
            Equipe = equipe, 
            Lojas = lojas, 
            EhSistema = EhSistema(),
            EhDono = EhDono()
        });
    }

    // POST: /equipe
    [HttpPost("/equipe")]
    public async Task<IActionResult> Store([FromBody] DtoNovoColaborador model)
    {
        if (!EhAdministrador())
        {
            return Forbid();
        }

        if (model.Perfil == PerfilUsuario.Sistema && !EhSistema())
        {
            Inertia.Share("erro", "Apenas usuários com perfil Sistema podem criar outros usuários de Sistema.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Dono && !EhSistema() && !EhDono())
        {
            Inertia.Share("erro", "Apenas Donos ou administradores de Sistema podem criar outros usuários com perfil Dono.");
            return RedirectToAction(nameof(Index));
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

        var oticaIdAtiva = ObterOticaId();
        var idsRede = ObterIdsOticasDaRede(oticaIdAtiva, _context);

        Otica? oticaAlvo = null;

        if (model.OticaId.HasValue && model.OticaId.Value != Guid.Empty)
        {
            oticaAlvo = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == model.OticaId.Value);
        }

        if (oticaAlvo == null && !string.IsNullOrWhiteSpace(model.FilialLoja))
        {
            oticaAlvo = await _context.Oticas.FirstOrDefaultAsync(o => o.Nome.ToLower() == model.FilialLoja.Trim().ToLower());
        }

        if (oticaAlvo == null)
        {
            oticaAlvo = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == oticaIdAtiva);
        }

        if (oticaAlvo == null)
        {
            oticaAlvo = new Otica { Id = Guid.NewGuid(), Nome = !string.IsNullOrWhiteSpace(model.FilialLoja) ? model.FilialLoja.Trim() : "Ótica Matriz", CriadoEm = DateTime.UtcNow };
            _context.Oticas.Add(oticaAlvo);
            await _context.SaveChangesAsync();
        }

        // Validação de segurança: Dono e Admin só podem alocar em lojas da sua própria rede
        if (!EhSistema() && !idsRede.Contains(oticaAlvo.Id))
        {
            Inertia.Share("erro", "Você só pode alocar colaboradores em lojas pertencentes à sua rede.");
            return RedirectToAction(nameof(Index));
        }

        var hashSenha = _criptografia.CriptografarSenha(model.Senha);

        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            OticaId = oticaAlvo.Id,
            Nome = model.Nome.Trim(),
            Email = model.Email.Trim().ToLower(),
            SenhaHash = hashSenha,
            FilialLoja = oticaAlvo.Nome,
            Perfil = model.Perfil,
            PercentualComissao = model.PercentualComissao > 0 ? model.PercentualComissao : 3.00m,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        Inertia.Share("sucesso", $"Colaborador(a) {novoUsuario.Nome} cadastrado(a) com sucesso!");
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

        if (model.Perfil == PerfilUsuario.Sistema && !EhSistema())
        {
            Inertia.Share("erro", "Apenas usuários com perfil Sistema podem promover contas para o perfil Sistema.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Dono && !EhSistema() && !EhDono())
        {
            Inertia.Share("erro", "Apenas Donos ou administradores de Sistema podem promover contas para o perfil Dono.");
            return RedirectToAction(nameof(Index));
        }

        var oticaIdAtiva = ObterOticaId();
        var idsRede = ObterIdsOticasDaRede(oticaIdAtiva, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        // Validação de acesso ao usuário
        if (!EhSistema() && !idsRede.Contains(usuario.OticaId))
        {
            return Forbid();
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.Id != id);

        if (emailExiste)
        {
            Inertia.Share("erro", "Este e-mail já está sendo utilizado por outro usuário.");
            return RedirectToAction(nameof(Index));
        }

        Otica? oticaAlvo = null;
        if (model.OticaId.HasValue && model.OticaId.Value != Guid.Empty)
        {
            oticaAlvo = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == model.OticaId.Value);
        }

        if (oticaAlvo == null && !string.IsNullOrWhiteSpace(model.FilialLoja))
        {
            oticaAlvo = await _context.Oticas.FirstOrDefaultAsync(o => o.Nome.ToLower() == model.FilialLoja.Trim().ToLower());
        }

        if (oticaAlvo != null)
        {
            if (!EhSistema() && !idsRede.Contains(oticaAlvo.Id))
            {
                Inertia.Share("erro", "Você só pode transferir colaboradores para lojas da sua rede.");
                return RedirectToAction(nameof(Index));
            }

            usuario.OticaId = oticaAlvo.Id;
            usuario.FilialLoja = oticaAlvo.Nome;
        }

        usuario.Nome = model.Nome.Trim();
        usuario.Email = model.Email.Trim().ToLower();
        usuario.Perfil = model.Perfil;
        usuario.Ativo = model.Ativo;
        usuario.PercentualComissao = model.PercentualComissao;

        if (!string.IsNullOrWhiteSpace(model.NovaSenha))
        {
            usuario.SenhaHash = _criptografia.CriptografarSenha(model.NovaSenha);
        }

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        Inertia.Share("sucesso", $"Dados do colaborador {usuario.Nome} atualizados com sucesso!");
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
        var idsRede = ObterIdsOticasDaRede(oticaId, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && (idsRede.Contains(u.OticaId) || EhSistema()));
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
        var idsRede = ObterIdsOticasDaRede(oticaId, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && (idsRede.Contains(u.OticaId) || EhSistema()));
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

public record DtoNovoColaborador(
    string Nome,
    string Email,
    string? FilialLoja,
    string Senha,
    PerfilUsuario Perfil = PerfilUsuario.Vendedor,
    decimal PercentualComissao = 3.00m,
    Guid? OticaId = null
);

public record DtoEditarColaborador(
    string Nome,
    string Email,
    string? FilialLoja,
    PerfilUsuario Perfil,
    bool Ativo,
    decimal PercentualComissao = 3.00m,
    string? NovaSenha = null,
    Guid? OticaId = null
);

