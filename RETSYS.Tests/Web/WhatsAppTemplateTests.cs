using Xunit;

namespace RETSYS.Tests.Web
{
    public class WhatsAppTemplateTests
    {
        [Fact]
        public void DeveSubstituirVariaveisDinamicasDoTemplateCorretamente()
        {
            string template = "Olá {cliente}! Seus óculos da OS *#{numero_os}* na *{otica}* estão prontos para retirada!";

            string cliente = "Maria";
            string numeroOs = "OS-00892";
            string otica = "Ótica Retina Matriz";

            string resultado = template
                .Replace("{cliente}", cliente)
                .Replace("{numero_os}", numeroOs)
                .Replace("{otica}", otica);

            Assert.Equal("Olá Maria! Seus óculos da OS *#OS-00892* na *Ótica Retina Matriz* estão prontos para retirada!", resultado);
        }

        [Fact]
        public void DeveGerarUrlApiWhatsAppCorretamenteFormatada()
        {
            string telefone = "(11) 98765-4321";
            string limpo = new string(telefone.Where(char.IsDigit).ToArray());
            string mensagem = "Olá João! Seus óculos estão prontos.";

            string url = $"https://api.whatsapp.com/send?phone=55{limpo}&text={Uri.EscapeDataString(mensagem)}";

            Assert.StartsWith("https://api.whatsapp.com/send?phone=5511987654321&text=", url);
            Assert.Contains(Uri.EscapeDataString(mensagem), url);
        }
    }
}
