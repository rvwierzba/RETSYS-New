using System;

namespace RETSYS.Domain.Entities
{
    public class ReceitaCliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public DateTime DataExame { get; set; } = DateTime.UtcNow;
        public DateTime? DataValidade { get; set; }

        public string MedicoNome { get; set; } = string.Empty;
        public string? MedicoCrm { get; set; }
        public string MedicoTipo { get; set; } = "OFTALMOLOGISTA"; // OFTALMOLOGISTA, OPTOMETRISTA, CLINICO
        public string? Clinica { get; set; }

        // --- Olho Direito (OD) - Grau de Longe ---
        public decimal OdEsferico { get; set; }
        public decimal OdCilindrico { get; set; }
        public int OdEixo { get; set; }
        public decimal? OdDnp { get; set; }
        public decimal? OdAltura { get; set; }
        public decimal? OdPrisma { get; set; }
        public string? OdBasePrisma { get; set; }

        // --- Olho Esquerdo (OE) - Grau de Longe ---
        public decimal OeEsferico { get; set; }
        public decimal OeCilindrico { get; set; }
        public int OeEixo { get; set; }
        public decimal? OeDnp { get; set; }
        public decimal? OeAltura { get; set; }
        public decimal? OePrisma { get; set; }
        public string? OeBasePrisma { get; set; }

        // --- Adição (Presbiopia / Visão Perto / Multifocal) ---
        public decimal? Adicao { get; set; }

        // --- Medidas Técnicas da Armação / Montagem ---
        public decimal? Aro { get; set; }
        public decimal? Ponte { get; set; }
        public decimal? MaiorDiagonal { get; set; }
        public decimal? AlturaVertical { get; set; }

        public string? Observacoes { get; set; }
        public string? FotoReceitaUrl { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Propriedades Computadas para Perto
        public decimal OdEsfericoPerto => OdEsferico + (Adicao ?? 0);
        public decimal OeEsfericoPerto => OeEsferico + (Adicao ?? 0);

        // Regra de Negócio: Verifica se a receita está vencida (mais de 1 ano do exame)
        public bool EstaVencida => DataValidade.HasValue ? DataValidade.Value < DateTime.UtcNow.Date : DataExame.AddYears(1) < DateTime.UtcNow.Date;

        // Regra de Negócio: Sugere recall / retorno anual (a partir de 330 dias do exame)
        public bool PrecisaRetornoAnual => DataExame.AddDays(330) <= DateTime.UtcNow.Date;
    }
}

