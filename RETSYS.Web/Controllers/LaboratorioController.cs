using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class LaboratorioController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public LaboratorioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /laboratorio
        [HttpGet("/laboratorio")]
        public async Task<IActionResult> Index()
        {
            var oticaId = ObterOticaId();

            var ordens = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.LentePreco)
                        .ThenInclude(lp => lp!.Lente)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.Armacao)
                .Where(os => os.OticaId == oticaId && os.Ativo && os.Status != "CANCELADO" && os.Status != "ENTREGUE")
                .OrderBy(os => os.DataPrevistaEntrega)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.Status,
                    EtapaLaboratorio = string.IsNullOrWhiteSpace(os.EtapaLaboratorio) ? "BALCAO" : os.EtapaLaboratorio,
                    os.LaboratorioParceiro,
                    os.NumeroPedidoLaboratorio,
                    DataEntrada = os.DataEntrada.ToString("dd/MM/yyyy"),
                    DataPrevistaEntrega = os.DataPrevistaEntrega.ToString("dd/MM/yyyy"),
                    os.LentePedida,
                    os.LenteChegou,
                    os.ConferidoLensometro,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",
                    ClienteTelefone = os.Cliente != null ? os.Cliente.Telefone : "",
                    ArmacaoNome = os.Financeiro != null && os.Financeiro.Armacao != null 
                        ? $"{os.Financeiro.Armacao.ModeloReferencia} ({os.Financeiro.Armacao.Cor})"
                        : (os.ArmacaoModeloManual ?? "Armação do Cliente"),
                    TipoLente = os.Financeiro != null && os.Financeiro.LentePreco != null && os.Financeiro.LentePreco.Lente != null
                        ? $"{os.Financeiro.LentePreco.Lente.Tipo} {os.Financeiro.LentePreco.Tratamento}"
                        : (os.LenteDescricaoManual ?? "Lente não especificada"),
                    Especificacoes = new
                    {
                        EsfericoLongeDireito = os.Receita != null ? os.Receita.OdEsferico : 0m,
                        EsfericoLongeEsquerdo = os.Receita != null ? os.Receita.OeEsferico : 0m,
                        CilindricoLongeDireito = os.Receita != null ? os.Receita.OdCilindrico : 0m,
                        CilindricoLongeEsquerdo = os.Receita != null ? os.Receita.OeCilindrico : 0m,
                        EixoLongeDireito = os.Receita != null ? os.Receita.OdEixo : 0,
                        EixoLongeEsquerdo = os.Receita != null ? os.Receita.OeEixo : 0,
                        DnpOd = os.Receita != null ? os.Receita.DnpOd : 0m,
                        DnpOe = os.Receita != null ? os.Receita.DnpOe : 0m,
                        AlturaOd = os.Receita != null ? os.Receita.AlturaMontagemOd : 0m,
                        AlturaOe = os.Receita != null ? os.Receita.AlturaMontagemOe : 0m,
                        Adicao = os.Receita != null ? os.Receita.Adicao : null,
                        EsfericoPertoDireito = os.Receita != null ? os.Receita.OdEsfericoPerto : 0m,
                        EsfericoPertoEsquerdo = os.Receita != null ? os.Receita.OeEsfericoPerto : 0m
                    }
                })
                .ToListAsync();

            return Inertia.Render("Laboratory/Index", new { Ordens = ordens });
        }

        // POST /laboratorio/{id}/avancar-etapa
        [HttpPost("/laboratorio/{id}/avancar-etapa")]
        public async Task<IActionResult> AvancarEtapa(Guid id, [FromBody] AvancarEtapaDto dto)
        {
            var oticaId = ObterOticaId();

            var os = await _context.OrdensServico.FirstOrDefaultAsync(o => o.Id == id && o.OticaId == oticaId);
            if (os == null)
                return NotFound();

            os.EtapaLaboratorio = dto.NovaEtapa;

            if (dto.NovaEtapa == "PRONTO_RETIRADA" || dto.NovaEtapa == "PRONTA")
            {
                os.Status = "PRONTA";
            }
            else if (dto.NovaEtapa == "EM_MONTAGEM")
            {
                os.Status = "EM_MONTAGEM";
            }
            else if (dto.NovaEtapa == "LABORATORIO_EXTERNO")
            {
                os.Status = "AGUARDANDO_LENTE";
                if (!string.IsNullOrWhiteSpace(dto.LaboratorioParceiro))
                    os.LaboratorioParceiro = dto.LaboratorioParceiro;
                if (!string.IsNullOrWhiteSpace(dto.NumeroPedidoLab))
                    os.NumeroPedidoLaboratorio = dto.NumeroPedidoLab;
                os.DataEnvioLaboratorio = DateTime.UtcNow;
            }

            if (dto.ConferidoLensometro.HasValue && dto.ConferidoLensometro.Value)
            {
                os.ConferidoLensometro = true;
                os.ConferidoLensometroEm = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (Request.Headers.ContainsKey("X-Inertia"))
            {
                return RedirectToAction(nameof(Index));
            }

            return Ok(new { Sucesso = true, Status = os.Status, Etapa = os.EtapaLaboratorio });
        }
    }

    public class AvancarEtapaDto
    {
        public string NovaEtapa { get; set; } = string.Empty;
        public string? LaboratorioParceiro { get; set; }
        public string? NumeroPedidoLab { get; set; }
        public bool? ConferidoLensometro { get; set; }
    }
}
