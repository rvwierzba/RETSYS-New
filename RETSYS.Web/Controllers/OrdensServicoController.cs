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

        // 1. Listagem de OSs com Suporte a Filtro por Vendedora, Status de Cancelamento e Projeção Completa
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index([FromQuery] string? filtroComposicao, [FromQuery] Guid? vendedorId)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Vendedor)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Include(os => os.Parcelas)
                .Where(os => os.Ativo);

            if (string.Equals(perfilClaim, "VENDEDOR", StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(usuarioIdClaim, out Guid vendedorLogadoId))
            {
                query = query.Where(os => os.VendedorId == vendedorLogadoId);
            }
            else if (vendedorId.HasValue && vendedorId.Value != Guid.Empty)
            {
                query = query.Where(os => os.VendedorId == vendedorId.Value);
            }

            query = filtroComposicao switch
            {
                "armacao" => query.Where(os => os.Financeiro != null && os.Financeiro.ValorArmacao > 0),
                "lente" => query.Where(os => os.Financeiro != null && os.Financeiro.ValorLente > 0),
                "completo" => query.Where(os => os.Financeiro != null &&
                                               os.Financeiro.ValorArmacao > 0 &&
                                               os.Financeiro.ValorLente > 0),
                _ => query
            };

            var queryValidasFaturamento = query.Where(os =>
                os.Status != "CANCELADO" &&
                os.Status != "CANCELADA");

            decimal totalFiltroAtivo = filtroComposicao switch
            {
                "armacao" => await queryValidasFaturamento.SumAsync(os =>
                    os.Financeiro != null ? os.Financeiro.ValorArmacao : 0),

                "lente" => await queryValidasFaturamento.SumAsync(os =>
                    os.Financeiro != null ? os.Financeiro.ValorLente : 0),

                _ => await queryValidasFaturamento.SumAsync(os =>
                    os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0)
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
                    os.MedicoCrm,
                    os.MedicoTipo,
                    os.Observacoes,
                    VendedorNome = os.Vendedor != null ? os.Vendedor.Nome : "Não informado",
                    os.VendedorId,

                    Cliente = os.Cliente != null
                        ? new
                        {
                            os.Cliente.Id,
                            os.Cliente.Nome,
                            os.Cliente.CPF,
                            os.Cliente.Telefone,
                            os.Cliente.Email,
                            os.Cliente.Logradouro,
                            os.Cliente.Numero,
                            os.Cliente.Bairro,
                            os.Cliente.Cidade,
                            os.Cliente.Estado,
                            os.Cliente.Convenio
                        }
                        : null,

                    ClienteNome = os.Cliente != null
                        ? os.Cliente.Nome
                        : "Cliente Não Identificado",

                    ValorTotal = os.Financeiro != null
                        ? os.Financeiro.ValorTotalLiquido
                        : 0,

                    Financeiro = os.Financeiro != null
                        ? new
                        {
                            os.Financeiro.ValorArmacao,
                            os.Financeiro.ValorLente,
                            os.Financeiro.ValorTotalBruto,
                            os.Financeiro.DescontoReais,
                            os.Financeiro.DescontoPercentual,
                            os.Financeiro.ValorTotalLiquido,
                            os.Financeiro.FormaPagamento,
                            os.Financeiro.Parcelas,
                            os.Financeiro.ValorEntrada
                        }
                        : null,

                    Receita = os.Receita != null
                        ? new
                        {
                            os.Receita.OdEsferico,
                            os.Receita.OdCilindrico,
                            os.Receita.OdEixo,
                            os.Receita.OeEsferico,
                            os.Receita.OeCilindrico,
                            os.Receita.OeEixo,
                            os.Receita.Adicao,
                            os.Receita.DnpOd,
                            os.Receita.DnpOe,
                            os.Receita.AlturaMontagemOd,
                            os.Receita.AlturaMontagemOe,
                            os.Receita.Aro,
                            os.Receita.Dm,
                            os.Receita.Vert,
                            os.Receita.Po,
                            os.Receita.CoOd,
                            os.Receita.CoOe,
                            os.Receita.ObsReceita,
                            EsfericoPertoDireito = os.Receita.OdEsferico + (os.Receita.Adicao ?? 0),
                            EsfericoPertoEsquerdo = os.Receita.OeEsferico + (os.Receita.Adicao ?? 0)
                        }
                        : null,

                    Parcelas = os.Parcelas.Select(p => new
                    {
                        p.NumeroParcela,
                        p.DescricaoParcela,
                        p.Valor,
                        p.DataVencimento
                    }).ToList()
                })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .Select(u => new
                {
                    u.Id,
                    u.Nome
                })
                .ToListAsync();

            return Inertia.Render("OrdensServico/Index", new
            {
                Ordens = ordens,
                Vendedores = vendedores,
                FiltroAtivo = filtroComposicao ?? "total",
                VendedorFiltro = vendedorId,
                TotalFiltroAtivo = totalFiltroAtivo
            });
        }

        // 2. Abre a tela de cadastro de uma nova OS
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IQueryable<Usuario> queryVendedores = _context.Usuarios
                .Where(u => u.Ativo);

            if (Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId))
            {
                var usuarioLogado = await _context.Usuarios.FindAsync(usuarioLogadoId);

                if (usuarioLogado != null &&
                    !string.IsNullOrWhiteSpace(usuarioLogado.FilialLoja))
                {
                    queryVendedores = queryVendedores
                        .Where(u => u.FilialLoja == usuarioLogado.FilialLoja);
                }
            }

            var vendedores = await queryVendedores
                .OrderBy(u => u.Nome)
                .Select(u => new
                {
                    u.Id,
                    u.Nome
                })
                .ToListAsync();

            var armacoes = await _context.Armacoes
                .Include(a => a.Marca)
                .Where(a => a.QuantidadeEstoque > 0 && a.Ativo)
                .Select(a => new
                {
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

            return Inertia.Render("OrdensServico/Create", new
            {
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes
            });
        }

        // 3. Busca rápida de Cliente por CPF
        [HttpGet("/api/clientes/buscar-cpf/{cpf}")]
        public async Task<IActionResult> BuscarPorCpf(string cpf)
        {
            var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(cleanCpf))
            {
                return BadRequest(new { mensagem = "CPF inválido." });
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.CPF != null &&
                    c.CPF.Replace(".", "").Replace("-", "") == cleanCpf);

            if (cliente == null)
            {
                return NotFound();
            }

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

        // 4. Gravação de Nova OS e geração automática da comissão
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store(
            [FromForm] IFormCollection formCollection,
            [FromQuery] int? quantidadeParcelas)
        {
            try
            {
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

                Guid vendedorId = Guid.Parse(formCollection["vendedorId"].ToString());

                var vendedor = await _context.Usuarios.FindAsync(vendedorId);

                if (vendedor == null)
                {
                    return BadRequest(new { mensagem = "Vendedor não localizado." });
                }

                string cpfInformado = new string(
                    formCollection["cpf"].ToString().Where(char.IsDigit).ToArray()
                );

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.CPF != null &&
                        c.CPF.Replace(".", "").Replace("-", "") == cpfInformado);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Id = Guid.NewGuid(),
                        CPF = cpfInformado,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Clientes.Add(cliente);
                }

                cliente.Nome = formCollection["nome"].ToString();
                cliente.Telefone = formCollection.ContainsKey("telefone")
                    ? formCollection["telefone"].ToString()
                    : "";

                cliente.Logradouro = formCollection.ContainsKey("logradouro")
                    ? formCollection["logradouro"].ToString()
                    : "";

                cliente.Numero = formCollection.ContainsKey("numero")
                    ? formCollection["numero"].ToString()
                    : "";

                cliente.Bairro = formCollection.ContainsKey("bairro")
                    ? formCollection["bairro"].ToString()
                    : "";

                cliente.Cidade = formCollection.ContainsKey("cidade")
                    ? formCollection["cidade"].ToString()
                    : "";

                cliente.Estado = formCollection.ContainsKey("estado")
                    ? formCollection["estado"].ToString()
                    : "";

                cliente.Cep = formCollection.ContainsKey("cep")
                    ? formCollection["cep"].ToString()
                    : "";

                cliente.Complemento = formCollection.ContainsKey("complemento")
                    ? formCollection["complemento"].ToString()
                    : null;

                cliente.Convenio = formCollection.ContainsKey("convenio")
                    ? formCollection["convenio"].ToString()
                    : null;

                cliente.Email = formCollection.ContainsKey("email")
                    ? formCollection["email"].ToString()
                    : null;

                if (formCollection.ContainsKey("dataNascimento") &&
                    DateTime.TryParse(formCollection["dataNascimento"].ToString(), out var dn))
                {
                    cliente.DataNascimento = DateTime.SpecifyKind(dn, DateTimeKind.Utc);
                }

                cliente.UpdatedAt = DateTime.UtcNow;

                Guid? armacaoId = null;

                if (formCollection.ContainsKey("armacaoId") &&
                    Guid.TryParse(formCollection["armacaoId"].ToString(), out Guid armGuid))
                {
                    armacaoId = armGuid;
                }

                Guid? lentePrecoId = null;

                if (formCollection.ContainsKey("lentePrecoId") &&
                    Guid.TryParse(formCollection["lentePrecoId"].ToString(), out Guid lenGuid))
                {
                    lentePrecoId = lenGuid;
                }

                decimal valorArmacao = formCollection.ContainsKey("valorArmacao") &&
                                       decimal.TryParse(formCollection["valorArmacao"].ToString(), out var vArm)
                    ? vArm
                    : 0m;

                decimal valorLente = formCollection.ContainsKey("valorLente") &&
                                     decimal.TryParse(formCollection["valorLente"].ToString(), out var vLen)
                    ? vLen
                    : 0m;

                decimal totalBruto = valorArmacao + valorLente;

                decimal descontoReais = formCollection.ContainsKey("descontoReais") &&
                                        decimal.TryParse(formCollection["descontoReais"].ToString(), out var dReais)
                    ? dReais
                    : 0m;

                decimal descontoPercentual = totalBruto > 0
                    ? Math.Round((descontoReais / totalBruto) * 100, 2)
                    : 0m;

                bool ehAdmin =
                    string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

                if (!ehAdmin && descontoPercentual > vendedor.LimiteDesconto)
                {
                    return BadRequest(new
                    {
                        mensagem = "Desconto acima do limite autorizado. Solicite aprovação do administrador."
                    });
                }

                decimal valorTotalLiquido = Math.Max(0, totalBruto - descontoReais);

                string formaPagamento = formCollection.ContainsKey("formaPagamento")
                    ? formCollection["formaPagamento"].ToString()
                    : "DINHEIRO";

                int? parcelasFinais = null;
                int loopParcelas = 1;

                if (formaPagamento == "CARTAO_CREDITO")
                {
                    if (quantidadeParcelas == null || quantidadeParcelas <= 0)
                    {
                        return BadRequest(new
                        {
                            mensagem = "Defina o número de parcelas para o cartão de crédito."
                        });
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
                    DataPrevistaEntrega = formCollection.ContainsKey("dataPrevistaEntrega") &&
                                          DateTime.TryParse(
                                              formCollection["dataPrevistaEntrega"].ToString(),
                                              out var dpe)
                        ? DateTime.SpecifyKind(dpe, DateTimeKind.Utc)
                        : DateTime.UtcNow.AddDays(7),

                    Status = "EM_ABERTO",
                    MedicoNome = formCollection.ContainsKey("medicoNome")
                        ? formCollection["medicoNome"].ToString()
                        : null,

                    MedicoCrm = formCollection.ContainsKey("medicoCrm")
                        ? formCollection["medicoCrm"].ToString()
                        : null,

                    MedicoTipo = formCollection.ContainsKey("medicoTipo")
                        ? formCollection["medicoTipo"].ToString()
                        : "NAO_ESPECIFICADO",

                    Observacoes = formCollection.ContainsKey("observacoes")
                        ? formCollection["observacoes"].ToString()
                        : null,

                    IsRetroativa = false,
                    Ativo = true
                };

                string? caminhoFotoAnexa = null;

                if (Request.Form.Files.Count > 0)
                {
                    var arquivoFoto = Request.Form.Files.GetFile("fotoReceitaArquivo");

                    if (arquivoFoto != null && arquivoFoto.Length > 0)
                    {
                        var pastaUploads = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "receitas"
                        );

                        if (!Directory.Exists(pastaUploads))
                        {
                            Directory.CreateDirectory(pastaUploads);
                        }

                        var nomeArquivo = Guid.NewGuid() + Path.GetExtension(arquivoFoto.FileName);
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

                int odEixoValidado = Math.Clamp(odEixo, 0, 180);
                int oeEixoValidado = Math.Clamp(oeEixo, 0, 180);

                decimal? adicao = decimal.TryParse(formCollection["adicao"].ToString(), out var adVal)
                    ? adVal
                    : null;

                decimal.TryParse(formCollection["dnpOd"].ToString(), out var dnpOd);
                decimal.TryParse(formCollection["dnpOe"].ToString(), out var dnpOe);

                decimal? alturaOd = decimal.TryParse(
                    formCollection["alturaMontagemOd"].ToString(),
                    out var altOdVal)
                    ? altOdVal
                    : decimal.TryParse(formCollection["alturaMontagem"].ToString(), out var altGenVal)
                        ? altGenVal
                        : null;

                decimal? alturaOe = decimal.TryParse(
                    formCollection["alturaMontagemOe"].ToString(),
                    out var altOeVal)
                    ? altOeVal
                    : decimal.TryParse(formCollection["alturaMontagem"].ToString(), out var altGenVal2)
                        ? altGenVal2
                        : null;

                decimal? aro = decimal.TryParse(formCollection["aro"].ToString(), out var valAro)
                    ? Math.Clamp(valAro, 0m, 80m)
                    : null;

                decimal? dm = decimal.TryParse(formCollection["dm"].ToString(), out var valDm)
                    ? Math.Clamp(valDm, 0m, 80m)
                    : null;

                decimal? vert = decimal.TryParse(formCollection["vert"].ToString(), out var valVert)
                    ? Math.Clamp(valVert, 0m, 80m)
                    : null;

                decimal? po = decimal.TryParse(formCollection["po"].ToString(), out var valPo)
                    ? Math.Clamp(valPo, 0m, 25m)
                    : null;

                decimal? coOd = decimal.TryParse(formCollection["coOd"].ToString(), out var valCoOd)
                    ? Math.Clamp(valCoOd, 0m, 80m)
                    : null;

                decimal? coOe = decimal.TryParse(formCollection["coOe"].ToString(), out var valCoOe)
                    ? Math.Clamp(valCoOe, 0m, 80m)
                    : null;

                string obsReceitaFinal = formCollection.ContainsKey("obsReceita")
                    ? formCollection["obsReceita"].ToString()
                    : "";

                if (!string.IsNullOrEmpty(caminhoFotoAnexa))
                {
                    obsReceitaFinal = $"[Anexo da Receita: {caminhoFotoAnexa}] {obsReceitaFinal}";
                }

                novaOS.Receita = new OsReceita
                {
                    OsId = novaOS.Id,
                    OdEsferico = odEsf,
                    OdCilindrico = odCilindrico,
                    OdEixo = odEixoValidado,
                    OeEsferico = oeEsf,
                    OeCilindrico = oeCilindrico,
                    OeEixo = oeEixoValidado,
                    Adicao = adicao,
                    DnpOd = dnpOd,
                    DnpOe = dnpOe,
                    AlturaMontagemOd = alturaOd,
                    AlturaMontagemOe = alturaOe,
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
                    ValorEntrada = decimal.TryParse(formCollection["valorEntrada"].ToString(), out var entVal)
                        ? entVal
                        : null
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
                        Valor = i == loopParcelas
                            ? valorTotalLiquido - (valorParcela * (loopParcelas - 1))
                            : valorParcela,

                        DataVencimento = DateTime.UtcNow.AddMonths(i)
                    });
                }

                if (armacaoId.HasValue)
                {
                    var armacao = await _context.Armacoes.FindAsync(armacaoId.Value);

                    if (armacao != null)
                    {
                        armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - 1);
                    }
                }

                _context.OrdensServico.Add(novaOS);

                // Comissão é registrada junto com a OS, preservando a taxa vigente da vendedora.
                if (vendedor.ComissaoAtiva && vendedor.PercentualComissao > 0)
                {
                    decimal percentualComissao = vendedor.PercentualComissao;

                    _context.Comissoes.Add(new Comissao
                    {
                        Id = Guid.NewGuid(),
                        OrdemServicoId = novaOS.Id,
                        VendedorId = vendedor.Id,
                        ValorBase = totalBruto,
                        PercentualAplicado = percentualComissao,
                        ValorComissao = Math.Round(
                            totalBruto * percentualComissao / 100m,
                            2,
                            MidpointRounding.AwayFromZero
                        ),
                        Status = "PENDENTE",
                        DataGeracao = DateTime.UtcNow,
                        PeriodoReferencia = novaOS.DataEntrada.ToString("yyyy-MM"),
                        Observacoes = $"Comissão gerada automaticamente na emissão da OS {novaOS.NumeroOS}."
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    numeroOS = novaOS.NumeroOS
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Falha ao processar a Ordem de Serviço.",
                    erro = ex.Message
                });
            }
        }

        // 5. Alteração de Status com Atualização de Estoque e Estorno de Comissão
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] string novoStatus)
        {
            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (ordem == null)
            {
                return NotFound();
            }

            string statusAnterior = ordem.Status;

            var statusValidos = new[]
            {
                "EM_ABERTO",
                "EM_LABORATORIO",
                "PRONTO",
                "ENTREGUE",
                "CANCELADO"
            };

            if (!statusValidos.Contains(novoStatus) || statusAnterior == novoStatus)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ordem.IsRetroativa)
            {
                ordem.Status = novoStatus;

                if (novoStatus == "CANCELADO")
                {
                    await EstornarComissoesDaOrdemAsync(ordem.Id);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            if (novoStatus == "CANCELADO" &&
                statusAnterior != "CANCELADO" &&
                statusAnterior != "CANCELADA" &&
                ordem.Financeiro?.ArmacaoId != null)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);

                if (armacao != null)
                {
                    armacao.QuantidadeEstoque++;
                }
            }

            if (novoStatus == "CANCELADO")
            {
                await EstornarComissoesDaOrdemAsync(ordem.Id);
            }

            ordem.Status = novoStatus;

            if (novoStatus == "ENTREGUE")
            {
                ordem.DataEntregaReal = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 6. Cancelamento Seguro da OS
        [HttpPost("/ordens/cancelar/{id:guid}")]
        [HttpPost("/ordens/excluir/{id:guid}")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (ordem == null)
            {
                return NotFound();
            }

            bool jaEstavaCancelada =
                ordem.Status == "CANCELADO" ||
                ordem.Status == "CANCELADA";

            if (!jaEstavaCancelada &&
                !ordem.IsRetroativa &&
                ordem.Financeiro?.ArmacaoId != null)
            {
                var armacao = await _context.Armacoes.FindAsync(ordem.Financeiro.ArmacaoId);

                if (armacao != null)
                {
                    armacao.QuantidadeEstoque++;
                }
            }

            await EstornarComissoesDaOrdemAsync(ordem.Id);

            ordem.Ativo = true;
            ordem.Status = "CANCELADO";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Estorna apenas comissões ainda pendentes.
        // Comissão já paga não é apagada nem alterada automaticamente para preservar auditoria financeira.
        private async Task EstornarComissoesDaOrdemAsync(Guid ordemServicoId)
        {
            var comissoesPendentes = await _context.Comissoes
                .Where(c =>
                    c.OrdemServicoId == ordemServicoId &&
                    c.Status == "PENDENTE")
                .ToListAsync();

            foreach (var comissao in comissoesPendentes)
            {
                comissao.Status = "ESTORNADO";
                comissao.Observacoes = string.IsNullOrWhiteSpace(comissao.Observacoes)
                    ? "Comissão estornada automaticamente devido ao cancelamento da OS."
                    : $"{comissao.Observacoes} Comissão estornada automaticamente devido ao cancelamento da OS.";
            }
        }

        // 7. Processamento de Leitura de Receita por IA
        [HttpPost("/ordens/processar-receita-ia")]
        public async Task<IActionResult> ProcessarReceitaIA(IFormFile imagemReceita)
        {
            if (imagemReceita == null || imagemReceita.Length == 0)
            {
                return BadRequest(new
                {
                    mensagem = "Nenhuma imagem anexada."
                });
            }

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

                using var conteudoHttp = new StringContent(
                    JsonSerializer.Serialize(payloadOllama),
                    Encoding.UTF8,
                    "application/json"
                );

                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };

                var respostaOllama = await httpClient.PostAsync(
                    "http://ollama:11434/api/generate",
                    conteudoHttp
                );

                if (!respostaOllama.IsSuccessStatusCode)
                {
                    return StatusCode(500, "Erro no motor local de IA.");
                }

                string jsonString = await respostaOllama.Content.ReadAsStringAsync();

                using var documentoJson = JsonDocument.Parse(jsonString);

                if (documentoJson.RootElement.TryGetProperty("response", out var elementoResposta))
                {
                    return Content(
                        elementoResposta.GetString()!,
                        "application/json"
                    );
                }

                return BadRequest("Falha ao decodificar dados da IA.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ollama Error]: {ex.Message}");

                return StatusCode(500, new
                {
                    mensagem = "Falha no pipeline de IA.",
                    erro = ex.Message
                });
            }
        }

        [HttpPost("/ordens-servico/processar-receita-ia")]
        public async Task<IActionResult> ProcessarReceitaIa(
            IFormFile foto,
            [FromServices] IServicoIa servicoIa)
        {
            if (foto == null || foto.Length == 0)
            {
                return BadRequest(new
                {
                    erro = "Envie uma foto válida da receita médica."
                });
            }

            using var stream = foto.OpenReadStream();

            var resultado = await servicoIa.ProcessarFotoReceitaAsync(stream);

            if (resultado == null)
            {
                return BadRequest(new
                {
                    erro = "Não foi possível interpretar a receita com clareza. Preencha os campos manualmente."
                });
            }

            return Ok(resultado);
        }
    }
}
