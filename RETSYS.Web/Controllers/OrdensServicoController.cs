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
using System.Collections.Generic;

namespace RETSYS.Web.Controllers
{
    public class OrdensServicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdensServicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de todas as OSs com Isolamento por Perfil (RBAC) e Filtros de Composição
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index([FromQuery] string? filtroComposicao)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Where(os => os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Ativo);

            if (perfilClaim == "VENDEDOR" && Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                query = query.Where(os => os.VendedorId == vendedorId);
            }

            query = filtroComposicao switch
            {
                "armacao" => query.Where(os => os.Financeiro.ValorArmacao > 0),
                "lente" => query.Where(os => os.Financeiro.ValorLente > 0),
                "completo" => query.Where(os => os.Financeiro.ValorArmacao > 0 && os.Financeiro.ValorLente > 0),
                _ => query
            };

            decimal totalFiltroAtivo = filtroComposicao switch
            {
                "armacao" => await query.SumAsync(os => os.Financeiro.ValorArmacao),
                "lente" => await query.SumAsync(os => os.Financeiro.ValorLente),
                _ => await query.SumAsync(os => os.Financeiro.ValorTotalLiquido)
            };

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
                        EsfericoPertoDireito = os.Receita.OdEsfericoPerto,
                        EsfericoPertoEsquerdo = os.Receita.OeEsfericoPerto
                    }
                })
                .ToListAsync();

            // Corrigido caminho de renderização para a nova pasta unificada
            return Inertia.Render("OrdensServico/Index", new { 
                Ordens = ordens,
                FiltroAtivo = filtroComposicao ?? "total",
                TotalFiltroAtivo = totalFiltroAtivo
            });
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

            // Proteção inclusa contra Lente pai nula para mitigar quebras de carregamento de página
            var lentes = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo && lp.Lente != null && lp.Lente.Ativo)
                .Select(lp => new
                {
                    lp.Id, 
                    Laboratorio = lp.Lente.Laboratorio,
                    Tipo = lp.Lente.Tipo,
                    lp.IndiceRefracao,
                    lp.Tratamento, 
                    lp.PrecoVenda
                })
                .ToListAsync();

            // Corrigido caminho de renderização para a nova pasta unificada
            return Inertia.Render("OrdensServico/Create", new { 
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes
            });
        }

        [HttpGet("/api/clientes/buscar-cpf/{cpf}")]
        public async Task<IActionResult> BuscarPorCpf(string cpf)
        {
            var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(cleanCpf)) return BadRequest(new { mensagem = "CPF inválido." });

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.CPF.Replace(".", "").Replace("-", "") == cleanCpf);

            if (cliente == null) return NotFound();

            return Ok(new
            {
                id = cliente.Id,
                nome = cliente.Nome,
                cpf = cliente.CPF,
                telefone = cliente.Telefone,
                dataNascimento = cliente.DataNascimento?.ToString("yyyy-MM-dd"),
                cep = cliente.Cep,
                logradouro = cliente.Logradouro,
                numero = cliente.Numero,
                complemento = cliente.Complemento,
                bairro = cliente.Bairro,
                cidade = cliente.Cidade,
                estado = cliente.Estado,
                convenio = cliente.Convenio,
                email = cliente.Email
            });
        }

        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] JsonElement raiz, [FromQuery] int? quantidadeParcelas)
        {
            try
            {
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

                Guid vendedorId = Guid.Parse(raiz.GetProperty("vendedorId").GetString()!);
                var vendedor = await _context.Usuarios.FindAsync(vendedorId);
                if (vendedor == null) return BadRequest(new { salesman = "Vendedor não localizado." });

                string cpfInformado = new string(raiz.GetProperty("cpf").GetString()!.Where(char.IsDigit).ToArray());

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.CPF.Replace(".", "").Replace("-", "") == cpfInformado);

                if (cliente == null)
                {
                    cliente = new Cliente { Id = Guid.NewGuid(), CPF = cpfInformado, CreatedAt = DateTime.UtcNow };
                    _context.Clientes.Add(cliente);
                }

                cliente.Nome = raiz.GetProperty("nome").GetString()!;
                cliente.Telefone = raiz.GetProperty("telefone").GetString()!;
                cliente.Logradouro = raiz.GetProperty("logradouro").GetString()!;
                cliente.Numero = raiz.GetProperty("numero").GetString()!;
                cliente.Bairro = raiz.GetProperty("bairro").GetString()!;
                cliente.Cidade = raiz.GetProperty("cidade").GetString()!;
                cliente.Estado = raiz.GetProperty("estado").GetString()!;
                cliente.Cep = raiz.GetProperty("cep").GetString()!;
                cliente.Complemento = raiz.TryGetProperty("complemento", out var comp) ? comp.GetString() : null;
                cliente.Convenio = raiz.TryGetProperty("convenio", out var conv) ? conv.GetString() : null;
                cliente.Email = raiz.TryGetProperty("email", out var em) ? em.GetString() : null;

                if (raiz.TryGetProperty("dataNascimento", out var dnProp) && !string.IsNullOrEmpty(dnProp.GetString()))
                {
                    cliente.DataNascimento = DateTime.SpecifyKind(DateTime.Parse(dnProp.GetString()!), DateTimeKind.Utc);
                }
                cliente.UpdatedAt = DateTime.UtcNow;

                decimal valorArmacao = raiz.GetProperty("valorArmacao").GetDecimal();
                decimal valorLente = raiz.GetProperty("valorLente").GetDecimal();
                decimal totalBruto = valorArmacao + valorLente; 

                decimal descontoPercentual = raiz.GetProperty("descontoPercentual").GetDecimal();

                if (perfilClaim != "ADMIN" && descontoPercentual > vendedor.LimiteDesconto)
                {
                    return BadRequest(new { mensagem = "Desconto acima do limite autorizado." });
                }

                decimal descontoReais = Math.Round(totalBruto * (descontoPercentual / 100), 2);
                decimal valorTotalLiquido = totalBruto - descontoReais;

                string formaPagamento = raiz.GetProperty("formaPagamento").GetString()!;
                int? parcelasFinais = null;
                int loopParcelas = 1;

                if (formaPagamento == "CARTAO_CREDITO")
                {
                    if (quantidadeParcelas == null || quantidadeParcelas <= 0)
                    {
                        return BadRequest(new { mensagem = "Defina o número de parcelas para o cartão de crédito." });
                    }
                    parcelasFinais = quantidadeParcelas.Value;
                    loopParcelas = parcelasFinais.Value;
                }

                var novaOS = new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = cliente.Id,
                    VendedorId = vendedor.Id,
                    DataEntrada = DateTime.UtcNow,
                    DataPrevistaEntrega = DateTime.SpecifyKind(raiz.GetProperty("dataPrevistaEntrega").GetDateTime(), DateTimeKind.Utc),
                    Status = "EM_ABERTO",
                    MedicoNome = raiz.TryGetProperty("medicoNome", out var mn) ? mn.GetString() : null,
                    MedicoCrm = raiz.TryGetProperty("medicoCrm", out var mc) ? mc.GetString() : null,
                    MedicoTipo = raiz.GetProperty("medicoTipo").GetString() ?? "NAO_ESPECIFICADO",
                    Observacoes = raiz.TryGetProperty("observacoes", out var obs) ? obs.GetString() : null,
                    IsRetroativa = false,
                    Ativo = true
                };

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
                    AlturaMontagem = raiz.TryGetProperty("alturaMontagem", out var alt) && alt.ValueKind != JsonValueKind.Null ? alt.GetDecimal() : null
                };

                novaOS.Financeiro = new OsFinanceiro
                {
                    OsId = novaOS.Id,
                    ArmacaoId = Guid.Parse(raiz.GetProperty("armacaoId").GetString()!),
                    LentePrecoId = Guid.Parse(raiz.GetProperty("lentePrecoId").GetString()!),
                    ValorArmacao = valorArmacao,
                    ValorLente = valorLente,
                    ValorTotalBruto = totalBruto,
                    DescontoPercentual = descontoPercentual,
                    DescontoReais = descontoReais,
                    ValorTotalLiquido = valorTotalLiquido,
                    FormaPagamento = formaPagamento,
                    Parcelas = parcelasFinais,
                    ValorEntrada = raiz.TryGetProperty("valorEntrada", out var ent) && ent.ValueKind != JsonValueKind.Null ? ent.GetDecimal() : null
                };

                decimal valorParcela = Math.Round(valorTotalLiquido / loopParcelas, 2);
                for (int i = 1; i <= loopParcelas; i++)
                {
                    novaOS.Parcelas.Add(new ParcelaPagamento
                    {
                        Id = Guid.NewGuid(),
                        OrdemServicoId = novaOS.Id,
                        NumeroParcela = i,
                        DescricaoParcela = $"PARC. {i}/{loopParcelas} - OS: {novaOS.NumeroOS}",
                        Valor = i == loopParcelas ? (valorTotalLiquido - (valorParcela * (loopParcelas - 1))) : valorParcela,
                        DataVencimento = DateTime.UtcNow.AddMonths(i)
                    });
                }

                _context.OrdensServico.Add(novaOS);
                await _context.SaveChangesAsync();

                return Ok(new { numeroOS = novaOS.NumeroOS });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Falha ao processar a Ordem de Serviço.", erro = ex.Message });
            }
        }

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

            if (ordem.IsRetroativa)
            {
                ordem.Status = novoStatus;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            bool estadoAtualAbateEstoque = (novoStatus == "EM_LABORATORIO" || novoStatus == "ENTREGUE");
            bool estadoAnteriorJaHaviaAbatido = (statusAnterior == "EM_LABORATORIO" || statusAnterior == "ENTREGUE");

            if (estadoAtualAbateEstoque && !estadoAnteriorJaHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - 1);
            }
            else if (novoStatus == "CANCELADO" && estadoAnteriorJaHaviaAbatido)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque++;
            }

            ordem.Status = novoStatus;

            if (novoStatus == "ENTREGUE")
            {
                ordem.DataEntregaReal = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/ordens/processar-receita-ia")]
        public async Task<IActionResult> ProcessarReceitaIA(IFormFile imagemReceita)
        {
            if (imagemReceita == null || imagemReceita.Length == 0) return BadRequest(new { mensagem = "Nenhuma imagem anexada." });

            try
            {
                using var ms = new MemoryStream();
                await imagemReceita.CopyToAsync(ms);
                string base64Imagem = Convert.ToBase64String(ms.ToArray());

                var payloadOllama = new
                {
                    model = "moondream",
                    prompt = "Analyze this optical prescription image. Extract values into JSON using keys: medicoNome, odEsferico, odCilindrico, odEixo, oeEsferico, oeCilindrico, oeEixo, adicao.",
                    images = new[] { base64Imagem },
                    stream = false,
                    format = "json"
                };

                using var conteudoHttp = new StringContent(JsonSerializer.Serialize(payloadOllama), Encoding.UTF8, "application/json");
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

                var respostaOllama = await httpClient.PostAsync("http://ollama:11434/api/generate", conteudoHttp);
                if (!respostaOllama.IsSuccessStatusCode) return StatusCode(500, "Erro no motor local de IA.");

                string jsonString = await respostaOllama.Content.ReadAsStringAsync();
                using var documentoJson = JsonDocument.Parse(jsonString);

                if (documentoJson.RootElement.TryGetProperty("response", out var elementoResposta))
                {
                    return Content(elementoResposta.GetString()!, "application/json");
                }

                return BadRequest("Falha ao decodificar dados da IA.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Falha no pipeline de IA.", erro = ex.Message });
            }
        }
    }
}