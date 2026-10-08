using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;

namespace RETSYS.Web.Controllers
{
    /// <summary>
    /// Controller base para todos os controllers que operam em contexto multi-tenant (por Ótica).
    /// Centraliza a leitura do OticaId:
    /// - Admin e Vendedores: SEMPRE obtido pelo vínculo direto do usuário logado (Usuario.OticaId).
    /// - Dono: Pode alternar entre as lojas da sua rede (Matriz e Filiais vinculadas).
    /// - Sistema (perfil teste/Deus): Pode selecionar livremente qualquer Ótica via Seletor de Sessão.
    /// </summary>
    public abstract class TenantController : Controller
    {
        protected bool EhSistema()
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return string.Equals(perfilClaim, nameof(PerfilUsuario.Sistema), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "SISTEMA", StringComparison.OrdinalIgnoreCase)
                || User.IsInRole("Sistema");
        }

        protected bool EhDono()
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return string.Equals(perfilClaim, nameof(PerfilUsuario.Dono), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "DONO", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "Proprietario", StringComparison.OrdinalIgnoreCase)
                || User.IsInRole("Dono");
        }

        protected bool EhAdministrador()
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return string.Equals(perfilClaim, nameof(PerfilUsuario.Admin), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, nameof(PerfilUsuario.Dono), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "DONO", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "Gerente", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, nameof(PerfilUsuario.Sistema), StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfilClaim, "SISTEMA", StringComparison.OrdinalIgnoreCase)
                || User.IsInRole("Admin")
                || User.IsInRole("Administrador")
                || User.IsInRole("Dono")
                || User.IsInRole("Gerente")
                || User.IsInRole("Sistema");
        }

        /// <summary>
        /// Obtém a Ótica ativa de acordo com o Perfil do Usuário:
        /// - Para perfil SISTEMA: usa a ótica ativa selecionada na sessão (ou fallback).
        /// - Para perfil DONO: usa a ótica da sessão se pertencer à sua rede (Matriz/Filiais), ou a loja padrão da sua conta.
        /// - Para perfis ADMIN e VENDEDOR: SEMPRE a ótica vinculada ao seu usuário.
        /// </summary>
        protected Guid ObterOticaId()
        {
            var context = HttpContext.RequestServices.GetService<ApplicationDbContext>();

            // 1. Perfil SISTEMA (Super-usuário/Teste): Seletor de Ótica livre via Sessão
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

            // 2. Perfil DONO (Proprietário da Rede): Seletor restrito às lojas da sua rede
            if (EhDono() && context != null)
            {
                var sessaoOticaId = HttpContext.Session.GetString("OticaAtivaId");
                if (Guid.TryParse(sessaoOticaId, out var oticaSessao) && oticaSessao != Guid.Empty)
                {
                    var oticaUsuarioId = ObterOticaUsuarioLogado(context);
                    if (oticaUsuarioId != Guid.Empty)
                    {
                        var matrizId = ObterMatrizIdEfetivo(oticaUsuarioId, context);
                        bool pertenceARede = context.Oticas.Any(o => o.Id == oticaSessao && (o.Id == matrizId || o.MatrizId == matrizId));
                        if (pertenceARede)
                        {
                            return oticaSessao;
                        }
                    }
                }
            }

            // 3. Perfis ADMIN, VENDEDOR (e fallback Dono): Vínculo OBRIGATÓRIO com o Usuário
            if (context != null)
            {
                var oticaDoUsuario = ObterOticaUsuarioLogado(context);
                if (oticaDoUsuario != Guid.Empty)
                {
                    return oticaDoUsuario;
                }
            }

            var claim = User.FindFirst("OticaId")?.Value;
            if (Guid.TryParse(claim, out var oticaIdClaim) && oticaIdClaim != Guid.Empty)
            {
                return oticaIdClaim;
            }

            // Se ainda não achou, fallback na primeira ótica do banco
            if (context != null)
            {
                var oticaPadrao = context.Oticas.FirstOrDefault(o => o.Id != Guid.Empty);
                if (oticaPadrao != null)
                {
                    return oticaPadrao.Id;
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Obtém o OticaId cadastrado diretamente no registro do usuário logado.
        /// </summary>
        protected Guid ObterOticaUsuarioLogado(ApplicationDbContext context)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            Usuario? usuario = null;

            if (Guid.TryParse(usuarioIdClaim, out var usuarioId))
            {
                usuario = context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == usuarioId);
            }

            if (usuario == null && !string.IsNullOrWhiteSpace(emailClaim))
            {
                usuario = context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == emailClaim.ToLower());
            }

            if (usuario != null && usuario.OticaId != Guid.Empty)
            {
                return usuario.OticaId;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Obtém o ID da Matriz da ótica informada. Se a ótica for uma filial, retorna o MatrizId.
        /// Se a ótica for matriz ou única, retorna seu próprio ID.
        /// Útil para compartilhamento de tabelas de preços de lentes e catálogo de rede.
        /// </summary>
        protected Guid ObterMatrizIdEfetivo(Guid oticaId, ApplicationDbContext? context)
        {
            if (oticaId == Guid.Empty || context == null) return oticaId;

            var otica = context.Oticas.AsNoTracking().FirstOrDefault(o => o.Id == oticaId);
            if (otica != null && otica.MatrizId.HasValue && otica.MatrizId.Value != Guid.Empty)
            {
                return otica.MatrizId.Value;
            }

            return oticaId;
        }

        /// <summary>
        /// Retorna a lista de IDs de todas as óticas pertencentes à mesma rede da ótica informada (Matriz + Filiais).
        /// </summary>
        protected List<Guid> ObterIdsOticasDaRede(Guid oticaId, ApplicationDbContext context)
        {
            if (oticaId == Guid.Empty) return new List<Guid>();

            var matrizId = ObterMatrizIdEfetivo(oticaId, context);

            var ids = context.Oticas
                .AsNoTracking()
                .Where(o => o.Id == matrizId || o.MatrizId == matrizId)
                .Select(o => o.Id)
                .ToList();

            if (!ids.Contains(oticaId)) ids.Add(oticaId);
            return ids;
        }
    }
}


