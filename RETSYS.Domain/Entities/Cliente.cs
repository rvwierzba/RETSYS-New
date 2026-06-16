using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Cliente
    {
        // Chave primária utilizando UUID (Guid) [cite: 544]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Nome { get; set; } = string.Empty; // cli_nome [cite: 544]
        
        public string? TelefoneFixo { get; set; } // cli_telefixo [cite: 544]
        
        public string Celular { get; set; } = string.Empty; // cli_celular [cite: 544]
        
        public string CPF { get; set; } = string.Empty; // cli_opf [cite: 544]
        
        public string? RG { get; set; } // cli_rg [cite: 544]
        
        public DateTime? DataNascimento { get; set; } // cli_datanascimento [cite: 544]
        
        public string? Email { get; set; } // cli_email [cite: 544]
        
        // Dados de Endereço unificados [cite: 544]
        public string CEP { get; set; } = string.Empty; // cli_cep [cite: 544]
        public string Rua { get; set; } = string.Empty; // cli_rua [cite: 544]
        public string Numero { get; set; } = string.Empty; // cli_numero [cite: 544]
        public string Bairro { get; set; } = string.Empty; // cli_bairro [cite: 544]
        public string Cidade { get; set; } = string.Empty; // cli_cidade [cite: 544]
        public string Estado { get; set; } = string.Empty; // cli_estado (UF) [cite: 544]
        
        public string? Observacoes { get; set; } // cliobs [cite: 544]
        
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow; // cli_data_cadastro [cite: 544]

        // Relacionamento: Um cliente pode ter várias Ordens de Serviço (Histórico do Cliente) [cite: 154]
        public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    }
}