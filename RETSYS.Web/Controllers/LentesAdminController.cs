using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RETSYS.Web.Controllers
{
    [Authorize] // Garante segurança: apenas utilizadores autenticados entram
    public class LentesAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LentesAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Renderiza a Tela Principal de Lentes (GET /lentes)
        [HttpGet("/lentes")]
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            bool isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                           role.Equals("Gerente", StringComparison.OrdinalIgnoreCase);

            // Carrega os blocos de lentes homologados ativos no inventário
            var lentes = await _context.Lentes
                .Where(l => l.Ativo)
                .OrderBy(l => l.Laboratorio)
                .ToListAsync();

            // Carrega toda a tabela/matriz de precificação parametrizada
            var precos = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Include(lp => lp.Tratamento)
                .Where(lp => lp.Ativo)
                .ToListAsync();

            // Carrega os tratamentos antirreflexo/fotossensíveis cadastrados
            var tratamentos = await _context.LentesTratamentos
                .Where(t => t.Ativo)
                .OrderBy(t => t.Nome)
                .ToListAsync();

            // Passa os dados estruturados como PROPS diretamente para o Vue 3 no Canvas
            return Inertia.Render("Lentes/Index", new
            {
                Lentes = lentes,
                Precos = precos,
                Tratamentos = tratamentos,
                IsAdmin = isAdmin
            });
        }

        // 2. Grava uma nova precificação na Matriz de Preços (POST /lentes/precos)
        [HttpPost("/lentes/precos")]
        public async Task<IActionResult> CadastrarPreco([FromBody] LentePreco preco)
        {
            // Bloqueio de alçada caso um vendedor tente forçar uma requisição HTTP direta
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !role.Equals("Gerente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                preco.Id = Guid.NewGuid();
                preco.Ativo = true;
                _context.LentesTabelaPrecos.Add(preco);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Cadastra um novo antirreflexo ou proteção fotossensível (POST /lentes/tratamentos)
        [HttpPost("/lentes/tratamentos")]
        public async Task<IActionResult> CadastrarTratamento([FromBody] LenteTratamento tratamento)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                tratamento.Id = Guid.NewGuid();
                tratamento.Ativo = true;
                _context.LentesTratamentos.Add(tratamento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 4. Remove um preço cadastrado na matriz (DELETE /lentes/precos/{id})
        [HttpDelete("/lentes/precos/{id:guid}")]
        public async Task<IActionResult> RemoverPreco(Guid id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !role.Equals("Gerente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index));
            }

            var preco = await _context.LentesTabelaPrecos.FindAsync(id);
            if (preco != null)
            {
                // Soft delete seguro para manter a integridade referencial histórica de OS passadas
                preco.Ativo = false; 
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. Remove um tratamento opcional (DELETE /lentes/tratamentos/{id})
        [HttpDelete("/lentes/tratamentos/{id:guid}")]
        public async Task<IActionResult> RemoverTratamento(Guid id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index));
            }

            var tratamento = await _context.LentesTratamentos.FindAsync(id);
            if (tratamento != null)
            {
                tratamento.Ativo = false; // Soft delete
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}