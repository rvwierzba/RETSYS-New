using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class OrdensServicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdensServicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de todas as OSs Clínicas (GET)
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index()
        {
            var ordens = await _context.OrdensServico
                .Include(os => os.Cliente)
                .OrderByDescending(os => os.DataVenda)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataVenda,
                    os.Medico,
                    os.TipoLente,
                    os.ValorTotal,
                    os.Status, // Sincronizado para alimentar os badges coloridos na listagem do Vue
                    ClienteNome = os.Cliente.Nome,
                    // Enviamos os dados de refração compactados para o Vue poder exibir num modal de detalhes se precisar
                    Refracao = new
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

            return Inertia.Render("Orders/Index", new { Ordens = ordens });
        }

        // 2. Abre a tela de cadastro de uma nova OS (GET)
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            // Busca dados simples dos clientes e vendedores ativos para alimentar os seletores no Vue
            var clientes = await _context.Clientes
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome, c.CPF })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            return Inertia.Render("Orders/Create", new { 
                Clientes = clientes, 
                Vendedores = vendedores 
            });
        }

        // 3. Processa a gravação da OS e gera o parcelamento (POST)
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] OrdemServico novaOS, [FromQuery] int quantidadeParcelas)
        {
            // Executa a nossa Regra de Negócio Ótica automatizada no Domínio
            novaOS.CalcularGrauDePerto();

            // Sorteia ou gera um número incremental de OS para o MVP
            novaOS.NumeroOS = "OS-" + DateTime.UtcNow.Ticks.ToString().Substring(10);
            novaOS.DataVenda = DateTime.UtcNow;
            novaOS.Status = "Aberta"; // Define o ciclo de vida inicial padrão da ordem

            // Gera as parcelas de pagamento automaticamente no servidor baseado no Valor Total
            int parcelas = quantidadeParcelas < 1 ? 1 : quantidadeParcelas;
            decimal valorParcela = Math.Round(novaOS.ValorTotal / parcelas, 2);

            for (int i = 1; i <= parcelas; i++)
            {
                novaOS.Parcelas.Add(new ParcelaPagamento
                {
                    Id = Guid.NewGuid(),
                    NumeroParcela = i,
                    DescricaoParcela = $"PARC. {i}/{parcelas} - REF a OS: {novaOS.NumeroOS}",
                    Valor = i == parcelas ? (novaOS.ValorTotal - (valorParcela * (parcelas - 1))) : valorParcela, // Ajusta diferença de centavos na última
                    DataVencimento = DateTime.UtcNow.AddMonths(i)
                });
            }

            _context.OrdensServico.Add(novaOS);
            await _context.SaveChangesAsync();

            // Redireciona o usuário para o painel gerencial após o sucesso
            return RedirectToRoute(new { controller = "Dashboard", action = "Index" });
        }    

        // 4. Modifica o ciclo de vida / status de produção da OS (POST)
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] string novoStatus)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null)
            {
                return NotFound();
            }

            // Lista de status homologados para o ecossistema da ótica
            var statusValidos = new[] { "Aberta", "No Laboratório", "Pronto", "Entregue" };
            if (statusValidos.Contains(novoStatus))
            {
                ordem.Status = novoStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}