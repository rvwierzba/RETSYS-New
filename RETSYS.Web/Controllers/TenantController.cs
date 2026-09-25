using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;

namespace RETSYS.Web.Controllers
{
    /// <summary>
    /// Controller base para todos os controllers que operam em contexto multi-tenant (por Ótica).
    /// Centraliza a leitura do OticaId a partir da sessão (para perfil Sistema), das claims do usuário autenticado e fallback no banco.
    /// </summary>
    public abstract class TenantController : Controller
    {
        protected bool EhSistema()
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return string.Equals(perfilClaim, nameof(PerfilUsuario.Sistema), StringComparison.OrdinalIgnoreCase)
                || User.IsInRole("Sistema");
        }

        protected bool EhAdministrador()
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return string.Equals(perfilClaim, nameof(PerfilUsuario.Admin), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, nameof(PerfilUsuario.Sistema), StringComparison.OrdinalIgnoreCase)
                || User.IsInRole("Admin")
                || User.IsInRole("Administrador")
                || User.IsInRole("Sistema");
        }

        protected Guid ObterOticaId()
        {
            var context = HttpContext.RequestServices.GetService<ApplicationDbContext>();

            // Se for usuário do perfil Sistema, verifica se há uma Ótica selecionada na Sessão
            if (EhSistema())
            {
                var sessaoOticaId = HttpContext.Session.GetString("OticaAtivaId");
                if (Guid.TryParse(sessaoOticaId, out var oticaSessao) && oticaSessao != Guid.Empty)
                {
                    if (context != null)
                    {
                        var oticaExiste = context.Oticas.Any(o => o.Id == oticaSessao);
                        if (oticaExiste)
                        {
                            return oticaSessao;
                        }
                    }
                    else
                    {
                        return oticaSessao;
                    }
                }
            }

            var claim = User.FindFirst("OticaId")?.Value;
            if (Guid.TryParse(claim, out var oticaId) && oticaId != Guid.Empty)
            {
                return oticaId;
            }

            // Fallback resiliente: se a claim OticaId não constar no cookie antigo, lê direto do usuário no banco
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (context != null)
            {
                Usuario? usuario = null;

                if (Guid.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                }

                if (usuario == null && !string.IsNullOrWhiteSpace(emailClaim))
                {
                    usuario = context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == emailClaim.ToLower());
                }

                if (usuario != null)
                {
                    if (usuario.OticaId != Guid.Empty)
                    {
                        return usuario.OticaId;
                    }

                    // Se o usuário por algum motivo ainda estiver sem OticaId ou com Guid zero no banco
                    var oticaExistente = context.Oticas.FirstOrDefault(o => o.Id != Guid.Empty);
                    if (oticaExistente == null)
                    {
                        oticaExistente = new Otica
                        {
                            Id = Guid.NewGuid(),
                            Nome = "Ótica RETSYS",
                            CriadoEm = DateTime.UtcNow
                        };
                        context.Oticas.Add(oticaExistente);
                        context.SaveChanges();
                    }

                    usuario.OticaId = oticaExistente.Id;
                    context.SaveChanges();
                    return usuario.OticaId;
                }

                // Se não localizou o usuário específico, busca a primeira Ótica válida cadastrada
                var oticaPadrao = context.Oticas.FirstOrDefault(o => o.Id != Guid.Empty);
                if (oticaPadrao != null)
                {
                    return oticaPadrao.Id;
                }
            }

            return Guid.Empty;
        }
    }
}

