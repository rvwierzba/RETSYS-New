using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            // 1. Identificação do Utilizador Logado e do seu Perfil de Acesso
            var emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            var usuarioLogado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == emailUsuario && u.Ativo);

            if (usuarioLogado == null)
            {
                return Redirect("/login");
            }

            // Definição de permissões com base no Perfil (Admin vs Vendedor)
            bool isAdmin = usuarioLogado.Perfil.ToString() == "Admin" || usuarioLogado.Perfil.ToString() == "Gerente";
            Guid? vendedorIdFiltro = isAdmin ? null : usuarioLogado.Id;

            // Filtros de mês/ano para os gráficos e rankings históricos
            int mesFiltro = mes ?? DateTime.UtcNow.Month;
            int anoFiltro = ano ?? DateTime.UtcNow.Year;
            DateTime hoje = DateTime.UtcNow.Date;

            // =========================================================================
            // CARDS DE RESUMO DO DIA (EXIGÊNCIA 01/07 - TOPO)
            // =========================================================================
            
            // OS Emitidas Hoje
            var queryOsHoje = _context.OrdensServico.Where(os => os.DataEntrada.Date == hoje);
            if (!isAdmin) queryOsHoje = queryOsHoje.Where(os => os.VendedorId == vendedorIdFiltro);
            int osHojeCount = await queryOsHoje.CountAsync();

            // Valor Faturado Hoje (soma valor_total_liquido das OS do dia)
            var faturadoHoje = await queryOsHoje
                .Include(os => os.Financeiro)
                .SumAsync(os => os.Financeiro.ValorTotalLiquido);

            // OS prontas aguardando retirada (Status: PRONTO)
            var queryProntas = _context.OrdensServico.Where(os => os.Status == "PRONTO");
            if (!isAdmin) queryProntas = queryProntas.Where(os => os.VendedorId == vendedorIdFiltro);
            int osProntasCount = await queryProntas.CountAsync();

            // OS com entrega atrasada (Badge Vermelho: Data prevista menor que hoje e não entregue/cancelada)
            var queryAtrasadas = _context.OrdensServico
                .Where(os => os.DataPrevistaEntrega.Date < hoje && os.Status != "ENTREGUE" && os.Status != "CANCELADA");
            if (!isAdmin) queryAtrasadas = queryAtrasadas.Where(os => os.VendedorId == vendedorIdFiltro);
            int osAtrasadasCount = await queryAtrasadas.CountAsync();

            // =========================================================================
            // SEÇÃO CENTRAL: GRÁFICO DE 30 DIAS & LISTA DE ÚLTIMAS 5 OS (EXIGÊNCIA 01/07)
            // =========================================================================

            // Gráfico de faturamento dos últimos 30 dias (linha)
            var dataLimite30Dias = hoje.AddDays(-30);
            var queryGrafico = _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Date >= dataLimite30Dias && os.DataEntrada.Date <= hoje);
            
            if (!isAdmin) queryGrafico = queryGrafico.Where(os => os.VendedorId == vendedorIdFiltro);

            // Coleta a data como DateTime e formata em memória para garantir portabilidade entre bancos de dados
            var faturamentoUltimos30DiasRaw = await queryGrafico
                .GroupBy(os => os.DataEntrada.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Valor = g.Sum(os => os.Financeiro.ValorTotalLiquido)
                })
                .OrderBy(g => g.Data)
                .ToListAsync();

            var faturamentoUltimos30Dias = faturamentoUltimos30DiasRaw
                .Select(g => new
                {
                    Data = g.Data.ToString("yyyy-MM-dd"),
                    g.Valor
                })
                .ToList();

            // Lista das últimas 5 OS emitidas com status em tempo real
            var queryUltimas5 = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Financeiro)
                .OrderByDescending(os => os.DataEntrada);

            var queryUltimas5Filtrada = isAdmin ? queryUltimas5 : queryUltimas5.Where(os => os.VendedorId == vendedorIdFiltro);

            var ultimas5OS = await queryUltimas5Filtrada
                .Take(5)
                .Select(os => new
                {
                    os.NumeroOS,
                    ClienteNome = os.Cliente.Nome,
                    os.Status,
                    Valor = os.Financeiro.ValorTotalLiquido,
                    DataEntrada = os.DataEntrada.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            // =========================================================================
            // ALERTAS: PRODUTOS BAIXOS & ENTREGAS VENCIDAS (EXIGÊNCIA 01/07)
            // =========================================================================

            // Alerta 1: Produtos com estoque abaixo do mínimo (Estoque < 3 un - Apenas Admin vê)
            var produtosEstoqueBaixo = new List<EstoqueBaixoDto>();
            if (isAdmin)
            {
                produtosEstoqueBaixo = await _context.Armacoes
                    .Where(a => a.QuantidadeEstoque < 3)
                    .Select(a => new EstoqueBaixoDto 
                    { 
                        ModeloReferencia = a.ModeloReferencia, 
                        QuantidadeEstoque = a.QuantidadeEstoque 
                    })
                    .ToListAsync();
            }

            // Alerta 2: OS com data prevista de entrega vencida (Processado em memória para evitar erros de TimeSpan no EF)
            var queryAlertasVencidos = _context.OrdensServico
                .Include(os => os.Cliente)
                .Where(os => os.DataPrevistaEntrega.Date < hoje && os.Status != "ENTREGUE" && os.Status != "CANCELADA");

            if (!isAdmin) queryAlertasVencidos = queryAlertasVencidos.Where(os => os.VendedorId == vendedorIdFiltro);

            var osVencidasAlertasRaw = await queryAlertasVencidos
                .Select(os => new
                {
                    os.NumeroOS,
                    ClienteNome = os.Cliente.Nome,
                    os.DataPrevistaEntrega
                })
                .ToListAsync();

            var osVencidasAlertas = osVencidasAlertasRaw
                .Select(os => new
                {
                    os.NumeroOS,
                    os.ClienteNome,
                    DiasAtraso = (hoje - os.DataPrevistaEntrega.Date).Days
                })
                .ToList();

            // =========================================================================
            // METRICAS HISTÓRICAS DA BARRA LATERAL / ABAS FILTRADAS
            // =========================================================================
            var queryTotalFaturado = _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro);

            if (!isAdmin) queryTotalFaturado = queryTotalFaturado.Where(os => os.VendedorId == vendedorIdFiltro);

            var totalFaturadoMensal = await queryTotalFaturado.SumAsync(os => os.Financeiro.ValorTotalLiquido);
            var totalOSMensal = await queryTotalFaturado.CountAsync();

            // Ranking de vendedores e faturamento de lojas (Mapeado usando DTOs seguros no C#)
            var rankingVendedores = new List<VendedorRankingDto>();
            var faturamentoPorLoja = new List<FaturamentoLojaDto>();

            if (isAdmin)
            {
                rankingVendedores = await _context.OrdensServico
                    .Include(os => os.Vendedor)
                    .Include(os => os.Financeiro)
                    .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                    .GroupBy(os => os.Vendedor.Nome)
                    .Select(g => new VendedorRankingDto
                    {
                        VendedorNome = g.Key,
                        TotalVendas = g.Sum(os => os.Financeiro.ValorTotalLiquido),
                        QuantidadeOS = g.Count()
                    })
                    .OrderByDescending(v => v.TotalVendas)
                    .ToListAsync();

                faturamentoPorLoja = await _context.OrdensServico
                    .Include(os => os.Vendedor)
                    .Include(os => os.Financeiro)
                    .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro)
                    .GroupBy(os => os.Vendedor.FilialLoja)
                    .Select(g => new FaturamentoLojaDto
                    {
                        Loja = string.IsNullOrEmpty(g.Key) ? "Não Informada" : g.Key,
                        Total = g.Sum(os => os.Financeiro.ValorTotalLiquido)
                    })
                    .ToListAsync();
            }

            // Retorna o payload estruturado para renderização no Vue 3 via Inertia
            return Inertia.Render("Dashboard/Index", new
            {
                PerfilUsuario = usuarioLogado.Perfil.ToString(),
                IsAdmin = isAdmin,
                
                // KPIs do Topo (Diários)
                ResumoHoje = new {
                    OsHoje = osHojeCount,
                    FaturadoHoje = faturadoHoje,
                    OsProntas = osProntasCount,
                    OsAtrasadas = osAtrasadasCount
                },

                // Dados Centrais
                FaturamentoGrafico = faturamentoUltimos30Dias,
                UltimasOS = ultimas5OS,

                // Alertas
                AlertasEstoque = produtosEstoqueBaixo,
                AlertasEntregasVencidas = osVencidasAlertas,

                // Filtros Mensais de Apoio
                MesFiltro = mesFiltro,
                AnoFiltro = anoFiltro,
                TotalFaturadoMensal = totalFaturadoMensal,
                TotalOSMensal = totalOSMensal,
                RankingVendedores = rankingVendedores,
                FaturamentoPorLoja = faturamentoPorLoja
            });
        }
    }

    // =========================================================================
    // DTOs AUXILIARES PARA PREVENÇÃO DE EXPRESSÕES DINÂMICAS LINQ (CS1963)
    // =========================================================================

    public class VendedorRankingDto
    {
        public string VendedorNome { get; set; } = string.Empty;
        public decimal TotalVendas { get; set; }
        public int QuantidadeOS { get; set; }
    }

    public class FaturamentoLojaDto
    {
        public string Loja { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class EstoqueBaixoDto
    {
        public string ModeloReferencia { get; set; } = string.Empty;
        public int QuantidadeEstoque { get; set; }
    }
}