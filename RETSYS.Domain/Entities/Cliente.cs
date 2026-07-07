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

        // --- Campos de histórico de MIGRAÇÃO (dados legados, apenas informativos) ---
        public decimal? ValorGasto { get; set; }
        public string? ProdutoAdquirido { get; set; }
        public DateTime? DataUltimaCompra { get; set; }

        // --- Última receita conhecida (migração de histórico / última OS) ---
        public DateTime? DataReceita { get; set; }

        public decimal? UltimaOdEsferico { get; set; }
        public decimal? UltimaOdCilindrico { get; set; }
        public int? UltimaOdEixo { get; set; }

        public decimal? UltimaOeEsferico { get; set; }
        public decimal? UltimaOeCilindrico { get; set; }
        public int? UltimaOeEixo { get; set; }

        public decimal? UltimaAdicao { get; set; }
        public decimal? UltimaDnpOd { get; set; }
        public decimal? UltimaDnpOe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propriedade de Navegação
        public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();

        // Regra de negócio: verifica se o cliente faz aniversário numa data específica.
        // Não depende de banco — fica na própria entidade.
        public bool FazAniversarioEm(DateTime data)
        {
            if (DataNascimento is null)
                return false;

            return DataNascimento.Value.Day == data.Day
                && DataNascimento.Value.Month == data.Month;
        }
    }
}
