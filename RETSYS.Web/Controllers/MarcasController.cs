using InertiaCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RETSYS.Domain.Entities;
using RETSYS.Infrastructure.Data;

namespace RETSYS.Web.Controllers;

[Authorize]
public class MarcasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MarcasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /marcas
    [HttpGet("/marcas")]
    public async Task<IActionResult> Index()
    {
        var marcas = await _context.Marcas
            .AsNoTracking()
            .OrderBy(m => m.Nome)
            .Select(m => new
            {
                m.Id,
                m.Nome,
                m.Descricao,
                m.Ativo,
                m.CriadoEm,
                TotalArmacoes = _context.Armacoes.Count(a => a.MarcaId == m.Id)
            })
            .ToListAsync();

        return Inertia.Render("Marcas/Index", new { Marcas = marcas });
    }

    // POST: /marcas (Atende formulário do Inertia e chamadas Axios de cadastro rápido)
    [HttpPost("/marcas")]
    public async Task<IActionResult> Store([FromBody] DtoNovaMarca model)
    {
        bool eRequisicaoAxios = Request.Headers["X-Inertia"].Count == 0 && 
                                Request.Headers.Accept.ToString().Contains("application/json");

        if (string.IsNullOrWhiteSpace(model.Nome))
        {
            if (eRequisicaoAxios) return BadRequest(new { mensagem = "O nome da marca é obrigatório." });
            Inertia.Share("erro", "O nome da marca é obrigatório.");
            return RedirectToAction(nameof(Index));
        }

        var nomeExiste = await _context.Marcas
            .AnyAsync(m => m.Nome.ToLower() == model.Nome.Trim().ToLower());

        if (nomeExiste)
        {
            if (eRequisicaoAxios) return BadRequest(new { mensagem = "Já existe uma marca cadastrada com este nome." });
            Inertia.Share("erro", "Já existe uma marca cadastrada com este nome.");
            return RedirectToAction(nameof(Index));
        }

        var novaMarca = new Marca
        {
            Id = Guid.NewGuid(),
            Nome = model.Nome.Trim(),
            Descricao = model.Descricao?.Trim() ?? string.Empty,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Marcas.Add(novaMarca);
        await _context.SaveChangesAsync();

        if (eRequisicaoAxios)
        {
            return Ok(new { id = novaMarca.Id, nome = novaMarca.Nome });
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /marcas/editar/{id:guid}
    [HttpPost("/marcas/editar/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] DtoEditarMarca model)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { message = "Marca não encontrada." });
        }

        var nomeExiste = await _context.Marcas
            .AnyAsync(m => m.Nome.ToLower() == model.Nome.Trim().ToLower() && m.Id != id);

        if (nomeExiste)
        {
            Inertia.Share("erro", "Este nome já está em uso por outra marca.");
            return RedirectToAction(nameof(Index));
        }

        marca.Nome = model.Nome.Trim();
        marca.Descricao = model.Descricao?.Trim() ?? string.Empty;
        marca.Ativo = model.Ativo;

        _context.Marcas.Update(marca);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /marcas/alternar-status/{id:guid}
    [HttpPost("/marcas/alternar-status/{id:guid}")]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca != null)
        {
            marca.Ativo = !marca.Ativo;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /marcas/excluir/{id:guid}
    [HttpPost("/marcas/excluir/{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var possuiArmacoes = await _context.Armacoes.AnyAsync(a => a.MarcaId == id);
        if (possuiArmacoes)
        {
            Inertia.Share("erro", "Não é possível excluir uma marca que possui armações vinculadas.");
            return RedirectToAction(nameof(Index));
        }

        var marca = await _context.Marcas.FindAsync(id);
        if (marca != null)
        {
            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

public record DtoNovaMarca(string Nome, string? Descricao);
public record DtoEditarMarca(string Nome, string? Descricao, bool Ativo);