using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RETSYS.Domain.Entities
{
    public class Lente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Código único de controle (SKU) da lente base
        public string CodigoSku { get; set; } = string.Empty;

        // Fornecedor ou laboratório fabricante (ex: Essilor, Hoya, Zeiss)
        public string Laboratorio { get; set; } = string.Empty;

        // Tipos homologados: MONOFOCAL, BIFOCAL, PROGRESSIVA ou OCUPACIONAL
        public string Tipo { get; set; } = string.Empty;

        // Faixas de limites de grau que a lente atende para validação de receitas
        public decimal GraduacaoMin { get; set; }
        public decimal GraduacaoMax { get; set; }

        public bool Ativo { get; set; } = true;

        [Column("surfacada")]
        public bool Surfacada { get; set; } = false;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Navegação: uma lente base pode ter várias variações de preço/tratamento/índice
        public virtual ICollection<LentePreco> Precos { get; set; } = new List<LentePreco>();
    }
}
