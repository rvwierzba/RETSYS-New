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
    public class ReceitasController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public ReceitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /clientes/{clienteId}/receitas
        [HttpGet("/clientes/{clienteId}/receitas")]
        public async Task<IActionResult> PorCliente(Guid clienteId)
        {
            var oticaId = ObterOticaId();

            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.OticaId == oticaId)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.CPF,
                    c.Telefone,
                    DataNascimento = c.DataNascimento.HasValue ? c.DataNascimento.Value.ToString("dd/MM/yyyy") : null,
                    c.Cidade,
                    c.Estado
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound();

            var receitas = await _context.ReceitasCliente
                .Where(r => r.ClienteId == clienteId && r.OticaId == oticaId && r.Ativo)
                .OrderByDescending(r => r.DataExame)
                .Select(r => new
                {
                    r.Id,
                    DataExame = r.DataExame.ToString("dd/MM/yyyy"),
                    DataValidade = r.DataValidade.HasValue ? r.DataValidade.Value.ToString("dd/MM/yyyy") : null,
                    r.MedicoNome,
                    r.MedicoCrm,
                    r.MedicoTipo,
                    r.Clinica,
                    r.OdEsferico,
                    r.OdCilindrico,
                    r.OdEixo,
                    r.OdDnp,
                    r.OdAltura,
                    r.OeEsferico,
                    r.OeCilindrico,
                    r.OeEixo,
                    r.OeDnp,
                    r.OeAltura,
                    r.Adicao,
                    OdEsfericoPerto = r.OdEsferico + (r.Adicao ?? 0),
                    OeEsfericoPerto = r.OeEsferico + (r.Adicao ?? 0),
                    r.Aro,
                    r.Ponte,
                    r.MaiorDiagonal,
                    r.AlturaVertical,
                    r.Observacoes,
                    r.FotoReceitaUrl,
                    CriadoEm = r.CriadoEm.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Inertia.Render("Clientes/FichaClinica", new
            {
                Cliente = cliente,
                Receitas = receitas
            });
        }

        // POST /clientes/{clienteId}/receitas
        [HttpPost("/clientes/{clienteId}/receitas")]
        public async Task<IActionResult> Salvar(Guid clienteId, [FromBody] ReceitaInputDto dto)
        {
            var oticaId = ObterOticaId();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId && c.OticaId == oticaId);
            if (cliente == null)
                return NotFound();

            var receita = new ReceitaCliente
            {
                OticaId = oticaId,
                ClienteId = clienteId,
                DataExame = dto.DataExame ?? DateTime.UtcNow,
                DataValidade = dto.DataValidade,
                MedicoNome = string.IsNullOrWhiteSpace(dto.MedicoNome) ? "Não especificado" : dto.MedicoNome.Trim(),
                MedicoCrm = dto.MedicoCrm?.Trim(),
                MedicoTipo = string.IsNullOrWhiteSpace(dto.MedicoTipo) ? "OFTALMOLOGISTA" : dto.MedicoTipo.Trim(),
                Clinica = dto.Clinica?.Trim(),
                OdEsferico = dto.OdEsferico,
                OdCilindrico = dto.OdCilindrico,
                OdEixo = dto.OdEixo,
                OdDnp = dto.OdDnp,
                OdAltura = dto.OdAltura,
                OeEsferico = dto.OeEsferico,
                OeCilindrico = dto.OeCilindrico,
                OeEixo = dto.OeEixo,
                OeDnp = dto.OeDnp,
                OeAltura = dto.OeAltura,
                Adicao = dto.Adicao,
                Aro = dto.Aro,
                Ponte = dto.Ponte,
                MaiorDiagonal = dto.MaiorDiagonal,
                AlturaVertical = dto.AlturaVertical,
                Observacoes = dto.Observacoes?.Trim(),
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            _context.ReceitasCliente.Add(receita);

            // Atualiza também o resumo do cliente para retrocompatibilidade rápida
            cliente.DataReceita = receita.DataExame;
            cliente.UltimaOdEsferico = receita.OdEsferico;
            cliente.UltimaOdCilindrico = receita.OdCilindrico;
            cliente.UltimaOdEixo = receita.OdEixo;
            cliente.UltimaOeEsferico = receita.OeEsferico;
            cliente.UltimaOeCilindrico = receita.OeCilindrico;
            cliente.UltimaOeEixo = receita.OeEixo;
            cliente.UltimaAdicao = receita.Adicao;
            cliente.UltimaDnpOd = receita.OdDnp;
            cliente.UltimaDnpOe = receita.OeDnp;
            cliente.UltimaAlturaMontagem = receita.OdAltura ?? receita.OeAltura;
            cliente.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Sucesso = true, ReceitaId = receita.Id });
        }
    }

    public class ReceitaInputDto
    {
        public DateTime? DataExame { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? MedicoNome { get; set; }
        public string? MedicoCrm { get; set; }
        public string? MedicoTipo { get; set; }
        public string? Clinica { get; set; }
        public decimal OdEsferico { get; set; }
        public decimal OdCilindrico { get; set; }
        public int OdEixo { get; set; }
        public decimal? OdDnp { get; set; }
        public decimal? OdAltura { get; set; }
        public decimal OeEsferico { get; set; }
        public decimal OeCilindrico { get; set; }
        public int OeEixo { get; set; }
        public decimal? OeDnp { get; set; }
        public decimal? OeAltura { get; set; }
        public decimal? Adicao { get; set; }
        public decimal? Aro { get; set; }
        public decimal? Ponte { get; set; }
        public decimal? MaiorDiagonal { get; set; }
        public decimal? AlturaVertical { get; set; }
        public string? Observacoes { get; set; }
    }
}

