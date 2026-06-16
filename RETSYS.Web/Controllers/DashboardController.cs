using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/dashboard")]
        public async Task<IActionResult> Index([FromQuery] int? mes, [FromQuery] int? ano)
        {
            // Define o mês e ano corrente como padrão caso venham nulos
            int mesFiltro = mes ?? DateTime.UtcNow.Month;
            int anoFiltro = ano ?? DateTime.UtcNow.Year;

            // 1. Total faturado no período selecionado
            var totalFaturado = await _context.OrdensServico
                .Where(os => os.DataVenda.Month == mesFiltro && os.DataVenda.Year == anoFiltro)
                .SumAsync(os => os.ValorTotal);

            // 2. Quantidade total de Ordens de Serviço no período
            var totalOS = await _context.OrdensServico
                .Where(os => os.DataVenda.Month == mesFiltro && os.DataVenda.Year == anoFiltro)
                .CountAsync();

            // 3. Ranking de Desempenho dos Vendedores (Métricas solicitadas)
            var rankingVendedores = await _context.OrdensServico
                .Include(os => os.Usuario)
                .Where(os => os.DataVenda.Month == mesFiltro && os.DataVenda.Year == anoFiltro)
                .GroupBy(os => os.Usuario.Nome)
                .Select(g => new
                {
                    VendedorNome = g.Key,
                    TotalVendas = g.Sum(os => os.ValorTotal),
                    QuantidadeOS = g.Count()
                })
                .OrderByDescending(v => v.TotalVendas)
                .ToListAsync();

            // 4. Divisão de faturamento por Filial de Loja
            var faturamentoPorLoja = await _context.OrdensServico
                .Include(os => os.Usuario)
                .Where(os => os.DataVenda.Month == mesFiltro && os.DataVenda.Year == anoFiltro)
                .GroupBy(os => os.Usuario.FilialLoja)
                .Select(g => new
                {
                    Loja = string.IsNullOrEmpty(g.Key) ? "Não Informada" : g.Key,
                    Total = g.Sum(os => os.ValorTotal)
                })
                .ToListAsync();

            // Envia os dados consolidados para a View do Vue 3
            return Inertia.Render("Dashboard/Index", new
            {
                MesFiltro = mesFiltro,
                AnoFiltro = anoFiltro,
                TotalFaturado = totalFaturado,
                TotalOS = totalOS,
                RankingVendedores = rankingVendedores,
                FaturamentoPorLoja = faturamentoPorLoja
            });
        }
    }
}