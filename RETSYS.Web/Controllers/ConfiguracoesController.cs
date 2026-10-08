using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace RETSYS.Web.Controllers
{
    [Authorize]
    public class ConfiguracoesController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public const string TEMPLATE_CADASTRO_PADRAO = @"Olá {cliente}! 👋 A sua Ordem de Serviço *#{numero_os}* na *{otica}* foi registrada com sucesso!

📋 *Resumo do Pedido:*
{resumo_pedido}

📅 *Previsão de Entrega:* {previsao_entrega}

Seus óculos serão montados com toda a atenção e passarão por uma rigorosa conferência de qualidade. ✨
Assim que estiverem prontos para retirada, te avisaremos por aqui no WhatsApp.

Agradecemos imensamente pela preferência e confiança! 👓";

        public const string TEMPLATE_PRONTO_PADRAO = @"Olá {cliente}! ✨ Ótimas notícias: Seus óculos da Ordem de Serviço *#{numero_os}* estão *PRONTOS* para retirada na *{otica}*! 👓🎉

💳 *Situação do Pagamento:* {status_pagamento}
{saldo_devedor}

Você já pode passar em nossa loja para retirar seus óculos e realizar o ajuste facial. Aguardamos sua visita! 😊";

        [HttpGet("/configuracoes")]
        public async Task<IActionResult> Index()
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

            var oticaId = ObterOticaId();

            var config = await _context.ConfiguracoesLoja
                .FirstOrDefaultAsync(c => c.OticaId == oticaId);

            if (config == null)
            {
                var otica = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == oticaId);
                string nomeInicial = otica?.Nome ?? "Ótica RETSYS";

                config = new ConfiguracaoLoja
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
                    NomeLoja = nomeInicial,
                    Cnpj = "",
                    PixApiKey = "",
                    WhatsappNumero = "",
                    WhatsappMsgCadastroTemplate = TEMPLATE_CADASTRO_PADRAO,
                    WhatsappMsgProntoTemplate = TEMPLATE_PRONTO_PADRAO
                };

                _context.ConfiguracoesLoja.Add(config);
                await _context.SaveChangesAsync();
            }

            return Inertia.Render("Configuracoes/Index", new
            {
                NomeLoja = config.NomeLoja,
                Cnpj = config.Cnpj ?? "",
                PixApiKey = config.PixApiKey ?? "",
                WhatsappNumero = config.WhatsappNumero ?? "",
                WhatsappMsgCadastroTemplate = string.IsNullOrWhiteSpace(config.WhatsappMsgCadastroTemplate)
                    ? TEMPLATE_CADASTRO_PADRAO
                    : config.WhatsappMsgCadastroTemplate,
                WhatsappMsgProntoTemplate = string.IsNullOrWhiteSpace(config.WhatsappMsgProntoTemplate)
                    ? TEMPLATE_PRONTO_PADRAO
                    : config.WhatsappMsgProntoTemplate,
                TemplatePadraoCadastro = TEMPLATE_CADASTRO_PADRAO,
                TemplatePadraoPronto = TEMPLATE_PRONTO_PADRAO
            });
        }

        [HttpPost("/configuracoes")]
        public async Task<IActionResult> Salvar([FromBody] DtoConfigSalvar dados)
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

            var oticaId = ObterOticaId();

            var config = await _context.ConfiguracoesLoja
                .FirstOrDefaultAsync(c => c.OticaId == oticaId);

            if (config == null)
            {
                config = new ConfiguracaoLoja
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
                    NomeLoja = !string.IsNullOrWhiteSpace(dados.NomeLoja) ? dados.NomeLoja.Trim() : "Ótica RETSYS",
                    Cnpj = dados.Cnpj?.Trim() ?? "",
                    PixApiKey = dados.PixApiKey?.Trim() ?? "",
                    WhatsappNumero = dados.WhatsappNumero?.Trim() ?? "",
                    WhatsappMsgCadastroTemplate = dados.WhatsappMsgCadastroTemplate?.Trim(),
                    WhatsappMsgProntoTemplate = dados.WhatsappMsgProntoTemplate?.Trim()
                };
                _context.ConfiguracoesLoja.Add(config);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dados.NomeLoja)) config.NomeLoja = dados.NomeLoja.Trim();
                config.Cnpj = dados.Cnpj?.Trim() ?? "";
                config.PixApiKey = dados.PixApiKey?.Trim() ?? "";
                config.WhatsappNumero = dados.WhatsappNumero?.Trim() ?? "";
                config.WhatsappMsgCadastroTemplate = dados.WhatsappMsgCadastroTemplate?.Trim();
                config.WhatsappMsgProntoTemplate = dados.WhatsappMsgProntoTemplate?.Trim();
                _context.ConfiguracoesLoja.Update(config);
            }

            await _context.SaveChangesAsync();

            Inertia.Share("PixHabilitadoNestaLoja", !string.IsNullOrEmpty(config.PixApiKey));

            return RedirectToAction(nameof(Index));
        }
    }

    public record DtoConfigSalvar(
        string NomeLoja, 
        string Cnpj, 
        string? PixApiKey, 
        string? WhatsappNumero, 
        string? WhatsappMsgCadastroTemplate, 
        string? WhatsappMsgProntoTemplate);
}