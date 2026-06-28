using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty; // Único e formatado
        public string Telefone { get; set; } = string.Empty; // WhatsApp preferencial
        public DateTime? DataNascimento { get; set; }
        
        // Dados de Endereço (Atualizados automaticamente via CEP)
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // Sigla UF
        public string Cep { get; set; } = string.Empty;
        
        public string? Convenio { get; set; }
        public string? Email { get; set; }
        public string? Observacoes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propriedade de Navegação
        public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    }
}