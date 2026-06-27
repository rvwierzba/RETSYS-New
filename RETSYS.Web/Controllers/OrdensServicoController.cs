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
using System.Security.Claims;
using System.Linq;

namespace RETSYS.Web.Controllers
{
    public class OrdensServicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdensServicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de todas as OSs com Isolamento por Perfil (RBAC)
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            IQueryable<OrdemServico> query = _context.OrdensServico.Include(os => os.Cliente);

            // 🛡️ Ativado: O Vendedor só acessa as próprias OSs e clientes cadastrados por ele
            if (perfilClaim == "VENDEDOR" && Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                query = query.Where(os => os.VendedorId == vendedorId);
            }

            var ordens = await query
                .OrderByDescending(os => os.DataVenda)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataVenda,
                    os.Medico,
                    os.TipoLente,
                    os.ValorTotal,
                    os.Status, 
                    ClienteNome = os.Cliente.Nome,
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
            var clientes = await _context.Clientes
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome, c.CPF })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            
            var armacoes = await _context.Armacoes
                .Where(a => a.Ativo && a.QuantidadeEstoque > 0)
                .Select(a => new { a.Id, a.ModeloReferencia, a.Cor, a.QuantidadeEstoque, a.PrecoVenda })
                .ToListAsync();

            var lentes = await _context.Lentes
                .Where(l => l.Ativo)
                .Select(l => new { l.Id, l.Laboratorio, l.Tipo, l.Tratamento, l.PrecoVenda })
                .ToListAsync();

            return Inertia.Render("Orders/Create", new { 
                Clientes = clientes, 
                Vendedores = vendedores,
                Armacoes = armacoes, 
                Lentes = lentes     
            });
        }

        // 3. Processa a gravação da OS com validações financeiras completas e Motor de Comissão (POST)
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] OrdemServico novaOS, [FromQuery] int quantidadeParcelas)
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            // Busca as configurações do vendedor para validar o teto de desconto autorizado
            var vendedor = await _context.Usuarios.FindAsync(novaOS.VendedorId);
            if (vendedor == null)
            {
                Inertia.Share("erro", "Vendedor responsável não localizado.");
                return RedirectToAction(nameof(Criar));
            }

            // 🔄 Ativado: Cálculo e validação automática do limite de desconto do perfil
            decimal totalBruto = novaOS.ValorTotalBruto > 0 ? novaOS.ValorTotalBruto : 1;
            novaOS.DescontoPercentual = (novaOS.DescontoReais / totalBruto) * 100;

            if (perfilClaim != "ADMIN" && novaOS.DescontoPercentual > vendedor.LimiteDesconto)
            {
                Inertia.Share("erro", "Desconto acima do limite autorizado. Solicite aprovação do administrador.");
                return RedirectToAction(nameof(Criar));
            }

            // Numeração sequencial automática por ano (OS-AAAA-NNNNN)
            int anoAtual = DateTime.UtcNow.Year;
            int sequencialAno = await _context.OrdensServico.Where(os => os.DataVenda.Year == anoAtual).CountAsync() + 1;
            novaOS.NumeroOS = $"OS-{anoAtual}-{sequencialAno.ToString().PadLeft(5, '0')}";

            novaOS.DataVenda = DateTime.UtcNow;
            novaOS.Status = "EM_ABERTO"; 
            novaOS.CalcularGrauDePerto();

            // Gerador de parcelas baseado no modelo financeiro
            int parcelas = quantidadeParcelas < 1 ? 1 : quantidadeParcelas;
            decimal valorParcela = Math.Round(novaOS.ValorTotal / parcelas, 2);

            for (int i = 1; i <= parcelas; i++)
            {
                novaOS.Parcelas.Add(new ParcelaPagamento
                {
                    Id = Guid.NewGuid(),
                    NumeroParcela = i,
                    DescricaoParcela = $"PARC. {i}/{parcelas} - REF a OS: {novaOS.NumeroOS}",
                    Valor = i == parcelas ? (novaOS.ValorTotal - (valorParcela * (parcelas - 1))) : valorParcela,
                    DataVencimento = DateTime.UtcNow.AddMonths(i)
                });
            }

            // =======================================================
            // 🔥 MOTOR DE COMISSÕES INTEGRADO (MÓDULO 5)
            // =======================================================
            
            // Busca a taxa global ativa configurada no sistema. Fallback padrão: 5%
            var configComissao = await _context.ConfiguracoesComissao.FirstOrDefaultAsync(cc => cc.Ativo);
            decimal percentualTaxa = configComissao?.PercentualComissao ?? 5.00m;

            // A comissão só é instanciada se o vendedor puder pontuar comissões ativas
            if (vendedor.ComissaoAtiva)
            {
                decimal valorComissaoCalculado = Math.Round(novaOS.ValorTotalBruto * (percentualTaxa / 100), 2);

                var novaComissao = new Comissao
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = novaOS.Id,
                    VendedorId = vendedor.Id, 
                    ValorBase = novaOS.ValorTotalBruto,
                    PercentualAplicado = percentualTaxa,
                    ValorComissao = valorComissaoCalculado,
                    Status = "PENDENTE",
                    DataGeracao = DateTime.UtcNow,
                    PeriodoReferencia = DateTime.UtcNow.ToString("yyyy-MM"), // Agrupador para fechamentos mensais
                    Observacoes = $"Comissão automática gerada na emissão da {novaOS.NumeroOS}"
                };

                _context.Comissoes.Add(novaComissao);
            }

            // Salva todo o bloco atômico (OS, parcelas e snapshot da comissão)
            _context.OrdensServico.Add(novaOS);
            await _context.SaveChangesAsync();

            return RedirectToRoute(new { controller = "Dashboard", action = "Index" });
        }    

        // 4. Modifica o status de produção e executa a baixa/reposição física do estoque (POST)
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] string novoStatus)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();

            string statusAnterior = ordem.Status;
            var statusValidos = new[] { "EM_ABERTO", "EM_LABORATORIO", "PRONTO", "ENTREGUE", "CANCELADO" };

            if (!statusValidos.Contains(novoStatus) || statusAnterior == novoStatus)
            {
                return RedirectToAction(nameof(Index));
            }

            // 📦 Ativado: Controle inteligente do fluxo físico de mercadorias do estoque
            bool estadoAtualAbateEstoque = (novoStatus == "EM_LABORATORIO" || novoStatus == "ENTREGUE");
            bool estadoAnteriorJáHaviaAbatido = (statusAnterior == "EM_LABORATORIO" || statusAnterior == "ENTREGUE");

            // Cenário A: OS entrou em produção ou saiu para entrega -> Remove 1 unidade do inventário
            if (estadoAtualAbateEstoque && !estadoAnteriorJáHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - 1);

                var lente = await _context.Lentes.FindAsync(ordem.LenteId);
                if (lente != null) lente.QuantidadeEstoque = Math.Max(0, lente.QuantidadeEstoque - 1);
            }
            // Cenário B: OS cancelada vindo de um estado que já tinha dado baixa -> Repõe os produtos
            else if (novoStatus == "CANCELADO" && estadoAnteriorJáHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque++;

                var lente = await _context.Lentes.FindAsync(ordem.LenteId);
                if (lente != null) lente.QuantidadeEstoque++;
            }

            ordem.Status = novoStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 5. Motor de Visão Computacional via Ollama + Moondream (POST)
        [HttpPost("/ordens/processar-receita-ia")]
        public async Task<IActionResult> ProcessarReceitaIA(IFormFile imagemReceita)
        {
            if (imagemReceita == null || imagemReceita.Length == 0)
            {
                return BadRequest(new { mensagem = "Nenhum arquivo de imagem foi anexado." });
            }

            try
            {
                using var ms = new MemoryStream();
                await imagemReceita.CopyToAsync(ms);
                byte[] arrBytes = ms.ToArray();
                string base64Imagem = Convert.ToBase64String(arrBytes);

                var payloadOllama = new
                {
                    model = "moondream",
                    prompt = "Analyze this optical prescription image. Extract the clinical values into a single JSON object using exactly these keys: " +
                            "medico (string), tipoLente (string), esfericoLongeDireito (number), esfericoLongeEsquerdo (number), " +
                            "cilindricoLongeDireito (number), cilindricoLongeEsquerdo (number), eixoLongeDireito (number), " +
                            "eixoLongeEsquerdo (number), adicao (number). If any value is missing or not mentioned, return 0 or null. " +
                            "Output ONLY the raw JSON object, do not include any markdown backticks, explanations or conversational text.",
                    images = new[] { base64Imagem },
                    stream = false,
                    format = "json"
                };

                string jsonPayload = JsonSerializer.Serialize(payloadOllama);
                using var conteudoHttp = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60); 

                string urlOllama = "http://ollama:11434/api/generate"; 
                var respostaOllama = await httpClient.PostAsync(urlOllama, conteudoHttp);
                
                if (!respostaOllama.IsSuccessStatusCode)
                {
                    return StatusCode((int)respostaOllama.StatusCode, new { mensagem = "O motor Ollama recusou a requisição ou está sobrecarregado." });
                }

                string jsonString = await respostaOllama.Content.ReadAsStringAsync();
                using var documentoJson = JsonDocument.Parse(jsonString);
                
                if (documentoJson.RootElement.TryGetProperty("response", out var elementoResposta))
                {
                    string jsonExtraidoDaIa = elementoResposta.GetString();
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