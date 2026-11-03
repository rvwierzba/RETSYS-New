using System.Security.Claims;
using InertiaCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RETSYS.Web.Infrastructure;
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
            MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem gerenciar a equipe.");
            return Redirect("/dashboard");
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
        // Admin aloca apenas na própria loja; Dono na rede; Sistema em todas
        var oticasPermitidas = ObterOticasPermitidas(_context);
        var lojas = await _context.Oticas.AsNoTracking()
            .Where(o => oticasPermitidas.Contains(o.Id))
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
            MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem gerenciar a equipe.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Sistema && !EhSistema())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas usuários com perfil Sistema podem criar outros usuários de Sistema.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Dono && !EhSistema() && !EhDono())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas Donos ou administradores de Sistema podem criar outros usuários com perfil Dono.");
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Senha))
        {
            MensagemFlash.Definir(HttpContext, "erro", "Preencha todos os campos obrigatórios (Nome, E-mail e Senha).");
            return RedirectToAction(nameof(Index));
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (emailExiste)
        {
            MensagemFlash.Definir(HttpContext, "erro", "Este e-mail corporativo já está em uso pela equipe.");
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
            MensagemFlash.Definir(HttpContext, "erro", "Loja de alocação não encontrada.");
            return RedirectToAction(nameof(Index));
        }

        // Validação de segurança: Admin só aloca na própria loja; Dono na sua rede
        if (!ObterOticasPermitidas(_context).Contains(oticaAlvo.Id))
        {
            MensagemFlash.Definir(HttpContext, "erro", "Você só pode alocar colaboradores em lojas que administra.");
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

        MensagemFlash.Definir(HttpContext, "sucesso", $"Colaborador(a) {novoUsuario.Nome} cadastrado(a) com sucesso!");
        return RedirectToAction(nameof(Index));
    }

    // POST: /equipe/editar/{id:guid}
    [HttpPost("/equipe/editar/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] DtoEditarColaborador model)
    {
        if (!EhAdministrador())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem gerenciar a equipe.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Sistema && !EhSistema())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas usuários com perfil Sistema podem promover contas para o perfil Sistema.");
            return RedirectToAction(nameof(Index));
        }

        if (model.Perfil == PerfilUsuario.Dono && !EhSistema() && !EhDono())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas Donos ou administradores de Sistema podem promover contas para o perfil Dono.");
            return RedirectToAction(nameof(Index));
        }

        var oticaIdAtiva = ObterOticaId();
        var idsRede = ObterIdsOticasDaRede(oticaIdAtiva, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null)
        {
            MensagemFlash.Definir(HttpContext, "erro", "Usuário não encontrado.");
            return RedirectToAction(nameof(Index));
        }

        // Validação de acesso ao usuário (loja permitida e hierarquia de perfis)
        if (!PodeGerenciar(usuario))
        {
            MensagemFlash.Definir(HttpContext, "erro", "Você não tem permissão para editar este usuário.");
            return RedirectToAction(nameof(Index));
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.Id != id);

        if (emailExiste)
        {
            MensagemFlash.Definir(HttpContext, "erro", "Este e-mail já está sendo utilizado por outro usuário.");
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
            if (!ObterOticasPermitidas(_context).Contains(oticaAlvo.Id))
            {
                MensagemFlash.Definir(HttpContext, "erro", "Você só pode transferir colaboradores para lojas que administra.");
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

        MensagemFlash.Definir(HttpContext, "sucesso", $"Dados do colaborador {usuario.Nome} atualizados com sucesso!");
        return RedirectToAction(nameof(Index));
    }

    // POST: /equipe/alternar-status/{id:guid}
    [HttpPost("/equipe/alternar-status/{id:guid}")]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        if (!EhAdministrador())
        {
            MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem gerenciar a equipe.");
            return RedirectToAction(nameof(Index));
        }

        var oticaId = ObterOticaId();
        var idsRede = ObterIdsOticasDaRede(oticaId, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario != null && PodeGerenciar(usuario) && !EhUsuarioLogado(usuario))
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
            MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem gerenciar a equipe.");
            return RedirectToAction(nameof(Index));
        }

        var oticaId = ObterOticaId();
        var idsRede = ObterIdsOticasDaRede(oticaId, _context);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario != null && PodeGerenciar(usuario) && !EhUsuarioLogado(usuario))
        {
            try
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                MensagemFlash.Definir(HttpContext, "sucesso", $"Colaborador(a) {usuario.Nome} excluído(a).");
            }
            catch (DbUpdateException)
            {
                // Usuário com OSs, vendas ou comissões vinculadas não pode ser apagado (histórico contábil)
                MensagemFlash.Definir(HttpContext, "erro", "Este colaborador possui vendas/OSs vinculadas e não pode ser excluído. Desative o acesso em vez de excluir.");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private static int NivelPerfil(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Sistema => 4,
        PerfilUsuario.Dono => 3,
        PerfilUsuario.Admin => 2,
        _ => 1
    };

    private int NivelUsuarioLogado()
    {
        if (EhSistema()) return 4;
        if (EhDono()) return 3;
        if (EhAdministrador()) return 2;
        return 1;
    }

    // Só gerencia usuários de lojas permitidas e de perfil igual ou inferior ao seu
    private bool PodeGerenciar(Usuario alvo)
    {
        return ObterOticasPermitidas(_context).Contains(alvo.OticaId)
            && NivelPerfil(alvo.Perfil) <= NivelUsuarioLogado();
    }

    private bool EhUsuarioLogado(Usuario alvo)
    {
        return Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idLogado) && idLogado == alvo.Id;
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

