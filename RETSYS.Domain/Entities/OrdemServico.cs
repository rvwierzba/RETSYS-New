using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class OrdemServico
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        
        public string NumeroOS { get; set; } = string.Empty; 
        
        // ==========================================
        // CHAVES ESTRANGEIRAS E RELACIONAMENTOS (RBAC)
        // ==========================================
        
        public Guid ClienteId { get; set; } 
        public Cliente Cliente { get; set; } = null!;
        
        // Vínculo explícito com a vendedora para regras de comissão e isolamento
        public Guid VendedorId { get; set; } 
        public Usuario Vendedor { get; set; } = null!;

        // Vínculos com o estoque para baixa automatizada de inventário
        public Guid ArmacaoId { get; set; }
        public Armacao Armacao { get; set; } = null!;

        public Guid LenteId { get; set; }
        public Lente Lente { get; set; } = null!;
        
        // ==========================================
        // DADOS CLÍNICOS E MÉDICOS
        // ==========================================
        
        public string Medico { get; set; } = string.Empty; 
        public string MedicoCrm { get; set; } = string.Empty;
        public DateTime DataReceita { get; set; } 
        public string TipoLente { get; set; } = string.Empty; 
        
        // ==========================================
        // BLOCO DE REFRAÇÃO: VISÃO DE LONGE
        // ==========================================
        
        public decimal EsfericoLongeDireito { get; set; } 
        public decimal EsfericoLongeEsquerdo { get; set; } 
        public decimal CilindricoLongeDireito { get; set; } 
        public decimal CilindricoLongeEsquerdo { get; set; } 
        public int EixoLongeDireito { get; set; } 
        public int EixoLongeEsquerdo { get; set; } 

        public decimal Adicao { get; set; } 

        // ==========================================
        // BLOCO DE REFRAÇÃO: VISÃO DE PERTO (AUTO)
        // ==========================================
        
        public decimal EsfericoPertoDireito { get; set; } 
        public decimal EsfericoPertoEsquerdo { get; set; } 
        public decimal CilindricoPertoDireito { get; set; } 
        public decimal CilindricoPertoEsquerdo { get; set; } 
        public int EixoPertoDireito { get; set; } 
        public int EixoPertoEsquerdo { get; set; } 

        // ==========================================
        // MEDIDAS TÉCNICAS REQUERIDAS DO MVP
        // ==========================================
        
        public decimal DnpOd { get; set; } // Distância Naso-Pupilar Olho Direito
        public decimal DnpOe { get; set; } // Distância Naso-Pupilar Olho Esquerdo
        public decimal AlturaMontagem { get; set; } // Obrigatório para lentes progressivas

        // ==========================================
        // VALORES E CONTROLE FINANCEIRO
        // ==========================================
        
        public decimal ValorTotalBruto { get; set; } // Soma dos preços sem desconto
        public decimal DescontoReais { get; set; } 
        public decimal DescontoPercentual { get; set; } // Calculado em tempo real pelo servidor
        public decimal ValorTotal { get; set; } // Valor líquido final cobrado do cliente
        public decimal ValorEntrada { get; set; }
        
        public string FormaPagamento { get; set; } = "DINHEIRO";
        public string? Observacoes { get; set; } 
        
        // Linha do tempo e ciclo de vida da OS
        public DateTime DataVenda { get; set; } = DateTime.UtcNow; 
        public DateTime? DataPrevistaEntrega { get; set; }
        public DateTime? DataEntregaReal { get; set; }
        public DateTime? DataUltimoPagamento { get; set; } 

        public string Status { get; set; } = "EM_ABERTO";

        // Parcelamento associado
        public ICollection<ParcelaPagamento> Parcelas { get; set; } = new List<ParcelaPagamento>();

        /// <summary>
        /// Regra de Negócio Ótica: Executa o cálculo automático das lentes de perto
        /// </summary>
        public void CalcularGrauDePerto()
        {
            EsfericoPertoDireito = EsfericoLongeDireito + Adicao;
            EsfericoPertoEsquerdo = EsfericoLongeEsquerdo + Adicao;

            CilindricoPertoDireito = CilindricoLongeDireito;
            CilindricoPertoEsquerdo = CilindricoLongeEsquerdo;
            EixoPertoDireito = EixoLongeDireito;
            EixoPertoEsquerdo = EixoLongeEsquerdo;
        }
    }
}