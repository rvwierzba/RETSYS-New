using System;

namespace RETSYS.Domain.Entities
{
    public class Armacao
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        // Mapeado como código único de barras/etiqueta (SKU)
        public string CodigoSku { get; set; } = string.Empty;

        // Preservada a arquitetura relacional com a entidade Marca
        public Guid MarcaId { get; set; }
        public Marca Marca { get; set; } = null!;

        public string ModeloReferencia { get; set; } = string.Empty;

        public string Cor { get; set; } = string.Empty;

        // Especificações físicas e ópticas
        public string Tamanho { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;

        // --- Atributos Especialistas de Ótica ---
        public string Formato { get; set; } = "Retangular"; // Redondo, Quadrado, Aviador, Gatinho, Retangular, Oval, Hexagonal, Esportivo
        public string Genero { get; set; } = "Unissex";     // Unissex, Masculino, Feminino, Infantil
        public decimal? Aro { get; set; }                   // Largura da lente (ex: 52 mm)
        public decimal? Ponte { get; set; }                 // Distância entre lentes (ex: 18 mm)
        public decimal? Haste { get; set; }                 // Comprimento da haste (ex: 140 mm)

        public string Fornecedor { get; set; } = string.Empty;

        // Controles financeiros (Preço de custo visível apenas para ADMIN/GERENTE)
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }

        // Indicadores de inventário e gatilhos de nível mínimo
        public int QuantidadeEstoque { get; set; } = 0;
        public int QuantidadeMinima { get; set; }

        public bool Ativo { get; set; } = true;

        // Loja/Unidade onde a armação está estocada
        public string LojaUnidade { get; set; } = "Matriz";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
