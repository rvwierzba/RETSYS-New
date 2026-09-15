using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace RETSYS.Web.Controllers
{
    public class ConfiguracoesController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/configuracoes")]
        public async Task<IActionResult> Index()
        {
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
                    PixApiKey = ""
                };

                _context.ConfiguracoesLoja.Add(config);
                await _context.SaveChangesAsync();
            }

            return Inertia.Render("Configuracoes/Index", new
            {
                NomeLoja = config.NomeLoja,
                Cnpj = config.Cnpj ?? "",
                PixApiKey = config.PixApiKey ?? ""
            });
        }

        [HttpPost("/configuracoes")]
        public async Task<IActionResult> Salvar([FromBody] DtoConfigSalvar dados)
        {
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
                    PixApiKey = dados.PixApiKey?.Trim() ?? ""
                };
                _context.ConfiguracoesLoja.Add(config);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dados.NomeLoja)) config.NomeLoja = dados.NomeLoja.Trim();
                config.Cnpj = dados.Cnpj?.Trim() ?? "";
                config.PixApiKey = dados.PixApiKey?.Trim() ?? "";
                _context.ConfiguracoesLoja.Update(config);
            }

            await _context.SaveChangesAsync();

            Inertia.Share("PixHabilitadoNestaLoja", !string.IsNullOrEmpty(config.PixApiKey));

            return RedirectToAction(nameof(Index));
        }
    }

    public record DtoConfigSalvar(string NomeLoja, string Cnpj, string? PixApiKey);
}