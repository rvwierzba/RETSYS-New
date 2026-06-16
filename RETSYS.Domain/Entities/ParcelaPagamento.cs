using System;
using RETSYS.Domain.Enums;

namespace RETSYS.Domain.Entities
{
    public class ParcelaPagamento
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // pgar_id [cite: 552]
        
        // Vínculo com a Ordem de Serviço [cite: 552]
        public Guid OrdemServicoId { get; set; } // fkos_id [cite: 552]
        public OrdemServico OrdemServico { get; set; } = null!;
        
        public int NumeroParcela { get; set; } // pgar_num_parcelas [cite: 552]
        
        public string DescricaoParcela { get; set; } = string.Empty; // pgar_parcela_nome [cite: 552]
        
        public decimal Valor { get; set; } // pgar_valor_parcela [cite: 552]
        
        public DateTime DataVencimento { get; set; }
        
        public DateTime? DataPagamento { get; set; } // pgar_data_pagamento [cite: 552]
        
        public MetodoPagamento? Metodo { get; set; } // pgar_forma_pagamento [cite: 552]
        
        // --- Campos Modernos para o Ecossistema PIX ---
        public string? PixQrCodePayload { get; set; } // String do Pix Copia e Cola
        public string? PixTxId { get; set; }          // Identificador da transação para Webhook
        
        // Propriedade calculada baseada na existência da data de pagamento [cite: 552]
        public bool Paga => DataPagamento.HasValue;
    }
}