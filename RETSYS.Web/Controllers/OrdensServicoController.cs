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
using RETSYS.Domain.Interfaces;

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

            if (string.Equals(perfilClaim, "VENDEDOR", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(usuarioIdClaim, out Guid vendedorId))
            {
                query = query.Where(os => os.VendedorId == vendedorId);
            }

            query = filtroComposicao switch
            {
                "armacao" => query.Where(os => os.Financeiro != null && os.Financeiro.ValorArmacao > 0),
                "lente" => query.Where(os => os.Financeiro != null && os.Financeiro.ValorLente > 0),
                "completo" => query.Where(os => os.Financeiro != null && os.Financeiro.ValorArmacao > 0 && os.Financeiro.ValorLente > 0),
                _ => query
            };

            decimal totalFiltroAtivo = filtroComposicao switch
            {
                "armacao" => await query.SumAsync(os => os.Financeiro != null ? os.Financeiro.ValorArmacao : 0),
                "lente" => await query.SumAsync(os => os.Financeiro != null ? os.Financeiro.ValorLente : 0),
                _ => await query.SumAsync(os => os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0)
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
                    ValorTotal = os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0,
                    Refracao = os.Receita != null ? new
                    {
                        EsfericoLongeDireito = os.Receita.OdEsferico,
                        CilindricoLongeDireito = os.Receita.OdCilindrico,
                        EixoLongeDireito = os.Receita.OdEixo,
                        EsfericoLongeEsquerdo = os.Receita.OeEsferico,
                        CilindricoLongeEsquerdo = os.Receita.OeCilindrico,
                        EixoLongeEsquerdo = os.Receita.OeEixo,
                        os.Receita.Adicao,
                        EsfericoPertoDireito = os.Receita.OdEsferico + (os.Receita.Adicao ?? 0),
                        EsfericoPertoEsquerdo = os.Receita.OeEsferico + (os.Receita.Adicao ?? 0)
                    } : null
                })
                .ToListAsync();

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
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            IQueryable<Usuario> queryVendedores = _context.Usuarios.Where(u => u.Ativo);

            if (Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId))
            {
                var usuarioLogado = await _context.Usuarios.FindAsync(usuarioLogadoId);
                if (usuarioLogado != null && !string.IsNullOrWhiteSpace(usuarioLogado.FilialLoja))
                {
                    queryVendedores = queryVendedores.Where(u => u.FilialLoja == usuarioLogado.FilialLoja);
                }
            }

            var vendedores = await queryVendedores
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            var armacoes = await _context.Armacoes
                .Include(a => a.Marca)
                .Where(a => a.QuantidadeEstoque > 0 && a.Ativo)
                .Select(a => new { 
                    a.Id, 
                    MarcaNome = a.Marca != null ? a.Marca.Nome : "Sem Marca",
                    a.ModeloReferencia, 
                    a.Cor, 
                    a.QuantidadeEstoque, 
                    a.PrecoVenda 
                })
                .ToListAsync();

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

            return Inertia.Render("OrdensServico/Create", new { 
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes
            });
        }

        // 3. Busca rápida de Cliente por CPF (JSON / AJAX)
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

        // 4. Gravação Definitiva de Nova OS com Regras de Negócio de 10/08
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromBody] JsonElement raiz, [FromQuery] int? quantidadeParcelas)
        {
            try
            {
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

                Guid vendedorId = Guid.Parse(raiz.GetProperty("vendedorId").GetString()!);
                var vendedor = await _context.Usuarios.FindAsync(vendedorId);
                if (vendedor == null) return BadRequest(new { mensagem = "Vendedor não localizado." });

                string cpfInformado = new string(raiz.GetProperty("cpf").GetString()!.Where(char.IsDigit).ToArray());

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.CPF.Replace(".", "").Replace("-", "") == cpfInformado);

                if (cliente == null)
                {
                    cliente = new Cliente { Id = Guid.NewGuid(), CPF = cpfInformado, CreatedAt = DateTime.UtcNow };
                    _context.Clientes.Add(cliente);
                }

                cliente.Nome = raiz.GetProperty("nome").GetString()!;
                cliente.Telefone = raiz.TryGetProperty("telefone", out var tel) ? tel.GetString() ?? "" : "";
                cliente.Logradouro = raiz.TryGetProperty("logradouro", out var log) ? log.GetString() ?? "" : "";
                cliente.Numero = raiz.TryGetProperty("numero", out var num) ? num.GetString() ?? "" : "";
                cliente.Bairro = raiz.TryGetProperty("bairro", out var bai) ? bai.GetString() ?? "" : "";
                cliente.Cidade = raiz.TryGetProperty("cidade", out var cid) ? cid.GetString() ?? "" : "";
                cliente.Estado = raiz.TryGetProperty("estado", out var est) ? est.GetString() ?? "" : "";
                cliente.Cep = raiz.TryGetProperty("cep", out var cep) ? cep.GetString() ?? "" : "";
                cliente.Complemento = raiz.TryGetProperty("complemento", out var comp) ? comp.GetString() : null;
                cliente.Convenio = raiz.TryGetProperty("convenio", out var conv) ? conv.GetString() : null;
                cliente.Email = raiz.TryGetProperty("email", out var em) ? em.GetString() : null;

                if (raiz.TryGetProperty("dataNascimento", out var dnProp) && !string.IsNullOrEmpty(dnProp.GetString()))
                {
                    cliente.DataNascimento = DateTime.SpecifyKind(DateTime.Parse(dnProp.GetString()!), DateTimeKind.Utc);
                }
                cliente.UpdatedAt = DateTime.UtcNow;

                // Armação e Lente Opcionais (Guid?)
                Guid? armacaoId = null;
                if (raiz.TryGetProperty("armacaoId", out var armProp) && !string.IsNullOrWhiteSpace(armProp.GetString()) && Guid.TryParse(armProp.GetString(), out Guid armGuid))
                {
                    armacaoId = armGuid;
                }

                Guid? lentePrecoId = null;
                if (raiz.TryGetProperty("lentePrecoId", out var lenProp) && !string.IsNullOrWhiteSpace(lenProp.GetString()) && Guid.TryParse(lenProp.GetString(), out Guid lenGuid))
                {
                    lentePrecoId = lenGuid;
                }

                decimal valorArmacao = raiz.TryGetProperty("valorArmacao", out var vArm) ? vArm.GetDecimal() : 0m;
                decimal valorLente = raiz.TryGetProperty("valorLente", out var vLen) ? vLen.GetDecimal() : 0m;
                decimal totalBruto = valorArmacao + valorLente; 

                // 1.3 Desconto em Reais e validação do limite percentual do vendedor
                decimal descontoReais = raiz.TryGetProperty("descontoReais", out var dReais) ? dReais.GetDecimal() : 0m;
                decimal descontoPercentual = totalBruto > 0 ? Math.Round((descontoReais / totalBruto) * 100, 2) : 0m;

                bool ehAdmin = string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

                if (!ehAdmin && descontoPercentual > vendedor.LimiteDesconto)
                {
                    return BadRequest(new { mensagem = "Desconto acima do limite autorizado. Solicite aprovação do administrador." });
                }

                decimal valorTotalLiquido = Math.Max(0, totalBruto - descontoReais);

                string formaPagamento = raiz.TryGetProperty("formaPagamento", out var fp) ? fp.GetString() ?? "DINHEIRO" : "DINHEIRO";
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

                // 1.4 Data de Emissão Automática (Hoje)
                var novaOS = new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = cliente.Id,
                    VendedorId = vendedor.Id,
                    DataEntrada = DateTime.UtcNow,
                    DataPrevistaEntrega = raiz.TryGetProperty("dataPrevistaEntrega", out var dpe) 
                        ? DateTime.SpecifyKind(dpe.GetDateTime(), DateTimeKind.Utc) 
                        : DateTime.UtcNow.AddDays(7),
                    Status = "EM_ABERTO",
                    MedicoNome = raiz.TryGetProperty("medicoNome", out var mn) ? mn.GetString() : null,
                    MedicoCrm = raiz.TryGetProperty("medicoCrm", out var mc) ? mc.GetString() : null,
                    MedicoTipo = raiz.TryGetProperty("medicoTipo", out var mt) ? mt.GetString() ?? "NAO_ESPECIFICADO" : "NAO_ESPECIFICADO",
                    Observacoes = raiz.TryGetProperty("observacoes", out var obs) ? obs.GetString() : null,
                    IsRetroativa = false,
                    Ativo = true
                };

                // 1.2 Regra: Se houver receita/lente com grau, processa dados da receita
                bool temReceitaInformada = raiz.TryGetProperty("odEsferico", out _) || raiz.TryGetProperty("oeEsferico", out _);

                if (temReceitaInformada || lentePrecoId.HasValue)
                {
                    // 3.1 Cilíndrico negativo automático
                    decimal rawOdCil = raiz.TryGetProperty("odCilindrico", out var odC) ? odC.GetDecimal() : 0m;
                    decimal odCilindrico = rawOdCil > 0 ? -Math.Abs(rawOdCil) : rawOdCil;
                    odCilindrico = Math.Clamp(odCilindrico, -15.00m, 0m);

                    // 3.2 Eixo de 0 a 180
                    int odEixo = Math.Clamp(raiz.TryGetProperty("odEixo", out var odE) ? odE.GetInt32() : 0, 0, 180);

                    decimal rawOeCil = raiz.TryGetProperty("oeCilindrico", out var oeC) ? oeC.GetDecimal() : 0m;
                    decimal oeCilindrico = rawOeCil > 0 ? -Math.Abs(rawOeCil) : rawOeCil;
                    oeCilindrico = Math.Clamp(oeCilindrico, -15.00m, 0m);

                    int oeEixo = Math.Clamp(raiz.TryGetProperty("oeEixo", out var oeE) ? oeE.GetInt32() : 0, 0, 180);

                    decimal? adicao = raiz.TryGetProperty("adicao", out var ad) && ad.ValueKind != JsonValueKind.Null ? ad.GetDecimal() : null;
                    if (adicao.HasValue) adicao = Math.Clamp(adicao.Value, 0m, 3.50m);

                    decimal dnpOd = raiz.TryGetProperty("dnpOd", out var dnpD) ? dnpD.GetDecimal() : 0m;
                    decimal dnpOe = raiz.TryGetProperty("dnpOe", out var dnpE) ? dnpE.GetDecimal() : 0m;

                    decimal? alturaMontagem = raiz.TryGetProperty("alturaMontagem", out var alt) && alt.ValueKind != JsonValueKind.Null ? alt.GetDecimal() : null;

                    // 1.1 Medidas de montagem da armação
                    decimal? aro = raiz.TryGetProperty("aro", out var vAro) && vAro.ValueKind != JsonValueKind.Null ? vAro.GetDecimal() : null;
                    decimal? dm = raiz.TryGetProperty("dm", out var vDm) && vDm.ValueKind != JsonValueKind.Null ? vDm.GetDecimal() : null;
                    decimal? vert = raiz.TryGetProperty("vert", out var vVert) && vVert.ValueKind != JsonValueKind.Null ? vVert.GetDecimal() : null;
                    decimal? po = raiz.TryGetProperty("po", out var vPo) && vPo.ValueKind != JsonValueKind.Null ? vPo.GetDecimal() : null;
                    decimal? coOd = raiz.TryGetProperty("coOd", out var vCoOd) && vCoOd.ValueKind != JsonValueKind.Null ? vCoOd.GetDecimal() : null;
                    decimal? coOe = raiz.TryGetProperty("coOe", out var vCoOe) && vCoOe.ValueKind != JsonValueKind.Null ? vCoOe.GetDecimal() : null;

                    novaOS.Receita = new OsReceita
                    {
                        OsId = novaOS.Id,
                        OdEsferico = raiz.TryGetProperty("odEsferico", out var odEsf) ? odEsf.GetDecimal() : 0m,
                        OdCilindrico = odCilindrico,
                        OdEixo = odEixo,
                        OeEsferico = raiz.TryGetProperty("oeEsferico", out var oeEsf) ? oeEsf.GetDecimal() : 0m,
                        OeCilindrico = oeCilindrico,
                        OeEixo = oeEixo,
                        Adicao = adicao,
                        DnpOd = dnpOd,
                        DnpOe = dnpOe,
                        AlturaMontagem = alturaMontagem,

                        // Novos campos de montagem
                        Aro = aro,
                        Dm = dm,
                        Vert = vert,
                        Po = po,
                        CoOd = coOd,
                        CoOe = coOe,
                        ObsReceita = raiz.TryGetProperty("obsReceita", out var obsR) ? obsR.GetString() : null
                    };
                }

                novaOS.Financeiro = new OsFinanceiro
                {
                    OsId = novaOS.Id,
                    ArmacaoId = armacaoId,
                    LentePrecoId = lentePrecoId,
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

        // 5. Alteração de Status com Atualização de Estoque Nula-Safe
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

            if (ordem.Financeiro?.ArmacaoId != null)
            {
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
            }

            ordem.Status = novoStatus;

            if (novoStatus == "ENTREGUE")
            {
                ordem.DataEntregaReal = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 6. Processamento de Leitura de Receita por IA (Ollama/Moondream)
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
                Console.WriteLine($"[Ollama Error]: {ex.Message}");
                return StatusCode(500, new { mensagem = "Falha no pipeline de IA.", erro = ex.Message });
            }
        }

        [HttpPost("/ordens-servico/processar-receita-ia")]
        public async Task<IActionResult> ProcessarReceitaIa(IFormFile foto, [FromServices] IServicoIa servicoIa)
        {
            if (foto == null || foto.Length == 0)
            {
                return BadRequest(new { erro = "Envie uma foto válida da receita médica." });
            }

            using var stream = foto.OpenReadStream();
            var resultado = await servicoIa.ProcessarFotoReceitaAsync(stream);

            if (resultado == null)
            {
                return BadRequest(new { erro = "Não foi possível interpretar a receita com clareza. Preencha os campos manualmente." });
            }

            return Ok(resultado);
        }
    }
}