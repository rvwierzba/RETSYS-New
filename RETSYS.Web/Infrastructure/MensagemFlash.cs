using InertiaCore;
using Microsoft.AspNetCore.Http;

namespace RETSYS.Web.Infrastructure
{
    /// <summary>
    /// Mensagens de retorno ("erro" / "sucesso") exibidas pelo toast do layout.
    /// O Inertia.Share vale só para a requisição atual; quando a action redireciona, o middleware
    /// em Program.cs guarda a mensagem na sessão e a devolve como prop na próxima página.
    /// </summary>
    public static class MensagemFlash
    {
        public const string PrefixoItem = "MensagemFlash:";

        public static void Definir(HttpContext context, string chave, string? texto)
        {
            Inertia.Share(chave, texto);
            context.Items[PrefixoItem + chave] = texto;
        }
    }
}
