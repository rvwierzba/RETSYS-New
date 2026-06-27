using System;

namespace RETSYS.Domain.Entities
{
    public class FechamentoComissao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid VendedorId { get; set; }
        public Usuario Vendedor { get; set; } = null!;
        
        public string PeriodoReferencia { get; set; } = string.Empty; // Formato AAAA-MM
        
        public decimal TotalVendasBrutas { get; set; }
        
        public decimal TotalComissao { get; set; }
        
        public int QtdOs { get; set; }
        
        // Status possíveis: ABERTO, FECHADO, PAGO
        public string Status { get; set; } = "ABERTO";
        
        public DateTime? DataFechamento { get; set; }
        
        public DateTime? DataPagamento { get; set; }
        
        public Guid? FechadoPorId { get; set; }
        public Usuario? FechadoPor { get; set; }
    }
}