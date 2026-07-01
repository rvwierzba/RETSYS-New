using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RETSYS.Domain.Entities
{
    public class Lente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Código único de controle (SKU) da lente
        public string CodigoSku { get; set; } = string.Empty;
        
        // Fornecedor ou laboratório fabricante (ex: Essilor, Hoya, Zeiss)
        public string Laboratorio { get; set; } = string.Empty;
        
        // Tipos homologados: MONOFOCAL, BIFOCAL, PROGRESSIVA ou OCUPACIONAL
        public string Tipo { get; set; } = string.Empty;
        
        // Descrição do antirreflexo, filtro azul, fotossensível (ex: Crizal Forte)
        public string Tratamento { get; set; } = string.Empty;
        
        // Espessura da lente (ex: 1.56, 1.67, 1.74)
        public decimal IndiceRefracao { get; set; }
        
        // Faixas de limites de grau que a lente atende para validação de receitas
        public decimal GraduacaoMin { get; set; }
        public decimal GraduacaoMax { get; set; }
        
        // Controles de preço (Preço de Custo é restrito apenas para visualização do ADMIN)
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        
        // Controle de inventário e gatilho de reposição
        public int QuantidadeEstoque { get; set; } = 0;
        public int QuantidadeMinima { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        [Column("surfacada")]
        public bool Surfacada { get; set; } = false;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}