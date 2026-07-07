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
            // ✅ REMOVIDO: .Include(lp => lp.Tratamento) — agora Tratamento é string, não navegação
            var precos = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo)
                .ToListAsync();

            // ✅ NOVO: Sugestões de tratamento para autocomplete no front,
            // reaproveitando os valores de texto já cadastrados na própria matriz de preços
            // (substitui a antiga tabela lentes_tratamentos)
            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            // Passa os dados estruturados como PROPS diretamente para o Vue 3 no Canvas
            return Inertia.Render("Lentes/Index", new
            {
                Lentes = lentes,
                Precos = precos,
                TratamentosSugeridos = tratamentosSugeridos, // ✅ NOVO: lista de strings p/ autocomplete
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
      

        // 3. Remove um preço cadastrado na matriz (DELETE /lentes/precos/{id})
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

        // ❌ REMOVIDO POR COMPLETO: método RemoverTratamento(Guid id)
        // Motivo: não existe mais entidade LenteTratamento nem tabela lentes_tratamentos.
    }
}
