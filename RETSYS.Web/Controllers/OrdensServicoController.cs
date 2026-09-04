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
    public class OrdensServicoController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public OrdensServicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listagem de OSs com suporte a filtro por vendedora, status e composição.
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index(
            [FromQuery] string? filtroComposicao,
            [FromQuery] Guid? vendedorId)
        {
            var oticaId = ObterOticaId();
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Vendedor)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Include(os => os.Parcelas)
                .Include(os => os.PedidoLentePor)
                .Where(os => os.Ativo && os.OticaId == oticaId);

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
                "armacao" => query.Where(os =>
                    os.Financeiro != null &&
                    os.Financeiro.ValorArmacao > 0),

                "lente" => query.Where(os =>
                    os.Financeiro != null &&
                    os.Financeiro.ValorLente > 0),

                "completo" => query.Where(os =>
                    os.Financeiro != null &&
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
                    os.Financeiro != null
                        ? os.Financeiro.ValorArmacao
                        : 0),

                "lente" => await queryValidasFaturamento.SumAsync(os =>
                    os.Financeiro != null
                        ? os.Financeiro.ValorLente
                        : 0),

                _ => await queryValidasFaturamento.SumAsync(os =>
                    os.Financeiro != null
                        ? os.Financeiro.ValorTotalLiquido
                        : 0)
            };

            var ordens = await query
                .OrderByDescending(os => os.DataEntrada)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataEntrada,
                    os.DataPrevistaEntrega,
                    os.DataEntregaReal,
                    os.Status,
                    Medico = os.MedicoNome,
                    os.MedicoCrm,
                    os.MedicoTipo,
                    os.Observacoes,
                    os.IsRetroativa,

                    // Controle de Lentes Pedidas (Seção 3.2)
                    os.LentePedida,
                    os.DataPedidoLente,
                    PedidoLentePorNome = os.PedidoLentePor != null ? os.PedidoLentePor.Nome : null,

                    VendedorNome = os.Vendedor != null
                        ? os.Vendedor.Nome
                        : "Não informado",

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
                        : "Cliente não identificado",

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
                            os.Financeiro.ValorEntrada,
                            os.Financeiro.ValorRestante,
                            os.Financeiro.PagamentoConferido,
                            os.Financeiro.ValorRecebidoRetirada,
                            os.Financeiro.FormaPagamentoRetirada,
                            os.Financeiro.DataQuitacao
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

                            EsfericoPertoDireito =
                                os.Receita.OdEsferico +
                                (os.Receita.Adicao ?? 0),

                            EsfericoPertoEsquerdo =
                                os.Receita.OeEsferico +
                                (os.Receita.Adicao ?? 0)
                        }
                        : null,

                    Parcelas = os.Parcelas
                        .OrderBy(p => p.NumeroParcela)
                        .Select(p => new
                        {
                            p.NumeroParcela,
                            p.DescricaoParcela,
                            p.Valor,
                            p.DataVencimento
                        })
                        .ToList()
                })
                .ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo && u.OticaId == oticaId)
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

        // 2. Tela de cadastro de nova OS (Gera e sugere o próximo número sequencial).
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            var oticaId = ObterOticaId();
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IQueryable<Usuario> queryVendedores = _context.Usuarios
                .Where(u => u.Ativo && u.OticaId == oticaId);

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
                .Where(a => a.OticaId == oticaId && a.QuantidadeEstoque > 0 && a.Ativo)
                .Select(a => new
                {
                    a.Id,
                    MarcaNome = a.Marca != null
                        ? a.Marca.Nome
                        : "Sem marca",

                    a.ModeloReferencia,
                    a.Cor,
                    a.QuantidadeEstoque,
                    a.PrecoVenda
                })
                .ToListAsync();

            var lentes = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo && lp.Lente != null && lp.Lente.Ativo && lp.Lente.OticaId == oticaId)
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

            string proximoNumeroOS = await GerarProximoNumeroOsAsync(oticaId);

            return Inertia.Render("OrdensServico/Create", new
            {
                ProximoNumeroOS = proximoNumeroOS,
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes
            });
        }

        // 3. Busca rápida de cliente por CPF.
        [HttpGet("/api/clientes/buscar-cpf/{cpf}")]
        public async Task<IActionResult> BuscarPorCpf(string cpf)
        {
            var oticaId = ObterOticaId();
            var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(cleanCpf))
            {
                return BadRequest(new
                {
                    mensagem = "CPF inválido."
                });
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.OticaId == oticaId &&
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

        // 4. Criação de nova OS (Numeração Editável, Validação de Desconto e Entrada Parcial).
        [HttpPost("/ordens")]
        public async Task<IActionResult> Store(
            [FromForm] IFormCollection formCollection,
            [FromQuery] int? quantidadeParcelas)
        {
            try
            {
                var oticaId = ObterOticaId();
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(formCollection["vendedorId"].ToString(), out Guid vendedorId))
                {
                    return BadRequest(new
                    {
                        mensagem = "Selecione uma vendedora válida."
                    });
                }

                var vendedor = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == vendedorId && u.OticaId == oticaId);

                if (vendedor == null || !vendedor.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "Vendedora não localizada ou inativa."
                    });
                }

                bool ehAdminOuGerente =
                    string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase);

                if (!ehAdminOuGerente &&
                    (!Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId) ||
                     usuarioLogadoId != vendedorId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        mensagem = "Você só pode emitir Ordens de Serviço vinculadas ao seu próprio usuário."
                    });
                }

                // --- SEÇÃO 2: NUMERAÇÃO DA OS EDITÁVEL (por Ótica) ---
                string numeroOsDigitado = formCollection.ContainsKey("numeroOS")
                    ? formCollection["numeroOS"].ToString().Trim()
                    : "";

                if (string.IsNullOrWhiteSpace(numeroOsDigitado))
                {
                    numeroOsDigitado = await GerarProximoNumeroOsAsync(oticaId);
                }
                else
                {
                    bool jaExiste = await _context.OrdensServico
                        .AnyAsync(os => os.NumeroOS == numeroOsDigitado && os.OticaId == oticaId && os.Ativo);

                    if (jaExiste)
                    {
                        return BadRequest(new
                        {
                            mensagem = "Já existe uma OS com este número."
                        });
                    }
                }

                string cpfInformado = new string(
                    formCollection["cpf"].ToString().Where(char.IsDigit).ToArray()
                );

                if (string.IsNullOrWhiteSpace(cpfInformado))
                {
                    return BadRequest(new
                    {
                        mensagem = "Informe um CPF válido para o cliente."
                    });
                }

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.OticaId == oticaId &&
                        c.CPF != null &&
                        c.CPF.Replace(".", "").Replace("-", "") == cpfInformado);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Id = Guid.NewGuid(),
                        OticaId = oticaId,
                        CPF = cpfInformado,
                        CreatedAt = DateTime.UtcNow
                    };

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

                if (formCollection.ContainsKey("dataNascimento") &&
                    DateTime.TryParse(formCollection["dataNascimento"].ToString(), out var dataNascimento))
                {
                    cliente.DataNascimento = DateTime.SpecifyKind(dataNascimento, DateTimeKind.Utc);
                }

                cliente.UpdatedAt = DateTime.UtcNow;

                Guid? armacaoId = null;
                if (formCollection.ContainsKey("armacaoId") &&
                    Guid.TryParse(formCollection["armacaoId"].ToString(), out Guid armGuid))
                {
                    armacaoId = armGuid;
                }

                Guid? lentePrecoId = null;
                if (formCollection.ContainsKey("lenteId") &&
                    Guid.TryParse(formCollection["lenteId"].ToString(), out Guid lenteGuid))
                {
                    lentePrecoId = lenteGuid;
                }

                decimal valorArmacao = formCollection.ContainsKey("valorArmacao") &&
                                       decimal.TryParse(formCollection["valorArmacao"].ToString(), out var valorArmacaoInformado)
                    ? valorArmacaoInformado
                    : 0m;

                decimal valorLente = formCollection.ContainsKey("valorLente") &&
                                     decimal.TryParse(formCollection["valorLente"].ToString(), out var valorLenteInformado)
                    ? valorLenteInformado
                    : 0m;

                if (valorArmacao < 0 || valorLente < 0)
                {
                    return BadRequest(new
                    {
                        mensagem = "Os valores da armação e da lente não podem ser negativos."
                    });
                }

                decimal totalBruto = valorArmacao + valorLente;

                decimal descontoReais = formCollection.ContainsKey("descontoReais") &&
                                        decimal.TryParse(formCollection["descontoReais"].ToString(), out var descontoInformado)
                    ? descontoInformado
                    : 0m;

                if (descontoReais < 0 || (totalBruto > 0 && descontoReais > totalBruto))
                {
                    return BadRequest(new
                    {
                        mensagem = "O desconto informado é inválido."
                    });
                }

                // --- SEÇÃO 1: CORREÇÃO NO CÁLCULO E VALIDAÇÃO DO DESCONTO ---
                decimal descontoPercentual = 0m;
                if (totalBruto > 0)
                {
                    descontoPercentual = Math.Round((descontoReais / totalBruto) * 100m, 2);

                    if (!ehAdminOuGerente && vendedor.LimiteDesconto > 0 && descontoPercentual > vendedor.LimiteDesconto)
                    {
                        return BadRequest(new
                        {
                            mensagem = $"Desconto de {descontoPercentual}% acima do limite autorizado ({vendedor.LimiteDesconto}%). Solicite aprovação do administrador."
                        });
                    }
                }

                decimal valorTotalLiquido = Math.Max(0, totalBruto - descontoReais);

                // --- SEÇÃO 5: VALOR DE ENTRADA E SALDO RESTANTE ---
                decimal valorEntrada = formCollection.ContainsKey("valorEntrada") &&
                                       decimal.TryParse(formCollection["valorEntrada"].ToString(), out var entVal)
                    ? entVal
                    : 0m;

                if (valorEntrada < 0)
                {
                    return BadRequest(new
                    {
                        mensagem = "O valor de entrada não pode ser negativo."
                    });
                }

                if (valorEntrada > valorTotalLiquido)
                {
                    return BadRequest(new
                    {
                        mensagem = "O valor de entrada não pode ser maior que o valor total líquido da OS."
                    });
                }

                decimal valorRestante = valorTotalLiquido - valorEntrada;

                // --- SEÇÃO 4.5: FORMA DE PAGAMENTO (SEM CONVENIO) ---
                string formaPagamento = formCollection.ContainsKey("formaPagamento")
                    ? formCollection["formaPagamento"].ToString().ToUpper()
                    : "DINHEIRO";

                if (formaPagamento == "CONVENIO")
                {
                    return BadRequest(new
                    {
                        mensagem = "Convênio não é uma forma de pagamento válida. Escolha Dinheiro, PIX, Cartão ou Boleto."
                    });
                }

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

                Armacao? armacaoSelecionada = null;

                if (armacaoId.HasValue)
                {
                    armacaoSelecionada = await _context.Armacoes
                        .FirstOrDefaultAsync(a =>
                            a.Id == armacaoId.Value &&
                            a.OticaId == oticaId &&
                            a.Ativo);

                    if (armacaoSelecionada == null)
                    {
                        return BadRequest(new
                        {
                            mensagem = "A armação selecionada não foi localizada ou está inativa."
                        });
                    }

                    if (armacaoSelecionada.QuantidadeEstoque <= 0)
                    {
                        return BadRequest(new
                        {
                            mensagem = "A armação selecionada não possui estoque disponível."
                        });
                    }
                }

                if (lentePrecoId.HasValue)
                {
                    bool lenteExiste = await _context.LentesTabelaPrecos
                        .AnyAsync(lp =>
                            lp.Id == lentePrecoId.Value &&
                            lp.Ativo &&
                            lp.Lente != null &&
                            lp.Lente.Ativo &&
                            lp.Lente.OticaId == oticaId);

                    if (!lenteExiste)
                    {
                        return BadRequest(new
                        {
                            mensagem = "A lente selecionada não foi localizada ou está inativa."
                        });
                    }
                }

                var novaOS = new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
                    NumeroOS = numeroOsDigitado,
                    ClienteId = cliente.Id,
                    VendedorId = vendedor.Id,
                    DataEntrada = DateTime.UtcNow,

                    DataPrevistaEntrega = formCollection.ContainsKey("dataPrevistaEntrega") &&
                                          DateTime.TryParse(
                                              formCollection["dataPrevistaEntrega"].ToString(),
                                              out var dataPrevistaEntrega)
                        ? DateTime.SpecifyKind(dataPrevistaEntrega, DateTimeKind.Utc)
                        : DateTime.UtcNow.AddDays(7),

                    Status = "EM_ABERTO",

                    MedicoNome = formCollection.ContainsKey("medicoNome") ? formCollection["medicoNome"].ToString() : null,
                    MedicoCrm = formCollection.ContainsKey("medicoCrm") ? formCollection["medicoCrm"].ToString() : null,
                    MedicoTipo = formCollection.ContainsKey("medicoTipo") ? formCollection["medicoTipo"].ToString() : "NAO_ESPECIFICADO",
                    Observacoes = formCollection.ContainsKey("observacoes") ? formCollection["observacoes"].ToString() : null,

                    LentePedida = false,
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

                        await using var stream = new FileStream(caminhoCompleto, FileMode.Create);
                        await arquivoFoto.CopyToAsync(stream);

                        caminhoFotoAnexa = "/uploads/receitas/" + nomeArquivo;
                    }
                }

                decimal.TryParse(formCollection["odEsferico"].ToString(), out var odEsferico);
                decimal.TryParse(formCollection["oeEsferico"].ToString(), out var oeEsferico);
                decimal.TryParse(formCollection["odCilindrico"].ToString(), out var rawOdCilindrico);
                decimal.TryParse(formCollection["oeCilindrico"].ToString(), out var rawOeCilindrico);
                int.TryParse(formCollection["odEixo"].ToString(), out var odEixo);
                int.TryParse(formCollection["oeEixo"].ToString(), out var oeEixo);

                decimal odCilindrico = rawOdCilindrico > 0 ? -Math.Abs(rawOdCilindrico) : rawOdCilindrico;
                decimal oeCilindrico = rawOeCilindrico > 0 ? -Math.Abs(rawOeCilindrico) : rawOeCilindrico;

                int odEixoValidado = Math.Clamp(odEixo, 0, 180);
                int oeEixoValidado = Math.Clamp(oeEixo, 0, 180);

                decimal? adicao = decimal.TryParse(formCollection["adicao"].ToString(), out var adicaoInformada)
                    ? adicaoInformada
                    : null;

                decimal.TryParse(formCollection["dnpOd"].ToString(), out var dnpOd);
                decimal.TryParse(formCollection["dnpOe"].ToString(), out var dnpOe);

                decimal? alturaOd = decimal.TryParse(formCollection["alturaMontagemOd"].ToString(), out var alturaOdInformada)
                    ? alturaOdInformada
                    : decimal.TryParse(formCollection["alturaMontagem"].ToString(), out var alturaGeral)
                        ? alturaGeral
                        : null;

                decimal? alturaOe = decimal.TryParse(formCollection["alturaMontagemOe"].ToString(), out var alturaOeInformada)
                    ? alturaOeInformada
                    : decimal.TryParse(formCollection["alturaMontagem"].ToString(), out var alturaGeralOe)
                        ? alturaGeralOe
                        : null;

                decimal? aro = decimal.TryParse(formCollection["aro"].ToString(), out var valorAro) ? Math.Clamp(valorAro, 0m, 80m) : null;
                decimal? dm = decimal.TryParse(formCollection["dm"].ToString(), out var valorDm) ? Math.Clamp(valorDm, 0m, 80m) : null;
                decimal? vert = decimal.TryParse(formCollection["vert"].ToString(), out var valorVert) ? Math.Clamp(valorVert, 0m, 80m) : null;
                decimal? po = decimal.TryParse(formCollection["po"].ToString(), out var valorPo) ? Math.Clamp(valorPo, 0m, 25m) : null;
                decimal? coOd = decimal.TryParse(formCollection["coOd"].ToString(), out var valorCoOd) ? Math.Clamp(valorCoOd, 0m, 80m) : null;
                decimal? coOe = decimal.TryParse(formCollection["coOe"].ToString(), out var valorCoOe) ? Math.Clamp(valorCoOe, 0m, 80m) : null;

                string observacaoReceita = formCollection.ContainsKey("obsReceita")
                    ? formCollection["obsReceita"].ToString()
                    : "";

                if (!string.IsNullOrEmpty(caminhoFotoAnexa))
                {
                    observacaoReceita = $"[Anexo da Receita: {caminhoFotoAnexa}] {observacaoReceita}";
                }

                novaOS.Receita = new OsReceita
                {
                    OsId = novaOS.Id,
                    OdEsferico = odEsferico,
                    OdCilindrico = odCilindrico,
                    OdEixo = odEixoValidado,
                    OeEsferico = oeEsferico,
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
                    ObsReceita = observacaoReceita
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
                    ValorEntrada = valorEntrada,
                    ValorRestante = valorRestante
                };

                decimal valorParcela = Math.Round(
                    valorTotalLiquido / loopParcelas,
                    2,
                    MidpointRounding.AwayFromZero
                );

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

                if (armacaoSelecionada != null)
                {
                    armacaoSelecionada.QuantidadeEstoque--;
                }

                _context.OrdensServico.Add(novaOS);

                if (vendedor.ComissaoAtiva && vendedor.PercentualComissao > 0)
                {
                    decimal percentualComissao = vendedor.PercentualComissao;

                    _context.Comissoes.Add(new Comissao
                    {
                        Id = Guid.NewGuid(),
                        OrdemServicoId = novaOS.Id,
                        VendedorId = vendedor.Id,
                        ValorBase = valorTotalLiquido,
                        PercentualAplicado = percentualComissao,

                        ValorComissao = Math.Round(
                            valorTotalLiquido * percentualComissao / 100m,
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
                    mensagem = "Falha ao processar a Ordem de Serviço.",
                    erro = ex.Message
                });
            }
        }

        // --- SEÇÃO 3.2: MARCAR LENTE COMO PEDIDA ---
        [HttpPost("/ordens/marcar-lente-pedida/{id:guid}")]
        public async Task<IActionResult> MarcarLentePedida(Guid id)
        {
            var oticaId = ObterOticaId();

            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null)
            {
                return NotFound();
            }

            bool temLente = (ordem.Financeiro != null && ordem.Financeiro.ValorLente > 0) ||
                            !string.IsNullOrEmpty(ordem.LenteDescricaoManual);

            if (!temLente)
            {
                return BadRequest(new
                {
                    mensagem = "Esta Ordem de Serviço não possui lentes associadas."
                });
            }

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);

            ordem.LentePedida = true;
            ordem.DataPedidoLente = DateTime.UtcNow;
            ordem.PedidoLentePorId = usuarioLogadoId != Guid.Empty ? usuarioLogadoId : null;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- SEÇÃO 3.1 E 6: MARCAR COMO ENTREGUE E QUITAR SALDO NA RETIRADA ---
        [HttpPost("/ordens/quitar-e-entregar/{id:guid}")]
        public async Task<IActionResult> QuitarEEntregar(Guid id, [FromForm] IFormCollection form)
        {
            var oticaId = ObterOticaId();

            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null)
            {
                return NotFound();
            }

            if (ordem.Status == "CANCELADO" || ordem.Status == "CANCELADA")
            {
                return BadRequest(new
                {
                    mensagem = "Não é possível entregar uma OS que foi cancelada."
                });
            }

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);

            string opcaoQuitacao = form["opcaoQuitacao"].ToString().Trim(); // "JA_PAGO" ou "PAGO_RETIRADA"

            if (ordem.Financeiro != null)
            {
                if (opcaoQuitacao == "PAGO_RETIRADA")
                {
                    string formaRetirada = form["formaPagamentoRetirada"].ToString().ToUpper();

                    if (string.IsNullOrWhiteSpace(formaRetirada) || formaRetirada == "CONVENIO")
                    {
                        return BadRequest(new
                        {
                            mensagem = "Selecione uma forma de pagamento válida para a retirada."
                        });
                    }

                    int? parcelasRetirada = null;

                    if (formaRetirada == "CARTAO_CREDITO" &&
                        int.TryParse(form["parcelasRetirada"].ToString(), out int pRet))
                    {
                        parcelasRetirada = pRet;
                    }

                    ordem.Financeiro.ValorRecebidoRetirada = ordem.Financeiro.ValorRestante;
                    ordem.Financeiro.FormaPagamentoRetirada = formaRetirada;
                    ordem.Financeiro.ParcelasRetirada = parcelasRetirada;
                }
                else // JA_PAGO
                {
                    ordem.Financeiro.ValorRecebidoRetirada = 0m;
                    ordem.Financeiro.FormaPagamentoRetirada = null;
                    ordem.Financeiro.ParcelasRetirada = null;
                }

                ordem.Financeiro.ValorRestante = 0m;
                ordem.Financeiro.DataQuitacao = DateTime.UtcNow;
                ordem.Financeiro.QuitacaoRegistradaPorId = usuarioLogadoId != Guid.Empty ? usuarioLogadoId : null;
            }

            ordem.Status = "ENTREGUE";
            ordem.DataEntregaReal = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 5. Alteração de status padrão.
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(
            Guid id,
            [FromQuery] string novoStatus)
        {
            var oticaId = ObterOticaId();

            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId);

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

            if (novoStatus == "CANCELADO" &&
                await OrdemPossuiComissaoPagaAsync(ordem.Id))
            {
                Inertia.Share(
                    "erro",
                    "Esta OS possui comissão já paga. O cancelamento exige um ajuste financeiro manual e auditável."
                );

                return RedirectToAction(nameof(Index));
            }

            if (novoStatus == "CANCELADO" &&
                statusAnterior != "CANCELADO" &&
                statusAnterior != "CANCELADA" &&
                !ordem.IsRetroativa &&
                ordem.Financeiro?.ArmacaoId != null)
            {
                var armacao = await _context.Armacoes
                    .FirstOrDefaultAsync(a => a.Id == ordem.Financeiro.ArmacaoId && a.OticaId == oticaId);

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
            else if (statusAnterior == "ENTREGUE")
            {
                ordem.DataEntregaReal = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 6. Cancelamento lógico seguro da OS.
        [HttpPost("/ordens/cancelar/{id:guid}")]
        [HttpPost("/ordens/excluir/{id:guid}")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var oticaId = ObterOticaId();

            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId);

            if (ordem == null)
            {
                return NotFound();
            }

            if (await OrdemPossuiComissaoPagaAsync(ordem.Id))
            {
                Inertia.Share(
                    "erro",
                    "Esta OS possui comissão já paga. O cancelamento exige um ajuste financeiro manual e auditável."
                );

                return RedirectToAction(nameof(Index));
            }

            bool jaEstavaCancelada =
                ordem.Status == "CANCELADO" ||
                ordem.Status == "CANCELADA";

            if (!jaEstavaCancelada &&
                !ordem.IsRetroativa &&
                ordem.Financeiro?.ArmacaoId != null)
            {
                var armacao = await _context.Armacoes
                    .FirstOrDefaultAsync(a => a.Id == ordem.Financeiro.ArmacaoId && a.OticaId == oticaId);

                if (armacao != null)
                {
                    armacao.QuantidadeEstoque++;
                }
            }

            await EstornarComissoesDaOrdemAsync(ordem.Id);

            ordem.Ativo = true;
            ordem.Status = "CANCELADO";
            ordem.DataEntregaReal = null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GerarProximoNumeroOsAsync(Guid oticaId)
        {
            var anoAtual = DateTime.UtcNow.Year;
            var prefixo = $"OS-{anoAtual}-";

            var ultimasOS = await _context.OrdensServico
                .Where(os => os.OticaId == oticaId && os.NumeroOS.StartsWith(prefixo))
                .Select(os => os.NumeroOS)
                .ToListAsync();

            int maiorSequencia = 0;

            foreach (var num in ultimasOS)
            {
                var partes = num.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int seq))
                {
                    if (seq > maiorSequencia)
                    {
                        maiorSequencia = seq;
                    }
                }
            }

            return $"{prefixo}{(maiorSequencia + 1):D5}";
        }

        private async Task<bool> OrdemPossuiComissaoPagaAsync(Guid ordemServicoId)
        {
            return await _context.Comissoes.AnyAsync(c =>
                c.OrdemServicoId == ordemServicoId &&
                c.Status == "PAGO");
        }

        private async Task EstornarComissoesDaOrdemAsync(Guid ordemServicoId)
        {
            var comissoesParaEstorno = await _context.Comissoes
                .Where(c =>
                    c.OrdemServicoId == ordemServicoId &&
                    (c.Status == "PENDENTE" || c.Status == "FECHADO"))
                .ToListAsync();

            if (!comissoesParaEstorno.Any())
            {
                return;
            }

            var idsComissoesEstornadas = comissoesParaEstorno
                .Select(c => c.Id)
                .ToList();

            var fechamentosAfetados = comissoesParaEstorno
                .Where(c => c.Status == "FECHADO")
                .Select(c => new
                {
                    c.VendedorId,
                    c.PeriodoReferencia
                })
                .Distinct()
                .ToList();

            foreach (var comissao in comissoesParaEstorno)
            {
                string statusAnterior = comissao.Status;

                comissao.Status = "CANCELADO";
                comissao.DataPagamento = null;

                comissao.Observacoes = string.IsNullOrWhiteSpace(comissao.Observacoes)
                    ? $"Comissão cancelada automaticamente porque a OS foi cancelada. Status anterior: {statusAnterior}."
                    : $"{comissao.Observacoes} Comissão cancelada automaticamente porque a OS foi cancelada. Status anterior: {statusAnterior}.";
            }

            foreach (var referencia in fechamentosAfetados)
            {
                var fechamento = await _context.FechamentosComissao
                    .FirstOrDefaultAsync(f =>
                        f.VendedorId == referencia.VendedorId &&
                        f.PeriodoReferencia == referencia.PeriodoReferencia &&
                        f.Status == "FECHADO");

                if (fechamento == null)
                {
                    continue;
                }

                var comissoesAindaFechadas = await _context.Comissoes
                    .Where(c =>
                        c.VendedorId == fechamento.VendedorId &&
                        c.PeriodoReferencia == fechamento.PeriodoReferencia &&
                        c.Status == "FECHADO" &&
                        !idsComissoesEstornadas.Contains(c.Id))
                    .ToListAsync();

                fechamento.TotalVendasBrutas = comissoesAindaFechadas.Sum(c => c.ValorBase);
                fechamento.TotalComissao = comissoesAindaFechadas.Sum(c => c.ValorComissao);

                fechamento.QtdOs = comissoesAindaFechadas
                    .Select(c => c.OrdemServicoId)
                    .Distinct()
                    .Count();

                if (!comissoesAindaFechadas.Any())
                {
                    fechamento.Status = "ABERTO";
                    fechamento.DataFechamento = null;
                    fechamento.DataPagamento = null;
                }
            }
        }

        // 7. Processamento de receita por IA via Ollama.
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
                using var memoryStream = new MemoryStream();
                await imagemReceita.CopyToAsync(memoryStream);
                string base64Imagem = Convert.ToBase64String(memoryStream.ToArray());

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
                    return StatusCode(500, new
                    {
                        mensagem = "Erro no motor local de IA."
                    });
                }

                string jsonString = await respostaOllama.Content.ReadAsStringAsync();
                using var documentoJson = JsonDocument.Parse(jsonString);

                if (documentoJson.RootElement.TryGetProperty("response", out var elementoResposta))
                {
                    return Content(
                        elementoResposta.GetString() ?? "{}",
                        "application/json"
                    );
                }

                return BadRequest(new
                {
                    mensagem = "Falha ao decodificar os dados retornados pela IA."
                });
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

        // 8. Processamento de receita pela implementação registrada de IServicoIa.
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

            await using var stream = foto.OpenReadStream();
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
