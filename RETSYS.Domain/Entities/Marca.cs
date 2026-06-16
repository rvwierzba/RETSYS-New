using System;
using System.Collections.Generic;

namespace RETSYS.Domain.Entities
{
    public class Marca
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Nome { get; set; } = string.Empty;
        
        public string Descricao { get; set; } = string.Empty;
        
        public bool Ativo { get; set; } = true;
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Relacionamento 1-para-Muitos: Uma Marca possui várias Armações
        public ICollection<Armacao> Armacoes { get; set; } = new List<Armacao>();
    }
}