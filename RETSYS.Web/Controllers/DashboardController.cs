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
            // CARDS DE RESUMO DO DIA (TOPO)
            // =========================================================================
            
            // OS Emitidas Hoje
            var queryOsHoje = _context.OrdensServico.Where(os => os.DataEntrada.Date == hoje && os.Status != "CANCELADO" && os.Status != "CANCELADA");
            if (!isAdmin) queryOsHoje = queryOsHoje.Where(os => os.VendedorId == vendedorIdFiltro);
            int osHojeCount = await queryOsHoje.CountAsync();

            // Valor Faturado Hoje (soma valor_total_liquido das OS do dia)
            var faturadoHoje = await queryOsHoje
                .Include(os => os.Financeiro)
                .SumAsync(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0);

            // OS prontas aguardando retirada (Status: PRONTO)
            var queryProntas = _context.OrdensServico.Where(os => os.Status == "PRONTO" && os.Ativo);
            if (!isAdmin) queryProntas = queryProntas.Where(os => os.VendedorId == vendedorIdFiltro);
            int osProntasCount = await queryProntas.CountAsync();

            // =========================================================================
            // 🔬 REQUISITO SEÇÃO 3.3: ALERTA DE LENTES NÃO PEDIDAS
            // =========================================================================
            var queryLentesNaoPedidas = _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.Ativo 
                       && os.Status != "CANCELADO" 
                       && os.Status != "CANCELADA"
                       && !os.LentePedida
                       && ((os.Financeiro != null && os.Financeiro.ValorLente > 0) || !string.IsNullOrEmpty(os.LenteDescricaoManual)));

            if (!isAdmin) queryLentesNaoPedidas = queryLentesNaoPedidas.Where(os => os.VendedorId == vendedorIdFiltro);

            int osLentesNaoPedidasCount = await queryLentesNaoPedidas.CountAsync();

            // Destaque crítico: Lentes não pedidas há mais de 1 dia desde a emissão da OS
            DateTime dataLimite1Dia = hoje.AddDays(-1);
            int osLentesNaoPedidasCriticas = await queryLentesNaoPedidas
                .Where(os => os.DataEntrada.Date <= dataLimite1Dia)
                .CountAsync();

            // =========================================================================
            // 🛡️ REQUISITO - SEÇÃO 7.2: DISTINÇÃO ENTRE VENCIDAS X ATRASADAS
            // =========================================================================

            // Entregas Vencidas: Passou do prazo estimado e ainda NÃO foi entregue ao cliente 
            var queryVencidas = _context.OrdensServico
                .Where(os => os.DataPrevistaEntrega.Date < hoje && os.Status != "ENTREGUE" && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo);
            if (!isAdmin) queryVencidas = queryVencidas.Where(os => os.VendedorId == vendedorIdFiltro);
            int osVencidasCount = await queryVencidas.CountAsync();

            // Entregas Atrasadas: Já foi entregue (ENTREGUE), mas a data real superou o prazo 
            var queryAtrasadasReal = _context.OrdensServico
                .Where(os => os.Status == "ENTREGUE" && os.DataEntregaReal.HasValue && os.DataEntregaReal.Value.Date > os.DataPrevistaEntrega.Date && os.Ativo);
            if (!isAdmin) queryAtrasadasReal = queryAtrasadasReal.Where(os => os.VendedorId == vendedorIdFiltro);
            int osAtrasadasRealCount = await queryAtrasadasReal.CountAsync();

            // =========================================================================
            // 💰 REQUISITO - SEÇÃO 1: CARD DINÂMICO "MINHA COMISSÃO"
            // =========================================================================
            string periodoAtual = hoje.ToString("yyyy-MM"); 

            var queryComissaoMes = _context.Comissoes
                .Where(c => c.PeriodoReferencia == periodoAtual 
                         && (c.Status == "PENDENTE" || c.Status == "PAGO")); 

            if (!isAdmin)
            {
                queryComissaoMes = queryComissaoMes.Where(c => c.VendedorId == vendedorIdFiltro);
            }

            decimal minhaComissaoMes = await queryComissaoMes.SumAsync(c => c.ValorComissao);

            // =========================================================================
            // SEÇÃO CENTRAL: GRÁFICO DE 30 DIAS & LISTA DE ÚLTIMAS 5 OS
            // =========================================================================

            var dataLimite30Dias = hoje.AddDays(-30);
            var queryGrafico = _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Date >= dataLimite30Dias && os.DataEntrada.Date <= hoje && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo);
            
            if (!isAdmin) queryGrafico = queryGrafico.Where(os => os.VendedorId == vendedorIdFiltro);

            var faturamentoUltimos30DiasRaw = await queryGrafico
                .GroupBy(os => os.DataEntrada.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Valor = g.Sum(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0)
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

            var queryUltimas5 = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Financeiro)
                .Where(os => os.Ativo)
                .OrderByDescending(os => os.DataEntrada);

            var queryUltimas5Filtrada = isAdmin ? queryUltimas5 : queryUltimas5.Where(os => os.VendedorId == vendedorIdFiltro);

            var ultimas5OS = await queryUltimas5Filtrada
                .Take(5)
                .Select(os => new
                {
                    os.NumeroOS,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado",
                    os.Status,
                    Valor = os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0,
                    DataEntrada = os.DataEntrada.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            // =========================================================================
            // ALERTAS: PRODUTOS BAIXOS & ENTREGAS VENCIDAS
            // =========================================================================

            var armacoesEstoqueBaixo = new List<EstoqueBaixoDto>();
            if (isAdmin)
            {
                armacoesEstoqueBaixo = await _context.Armacoes
                    .Where(a => a.QuantidadeEstoque < 3 && a.Ativo)
                    .Select(a => new EstoqueBaixoDto 
                    { 
                        ModeloReferencia = a.ModeloReferencia, 
                        QuantidadeEstoque = a.QuantidadeEstoque 
                    })
                    .ToListAsync();
            }

            var queryAlertasVencidos = _context.OrdensServico
                .Include(os => os.Cliente)
                .Where(os => os.DataPrevistaEntrega.Date < hoje && os.Status != "ENTREGUE" && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo);

            if (!isAdmin) queryAlertasVencidos = queryAlertasVencidos.Where(os => os.VendedorId == vendedorIdFiltro);

            var osVencidasAlertasRaw = await queryAlertasVencidos
                .Select(os => new { os.NumeroOS, ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Não informado", os.DataPrevistaEntrega })
                .ToListAsync();

            var osVencidasAlertas = osVencidasAlertasRaw
                .Select(os => new { os.NumeroOS, os.ClienteNome, DiasAtraso = (hoje - os.DataPrevistaEntrega.Date).Days })
                .ToList();

            // =========================================================================
            // METRICAS HISTÓRICAS DA BARRA LATERAL
            // =========================================================================
            var queryTotalFaturado = _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo);

            if (!isAdmin) queryTotalFaturado = queryTotalFaturado.Where(os => os.VendedorId == vendedorIdFiltro);

            var totalFaturadoMensal = await queryTotalFaturado.SumAsync(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0);
            var totalOSMensal = await queryTotalFaturado.CountAsync();

            var rankingVendedores = new List<VendedorRankingDto>();
            var faturamentoPorLoja = new List<FaturamentoLojaDto>();

            if (isAdmin)
            {
                rankingVendedores = await _context.OrdensServico
                    .Include(os => os.Vendedor)
                    .Include(os => os.Financeiro)
                    .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo)
                    .GroupBy(os => os.Vendedor != null ? os.Vendedor.Nome : "Sem Vendedor")
                    .Select(g => new VendedorRankingDto { VendedorNome = g.Key, TotalVendas = g.Sum(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0), QuantidadeOS = g.Count() })
                    .OrderByDescending(v => v.TotalVendas)
                    .ToListAsync();

                faturamentoPorLoja = await _context.OrdensServico
                    .Include(os => os.Vendedor)
                    .Include(os => os.Financeiro)
                    .Where(os => os.DataEntrada.Month == mesFiltro && os.DataEntrada.Year == anoFiltro && os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo)
                    .GroupBy(os => os.Vendedor != null ? os.Vendedor.FilialLoja : "Matriz")
                    .Select(g => new FaturamentoLojaDto { Loja = string.IsNullOrEmpty(g.Key) ? "Matriz" : g.Key, Total = g.Sum(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0) })
                    .ToListAsync();
            }

            return Inertia.Render("Dashboard/Index", new
            {
                PerfilUsuario = usuarioLogado.Perfil.ToString(),
                IsAdmin = isAdmin,
                
                ResumoHoje = new {
                    OsHoje = osHojeCount,
                    FaturadoHoje = faturadoHoje,
                    OsProntas = osProntasCount,
                    OsVencidas = osVencidasCount,
                    OsAtrasadas = osAtrasadasRealCount,
                    LentesNaoPedidas = osLentesNaoPedidasCount,               // Seção 3.3: Total de lentes não pedidas
                    LentesNaoPedidasCriticas = osLentesNaoPedidasCriticas     // Seção 3.3: Lentes não pedidas > 1 dia
                },

                MinhaComissaoMes = minhaComissaoMes,

                FaturamentoGrafico = faturamentoUltimos30Dias,
                UltimasOS = ultimas5OS,
                AlertasEstoque = armacoesEstoqueBaixo,
                AlertasEntregasVencidas = osVencidasAlertas,

                MesFiltro = mesFiltro,
                AnoFiltro = anoFiltro,
                TotalFaturadoMensal = totalFaturadoMensal,
                TotalOSMensal = totalOSMensal,
                RankingVendedores = rankingVendedores,
                FaturamentoPorLoja = faturamentoPorLoja
            });
        }
    }

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