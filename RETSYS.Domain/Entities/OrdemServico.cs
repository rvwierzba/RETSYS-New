using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class OrdemServico
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public string NumeroOS { get; set; } = string.Empty; // Gerado auto ou editável no balcão

        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        // Vínculo opcional com a receita/ficha clínica histórica
        public Guid? ReceitaClienteId { get; set; }
        public ReceitaCliente? ReceitaCliente { get; set; }

        // Vendedor responsável pelo atendimento
        public Guid? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
        public DateTime DataPrevistaEntrega { get; set; }
        public DateTime? DataEntregaReal { get; set; }

        public string Status { get; set; } = "EM_ABERTO"; // EM_ABERTO, AGUARDANDO_LENTE, EM_MONTAGEM, PRONTA, ENTREGUE, CANCELADO

        // =========================================================================
        // 🔬 ESTEIRA ESPECIALISTA DE LABORATÓRIO ÓPTICO & QUALIDADE
        // =========================================================================
        public string EtapaLaboratorio { get; set; } = "BALCAO"; 
        // BALCAO, AGUARDANDO_INSUMOS, LABORATORIO_EXTERNO, EM_MONTAGEM, LENSOMETRIA_QUALIDADE, PRONTO_RETIRADA, ENTREGUE

        public string? LaboratorioParceiro { get; set; } // Essilor, Hoya, Zeiss, Lab Proprio, etc.
        public string? NumeroPedidoLaboratorio { get; set; } // Número do pedido no lab externo
        public DateTime? DataEnvioLaboratorio { get; set; }
        public DateTime? DataRetornoLaboratorio { get; set; }

        public bool ConferidoLensometro { get; set; } = false;
        public Guid? ConferidoLensometroPorId { get; set; }
        public Usuario? ConferidoLensometroPor { get; set; }
        public DateTime? ConferidoLensometroEm { get; set; }

        public string? MedicoNome { get; set; }
        public string? MedicoCrm { get; set; }
        public string MedicoTipo { get; set; } = "NAO_ESPECIFICADO"; // OFTALMOLOGISTA, OPTOMETRISTA, NAO_ESPECIFICADO
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos 1:1 Obrigatórios
        public OsReceita Receita { get; set; } = null!;
        public OsFinanceiro Financeiro { get; set; } = null!;

        // Relacionamento 1:Muitos com as parcelas financeiras
        public ICollection<ParcelaPagamento> Parcelas { get; set; } = new List<ParcelaPagamento>();

        // =========================================================================
        // CONTROLE DE LENTES PEDIDAS
        // =========================================================================
        public bool LentePedida { get; set; } = false;
        public DateTime? DataPedidoLente { get; set; }
        public Guid? PedidoLentePorId { get; set; }
        public virtual Usuario? PedidoLentePor { get; set; }

        // =========================================================================
        // SUPORTE A ORDENS RETROATIVAS / HISTÓRICAS
        // =========================================================================
        public bool IsRetroativa { get; set; } = false;
        public string? ArmacaoModeloManual { get; set; }
        public string? LenteDescricaoManual { get; set; }
        public bool Ativo { get; set; } = true;
        public string LojaVenda { get; set; } = "Matriz";
        public string? DataAjustadaLog { get; set; }
        public DateTime? DataEntradaOriginal { get; set; }
        public string? UltimaEdicaoLog { get; set; }

        // =========================================================================
        // FLUXO DE ACOMPANHAMENTO DE OS E INTEGRAÇÃO WHATSAPP
        // =========================================================================
        public DateTime? DataPrevisaoLente { get; set; }
        public bool LenteChegou { get; set; } = false;
        public DateTime? DataChegadaLente { get; set; }

        public bool WhatsAppNotificadoCriacao { get; set; } = false;
        public bool WhatsAppNotificadoPronto { get; set; } = false;
    }
}
