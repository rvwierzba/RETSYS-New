using System;

namespace RETSYS.Domain.Entities
{
    public class ItemVendaBalcao
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VendaBalcaoId { get; set; }
        public VendaBalcao VendaBalcao { get; set; } = null!;

        public string TipoItem { get; set; } = "ARMACAO"; // ARMACAO, SOLAR, LENTE_CONTATO, ACESSORIO, LIMPEZA, OUTRO

        public Guid? ArmacaoId { get; set; }
        public Armacao? Armacao { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; } = 1;
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => Quantidade * PrecoUnitario;
    }
}

