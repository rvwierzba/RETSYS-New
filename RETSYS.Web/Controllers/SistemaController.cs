using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RETSYS.Web.Infrastructure;
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
        /// Alterna a ótica ativa em contexto de sessão para usuários com perfil Sistema ou Dono (dentro da sua rede).
        /// </summary>
        [HttpPost("/sistema/trocar-otica")]
        public async Task<IActionResult> TrocarOtica([FromBody] DtoTrocarOtica requisicao)
        {
            // Admin/Vendedor são fixos na própria loja (ObterOticaId ignora a sessão para eles)
            if (!EhSistema() && !EhDono())
            {
                MensagemFlash.Definir(HttpContext, "erro", "Seu perfil não permite alternar de loja.");
                return RedirecionarRetorno();
            }

            if (requisicao == null || requisicao.OticaId == Guid.Empty)
            {
                MensagemFlash.Definir(HttpContext, "erro", "Ótica inválida informada.");
                return RedirecionarRetorno();
            }

            var otica = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == requisicao.OticaId);
            if (otica == null)
            {
                MensagemFlash.Definir(HttpContext, "erro", "A ótica selecionada não foi encontrada.");
                return RedirecionarRetorno();
            }

            // Se for Dono, valida se a ótica selecionada pertence à rede do Dono
            if (EhDono() && !EhSistema())
            {
                var oticaUsuarioId = ObterOticaUsuarioLogado(_context);
                var matrizId = ObterMatrizIdEfetivo(oticaUsuarioId, _context);
                bool pertenceARede = otica.Id == matrizId || otica.MatrizId == matrizId;
                if (!pertenceARede)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Você não tem permissão para acessar esta ótica.");
                    return RedirecionarRetorno();
                }
            }

            HttpContext.Session.SetString("OticaAtivaId", otica.Id.ToString());
            MensagemFlash.Definir(HttpContext, "sucesso", $"Ambiente alternado para: {otica.Nome}");

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
                MensagemFlash.Definir(HttpContext, "erro", "O nome da nova ótica é obrigatório.");
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
            MensagemFlash.Definir(HttpContext, "sucesso", $"Nova ótica '{novaOtica.Nome}' criada com sucesso e ativada.");

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
