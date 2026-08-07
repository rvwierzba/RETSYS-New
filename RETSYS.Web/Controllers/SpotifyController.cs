using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RETSYS.Web.Controllers
{
    [ApiController]
    [Route("api/spotify")]
    public class SpotifyController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public SpotifyController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        // -------------------------------------------------------------------------
        // MÉTODOS AUXILIARES: Leitura do appsettings.json e Variáveis de Ambiente
        // -------------------------------------------------------------------------

        private string GetClientId()
        {
            var clientId = _configuration["Spotify:ClientId"] 
                ?? _configuration["SPOTIFY_CLIENT_ID"] 
                ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");

            return clientId?.Trim() ?? string.Empty;
        }

        private string GetClientSecret()
        {
            var clientSecret = _configuration["Spotify:ClientSecret"] 
                ?? _configuration["SPOTIFY_CLIENT_SECRET"] 
                ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

            return clientSecret?.Trim() ?? string.Empty;
        }

        private string GetRedirectUri()
        {
            var uriConfig = _configuration["Spotify:RedirectUri"] 
                ?? _configuration["SPOTIFY_REDIRECT_URI"] 
                ?? Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");

            if (!string.IsNullOrWhiteSpace(uriConfig))
            {
                return uriConfig.Trim();
            }

            // Fallback dinâmico adaptado ao protocolo e domínio da requisição em execução
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            return $"{scheme}://{Request.Host}/api/spotify/callback";
        }

        #region 1. FLUXO OAUTH2 (AUTENTICAÇÃO)

        // GET /api/spotify/login -> Redireciona para o login de autorização do Spotify
        [HttpGet("login")]
        public IActionResult Login()
        {
            var clientId = GetClientId();
            var redirectUri = GetRedirectUri();

            if (string.IsNullOrWhiteSpace(clientId))
            {
                Console.WriteLine("[Spotify Error]: ClientId não localizado no appsettings.json ('Spotify:ClientId').");
                return Redirect("/configuracoes?erro=spotify_missing_credentials");
            }

            var escopos = "user-read-playback-state user-modify-playback-state user-read-currently-playing";
            var urlAutenticacao = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(escopos)}&show_dialog=true";
            
            return Redirect(urlAutenticacao);
        }

        // GET /api/spotify/callback -> Troca o código temporário pelo Token de Acesso
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Código de autorização ausente.");

            var clientId = GetClientId();
            var clientSecret = GetClientSecret();
            var redirectUri = GetRedirectUri();

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                Console.WriteLine("[Spotify Error]: Credenciais do Spotify incompletas no appsettings.json.");
                return Redirect("/configuracoes?erro=spotify_missing_credentials");
            }

            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                
                var corpoParametros = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                };

                request.Content = new FormUrlEncodedContent(corpoParametros);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var documento = JsonDocument.Parse(jsonString);
                    var accessToken = documento.RootElement.GetProperty("access_token").GetString();

                    // Grava o token com segurança na Sessão Criptografada do servidor
                    HttpContext.Session.SetString("SpotifyToken", accessToken ?? "");
                    
                    return Redirect("/configuracoes");
                }
                else
                {
                    var erroDetalhes = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Spotify Token Exchange Failed]: {response.StatusCode} - {erroDetalhes}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Spotify OAuth Error]: {ex.Message}");
            }

            return Redirect("/configuracoes?erro=spotify_auth_failed");
        }

        #endregion

        #region 2. PONTE DE MÍDIA (STATUS E CONTROLES)

        // GET /api/spotify/status-atual -> Retorna o que está tocando no momento
        [HttpGet("status-atual")]
        public async Task<IActionResult> ObterStatusAtual()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            
            var estadoVazio = new { Titulo = "", Artista = "", CapaUrl = "", Tocando = false };
            if (string.IsNullOrEmpty(token)) return Ok(estadoVazio);

            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/currently-playing");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return Ok(estadoVazio);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("item", out var itemNode) && itemNode.ValueKind != JsonValueKind.Null)
                    {
                        var titulo = itemNode.GetProperty("name").GetString();
                        var tocando = root.GetProperty("is_playing").GetBoolean();
                        
                        var artista = itemNode.GetProperty("artists")[0].GetProperty("name").GetString();
                        
                        var albumNode = itemNode.GetProperty("album");
                        var capaUrl = albumNode.GetProperty("images")[1].GetProperty("url").GetString();

                        return Ok(new { Titulo = titulo, Artista = artista, CapaUrl = capaUrl, Tocando = tocando });
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("SpotifyToken");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Spotify Proxy Status Error]: {ex.Message}");
            }

            return Ok(estadoVazio);
        }

        // POST /api/spotify/controlar?comando=... -> Executa comandos do player no balcão
        [HttpPost("controlar")]
        public async Task<IActionResult> ExecutarComando([FromQuery] string comando)
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            try
            {
                var client = _clientFactory.CreateClient();
                string urlEndpoint = comando switch
                {
                    "tocar" => "https://api.spotify.com/v1/me/player/play",
                    "pausar" => "https://api.spotify.com/v1/me/player/pause",
                    "proxima" => "https://api.spotify.com/v1/me/player/next",
                    "anterior" => "https://api.spotify.com/v1/me/player/previous",
                    _ => ""
                };

                if (string.IsNullOrEmpty(urlEndpoint)) return BadRequest("Comando de mídia inválido.");

                var metodo = (comando == "tocar" || comando == "pausar") ? HttpMethod.Put : HttpMethod.Post;
                var request = new HttpRequestMessage(metodo, urlEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Spotify Command Error]: {ex.Message}");
            }

            return StatusCode(500);
        }

        #endregion
    }
}