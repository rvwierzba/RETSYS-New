using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Marca
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OticaId { get; set; }
        public Otica? Otica { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<Armacao> Armacoes { get; set; } = new List<Armacao>();
    }
}
