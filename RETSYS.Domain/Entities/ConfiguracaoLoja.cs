using System;

namespace RETSYS.Domain.Entities
{
    public class ConfiguracaoLoja
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Relação 1:1 — cada Ótica possui exatamente uma Configuração de Loja
        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public string NomeLoja { get; set; } = "Matriz";
        public string Cnpj { get; set; } = string.Empty;

        public string? PixApiKey { get; set; }

        public bool PixAtivo => !string.IsNullOrWhiteSpace(PixApiKey);
    }
}
