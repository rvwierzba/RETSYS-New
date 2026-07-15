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
    [Authorize]
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

            var lentes = await _context.Lentes
                .Where(l => l.Ativo)
                .OrderBy(l => l.Laboratorio)
                .ToListAsync();

            var precos = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo)
                .ToListAsync();

            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Inertia.Render("Lentes/Index", new
            {
                Lentes = lentes,
                Precos = precos,
                TratamentosSugeridos = tratamentosSugeridos,
                IsAdmin = isAdmin
            });
        }

        // 2. Grava uma nova precificação na Matriz de Preços (POST /lentes/precos)
        [HttpPost("/lentes/precos")]
        public async Task<IActionResult> CadastrarPreco([FromBody] NovoPrecoInput input)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !role.Equals("Gerente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index));
            }

            if (input != null && input.LenteId != Guid.Empty && input.PrecoVenda > 0)
            {
                var novoPreco = new LentePreco
                {
                    Id = Guid.NewGuid(),
                    LenteId = input.LenteId,
                    Tipo = input.Tipo.ToUpper().Trim(),
                    IndiceRefracao = input.IndiceRefracao,
                    Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim(),
                    PrecoCusto = input.PrecoCusto,
                    PrecoVenda = input.PrecoVenda,
                    Ativo = true
                };

                _context.LentesTabelaPrecos.Add(novoPreco);
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
                preco.Ativo = false; 
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class NovoPrecoInput
    {
        public Guid LenteId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal IndiceRefracao { get; set; }
        public string? Tratamento { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
    }
}