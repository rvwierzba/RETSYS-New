using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RETSYS.Domain.Entities
{
    [Table("lentes_tratamentos")]
    public class LenteTratamento
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("nome")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty; // Ex: Antirreflexo, Fotossensível, Azul, Espelhado

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Required]
        [Column("acrescimo_valor", TypeName = "decimal(10,2)")]
        public decimal AcrescimoValor { get; set; } = 0.00m; // Acréscimo automático ao preço base

        [Required]
        [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }
}