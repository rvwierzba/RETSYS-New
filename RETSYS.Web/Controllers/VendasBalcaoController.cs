using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class VendasBalcaoController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public VendasBalcaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /vendas/pdv
        [HttpGet("/vendas/pdv")]
        public async Task<IActionResult> Pdv()
        {
            var oticaId = ObterOticaId();

            var clientes = await _context.Clientes
                .Where(c => c.OticaId == oticaId)
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome, c.CPF, c.Telefone })
                .Take(200)
                .ToListAsync();

            var armacoes = await _context.Armacoes
                .Include(a => a.Marca)
                .Where(a => a.OticaId == oticaId && a.Ativo && a.QuantidadeEstoque > 0)
                .OrderBy(a => a.ModeloReferencia)
                .Select(a => new
                {
                    a.Id,
                    a.CodigoSku,
                    Marca = a.Marca.Nome,
                    a.ModeloReferencia,
                    a.Cor,
                    a.PrecoVenda,
                    a.QuantidadeEstoque,
                    a.Formato,
                    a.Genero
                })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.OticaId == oticaId && u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            var configLoja = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaId);

            return Inertia.Render("Vendas/PdvBalcao", new
            {
                Clientes = clientes,
                Armacoes = armacoes,
                Vendedores = vendedores,
                NomeLoja = configLoja?.NomeLoja ?? "RETSYS Ótica"
            });
        }

        // GET /vendas
        [HttpGet("/vendas")]
        public async Task<IActionResult> Index([FromQuery] string? busca, [FromQuery] string? filtro = "todas")
        {
            var oticaId = ObterOticaId();

            // 1. Vendas Balcão
            var queryBalcao = _context.VendasBalcao
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .Include(v => v.Itens)
                .Where(v => v.OticaId == oticaId)
                .OrderByDescending(v => v.DataVenda)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                queryBalcao = queryBalcao.Where(v => v.NumeroVenda.ToLower().Contains(termo)
                    || (v.Cliente != null && v.Cliente.Nome.ToLower().Contains(termo)));
            }

            var vendasBalcaoRaw = await queryBalcao
                .Take(100)
                .Select(v => new
                {
                    v.Id,
                    v.NumeroVenda,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : "Consumidor Final",
                    VendedorNome = v.Vendedor != null ? v.Vendedor.Nome : "Balcão",
                    v.DataVenda,
                    v.ValorTotalBruto,
                    v.DescontoReais,
                    v.ValorTotalLiquido,
                    v.FormaPagamento,
                    v.Status,
                    TotalItens = v.Itens.Count,
                    Tipo = "BALCAO"
                })
                .ToListAsync();

            // 2. Vendas de Ordens de Serviço (OSs da ótica)
            var queryOs = _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Cliente)
                .Include(o => o.Vendedor)
                .Include(o => o.Financeiro)
                .Where(o => o.OticaId == oticaId)
                .OrderByDescending(o => o.DataEntrada)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                queryOs = queryOs.Where(o => o.NumeroOS.ToLower().Contains(termo)
                    || (o.Cliente != null && o.Cliente.Nome.ToLower().Contains(termo)));
            }

            var vendasOsRaw = await queryOs
                .Take(100)
                .Select(o => new
                {
                    o.Id,
                    NumeroVenda = "OS-" + o.NumeroOS,
                    ClienteNome = o.Cliente != null ? o.Cliente.Nome : "Cliente",
                    VendedorNome = o.Vendedor != null ? o.Vendedor.Nome : "Atendente",
                    DataVenda = o.DataEntrada,
                    ValorTotalBruto = o.Financeiro != null ? o.Financeiro.ValorTotalBruto : 0m,
                    DescontoReais = o.Financeiro != null ? o.Financeiro.DescontoReais : 0m,
                    ValorTotalLiquido = o.Financeiro != null ? o.Financeiro.ValorTotalLiquido : 0m,
                    FormaPagamento = o.Financeiro != null ? o.Financeiro.FormaPagamento : "CARNE",
                    o.Status,
                    TotalItens = 1,
                    Tipo = "ORDEM_SERVICO"
                })
                .ToListAsync();

            // Formatação segura em memória (evita falha de tradução LINQ do Npgsql)
            var listaBalcao = vendasBalcaoRaw.Select(v => new
            {
                v.Id,
                v.NumeroVenda,
                v.ClienteNome,
                v.VendedorNome,
                DataVenda = v.DataVenda.ToString("dd/MM/yyyy HH:mm"),
                v.ValorTotalBruto,
                v.DescontoReais,
                v.ValorTotalLiquido,
                v.FormaPagamento,
                v.Status,
                v.TotalItens,
                v.Tipo
            }).ToList();

            var listaOs = vendasOsRaw.Select(v => new
            {
                v.Id,
                v.NumeroVenda,
                v.ClienteNome,
                v.VendedorNome,
                DataVenda = v.DataVenda.ToString("dd/MM/yyyy HH:mm"),
                v.ValorTotalBruto,
                v.DescontoReais,
                v.ValorTotalLiquido,
                v.FormaPagamento,
                v.Status,
                v.TotalItens,
                v.Tipo
            }).ToList();

            var todasVendas = listaBalcao
                .Concat(listaOs)
                .OrderByDescending(v => v.NumeroVenda)
                .ToList();

            var totalFaturamento = todasVendas.Sum(v => v.ValorTotalLiquido);

            return Inertia.Render("Vendas/Index", new
            {
                Vendas = todasVendas,
                TotalFaturamento = totalFaturamento,
                TotalVendasBalcao = listaBalcao.Count,
                TotalVendasOs = listaOs.Count,
                Busca = busca ?? "",
                Filtro = filtro ?? "todas"
            });
        }

        // POST /vendas/finalizar
        [HttpPost("/vendas/finalizar")]
        public async Task<IActionResult> Finalizar([FromBody] VendaBalcaoInputDto dto)
        {
            var oticaId = ObterOticaId();

            if (dto.Itens == null || dto.Itens.Count == 0)
                return BadRequest("A venda precisa conter pelo menos 1 item.");

            var countVendas = await _context.VendasBalcao.CountAsync(v => v.OticaId == oticaId);
            var numeroVenda = $"VB-{(countVendas + 1).ToString("D5")}";

            var valorBruto = dto.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
            var valorLiquido = Math.Max(0, valorBruto - (dto.DescontoReais ?? 0));

            var venda = new VendaBalcao
            {
                OticaId = oticaId,
                NumeroVenda = numeroVenda,
                ClienteId = dto.ClienteId,
                VendedorId = dto.VendedorId,
                DataVenda = DateTime.UtcNow,
                ValorTotalBruto = valorBruto,
                DescontoReais = dto.DescontoReais ?? 0,
                ValorTotalLiquido = valorLiquido,
                FormaPagamento = string.IsNullOrWhiteSpace(dto.FormaPagamento) ? "DINHEIRO" : dto.FormaPagamento,
                Parcelas = dto.Parcelas ?? 1,
                Status = "CONCLUIDA",
                Observacoes = dto.Observacoes,
                LojaUnidade = "Matriz",
                CriadoEm = DateTime.UtcNow
            };

            foreach (var itemDto in dto.Itens)
            {
                var item = new ItemVendaBalcao
                {
                    TipoItem = string.IsNullOrWhiteSpace(itemDto.TipoItem) ? "ARMACAO" : itemDto.TipoItem,
                    ArmacaoId = itemDto.ArmacaoId,
                    Descricao = itemDto.Descricao,
                    Quantidade = Math.Max(1, itemDto.Quantidade),
                    PrecoUnitario = itemDto.PrecoUnitario
                };

                // Baixa no estoque se for armação vinculada
                if (itemDto.ArmacaoId.HasValue)
                {
                    var armacao = await _context.Armacoes.FirstOrDefaultAsync(a => a.Id == itemDto.ArmacaoId.Value && a.OticaId == oticaId);
                    if (armacao != null)
                    {
                        armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - item.Quantidade);
                    }
                }

                venda.Itens.Add(item);
            }

            _context.VendasBalcao.Add(venda);
            await _context.SaveChangesAsync();

            return Ok(new { Sucesso = true, VendaId = venda.Id, NumeroVenda = venda.NumeroVenda });
        }

        // GET /vendas/{id}/comprovante
        [HttpGet("/vendas/{id}/comprovante")]
        public async Task<IActionResult> Comprovante(Guid id)
        {
            var oticaId = ObterOticaId();

            var vendaDb = await _context.VendasBalcao
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .Include(v => v.Itens)
                .FirstOrDefaultAsync(v => v.Id == id && v.OticaId == oticaId);

            if (vendaDb == null)
                return NotFound();

            var venda = new
            {
                vendaDb.Id,
                vendaDb.NumeroVenda,
                ClienteNome = vendaDb.Cliente != null ? vendaDb.Cliente.Nome : "Consumidor Final",
                ClienteCpf = vendaDb.Cliente?.CPF,
                ClienteTelefone = vendaDb.Cliente?.Telefone,
                VendedorNome = vendaDb.Vendedor != null ? vendaDb.Vendedor.Nome : "Balcão",
                DataVenda = vendaDb.DataVenda.ToString("dd/MM/yyyy HH:mm"),
                vendaDb.ValorTotalBruto,
                vendaDb.DescontoReais,
                vendaDb.ValorTotalLiquido,
                vendaDb.FormaPagamento,
                vendaDb.Parcelas,
                vendaDb.Observacoes,
                Itens = vendaDb.Itens.Select(i => new
                {
                    i.Descricao,
                    i.Quantidade,
                    i.PrecoUnitario,
                    Subtotal = i.Quantidade * i.PrecoUnitario
                }).ToList()
            };

            var configLoja = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaId);

            return Inertia.Render("Vendas/ComprovanteTermico", new
            {
                Venda = venda,
                Loja = new
                {
                    Nome = configLoja?.NomeLoja ?? "RETSYS ÓTICA",
                    Cnpj = configLoja?.Cnpj ?? "",
                    Telefone = configLoja?.WhatsappNumero ?? ""
                }
            });
        }
    }

    public class VendaBalcaoInputDto
    {
        public Guid? ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public decimal? DescontoReais { get; set; }
        public string? FormaPagamento { get; set; }
        public int? Parcelas { get; set; }
        public string? Observacoes { get; set; }
        public List<ItemVendaInputDto> Itens { get; set; } = new List<ItemVendaInputDto>();
    }

    public class ItemVendaInputDto
    {
        public string? TipoItem { get; set; }
        public Guid? ArmacaoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; } = 1;
        public decimal PrecoUnitario { get; set; }
    }
}
