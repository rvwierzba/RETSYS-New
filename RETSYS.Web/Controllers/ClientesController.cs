using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem e Busca Avançada (GET)
        [HttpGet("/clientes")]
        public async Task<IActionResult> Index([FromQuery] string? busca)
        {
            // Cria a query base apontando para a tabela de clientes
            var query = _context.Clientes.AsQueryable();

            // Se o usuário digitou algo no campo de busca, filtra por Nome ou CPF
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(termo) || c.CPF.Contains(termo));
            }

            var listaClientes = await query
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.CPF,
                    c.Telefone // Corrigido: Celular -> Telefone conforme o novo esquema do banco
                })
                .ToListAsync();

            // Renderiza Frontend/Pages/Clientes/Index.vue repassando a lista e o termo buscado
            return Inertia.Render("Clientes/Index", new { 
                Clientes = listaClientes,
                FiltroBusca = busca 
            });
        }

        // 2. Gravação de Novo Cliente (POST)
        [HttpPost("/clientes")]
        public async Task<IActionResult> Store([FromBody] Cliente novoCliente)
        {
            // Validações básicas de balcão
            if (string.IsNullOrWhiteSpace(novoCliente.Nome) || string.IsNullOrWhiteSpace(novoCliente.CPF))
            {
                return RedirectToAction(nameof(Index));
            }

            // Garante que o ID seja gerado como Guid padrão caso venha vazio
            if (novoCliente.Id == Guid.Empty)
            {
                novoCliente.Id = Guid.NewGuid();
            }

            _context.Clientes.Add(novoCliente);
            await _context.SaveChangesAsync();

            // Redireciona para o Index atualizando a lista do SPA na hora
            return RedirectToAction(nameof(Index));
        }

        // 3. Prontuário Ótico / Linha do Tempo Clínica (GET)
        [HttpGet("/clientes/{id:guid}/historico")]
        public async Task<IActionResult> Historico(Guid id)
        {
            // Busca o cliente respeitando a nova propriedade estrutural 'Telefone'
            var cliente = await _context.Clientes
                .Select(c => new { c.Id, c.Nome, c.CPF, c.Telefone, c.Convenio, c.Email, c.Observacoes })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Busca todas as OSs vinculadas a este paciente com o carregamento das tabelas 1:1
            var historicoOS = await _context.OrdensServico
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id)
                .OrderByDescending(os => os.DataEntrada) // Corrigido: DataVenda -> DataEntrada
                .Select(os => new
                {
                    os.NumeroOS,
                    DataVenda = os.DataEntrada, // Mantido o apelido para o front-end consumir sem quebras
                    Medico = os.MedicoNome,      // Corrigido: Medico -> MedicoNome
                    ValorTotal = os.Financeiro.ValorTotalLiquido, // Corrigido: lendo da tabela satélite comercial
                    os.Status,
                    Graus = new
                    {
                        EsfericoLongeDireito = os.Receita.OdEsferico,
                        CilindricoLongeDireito = os.Receita.OdCilindrico,
                        EixoLongeDireito = os.Receita.OdEixo,
                        EsfericoLongeEsquerdo = os.Receita.OeEsferico,
                        CilindricoLongeEsquerdo = os.Receita.OeCilindrico,
                        EixoLongeEsquerdo = os.Receita.OeEixo,
                        os.Receita.Adicao,
                        // Propriedades inteligentes calculadas em tempo real na memória da Entidade OsReceita
                        EsfericoPertoDireito = os.Receita.OdEsfericoPerto,
                        EsfericoPertoEsquerdo = os.Receita.OeEsfericoPerto,
                        CilindricoPertoDireito = os.Receita.OdCilindricoPerto,
                        CilindricoPertoEsquerdo = os.Receita.OeCilindricoPerto,
                        EixoPertoDireito = os.Receita.OdEixoPerto,
                        EixoPertoEsquerdo = os.Receita.OeEixoPerto
                    }
                })
                .ToListAsync();

            // 🔥 Inteligência de CRM: Total gasto calculado dinamicamente com base nas ordens já finalizadas e entregues
            decimal totalGastoHistorico = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .Where(os => os.ClienteId == id && os.Status == "ENTREGUE")
                .SumAsync(os => os.Financeiro.ValorTotalLiquido);

            return Inertia.Render("Clientes/Historico", new
            {
                Cliente = cliente,
                Historico = historicoOS,
                TotalGasto = totalGastoHistorico // Enviado para preencher o indicador visual na tela
            });
        }
    }
}