using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class OrdemServico
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // os_id 
        
        public string NumeroOS { get; set; } = string.Empty; // OS OS 
        
       // Chaves Estrangeiras e Relacionamentos 
        public Guid ClienteId { get; set; } // fk_id_cliente 
        public Cliente Cliente { get; set; } = null!;
        
        public Guid UsuarioId { get; set; } // Vinculo com o Vendedor/Usuario que operou [cite: 498, 563]
        public Usuario Usuario { get; set; } = null!;

        // Dados Clínicos e de Receita 
        public string Medico { get; set; } = string.Empty; // os_medico 
        public DateTime DataReceita { get; set; } // os_data_receita 
        public string TipoLente { get; set; } = string.Empty; // os_tipo_lente 
        public string MarcaModeloLente { get; set; } = string.Empty; // os_marca_modelo_lente 
        public string MaterialLente { get; set; } = string.Empty; // os_material_lente 
        
        // Bloco de Refração: Longe (Far) 
        public decimal EsfericoLongeDireito { get; set; } // os_esf_d_long 
        public decimal EsfericoLongeEsquerdo { get; set; } // os_esf_e_long 
        public decimal CilindricoLongeDireito { get; set; } // os_oil_d_long 
        public decimal CilindricoLongeEsquerdo { get; set; } // os_oil_e_long 
        public int EixoLongeDireito { get; set; } // os_eiko_d_long 
        public int EixoLongeEsquerdo { get; set; } // os_eixo_e_long 

        // Adição (Utilizada para calcular a visão de perto) 
        public decimal Adicao { get; set; } // os_adicao 

       // Bloco de Refração: Perto (Near) - Calculado Automaticamente pelo Sistema! 
        public decimal EsfericoPertoDireito { get; set; } // os_esf_d_pert 
        public decimal EsfericoPertoEsquerdo { get; set; } // os_esf_e_pert 
        public decimal CilindricoPertoDireito { get; set; } // os_cil_d_pert 
        public decimal CilindricoPertoEsquerdo { get; set; } // os_oil_e_pert 
        public int EixoPertoDireito { get; set; } // os_eiko_d_pert 
        public int EixoPertoEsquerdo { get; set; } // os_eiko_e_pert 

        // Valores e Controle Financeiro 
        public decimal ValorTotal { get; set; } // os_valor_total_venda 
        public string? Observacoes { get; set; } // os_obs 
        public DateTime DataVenda { get; set; } = DateTime.UtcNow; // os_data_venda 
        public DateTime? DataUltimoPagamento { get; set; } // os_data_final pagamer 

        // Relacionamento com as Parcelas Financeiras [cite: 552]
        public ICollection<ParcelaPagamento> Parcelas { get; set; } = new List<ParcelaPagamento>();

        // Campo de controle do ciclo de vida da OS (Padrão: Aberta)
        public string Status { get; set; } = "Aberta";

        /// <summary>
        /// Regra de Negócio Ótica: Executa o cálculo automático das lentes de perto
        /// </summary>
        public void CalcularGrauDePerto()
        {
            // Fórmula óptica padrão para lentes bifocais/multifocais:
            // Esférico Perto = Esférico Longe + Adição
            EsfericoPertoDireito = EsfericoLongeDireito + Adicao;
            EsfericoPertoEsquerdo = EsfericoLongeEsquerdo + Adicao;

            // Astigmatismo (Cilíndrico) e Eixo não se alteram na transição para perto
            CilindricoPertoDireito = CilindricoLongeDireito;
            CilindricoPertoEsquerdo = CilindricoLongeEsquerdo;
            EixoPertoDireito = EixoLongeDireito;
            EixoPertoEsquerdo = EixoLongeEsquerdo;
        }
    }
}