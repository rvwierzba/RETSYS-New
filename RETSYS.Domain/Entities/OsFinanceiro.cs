using System;

namespace RETSYS.Domain.Entities
{
    public class OsFinanceiro
    {
        public Guid OsId { get; set; } // FK e PK compartilhada
        public OrdemServico OrdemServico { get; set; } = null!;
        
        public Guid ArmacaoId { get; set; }
        public Armacao Armacao { get; set; } = null!;
        
        public Guid LenteId { get; set; }
        public Lente Lente { get; set; } = null!;
        
        public decimal ValorTotalBruto { get; set; }
        public decimal DescontoReais { get; set; }
        public decimal DescontoPercentual { get; set; } // Calculado automaticamente
        public decimal ValorTotalLiquido { get; set; } // Valor final cobrado do cliente
        
        public string FormaPagamento { get; set; } = "DINHEIRO"; // DINHEIRO, PIX, CARTAO_CREDITO, etc.
        public int Parcelas { get; set; } = 1;
        public decimal? ValorEntrada { get; set; }
    }
}