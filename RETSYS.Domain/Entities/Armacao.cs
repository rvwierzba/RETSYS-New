using System;

namespace RETSYS.Domain.Entities
{
    public class Armacao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Codigo { get; set; } = string.Empty; // Código de barras/etiqueta
        
        // Chave estrangeira para a entidade Marca
        public Guid MarcaId { get; set; }
        public Marca Marca { get; set; } = null!; // Propriedade de navegação do EF Core
        
        public string Modelo { get; set; } = string.Empty;
        
        public string Cor { get; set; } = string.Empty;
        
        public string Tamanho { get; set; } = string.Empty;
        
        public string Material { get; set; } = string.Empty;
        
        public int QuantidadeEstoque { get; set; }
        
        public decimal PrecoFinal { get; set; }
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}