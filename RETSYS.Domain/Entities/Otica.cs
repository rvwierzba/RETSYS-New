using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Otica
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
