using InertiaCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RETSYS.Domain.Entities;
using RETSYS.Infrastructure.Data;

namespace RETSYS.Web.Controllers;

[Authorize]
public class ArmacoesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArmacoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /estoque ou /armacoes
    [HttpGet("/estoque")]
    [HttpGet("/armacoes")]
    public async Task<IActionResult> Index()
    {
        var armacoes = await _context.Armacoes
            .Include(a => a.Marca)
            .AsNoTracking()
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new
            {
                a.Id,
                Codigo = a.CodigoSku,
                a.MarcaId,
                MarcaNome = a.Marca != null ? a.Marca.Nome : "Sem Marca",
                Modelo = a.ModeloReferencia,
                a.Cor,
                a.Tamanho,
                a.Material,
                a.Fornecedor,
                a.QuantidadeEstoque,
                a.QuantidadeMinima,
                a.PrecoCusto,
                PrecoFinal = a.PrecoVenda,
                a.Ativo,
                a.CriadoEm
            })
            .ToListAsync();

        var marcas = await _context.Marcas
            .Where(m => m.Ativo)
            .AsNoTracking()
            .OrderBy(m => m.Nome)
            .Select(m => new { m.Id, m.Nome })
            .ToListAsync();

        return Inertia.Render("Estoque/Index", new { Armacoes = armacoes, Marcas = marcas });
    }

    // POST: /armacoes
    [HttpPost("/armacoes")]
    public async Task<IActionResult> Store([FromBody] DtoNovaArmacao model)
    {
        if (string.IsNullOrWhiteSpace(model.Codigo) || string.IsNullOrWhiteSpace(model.Modelo) || model.MarcaId == Guid.Empty)
        {
            Inertia.Share("erro", "Preencha os campos obrigatórios (Código SKU, Modelo e Marca).");
            return RedirectToAction(nameof(Index));
        }

        var codigoExiste = await _context.Armacoes
            .AnyAsync(a => a.CodigoSku.ToLower() == model.Codigo.Trim().ToLower());

        if (codigoExiste)
        {
            Inertia.Share("erro", "Já existe uma armação cadastrada com este código SKU.");
            return RedirectToAction(nameof(Index));
        }

        var novaArmacao = new Armacao
        {
            Id = Guid.NewGuid(),
            CodigoSku = model.Codigo.Trim().ToUpper(),
            MarcaId = model.MarcaId,
            ModeloReferencia = model.Modelo.Trim(),
            Cor = model.Cor?.Trim() ?? string.Empty,
            Tamanho = model.Tamanho?.Trim() ?? string.Empty,
            Material = model.Material?.Trim() ?? string.Empty,
            Fornecedor = model.Fornecedor?.Trim() ?? string.Empty,
            QuantidadeEstoque = model.QuantidadeEstoque < 0 ? 0 : model.QuantidadeEstoque,
            QuantidadeMinima = model.QuantidadeMinima < 0 ? 0 : model.QuantidadeMinima,
            PrecoCusto = model.PrecoCusto < 0 ? 0 : model.PrecoCusto,
            PrecoVenda = model.PrecoFinal < 0 ? 0 : model.PrecoFinal,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Armacoes.Add(novaArmacao);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /armacoes/editar/{id:guid}
    [HttpPost("/armacoes/editar/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] DtoEditarArmacao model)
    {
        var armacao = await _context.Armacoes.FindAsync(id);
        if (armacao == null)
        {
            return NotFound(new { message = "Armação não encontrada." });
        }

        var codigoExiste = await _context.Armacoes
            .AnyAsync(a => a.CodigoSku.ToLower() == model.Codigo.Trim().ToLower() && a.Id != id);

        if (codigoExiste)
        {
            Inertia.Share("erro", "Este código SKU já pertence a outra armação.");
            return RedirectToAction(nameof(Index));
        }

        armacao.CodigoSku = model.Codigo.Trim().ToUpper();
        armacao.MarcaId = model.MarcaId;
        armacao.ModeloReferencia = model.Modelo.Trim();
        armacao.Cor = model.Cor?.Trim() ?? string.Empty;
        armacao.Tamanho = model.Tamanho?.Trim() ?? string.Empty;
        armacao.Material = model.Material?.Trim() ?? string.Empty;
        armacao.Fornecedor = model.Fornecedor?.Trim() ?? string.Empty;
        armacao.QuantidadeEstoque = model.QuantidadeEstoque;
        armacao.QuantidadeMinima = model.QuantidadeMinima;
        armacao.PrecoCusto = model.PrecoCusto;
        armacao.PrecoVenda = model.PrecoFinal;

        _context.Armacoes.Update(armacao);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /armacoes/excluir/{id:guid}
    [HttpPost("/armacoes/excluir/{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var armacao = await _context.Armacoes.FindAsync(id);
        if (armacao != null)
        {
            _context.Armacoes.Remove(armacao);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

public record DtoNovaArmacao(
    string Codigo,
    Guid MarcaId,
    string Modelo,
    string? Cor,
    string? Tamanho,
    string? Material,
    string? Fornecedor,
    int QuantidadeEstoque,
    int QuantidadeMinima,
    decimal PrecoCusto,
    decimal PrecoFinal
);

public record DtoEditarArmacao(
    string Codigo,
    Guid MarcaId,
    string Modelo,
    string? Cor,
    string? Tamanho,
    string? Material,
    string? Fornecedor,
    int QuantidadeEstoque,
    int QuantidadeMinima,
    decimal PrecoCusto,
    decimal PrecoFinal
);