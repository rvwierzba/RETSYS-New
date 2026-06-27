using System;

namespace RETSYS.Domain.Entities
{
    public class ConfiguracaoComissao
    {
        public int Id { get; set; } // Sempre 1 registro ativo no sistema
        
        public decimal PercentualComissao { get; set; }
        
        public string BaseCalculo { get; set; } = "VALOR_BRUTO_OS";
        
        public string MomentoGeracao { get; set; } = "EMISSAO_OS";
        
        public bool Ativo { get; set; } = true;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public Guid UpdatedById { get; set; }
        public Usuario UpdatedBy { get; set; } = null!;
    }
}