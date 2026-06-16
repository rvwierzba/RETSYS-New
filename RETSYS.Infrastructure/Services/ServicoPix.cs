using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RETSYS.Domain.Interfaces;

namespace RETSYS.Infrastructure.Services
{
    public class ServicoPix : IServicoPix
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServicoPix> _logger;

        public ServicoPix(HttpClient httpClient, ILogger<ServicoPix> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PixResponseDto?> GerarCobrancaImediataAsync(string apiKeyLoja, string idParcela, decimal valor, string clienteNome, string clienteCpf)
        {
            // 1. Configurar o token dinâmico da ótica no cabeçalho HTTP
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", apiKeyLoja);

            // A OpenPix exige o valor em centavos como um número inteiro (Ex: R$ 50,00 vira 5000)
            long valorCentavos = (long)Math.Round(valor * 100);

            // 2. Montar o JSON idêntico ao manual da API deles
            var payload = new
            {
                correlationID = idParcela, 
                value = valorCentavos,
                comment = "Recebimento de Parcela - RETSYS",
                customer = new
                {
                    name = clienteNome,
                    taxID = clienteCpf.Replace(".", "").Replace("-", "").Trim()
                }
            };

            try
            {
                // 3. Disparar a requisição real para o servidor da OpenPix
                var resposta = await _httpClient.PostAsJsonAsync("https://api.openpix.com.br/v1/charge", payload);

                if (!resposta.IsSuccessStatusCode)
                {
                    var erro = await resposta.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na API OpenPix: {resposta.StatusCode} - {erro}");
                    return null;
                }

                // 4. Mapear a resposta de sucesso
                var dados = await resposta.Content.ReadFromJsonAsync<OpenPixEnvelope>();
                
                if (dados?.Charge == null) return null;

                return new PixResponseDto(
                    PixCopiaECola: dados.Charge.BrCode,
                    QrCodeImagemUrl: dados.Charge.QrCodeImage,
                    ChargeId: dados.Charge.CorrelationID
                );
            }
            catch (Exception ex)
            {
                // Se a internet da ótica cair ou a API oscilar, o sistema registra o erro mas não trava o caixa
                _logger.LogCritical($"Falha de conexão com o gateway de PIX: {ex.Message}");
                return null;
            }
        }
    }

    // Classes auxiliares para o C# conseguir ler o JSON da OpenPix
    public class OpenPixEnvelope
    {
        [JsonPropertyName("charge")]
        public OpenPixChargeDetail Charge { get; set; } = new();
    }

    public class OpenPixChargeDetail
    {
        [JsonPropertyName("correlationID")]
        public string CorrelationID { get; set; } = string.Empty;

        [JsonPropertyName("brCode")]
        public string BrCode { get; set; } = string.Empty;

        [JsonPropertyName("qrCodeImage")]
        public string QrCodeImage { get; set; } = string.Empty;
    }
}