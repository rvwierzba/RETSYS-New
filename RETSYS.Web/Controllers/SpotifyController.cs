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

        // Credenciais do Spotify obtidas no Dashboard do Spotify Developer
        // Podem ser fixadas aqui ou mapeadas no seu appsettings.json
        private const string ClientId = "SEU_SPOTIFY_CLIENT_ID";
        private const string ClientSecret = "SEU_SPOTIFY_CLIENT_SECRET";
        private const string RedirectUri = "https://localhost:5001/api/spotify/callback"; // Ajuste a porta local se necessário

        public SpotifyController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        #region 1. FLUXO OAUTH2 (AUTENTICAÇÃO)

        // GET /api/spotify/login -> Redireciona o Admin para o consentimento do Spotify
        [HttpGet("login")]
        public IActionResult Login()
        {
            var escopos = "user-read-playback-state user-modify-playback-state user-read-currently-playing";
            var urlAutenticacao = $"https://accounts.spotify.com/authorize?client_id={ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(RedirectUri)}&scope={Uri.EscapeDataString(escopos)}&show_dialog=true";
            
            return Redirect(urlAutenticacao);
        }

        // GET /api/spotify/callback -> Recebe o código do Spotify e troca pelo Token de Acesso
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Código de autorização ausente.");

            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                
                var corpoParametros = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", RedirectUri },
                    { "client_id", ClientId },
                    { "client_secret", ClientSecret }
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
                    
                    // Retorna para a tela de configurações com sucesso
                    return Redirect("/configuracoes");
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

        // GET /api/spotify/status-atual -> Retorna o que está tocando no ambiente
        [HttpGet("status-atual")]
        public async Task<IActionResult> ObterStatusAtual()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            
            // Fallback Silencioso: Se não houver login, devolve uma estrutura limpa sem estourar 401
            var estadoVazio = new { Titulo = "", Artista = "", CapaUrl = "", Tocando = false };
            if (string.IsNullOrEmpty(token)) return Ok(estadoVazio);

            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/currently-playing");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request);

                // HTTP 204 significa sucesso, mas nenhuma música está tocando no aplicativo do Spotify
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return Ok(estadoVazio);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    // Varre a árvore do JSON do Spotify de forma defensiva contra campos nulos
                    if (root.TryGetProperty("item", out var itemNode) && itemNode.ValueKind != JsonValueKind.Null)
                    {
                        var titulo = itemNode.GetProperty("name").GetString();
                        var tocando = root.GetProperty("is_playing").GetBoolean();
                        
                        // Captura o primeiro artista da lista
                        var artista = itemNode.GetProperty("artists")[0].GetProperty("name").GetString();
                        
                        // Captura a imagem do álbum de tamanho médio (índice 1 da lista)
                        var albumNode = itemNode.GetProperty("album");
                        var capaUrl = albumNode.GetProperty("images")[1].GetProperty("url").GetString();

                        return Ok(new { Titulo = titulo, Artista = artista, CapaUrl = capaUrl, Tocando = tocando });
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expirou. Limpa a sessão para convidar o admin a reconectar no painel
                    HttpContext.Session.Remove("SpotifyToken");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Spotify Proxy Status Error]: {ex.Message}");
            }

            return Ok(estadoVazio);
        }

        // POST /api/spotify/controlar?comando=... -> Repassa as ações do player do balcão
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