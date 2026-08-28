using System;

namespace RETSYS.Domain.Entities
{
    public class OsFinanceiro
    {
        public Guid OsId { get; set; } // FK e PK compartilhada
        public OrdemServico OrdemServico { get; set; } = null!;

        // Padrão opcional: Cliente pode levar só lente, só armação ou sem itens vinculados
        public Guid? ArmacaoId { get; set; }
        public Armacao? Armacao { get; set; }

        // Aponta para a variação exata vendida (índice + tratamento + preço)
        public Guid? LentePrecoId { get; set; }
        public LentePreco? LentePreco { get; set; }

        public decimal ValorTotalBruto { get; set; }
        public decimal DescontoReais { get; set; }
        public decimal DescontoPercentual { get; set; } // Calculado automaticamente
        public decimal ValorTotalLiquido { get; set; }  // Valor final cobrado do cliente

        public string FormaPagamento { get; set; } = "DINHEIRO"; // DINHEIRO, PIX, CARTAO_CREDITO, CARTAO_DEBITO, BOLETO
        public int? Parcelas { get; set; } = 1;
        public decimal ValorArmacao { get; set; } // Preço de venda da armação no momento da OS
        public decimal ValorLente { get; set; }   // Preço de venda da lente (LentePreco) no momento da OS

        // =========================================================================
        // SEÇÃO 5: ENTRADA E SALDO RESTANTE
        // =========================================================================
        public decimal? ValorEntrada { get; set; }
        public decimal ValorRestante { get; set; } = 0m;

        // =========================================================================
        // SEÇÃO 4.3: CONFERÊNCIA DE PAGAMENTO PELO GERENTE
        // =========================================================================
        public bool PagamentoConferido { get; set; } = false;
        public Guid? ConferidoPorId { get; set; }
        public virtual Usuario? ConferidoPor { get; set; }
        public DateTime? DataConferencia { get; set; }

        // =========================================================================
        // SEÇÃO 6: QUITAÇÃO DO SALDO NA RETIRADA
        // =========================================================================
        public decimal? ValorRecebidoRetirada { get; set; }
        public string? FormaPagamentoRetirada { get; set; }
        public int? ParcelasRetirada { get; set; }
        public DateTime? DataQuitacao { get; set; }
        public Guid? QuitacaoRegistradaPorId { get; set; }
        public virtual Usuario? QuitacaoRegistradaPor { get; set; }
    }
}