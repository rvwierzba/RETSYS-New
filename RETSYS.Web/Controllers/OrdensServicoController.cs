using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RETSYS.Web.Controllers
{
    public class OrdensServicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdensServicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de todas as OSs Clínicas (GET)
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index()
        {
            var ordens = await _context.OrdensServico
                .Include(os => os.Cliente)
                .OrderByDescending(os => os.DataVenda)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataVenda,
                    os.Medico,
                    os.TipoLente,
                    os.ValorTotal,
                    os.Status, // Sincronizado para alimentar os badges coloridos na listagem do Vue
                    ClienteNome = os.Cliente.Nome,
                    // Enviamos os dados de refração compactados para o Vue poder exibir num modal de detalhes se precisar
                    Refracao = new
                    {
                        os.EsfericoLongeDireito,
                        os.EsfericoLongeEsquerdo,
                        os.CilindricoLongeDireito,
                        os.CilindricoLongeEsquerdo,
                        os.EixoLongeDireito,
                        os.EixoLongeEsquerdo,
                        os.EsfericoPertoDireito,
                        os.EsfericoPertoEsquerdo,
                        os.CilindricoPertoDireito,
                        os.CilindricoPertoEsquerdo,
                        os.EixoPertoDireito,
                        os.EixoPertoEsquerdo,
                        os.Adicao
                    }
                })
                .ToListAsync();

            return Inertia.Render("Orders/Index", new { Ordens = ordens });
        }

        // 2. Abre a tela de cadastro de uma nova OS (GET)
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            // Busca dados simples dos clientes e vendedores ativos para alimentar os seletores no Vue
            var clientes = await _context.Clientes
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome, c.CPF })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            return Inertia.Render("Orders/Create", new { 
                Clientes = clientes, 
                Vendedores = vendedores 
            });
        }

        // 3. Processa a gravação da OS e gera o parcelamento (POST)
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] OrdemServico novaOS, [FromQuery] int quantidadeParcelas)
        {
            // Executa a nossa Regra de Negócio Ótica automatizada no Domínio
            novaOS.CalcularGrauDePerto();

            // Sorteia ou gera um número incremental de OS para o MVP
            novaOS.NumeroOS = "OS-" + DateTime.UtcNow.Ticks.ToString().Substring(10);
            novaOS.DataVenda = DateTime.UtcNow;
            novaOS.Status = "Aberta"; // Define o ciclo de vida inicial padrão da ordem

            // Gera as parcelas de pagamento automaticamente no servidor baseado no Valor Total
            int parcelas = quantidadeParcelas < 1 ? 1 : quantidadeParcelas;
            decimal valorParcela = Math.Round(novaOS.ValorTotal / parcelas, 2);

            for (int i = 1; i <= parcelas; i++)
            {
                novaOS.Parcelas.Add(new ParcelaPagamento
                {
                    Id = Guid.NewGuid(),
                    NumeroParcela = i,
                    DescricaoParcela = $"PARC. {i}/{parcelas} - REF a OS: {novaOS.NumeroOS}",
                    Valor = i == parcelas ? (novaOS.ValorTotal - (valorParcela * (parcelas - 1))) : valorParcela, // Ajusta diferença de centavos na última
                    DataVencimento = DateTime.UtcNow.AddMonths(i)
                });
            }

            _context.OrdensServico.Add(novaOS);
            await _context.SaveChangesAsync();

            // Redireciona o usuário para o painel gerencial após o sucesso
            return RedirectToRoute(new { controller = "Dashboard", action = "Index" });
        }    

        // 4. Modifica o ciclo de vida / status de produção da OS (POST)
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] string novoStatus)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null)
            {
                return NotFound();
            }

            // Lista de status homologados para o ecossistema da ótica
            var statusValidos = new[] { "Aberta", "No Laboratório", "Pronto", "Entregue" };
            if (statusValidos.Contains(novoStatus))
            {
                ordem.Status = novoStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. Motor de Visão Computacional Real via Ollama + Moondream (POST)
    [HttpPost("/ordens/processar-receita-ia")]
    public async Task<IActionResult> ProcessarReceitaIA(IFormFile imagemReceita)
    {
        if (imagemReceita == null || imagemReceita.Length == 0)
        {
            return BadRequest(new { mensagem = "Nenhum arquivo de imagem foi anexado." });
        }

        try
        {
            // 1. Converte o arquivo de imagem recebido do Vue para a string Base64 exigida pelo Ollama
            using var ms = new MemoryStream();
            await imagemReceita.CopyToAsync(ms);
            byte[] arrBytes = ms.ToArray();
            string base64Imagem = Convert.ToBase64String(arrBytes);

            // 2. Monta o payload estruturado forçando o Moondream a devolver um JSON puro
            var payloadOllama = new
            {
                model = "moondream",
                prompt = "Analyze this optical prescription image. Extract the clinical values into a single JSON object using exactly these keys: " +
                        "medico (string), tipoLente (string), esfericoLongeDireito (number), esfericoLongeEsquerdo (number), " +
                        "cilindricoLongeDireito (number), cilindricoLongeEsquerdo (number), eixoLongeDireito (number), " +
                        "eixoLongeEsquerdo (number), adicao (number). If any value is missing or not mentioned, return 0 or null. " +
                        "Output ONLY the raw JSON object, do not include any markdown backticks, explanations or conversational text.",
                images = new[] { base64Imagem },
                stream = false, // Desativa o stream para receber a resposta inteira de uma vez
                format = "json"  // Força o motor do Ollama a garantir uma saída estruturada em JSON válido
            };

            string jsonPayload = JsonSerializer.Serialize(payloadOllama);
            using var conteudoHttp = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 3. Dispara a requisição interna para dentro da rede isolada do Docker
            using var httpClient = new HttpClient();
            
            // Timeout estendido (60s) porque inferência de visão computacional local pode levar alguns segundos dependendo do hardware
            httpClient.Timeout = TimeSpan.FromSeconds(60); 

            // NOTA: "ollama" é o nome padrão do serviço do contêiner dentro da rede do seu docker-compose
            string urlOllama = "http://ollama:11434/api/generate"; 
            
            var respostaOllama = await httpClient.PostAsync(urlOllama, conteudoHttp);
            
            if (!respostaOllama.IsSuccessStatusCode)
            {
                return StatusCode((int)respostaOllama.StatusCode, new { mensagem = "O motor Ollama recusou a requisição ou está sobrecarregado." });
            }

            // 4. Captura a resposta do Ollama e extrai o texto gerado pela IA
            string jsonRespostaStr = await respostaOllama.Content.ReadAsStringAsync();
            using var documentoJson = JsonDocument.Parse(jsonRespostaStr);
            
            // O Ollama envelopa o resultado dentro da propriedade textual chamada "response"
            if (documentoJson.RootElement.TryGetProperty("response", out var elementoResposta))
            {
                string jsonExtraidoDaIa = elementoResposta.GetString();
                
                // Retorna o JSON clínico cru gerado pelo Moondream direto para o Vue ler de forma reativa
                return Content(jsonExtraidoDaIa, "application/json");
            }

            return BadRequest(new { mensagem = "Não foi possível ler os dados textuais da resposta da IA." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new { mensagem = "O motor local de IA estourou o tempo limite de processamento (Timeout)." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Falha crítica no pipeline interno de inteligência artificial.", erro = ex.Message });
        }

    } 
  }
}