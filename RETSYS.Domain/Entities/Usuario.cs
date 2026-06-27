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
        
        // Percentual máximo de desconto que a vendedora pode conceder
        public decimal LimiteDesconto { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        // Meta de vendas brutas do mês para análise de dashboard
        public decimal MetaMensal { get; set; }
        
        // Permite desativar comissão por vendedora mantendo o % global ativo
        public bool ComissaoAtiva { get; set; } = true;
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}