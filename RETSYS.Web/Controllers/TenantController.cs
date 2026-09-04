using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace RETSYS.Web.Controllers
{
    /// <summary>
    /// Controller base para todos os controllers que operam em contexto multi-tenant (por Ótica).
    /// Centraliza a leitura do OticaId a partir das claims do usuário autenticado.
    /// </summary>
    public abstract class TenantController : Controller
    {
        protected Guid ObterOticaId()
        {
            var claim = User.FindFirst("OticaId")?.Value;
            return Guid.TryParse(claim, out var oticaId) ? oticaId : Guid.Empty;
        }
    }
}
