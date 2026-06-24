using System;
using RETSYS.Domain.Enums;

namespace RETSYS.Domain.Entities
{
    public class Usuario
    {
        // Chave primária baseada em UUID (Guid)
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Nome { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        // Onde guardaremos a senha criptografada (hashPassword)
        public string SenhaHash { get; set; } = string.Empty;
        
        // Identifica a qual filial o funcionário pertence (ex: "Matriz", "Filial 1")
        public string FilialLoja { get; set; } = string.Empty;
              
        public string? FotoUrl { get; set; }
        
        // Nível de acesso (Vendedor, Gerente, Admin)
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Vendedor;
        
        public bool Ativo { get; set; } = true;
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}