using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System.Linq;
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

        // 1. Painel de Ordens na Esteira de Montagem (GET)
        [HttpGet("/laboratorio")]
        public async Task<IActionResult> Index()
        {
            var oticaId = ObterOticaId();

            var ordensParaMontagem = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.LentePreco)
                        .ThenInclude(lp => lp!.Lente)
                .Where(os => os.OticaId == oticaId && os.Status == "EM_LABORATORIO")
                .OrderBy(os => os.DataEntrada)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    TipoLente = os.Financeiro != null && os.Financeiro.LentePreco != null && os.Financeiro.LentePreco.Lente != null
                        ? os.Financeiro.LentePreco.Lente.Tipo
                        : "Não informado",
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",

                    Especificacoes = new
                    {
                        EsfericoLongeDireito = os.Receita != null ? os.Receita.OdEsferico : (decimal?)null,
                        EsfericoLongeEsquerdo = os.Receita != null ? os.Receita.OeEsferico : (decimal?)null,
                        CilindricoLongeDireito = os.Receita != null ? os.Receita.OdCilindrico : (decimal?)null,
                        CilindricoLongeEsquerdo = os.Receita != null ? os.Receita.OeCilindrico : (decimal?)null,
                        EixoLongeDireito = os.Receita != null ? os.Receita.OdEixo : (int?)null,
                        EixoLongeEsquerdo = os.Receita != null ? os.Receita.OeEixo : (int?)null,

                        EsfericoPertoDireito = os.Receita != null ? os.Receita.OdEsfericoPerto : (decimal?)null,
                        EsfericoPertoEsquerdo = os.Receita != null ? os.Receita.OeEsfericoPerto : (decimal?)null,
                        CilindricoPertoDireito = os.Receita != null ? os.Receita.OdCilindricoPerto : (decimal?)null,
                        CilindricoPertoEsquerdo = os.Receita != null ? os.Receita.OeCilindricoPerto : (decimal?)null,
                        EixoPertoDireito = os.Receita != null ? os.Receita.OdEixoPerto : (int?)null,
                        EixoPertoEsquerdo = os.Receita != null ? os.Receita.OeEixoPerto : (int?)null,

                        Adicao = os.Receita != null ? os.Receita.Adicao : (decimal?)null
                    }
                })
                .ToListAsync();

            return Inertia.Render("Laboratory/Index", new { FilaMontagem = ordensParaMontagem });
        }
    }
}
