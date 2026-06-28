using System;

namespace RETSYS.Domain.Entities
{
    public class OsReceita
    {
        public Guid OsId { get; set; } // FK e PK compartilhada (Garante o 1:1)
        public OrdemServico OrdemServico { get; set; } = null!;
        
        // Olho Direito (OD) - Longe
        public decimal OdEsferico { get; set; }
        public decimal OdCilindrico { get; set; }
        public int OdEixo { get; set; }
        
        // Olho Esquerdo (OE) - Longe
        public decimal OeEsferico { get; set; }
        public decimal OeCilindrico { get; set; }
        public int OeEixo { get; set; }
        
        public decimal? Adicao { get; set; } // Obrigatório para lentes progressivas / multifocais
        
        // Medidas de Centragem Técnica
        public decimal DnpOd { get; set; }
        public decimal DnpOe { get; set; }
        public decimal? AlturaMontagem { get; set; }
        
        public string? ObsReceita { get; set; }

        // =======================================================
        // 🔥 PROPRIEDADES COMPUTADAS (REGRAS DE NEGÓCIO ÓPTICAS)
        // =======================================================

        // Calcula automaticamente o Esférico de Perto do Olho Direito
        public decimal OdEsfericoPerto => OdEsferico + (Adicao ?? 0);

        // Calcula automaticamente o Esférico de Perto do Olho Esquerdo
        public decimal OeEsfericoPerto => OeEsferico + (Adicao ?? 0);

        // O Cilíndrico e o Eixo não sofrem alteração na transição para perto
        public decimal OdCilindricoPerto => OdCilindrico;
        public decimal OeCilindricoPerto => OeCilindrico;
        public int OdEixoPerto => OdEixo;
        public int OeEixoPerto => OeEixo;
    }
}