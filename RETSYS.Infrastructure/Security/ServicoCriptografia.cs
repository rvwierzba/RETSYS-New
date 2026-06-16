using BCrypt.Net;
using RETSYS.Domain.Interfaces;

namespace RETSYS.Infrastructure.Security
{
    public class ServicoCriptografia : IServicoCriptografia
    {
        // Fator de custo 12 é o equilíbrio perfeito entre segurança extrema e performance em 2026
        private const int FatorCusto = 12;

        public string CriptografarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(senha, FatorCusto);
        }

        public bool VerificarSenha(string senha, string senhaHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(senha, senhaHash);
        }
    }
}