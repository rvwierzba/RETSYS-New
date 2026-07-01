using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RETSYS.Domain.Entities
{
    [Table("lentes_tabela_precos")]
    public class LentePreco
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("lente_id")]
        public Guid LenteId { get; set; }

        [ForeignKey("LenteId")]
        public virtual Lente Lente { get; set; } = null!;

        [Required]
        [Column("tipo")]
        public string Tipo { get; set; } = "MONOFOCAL"; // MONOFOCAL, BIFOCAL, PROGRESSIVA, OCUPACIONAL

        [Required]
        [Column("indice_refracao", TypeName = "decimal(4,2)")]
        public decimal IndiceRefracao { get; set; } // Ex: 1.56, 1.67, 1.74

        [Column("tratamento_id")]
        public Guid? TratamentoId { get; set; }

        [ForeignKey("TratamentoId")]
        public virtual LenteTratamento? Tratamento { get; set; }

        [Required]
        [Column("preco_custo", TypeName = "decimal(10,2)")]
        public decimal PrecoCusto { get; set; } // Visível apenas para ADMIN

        [Required]
        [Column("preco_venda", TypeName = "decimal(10,2)")]
        public decimal PrecoVenda { get; set; }

        [Required]
        [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }
}