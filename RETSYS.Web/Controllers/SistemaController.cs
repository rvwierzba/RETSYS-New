using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;
using RETSYS.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    [Authorize]
    public class SistemaController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public SistemaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Alterna a ótica ativa em contexto de sessão para usuários com perfil Sistema.
        /// </summary>
        [HttpPost("/sistema/trocar-otica")]
        public async Task<IActionResult> TrocarOtica([FromBody] DtoTrocarOtica requisicao)
        {
            if (!EhSistema())
            {
                return Forbid();
            }

            if (requisicao == null || requisicao.OticaId == Guid.Empty)
            {
                Inertia.Share("erro", "Ótica inválida informada.");
                return RedirecionarRetorno();
            }

            var otica = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == requisicao.OticaId);
            if (otica == null)
            {
                Inertia.Share("erro", "A ótica selecionada não foi encontrada.");
                return RedirecionarRetorno();
            }

            HttpContext.Session.SetString("OticaAtivaId", otica.Id.ToString());
            Inertia.Share("sucesso", $"Ambiente alternado para: {otica.Nome}");

            return RedirecionarRetorno();
        }

        /// <summary>
        /// Permite aos gestores do Sistema cadastrar uma nova Ótica rapidamente.
        /// </summary>
        [HttpPost("/sistema/criar-otica")]
        public async Task<IActionResult> CriarOtica([FromBody] DtoCriarOtica requisicao)
        {
            if (!EhSistema())
            {
                return Forbid();
            }

            if (requisicao == null || string.IsNullOrWhiteSpace(requisicao.Nome))
            {
                Inertia.Share("erro", "O nome da nova ótica é obrigatório.");
                return RedirecionarRetorno();
            }

            var novaOtica = new Otica
            {
                Id = Guid.NewGuid(),
                Nome = requisicao.Nome.Trim(),
                CriadoEm = DateTime.UtcNow
            };

            _context.Oticas.Add(novaOtica);

            // Cria configuração inicial padrão para a nova ótica
            var configPadrao = new ConfiguracaoLoja
            {
                Id = Guid.NewGuid(),
                OticaId = novaOtica.Id,
                NomeLoja = novaOtica.Nome,
                Cnpj = "",
                PixApiKey = ""
            };
            _context.ConfiguracoesLoja.Add(configPadrao);

            await _context.SaveChangesAsync();

            // Já seleciona a nova ótica como ativa
            HttpContext.Session.SetString("OticaAtivaId", novaOtica.Id.ToString());
            Inertia.Share("sucesso", $"Nova ótica '{novaOtica.Nome}' criada com sucesso e ativada.");

            return RedirecionarRetorno();
        }

        private IActionResult RedirecionarRetorno()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }

    public record DtoTrocarOtica(Guid OticaId);
    public record DtoCriarOtica(string Nome);
}
