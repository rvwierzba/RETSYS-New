using Microsoft.AspNetCore.Mvc;
using InertiaCore;
using RETSYS.Domain;
using System.Threading.Tasks;

namespace RETSYS.Web.Controllers
{
    public class ConfiguracoesController : Controller
    {
        // Simulador de persistência em memória para manter os dados no balcão do MVP
        private static string _nomeLoja = "Ótica RETSYS - Matriz";
        private static string _cnpj = "00.000.000/0001-00";
        private static string _pixApiKey = ""; 

        [HttpGet("/configuracoes")]
        public IActionResult Index()
        {
            return Inertia.Render("Configuracoes/Index", new
            {
                NomeLoja = _nomeLoja,
                Cnpj = _cnpj,
                PixApiKey = _pixApiKey
            });
        }

        [HttpPost("/configuracoes")]
        public IActionResult Salvar([FromBody] DtoConfigSalvar dados)
        {
            if (!string.IsNullOrWhiteSpace(dados.NomeLoja)) _nomeLoja = dados.NomeLoja;
            if (!string.IsNullOrWhiteSpace(dados.Cnpj)) _cnpj = dados.Cnpj;
            
            // Atualiza a chave global que o módulo do caixa usa para saber se o PIX está ativo
            _pixApiKey = dados.PixApiKey ?? "";

            // Compartilha dinamicamente com o pipeline do Inertia se a OpenPix está ativa nesta sessão
            Inertia.Share("PixHabilitadoNestaLoja", !string.IsNullOrEmpty(_pixApiKey));

            return RedirectToAction(nameof(Index));
        }
    }

    // Record auxiliar estruturado para o transporte de dados corporativos
    public record DtoConfigSalvar(string NomeLoja, string Cnpj, string? PixApiKey);
}