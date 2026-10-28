using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class VendaBalcao
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public string NumeroVenda { get; set; } = string.Empty;

        public Guid? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public Guid? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        public DateTime DataVenda { get; set; } = DateTime.UtcNow;

        public decimal ValorTotalBruto { get; set; }
        public decimal DescontoReais { get; set; }
        public decimal ValorTotalLiquido { get; set; }

        public string FormaPagamento { get; set; } = "DINHEIRO"; // DINHEIRO, PIX, CARTAO_DEBITO, CARTAO_CREDITO, CARNE_LOJA
        public int? Parcelas { get; set; } = 1;

        public string Status { get; set; } = "CONCLUIDA"; // CONCLUIDA, CANCELADA

        public string? Observacoes { get; set; }
        public string LojaUnidade { get; set; } = "Matriz";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<ItemVendaBalcao> Itens { get; set; } = new List<ItemVendaBalcao>();
    }
}

