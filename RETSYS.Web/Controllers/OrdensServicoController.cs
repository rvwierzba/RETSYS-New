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

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro);

            if (perfilClaim == "VENDEDOR" && Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                query = query.Where(os => os.VendedorId == vendedorId);
            }

            var ordens = await query
                .OrderByDescending(os => os.DataEntrada)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataEntrada,
                    os.DataPrevistaEntrega,
                    os.Status,
                    Medico = os.MedicoNome,
                    ClienteNome = os.Cliente.Nome,
                    ValorTotal = os.Financeiro.ValorTotalLiquido,
                    Refracao = new
                    {
                        EsfericoLongeDireito = os.Receita.OdEsferico,
                        CilindricoLongeDireito = os.Receita.OdCilindrico,
                        EixoLongeDireito = os.Receita.OdEixo,
                        EsfericoLongeEsquerdo = os.Receita.OeEsferico,
                        CilindricoLongeEsquerdo = os.Receita.OeCilindrico,
                        EixoLongeEsquerdo = os.Receita.OeEixo,
                        os.Receita.Adicao,
                        // Propriedades computadas em tempo real na Entidade
                        EsfericoPertoDireito = os.Receita.OdEsfericoPerto,
                        EsfericoPertoEsquerdo = os.Receita.OeEsfericoPerto
                    }
                })
                .ToListAsync();

            return Inertia.Render("Orders/Index", new { Ordens = ordens });
        }

        // 2. Abre a tela de cadastro de uma nova OS (GET)
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            var armacoes = await _context.Armacoes
                .Where(a => a.QuantidadeEstoque > 0)
                .Select(a => new { a.Id, a.ModeloReferencia, a.Cor, a.QuantidadeEstoque, a.PrecoVenda })
                .ToListAsync();

            var lentes = await _context.Lentes
                .Select(l => new { l.Id, l.Laboratorio, l.Tipo, l.Tratamento, l.PrecoVenda })
                .ToListAsync();

            return Inertia.Render("Orders/Create", new { 
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes
            });
        }

        // 3. Processa a gravação Unificada da OS + Fluxo Automático de Cliente (POST)
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] JsonElement raiz, [FromQuery] int quantidadeParcelas)
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            Guid vendedorId = Guid.Parse(raiz.GetProperty("vendedorId").GetString()!);
            var vendedor = await _context.Usuarios.FindAsync(vendedorId);
            if (vendedor == null)
            {
                Inertia.Share("erro", "Vendedor responsável não localizado.");
                return RedirectToAction(nameof(Criar));
            }

            // 💳 VALIDAÇÃO DE ALÇADA DE DESCONTO
            decimal totalBruto = raiz.GetProperty("valorTotalBruto").GetDecimal();
            decimal descontoReais = raiz.GetProperty("descontoReais").GetDecimal();
            decimal descontoPercentual = totalBruto > 0 ? (descontoReais / totalBruto) * 100 : 0;

            if (perfilClaim != "ADMIN" && descontoPercentual > vendedor.LimiteDesconto)
            {
                Inertia.Share("erro", "Desconto acima do limite autorizado. Solicite aprovação do administrador.");
                return RedirectToAction(nameof(Criar));
            }

            // 👤 FLUXO DO CLIENTE (CRM AUTOMÁTICO)
            string cpfInformado = raiz.GetProperty("cpf").GetString()!;
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.CPF == cpfInformado);

            if (cliente == null)
            {
                // CPF Não cadastrado: Instancia um novo registro no CRM
                cliente = new Cliente { Id = Guid.NewGuid(), CPF = cpfInformado };
                _context.Clientes.Add(cliente);
            }

            // Mapeia ou atualiza os dados cadastrais com as informações mais recentes do balcão
            cliente.Nome = raiz.GetProperty("nome").GetString()!;
            cliente.Telefone = raiz.GetProperty("telefone").GetString()!;
            cliente.Logradouro = raiz.GetProperty("logradouro").GetString()!;
            cliente.Numero = raiz.GetProperty("numero").GetString()!;
            cliente.Complemento = raiz.TryGetProperty("complemento", out var comp) && comp.ValueKind != JsonValueKind.Null ? comp.GetString() : null;
            cliente.Bairro = raiz.GetProperty("bairro").GetString()!;
            cliente.Cidade = raiz.GetProperty("cidade").GetString()!;
            cliente.Estado = raiz.GetProperty("estado").GetString()!;
            cliente.Cep = raiz.GetProperty("cep").GetString()!;
            cliente.Convenio = raiz.TryGetProperty("convenio", out var conv) && conv.ValueKind != JsonValueKind.Null ? conv.GetString() : null;
            cliente.Email = raiz.TryGetProperty("email", out var mail) && mail.ValueKind != JsonValueKind.Null ? mail.GetString() : null;
            cliente.UpdatedAt = DateTime.UtcNow;

            // 📄 MONTAGEM DO CABEÇALHO DA OS
            var novaOS = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                VendedorId = vendedor.Id,
                DataEntrada = DateTime.UtcNow,
                DataPrevistaEntrega = raiz.GetProperty("dataPrevistaEntrega").GetDateTime(),
                Status = "EM_ABERTO",
                MedicoNome = raiz.GetProperty("medicoNome").GetString(),
                MedicoCrm = raiz.GetProperty("medicoCrm").GetString(),
                Observacoes = raiz.TryGetProperty("observacoes", out var obs) && obs.ValueKind != JsonValueKind.Null ? obs.GetString() : null
            };

            // Numeração Sequencial por Ano
            int anoAtual = DateTime.UtcNow.Year;
            int sequencialAno = await _context.OrdensServico.Where(os => os.DataEntrada.Year == anoAtual).CountAsync() + 1;
            novaOS.NumeroOS = $"OS-{anoAtual}-{sequencialAno.ToString().PadLeft(5, '0')}";

            // 👁️ MONTAGEM DA TABELA SATÉLITE CLÍNICA (os_receita)
            novaOS.Receita = new OsReceita
            {
                OsId = novaOS.Id,
                OdEsferico = raiz.GetProperty("odEsferico").GetDecimal(),
                OdCilindrico = raiz.GetProperty("odCilindrico").GetDecimal(),
                OdEixo = raiz.GetProperty("odEixo").GetInt32(),
                OeEsferico = raiz.GetProperty("oeEsferico").GetDecimal(),
                OeCilindrico = raiz.GetProperty("oeCilindrico").GetDecimal(),
                OeEixo = raiz.GetProperty("oeEixo").GetInt32(),
                Adicao = raiz.TryGetProperty("adicao", out var ad) && ad.ValueKind != JsonValueKind.Null ? ad.GetDecimal() : null,
                DnpOd = raiz.GetProperty("dnpOd").GetDecimal(),
                DnpOe = raiz.GetProperty("dnpOe").GetDecimal(),
                AlturaMontagem = raiz.TryGetProperty("alturaMontagem", out var alt) && alt.ValueKind != JsonValueKind.Null ? alt.GetDecimal() : null,
                ObsReceita = raiz.TryGetProperty("obsReceita", out var obsR) && obsR.ValueKind != JsonValueKind.Null ? obsR.GetString() : null
            };

            // 💳 MONTAGEM DA TABELA SATÉLITE COMERCIAL (os_financeiro)
            novaOS.Financeiro = new OsFinanceiro
            {
                OsId = novaOS.Id,
                ArmacaoId = Guid.Parse(raiz.GetProperty("armacaoId").GetString()!),
                LenteId = Guid.Parse(raiz.GetProperty("lenteId").GetString()!),
                ValorTotalBruto = totalBruto,
                DescontoReais = descontoReais,
                DescontoPercentual = descontoPercentual,
                ValorTotalLiquido = raiz.GetProperty("valorTotalLiquido").GetDecimal(),
                FormaPagamento = raiz.GetProperty("formaPagamento").GetString()!,
                Parcelas = quantidadeParcelas,
                ValorEntrada = raiz.TryGetProperty("valorEntrada", out var ent) && ent.ValueKind != JsonValueKind.Null ? ent.GetDecimal() : null
            };

            // 💸 GERADOR DE PARCELAS FINANCEIRAS
            int parcelas = quantidadeParcelas < 1 ? 1 : quantidadeParcelas;
            decimal valorParcela = Math.Round(novaOS.Financeiro.ValorTotalLiquido / parcelas, 2);

            for (int i = 1; i <= parcelas; i++)
            {
                novaOS.Parcelas.Add(new ParcelaPagamento
                {
                    Id = Guid.NewGuid(),
                    NumeroParcela = i,
                    DescricaoParcela = $"PARC. {i}/{parcelas} - REF OS: {novaOS.NumeroOS}",
                    Valor = i == parcelas ? (novaOS.Financeiro.ValorTotalLiquido - (valorParcela * (parcelas - 1))) : valorParcela,
                    DataVencimento = DateTime.UtcNow.AddMonths(i)
                });
            }

            // 💰 CÁLCULO DE COMISSÃO AUTOMÁTICA
            var configComissao = await _context.ConfiguracoesComissao.FirstOrDefaultAsync(cc => cc.Ativo);
            decimal percentualTaxa = configComissao?.PercentualComissao ?? 5.00m;

            if (vendedor.ComissaoAtiva)
            {
                _context.Comissoes.Add(new Comissao
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = novaOS.Id,
                    VendedorId = vendedor.Id,
                    ValorBase = novaOS.Financeiro.ValorTotalBruto,
                    PercentualAplicado = percentualTaxa,
                    ValorComissao = Math.Round(novaOS.Financeiro.ValorTotalBruto * (percentualTaxa / 100), 2),
                    Status = "PENDENTE",
                    PeriodoReferencia = DateTime.UtcNow.ToString("yyyy-MM")
                });
            }

            _context.OrdensServico.Add(novaOS);
            await _context.SaveChangesAsync();

            return RedirectToRoute(new { controller = "Dashboard", action = "Index" });
        }    

        // 4. Modifica o status de produção e executa a baixa/reposição física do estoque (POST)
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] string novoStatus)
        {
            var ordem = await _context.OrdensServico.Include(os => os.Financeiro).FirstOrDefaultAsync(os => os.Id == id);
            if (ordem == null) return NotFound();

            string statusAnterior = ordem.Status;
            var statusValidos = new[] { "EM_ABERTO", "EM_LABORATORIO", "PRONTO", "ENTREGUE", "CANCELADO" };

            if (!statusValidos.Contains(novoStatus) || statusAnterior == novoStatus)
            {
                return RedirectToAction(nameof(Index));
            }

            bool estadoAtualAbateEstoque = (novoStatus == "EM_LABORATORIO" || novoStatus == "ENTREGUE");
            bool estadoAnteriorJáHaviaAbatido = (statusAnterior == "EM_LABORATORIO" || statusAnterior == "ENTREGUE");

            if (estadoAtualAbateEstoque && !estadoAnteriorJáHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - 1);

                var lente = await _context.Lentes.FindAsync(ordem.Financeiro.LenteId);
                if (lente != null) lente.QuantidadeEstoque = Math.Max(0, lente.QuantidadeEstoque - 1);
            }
            else if (novoStatus == "CANCELADO" && estadoAnteriorJáHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque++;

                var lente = await _context.Lentes.FindAsync(ordem.Financeiro.LenteId);
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
                            "medicoNome (string), odEsferico (number), odCilindrico (number), odEixo (number), " +
                            "oeEsferico (number), oeCilindrico (number), oeEixo (number), adicao (number). If any value is missing or not mentioned, return 0 or null. " +
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