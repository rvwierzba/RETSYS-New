using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? CPF { get; set; } // Único por Ótica se preenchido, formatado
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
        public string? NumeroOsAntiga { get; set; }
        public decimal? ValorGasto { get; set; }
        public string? ProdutoAdquirido { get; set; }
        public DateTime? DataUltimaCompra { get; set; }

        // --- Última receita conhecida (retrocompatibilidade) ---
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
        public decimal? UltimaAlturaMontagem { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades de Navegação
        public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
        public ICollection<ReceitaCliente> Receitas { get; set; } = new List<ReceitaCliente>();
        public ICollection<VendaBalcao> VendasBalcao { get; set; } = new List<VendaBalcao>();

        // Regra de negócio: verifica se o cliente faz aniversário numa data específica.
        public bool FazAniversarioEm(DateTime data)
        {
            if (DataNascimento is null)
                return false;

            return DataNascimento.Value.Day == data.Day
                && DataNascimento.Value.Month == data.Month;
        }

        // Regra de negócio: Retorna true se a última receita conhecida completou 11 meses (pronta para recall)
        public bool PrecisaRecallAnual(DateTime referencia)
        {
            if (DataReceita is null)
                return false;

            var dias = (referencia.Date - DataReceita.Value.Date).TotalDays;
            return dias >= 330 && dias <= 420;
        }
    }
}
