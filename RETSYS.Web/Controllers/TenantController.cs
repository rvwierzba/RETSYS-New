using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;

namespace RETSYS.Web.Controllers
{
    /// <summary>
    /// Controller base para todos os controllers que operam em contexto multi-tenant (por Ótica).
    /// Centraliza a leitura do OticaId a partir das claims do usuário autenticado com fallback direto no banco de dados.
    /// </summary>
    public abstract class TenantController : Controller
    {
        protected Guid ObterOticaId()
        {
            var claim = User.FindFirst("OticaId")?.Value;
            if (Guid.TryParse(claim, out var oticaId) && oticaId != Guid.Empty)
            {
                return oticaId;
            }

            // Fallback resiliente: se a claim OticaId não constar no cookie antigo, lê direto do usuário no banco
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            var context = HttpContext.RequestServices.GetService<ApplicationDbContext>();
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

                    // Se o usuário por algum motivo ainda estiver sem OticaId no banco
                    var oticaExistente = context.Oticas.FirstOrDefault();
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
            }

            return Guid.Empty;
        }
    }
}
