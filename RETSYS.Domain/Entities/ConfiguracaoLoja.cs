using System;

namespace RETSYS.Domain.Entities
{
    public class ConfiguracaoLoja
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NomeLoja { get; set; } = "Matriz";
        
        // Token gerado pelo cliente no painel da OpenPix
        public string? PixApiKey { get; set; }
        
        // Propriedade calculada: Se houver chave, o módulo OpenPix fica visível
        public bool PixAtivo => !string.IsNullOrWhiteSpace(PixApiKey);
    }
}