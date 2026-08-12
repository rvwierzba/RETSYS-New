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

        // 4. Gravação de Nova OS (Suporta multipart/form-data para upload direto de foto + baixa imediata de estoque)
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store([FromForm] IFormCollection formCollection, [FromQuery] int? quantidadeParcelas)
        {
            try
            {
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

                Guid vendedorId = Guid.Parse(formCollection["vendedorId"].ToString());
                var vendedor = await _context.Usuarios.FindAsync(vendedorId);
                if (vendedor == null) return BadRequest(new { mensagem = "Vendedor não localizado." });

                string cpfInformado = new string(formCollection["cpf"].ToString().Where(char.IsDigit).ToArray());

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.CPF.Replace(".", "").Replace("-", "") == cpfInformado);

                if (cliente == null)
                {
                    cliente = new Cliente { Id = Guid.NewGuid(), CPF = cpfInformado, CreatedAt = DateTime.UtcNow };
                    _context.Clientes.Add(cliente);
                }

                cliente.Nome = formCollection["nome"].ToString();
                cliente.Telefone = formCollection.ContainsKey("telefone") ? formCollection["telefone"].ToString() : "";
                cliente.Logradouro = formCollection.ContainsKey("logradouro") ? formCollection["logradouro"].ToString() : "";
                cliente.Numero = formCollection.ContainsKey("numero") ? formCollection["numero"].ToString() : "";
                cliente.Bairro = formCollection.ContainsKey("bairro") ? formCollection["bairro"].ToString() : "";
                cliente.Cidade = formCollection.ContainsKey("cidade") ? formCollection["cidade"].ToString() : "";
                cliente.Estado = formCollection.ContainsKey("estado") ? formCollection["estado"].ToString() : "";
                cliente.Cep = formCollection.ContainsKey("cep") ? formCollection["cep"].ToString() : "";
                cliente.Complemento = formCollection.ContainsKey("complemento") ? formCollection["complemento"].ToString() : null;
                cliente.Convenio = formCollection.ContainsKey("convenio") ? formCollection["convenio"].ToString() : null;
                cliente.Email = formCollection.ContainsKey("email") ? formCollection["email"].ToString() : null;

                if (formCollection.ContainsKey("dataNascimento") && DateTime.TryParse(formCollection["dataNascimento"].ToString(), out var dn))
                {
                    cliente.DataNascimento = DateTime.SpecifyKind(dn, DateTimeKind.Utc);
                }
                cliente.UpdatedAt = DateTime.UtcNow;

                Guid? armacaoId = null;
                if (formCollection.ContainsKey("armacaoId") && Guid.TryParse(formCollection["armacaoId"].ToString(), out Guid armGuid))
                {
                    armacaoId = armGuid;
                }

                Guid? lentePrecoId = null;
                if (formCollection.ContainsKey("lentePrecoId") && Guid.TryParse(formCollection["lentePrecoId"].ToString(), out Guid lenGuid))
                {
                    lentePrecoId = lenGuid;
                }

                decimal valorArmacao = formCollection.ContainsKey("valorArmacao") && decimal.TryParse(formCollection["valorArmacao"].ToString(), out var vArm) ? vArm : 0m;
                decimal valorLente = formCollection.ContainsKey("valorLente") && decimal.TryParse(formCollection["valorLente"].ToString(), out var vLen) ? vLen : 0m;
                decimal totalBruto = valorArmacao + valorLente; 

                decimal descontoReais = formCollection.ContainsKey("descontoReais") && decimal.TryParse(formCollection["descontoReais"].ToString(), out var dReais) ? dReais : 0m;
                decimal descontoPercentual = totalBruto > 0 ? Math.Round((descontoReais / totalBruto) * 100, 2) : 0m;

                bool ehAdmin = string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

                if (!ehAdmin && descontoPercentual > vendedor.LimiteDesconto)
                {
                    return BadRequest(new { mensagem = "Desconto acima do limite autorizado. Solicite aprovação do administrador." });
                }

                decimal valorTotalLiquido = Math.Max(0, totalBruto - descontoReais);

                string formaPagamento = formCollection.ContainsKey("formaPagamento") ? formCollection["formaPagamento"].ToString() : "DINHEIRO";
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
                    DataPrevistaEntrega = formCollection.ContainsKey("dataPrevistaEntrega") && DateTime.TryParse(formCollection["dataPrevistaEntrega"].ToString(), out var dpe)
                        ? DateTime.SpecifyKind(dpe, DateTimeKind.Utc)
                        : DateTime.UtcNow.AddDays(7),
                    Status = "EM_ABERTO",
                    MedicoNome = formCollection.ContainsKey("medicoNome") ? formCollection["medicoNome"].ToString() : null,
                    MedicoCrm = formCollection.ContainsKey("medicoCrm") ? formCollection["medicoCrm"].ToString() : null,
                    MedicoTipo = formCollection.ContainsKey("medicoTipo") ? formCollection["medicoTipo"].ToString() : "NAO_ESPECIFICADO",
                    Observacoes = formCollection.ContainsKey("observacoes") ? formCollection["observacoes"].ToString() : null,
                    IsRetroativa = false,
                    Ativo = true
                };

                // PONTO 3: Tratar upload simples da foto da receita (sem obrigar IA)
                string? caminhoFotoAnexa = null;
                if (Request.Form.Files.Count > 0)
                {
                    var arquivoFoto = Request.Form.Files.GetFile("fotoReceitaArquivo");
                    if (arquivoFoto != null && arquivoFoto.Length > 0)
                    {
                        var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receitas");
                        if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                        var nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(arquivoFoto.FileName);
                        var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                        {
                            await arquivoFoto.CopyToAsync(stream);
                        }
                        caminhoFotoAnexa = "/uploads/receitas/" + nomeArquivo;
                    }
                }

                decimal.TryParse(formCollection["odEsferico"].ToString(), out var odEsf);
                decimal.TryParse(formCollection["oeEsferico"].ToString(), out var oeEsf);
                decimal.TryParse(formCollection["odCilindrico"].ToString(), out var rawOdCil);
                decimal.TryParse(formCollection["oeCilindrico"].ToString(), out var rawOeCil);
                int.TryParse(formCollection["odEixo"].ToString(), out var odEixo);
                int.TryParse(formCollection["oeEixo"].ToString(), out var oeEixo);

                decimal odCilindrico = rawOdCil > 0 ? -Math.Abs(rawOdCil) : rawOdCil;
                decimal oeCilindrico = rawOeCil > 0 ? -Math.Abs(rawOeCil) : rawOeCil;

                decimal? adicao = decimal.TryParse(formCollection["adicao"].ToString(), out var adVal) ? adVal : null;
                decimal.TryParse(formCollection["dnpOd"].ToString(), out var dnpOd);
                decimal.TryParse(formCollection["dnpOe"].ToString(), out var dnpOe);
                decimal? altura = decimal.TryParse(formCollection["alturaMontagem"].ToString(), out var altVal) ? altVal : null;

                decimal? aro = decimal.TryParse(formCollection["aro"].ToString(), out var valAro) ? Math.Clamp(valAro, 0m, 80m) : null;
                decimal? dm = decimal.TryParse(formCollection["dm"].ToString(), out var valDm) ? Math.Clamp(valDm, 0m, 80m) : null;
                decimal? vert = decimal.TryParse(formCollection["vert"].ToString(), out var valVert) ? Math.Clamp(valVert, 0m, 80m) : null;
                decimal? po = decimal.TryParse(formCollection["po"].ToString(), out var valPo) ? Math.Clamp(valPo, 0m, 25m) : null;
                decimal? coOd = decimal.TryParse(formCollection["coOd"].ToString(), out var valCoOd) ? Math.Clamp(valCoOd, 0m, 80m) : null;
                decimal? coOe = decimal.TryParse(formCollection["coOe"].ToString(), out var valCoOe) ? Math.Clamp(valCoOe, 0m, 80m) : null;

                string obsReceitaFinal = formCollection.ContainsKey("obsReceita") ? formCollection["obsReceita"].ToString() : "";
                if (!string.IsNullOrEmpty(caminhoFotoAnexa))
                {
                    obsReceitaFinal = $"[Anexo da Receita: {caminhoFotoAnexa}] " + obsReceitaFinal;
                }

                novaOS.Receita = new OsReceita
                {
                    OsId = novaOS.Id,
                    OdEsferico = odEsf,
                    OdCilindrico = odCilindrico,
                    OdEixo = Math.Clamp(odEixo, 0, 180),
                    OeEsferico = oeEsf,
                    OeCilindrico = oeCilindrico,
                    OeEixo = Math.Clamp(oeEixo, 0, 180),
                    Adicao = adicao,
                    DnpOd = dnpOd,
                    DnpOe = dnpOe,
                    AlturaMontagem = altura,
                    Aro = aro,
                    Dm = dm,
                    Vert = vert,
                    Po = po,
                    CoOd = coOd,
                    CoOe = coOe,
                    ObsReceita = obsReceitaFinal
                };

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
                    ValorEntrada = decimal.TryParse(formCollection["valorEntrada"].ToString(), out var entVal) ? entVal : null
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

                // PONTO 5: Baixa automática imediata do estoque da armação ao faturar a OS
                if (armacaoId.HasValue)
                {
                    var armacao = await _context.Armacoes.FindAsync(armacaoId.Value);
                    if (armacao != null)
                    {
                        armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - 1);
                    }
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

        // 5. Alteração de Status com Atualização de Estoque
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

            // Estorna o estoque se for cancelado
            if (novoStatus == "CANCELADO" && ordem.Financeiro?.ArmacaoId != null)
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

        // 6. PONTO 1: Exclusão / Cancelamento da OS + Devolução de Estoque
        [HttpPost("/ordens/excluir/{id:guid}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (ordem == null) return NotFound();

            // Estorna o estoque da armação se a OS estava ativa
            if (ordem.Financeiro?.ArmacaoId != null && ordem.Status != "CANCELADO")
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);
                if (armacao != null) armacao.QuantidadeEstoque++;
            }

            ordem.Ativo = false;
            ordem.Status = "CANCELADO";

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 7. Processamento de Leitura de Receita por IA (Ollama/Moondream)
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