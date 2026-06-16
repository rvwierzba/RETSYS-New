using System.Threading.Tasks;

namespace RETSYS.Domain.Interfaces
{
    public interface IServicoPix
    {
        // Método que recebe a chave da loja e os dados da parcela para gerar o PIX real
        Task<PixResponseDto?> GerarCobrancaImediataAsync(string apiKeyLoja, string idParcela, decimal valor, string clienteNome, string clienteCpf);
    }

    // O "pacotinho" de dados que o C# vai devolver para o Vue 3 renderizar na tela
    public record PixResponseDto(string PixCopiaECola, string QrCodeImagemUrl, string ChargeId);
}