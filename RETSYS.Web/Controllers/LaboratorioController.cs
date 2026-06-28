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
                .Include(os => os.Receita) // Carrega a tabela satélite clínica
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f.Lente) // Carrega os dados da lente para capturar o tipo homologado
                .Where(os => os.Status == "EM_LABORATORIO") // Sincronizado com a string padrão de produção
                .OrderBy(os => os.DataEntrada) // Corrigido: DataVenda -> DataEntrada
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    TipoLente = os.Financeiro.Lente.Tipo, // Extraído da relação física de estoque da lente
                    ClienteNome = os.Cliente.Nome,
                    
                    // Dados clínicos cruciais redirecionados para o novo lar: os_receita
                    Especificacoes = new
                    {
                        EsfericoLongeDireito = os.Receita.OdEsferico,
                        EsfericoLongeEsquerdo = os.Receita.OeEsferico,
                        CilindricoLongeDireito = os.Receita.OdCilindrico,
                        CilindricoLongeEsquerdo = os.Receita.OeCilindrico,
                        EixoLongeDireito = os.Receita.OdEixo,
                        EixoLongeEsquerdo = os.Receita.OeEixo,
                        
                        // Lendo as propriedades computadas dinamicamente na memória da classe OsReceita
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