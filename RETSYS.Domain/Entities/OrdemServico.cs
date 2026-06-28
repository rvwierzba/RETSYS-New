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
        
        public Guid VendedorId { get; set; }
        public Usuario Vendedor { get; set; } = null!;
        
        public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
        public DateTime DataPrevistaEntrega { get; set; }
        public DateTime? DataEntregaReal { get; set; }
        
        public string Status { get; set; } = "EM_ABERTO"; // EM_ABERTO, EM_LABORATORIO, PRONTO, ENTREGUE, CANCELADO
        
        public string? MedicoNome { get; set; }
        public string? MedicoCrm { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos 1:1 Obrigatórios (Novas tabelas do TCC)
        public OsReceita Receita { get; set; } = null!;
        public OsFinanceiro Financeiro { get; set; } = null!;
        
        // Relacionamento 1:Muitos com as parcelas financeiras
        public ICollection<ParcelaPagamento> Parcelas { get; set; } = new List<ParcelaPagamento>();
    }
}