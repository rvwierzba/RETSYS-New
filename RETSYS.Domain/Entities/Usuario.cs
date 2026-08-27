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

        // Percentual máximo de desconto que a vendedora pode conceder.
        // Toda nova vendedora inicia limitada a 5,00%.
        // Administradores e gerentes podem conceder descontos acima desse limite.
        public decimal LimiteDesconto { get; set; } = 5.00m;

        public bool Ativo { get; set; } = true;

        // Meta de vendas brutas do mês para análise de dashboard
        public decimal MetaMensal { get; set; }

        // Permite desativar comissão por vendedora mantendo o percentual global ativo
        public bool ComissaoAtiva { get; set; } = true;

        // Percentual individual de comissão da vendedora
        public decimal PercentualComissao { get; set; } = 3.00m;

        public DateTime? UltimoAcesso { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
