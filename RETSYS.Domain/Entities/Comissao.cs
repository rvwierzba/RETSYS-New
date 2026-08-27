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

        // Valor líquido final da OS, já considerando os descontos concedidos.
        public decimal ValorBase { get; set; }

        // Percentual individual da vendedora no momento da emissão da OS.
        public decimal PercentualAplicado { get; set; }

        // Calculado sobre o valor líquido real:
        // ValorBase * PercentualAplicado / 100.
        public decimal ValorComissao { get; set; }

        // Status possíveis: PENDENTE, PAGO, CANCELADO.
        public string Status { get; set; } = "PENDENTE";

        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;

        public DateTime? DataPagamento { get; set; }

        // Formato: AAAA-MM.
        public string PeriodoReferencia { get; set; } = string.Empty;

        public string? Observacoes { get; set; }
    }
}
