using System;

namespace RETSYS.Domain.Entities
{
    public class Comissao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;
        
        public Guid VendedorId { get; set; }
        public Usuario Vendedor { get; set; } = null!;
        
        public decimal ValorBase { get; set; } // valor_total_bruto da OS no momento
        
        public decimal PercentualAplicado { get; set; } // Taxa vigente no momento da emissão
        
        public decimal ValorComissao { get; set; } // Calculado automaticamente (Base * Percentual / 100)
        
        // Status possíveis: PENDENTE, PAGO, ESTORNADO
        public string Status { get; set; } = "PENDENTE"; 
        
        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataPagamento { get; set; }
        
        public string PeriodoReferencia { get; set; } = string.Empty; // Formato AAAA-MM
        
        public string? Observacoes { get; set; }
    }
}