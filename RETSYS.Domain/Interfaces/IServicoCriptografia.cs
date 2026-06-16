namespace RETSYS.Domain.Interfaces
{
    public interface IServicoCriptografia
    {
        // Transforma a senha em texto limpo em um hash seguro
        string CriptografarSenha(string senha);

        // Verifica se a senha digitada bate com o hash salvo no banco
        bool VerificarSenha(string senha, string senhaHash);
    }
}