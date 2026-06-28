using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System;
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
            int mesFiltro = mes ?? DateTime.UtcNow.Month;
            int anoFiltro = ano ?? DateTime.UtcNow.Year;

            // 1. Total faturado vindo da tabela os_financeiro
            var totalFaturado = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                .SumAsync(os => os.Financeiro.ValorTotalLiquido);

            // 2. Quantidade total de Ordens de Serviço
            var totalOS = await _context.OrdensServico
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                .CountAsync();

            // 3. Ranking de Desempenho dos Vendedores
            var rankingVendedores = await _context.OrdensServico
                .Include(os => os.Vendedor)
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                .GroupBy(os => os.Vendedor.Nome)
                .Select(g => new
                {
                    VendedorNome = g.Key,
                    TotalVendas = g.Sum(os => os.Financeiro.ValorTotalLiquido),
                    QuantidadeOS = g.Count()
                })
                .OrderByDescending(v => v.TotalVendas)
                .ToListAsync();

            // 4. Divisão de faturamento por Filial de Loja
            var faturamentoPorLoja = await _context.OrdensServico
                .Include(os => os.Vendedor)
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                .GroupBy(os => os.Vendedor.FilialLoja)
                .Select(g => new
                {
                    Loja = string.IsNullOrEmpty(g.Key) ? "Não Informada" : g.Key,
                    Total = g.Sum(os => os.Financeiro.ValorTotalLiquido)
                })
                .ToListAsync();

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