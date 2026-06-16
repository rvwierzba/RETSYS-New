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
                    c.Celular
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
            // Busca o cliente respeitando a propriedade real 'Celular' do seu banco
            var cliente = await _context.Clientes
                .Select(c => new { c.Id, c.Nome, c.CPF, c.Celular })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Busca todas as OSs vinculadas a este paciente para montar a evolução de grau
            var historicoOS = await _context.OrdensServico
                .Where(os => os.ClienteId == id)
                .OrderByDescending(os => os.DataVenda)
                .Select(os => new
                {
                    os.NumeroOS,
                    os.DataVenda,
                    os.Medico,
                    os.TipoLente,
                    os.ValorTotal,
                    os.Status,
                    Graus = new
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

            return Inertia.Render("Clientes/Historico", new
            {
                Cliente = cliente,
                Historico = historicoOS
            });
        }
    }
}