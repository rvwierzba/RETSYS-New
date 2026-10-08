using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Otica
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Auto-relacionamento para Matriz e Filiais
        public Guid? MatrizId { get; set; }
        public Otica? Matriz { get; set; }
        public ICollection<Otica> Filiais { get; set; } = new List<Otica>();

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}

