using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class LentesViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LentesViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/lentes")]
        public async Task<IActionResult> Index()
        {
            var lentes = await _context.Lentes
                .Where(l => l.Ativo)
                .OrderBy(l => l.Laboratorio)
                .Select(l => new {
                    l.Id,
                    l.Laboratorio,
                    l.Tipo,
                    l.Surfacada
                })
                .ToListAsync();

            var precos = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo)
                .Select(lp => new {
                    lp.Id,
                    Tipo = lp.Tipo,
                    IndiceRefracao = lp.IndiceRefracao,
                    Tratamento = lp.Tratamento,
                    PrecoCusto = lp.PrecoCusto,
                    PrecoVenda = lp.PrecoVenda,
                    Lente = new {
                        Laboratorio = lp.Lente.Laboratorio,
                        Tipo = lp.Lente.Tipo
                    }
                })
                .ToListAsync();

            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .ToListAsync();

            // Detecta dinamicamente se o usuário logado possui a role de Admin
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("admin");

            return Inertia.Render("Lentes/Index", new {
                Lentes = lentes,
                Precos = precos,
                TratamentosSugeridos = tratamentosSugeridos,
                IsAdmin = isAdmin
            });
        }
    }
}