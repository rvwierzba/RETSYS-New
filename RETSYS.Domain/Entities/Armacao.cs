using System;

namespace RETSYS.Domain.Entities
{
    public class Armacao
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Mapeado como código único de barras/etiqueta (SKU)
        public string CodigoSku { get; set; } = string.Empty; 
        
        // Preservada a sua arquitetura relacional com a entidade Marca
        public Guid MarcaId { get; set; }
        public Marca Marca { get; set; } = null!; 
        
        public string ModeloReferencia { get; set; } = string.Empty;
        
        public string Cor { get; set; } = string.Empty;
        
        // Mantidos os seus campos originais de especificação física
        public string Tamanho { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        
        public string Fornecedor { get; set; } = string.Empty;
        
        // Controles financeiros (Preço de custo visível apenas para ADMIN)
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        
        // Indicadores de inventário e gatilhos de nível mínimo
        public int QuantidadeEstoque { get; set; } = 0;
        public int QuantidadeMinima { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}