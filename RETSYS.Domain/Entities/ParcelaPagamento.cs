using System;
using RETSYS.Domain.Enums;

namespace RETSYS.Domain.Entities
{
    public class ParcelaPagamento
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Vínculo com a Ordem de Serviço
        public Guid OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;
        
        public int NumeroParcela { get; set; }
        public string DescricaoParcela { get; set; } = string.Empty;
        
        public decimal Valor { get; set; }
        
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        
        public MetodoPagamento? Metodo { get; set; }
        
        // --- Crediário Próprio / Carnê da Ótica ---
        public string? NumeroCarne { get; set; }
        public decimal? JurosAtraso { get; set; } = 0m;
        public decimal? MultaAtraso { get; set; } = 0m;
        public decimal? ValorPago { get; set; }
        public string? ObservacaoPagamento { get; set; }

        // --- Campos Modernos para o Ecossistema PIX ---
        public string? PixQrCodePayload { get; set; }
        public string? PixTxId { get; set; }
        
        // Propriedade calculada baseada na existência da data de pagamento
        public bool Paga => DataPagamento.HasValue;

        // Regra de Negócio: Dias em atraso
        public int DiasAtraso => (!Paga && DateTime.UtcNow.Date > DataVencimento.Date) 
            ? (DateTime.UtcNow.Date - DataVencimento.Date).Days 
            : 0;
    }
}
