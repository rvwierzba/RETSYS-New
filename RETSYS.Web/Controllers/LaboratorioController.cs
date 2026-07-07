using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class LaboratorioController : Controller
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
            var ordensParaMontagem = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f.LentePreco) // Ajustado: Lente -> LentePreco
                        .ThenInclude(lp => lp.Lente) // Um nível abaixo para chegar ao Tipo da lente base
                .Where(os => os.Status == "EM_LABORATORIO")
                .OrderBy(os => os.DataEntrada)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    TipoLente = os.Financeiro.LentePreco.Lente.Tipo, // Caminho corrigido
                    ClienteNome = os.Cliente.Nome,
                    
                    Especificacoes = new
                    {
                        EsfericoLongeDireito = os.Receita.OdEsferico,
                        EsfericoLongeEsquerdo = os.Receita.OeEsferico,
                        CilindricoLongeDireito = os.Receita.OdCilindrico,
                        CilindricoLongeEsquerdo = os.Receita.OeCilindrico,
                        EixoLongeDireito = os.Receita.OdEixo,
                        EixoLongeEsquerdo = os.Receita.OeEixo,
                        
                        EsfericoPertoDireito = os.Receita.OdEsfericoPerto,
                        EsfericoPertoEsquerdo = os.Receita.OeEsfericoPerto,
                        CilindricoPertoDireito = os.Receita.OdCilindricoPerto,
                        CilindricoPertoEsquerdo = os.Receita.OeCilindricoPerto,
                        EixoPertoDireito = os.Receita.OdEixoPerto,
                        EixoPertoEsquerdo = os.Receita.OeEixoPerto,
                        
                        os.Receita.Adicao
                    }
                })
                .ToListAsync();

            return Inertia.Render("Laboratory/Index", new { FilaMontagem = ordensParaMontagem });
        }
    }
}
