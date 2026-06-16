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
                .Where(os => os.Status == "No Laboratório")
                .OrderBy(os => os.DataVenda)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.TipoLente,
                    ClienteNome = os.Cliente.Nome,
                    // Dados clínicos brutos cruciais para a fabricação dos óculos
                    Especificacoes = new
                    {
                        os.EsfericoLongeDireito,
                        os.EsfericoLongeEsquerdo,
                        os.CilindricoLongeDireito,
                        os.CilindricoLongeEsquerdo,
                        os.EixoLongeDireito,
                        os.EixoLongeEsquerdo,
                        os.EsfericoPertoDireito,
                        os.EsfericoPertoEsquerdo,
                        os.CilindricoPertoDireito,
                        os.CilindricoPertoEsquerdo,
                        os.EixoPertoDireito,
                        os.EixoPertoEsquerdo,
                        os.Adicao
                    }
                })
                .ToListAsync();

            return Inertia.Render("Laboratory/Index", new { FilaMontagem = ordensParaMontagem });
        }
    }
}