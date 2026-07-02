using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class OrdemServico
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NumeroOS { get; set; } = string.Empty; // Gerado auto (Ex: OS-2026-00001)
        
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        
        // Torna-se Nullable para aceitar importações históricas do CRM sem vendedor ativo
        public Guid? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }
        
        public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
        public DateTime DataPrevistaEntrega { get; set; }
        public DateTime? DataEntregaReal { get; set; }
        
        public string Status { get; set; } = "EM_ABERTO"; // EM_ABERTO, EM_LABORATORIO, PRONTO, ENTREGUE, CANCELADO
        
        public string? MedicoNome { get; set; }
        public string? MedicoCrm { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos 1:1 Obrigatórios (Tabelas do TCC)
        public OsReceita Receita { get; set; } = null!;
        public OsFinanceiro Financeiro { get; set; } = null!;
        public string MedicoTipo { get; set; } = "NAO_ESPECIFICADO"; // OFTALMOLOGISTA, OPTOMETRISTA, NAO_ESPECIFICADO
        
        // Relacionamento 1:Muitos com as parcelas financeiras
        public ICollection<ParcelaPagamento> Parcelas { get; set; } = new List<ParcelaPagamento>();

        // =========================================================================
        // NOVOS CAMPOS PARA SUPORTE A ORDENS RETROATIVAS / HISTÓRICAS
        // =========================================================================
        
        // Identifica se a OS é um registro do passado (ignora baixa de estoque ativo e caixa de hoje)
        public bool IsRetroativa { get; set; } = false;

        // Armazena a descrição da armação antiga caso ela não exista no inventário de hoje
        public string? ArmacaoModeloManual { get; set; }

        // Armazena a descrição textual da lente comprada no passado
        public string? LenteDescricaoManual { get; set; }

        // Controle de exclusão lógica para integridade histórica do banco de dados
        public bool Ativo { get; set; } = true;
    }
}