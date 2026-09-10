using System;

namespace RETSYS.Domain.Entities
{
    public class OsAuditoriaLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public Guid OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

        public string CampoAlterado { get; set; } = string.Empty; // Ex: "DataEntrada", "ValorTotalLiquido"

        public string? ValorAntigo { get; set; }

        public string? ValorNovo { get; set; }

        public string Descricao { get; set; } = string.Empty;
    }
}

