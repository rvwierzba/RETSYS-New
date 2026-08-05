using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RETSYS.Domain.Interfaces;

namespace RETSYS.Infrastructure.Services;

public class ServicoOllama : IServicoIa
{
    private readonly HttpClient _httpClient;

    public ServicoOllama(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://ollama:11434/");
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<ResultadoLeituraReceitaDto?> ProcessarFotoReceitaAsync(Stream imagemStream)
    {
        using var memoryStream = new MemoryStream();
        await imagemStream.CopyToAsync(memoryStream);
        var bytesImagem = memoryStream.ToArray();
        var base64Imagem = Convert.ToBase64String(bytesImagem);

        var prompt = @"Analise esta imagem de receita médica óptica e extraia estritamente os valores em JSON com esta estrutura exata (use null se não encontrar o valor):
{
  ""esfericoLongeDireito"": number,
  ""cilindricoLongeDireito"": number,
  ""eixoLongeDireito"": number,
  ""esfericoLongeEsquerdo"": number,
  ""cilindricoLongeEsquerdo"": number,
  ""eixoLongeEsquerdo"": number,
  ""adicao"": number,
  ""medico"": string
}
Responda APENAS o objeto JSON puro, sem textos adicionais, explicações ou marcadores de codigo markdown.";

        var payload = new
        {
            model = "moondream",
            prompt = prompt,
            images = new[] { base64Imagem },
            stream = false
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("api/generate", content);
            if (!response.IsSuccessStatusCode) return null;

            // Ajustado aqui: response.Content.ReadAsStringAsync()
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var responseText = doc.RootElement.GetProperty("response").GetString();

            if (string.IsNullOrWhiteSpace(responseText)) return null;

            var jsonLimpo = responseText.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            return JsonSerializer.Deserialize<ResultadoLeituraReceitaDto>(jsonLimpo, options);
        }
        catch
        {
            return null;
        }
    }
}