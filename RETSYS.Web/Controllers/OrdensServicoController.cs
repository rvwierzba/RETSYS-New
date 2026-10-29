using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;
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

        // 1. Listagem de OSs com suporte a filtro por vendedora, status, composição e lente pendente.
        [HttpGet("/ordens")]
        public async Task<IActionResult> Index(
            [FromQuery] string? filtroComposicao,
            [FromQuery] string? filtroLentePedida,
            [FromQuery] string? filtroAtraso,
            [FromQuery] string? filtroStatus,
            [FromQuery] Guid? vendedorId,
            [FromQuery] string? busca,
            [FromQuery] Guid? lojaId)
        {
            var oticaId = ObterOticaId();
            var idsRede = ObterIdsOticasDaRede(oticaId, _context);
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
            bool ehVendedor = string.Equals(perfilClaim, nameof(PerfilUsuario.Vendedor), StringComparison.OrdinalIgnoreCase) || (!EhAdministrador() && !EhDono() && !EhSistema());
            Guid.TryParse(usuarioIdClaim, out Guid vendedorLogadoId);

            Guid? lojaAlvoId = (lojaId.HasValue && lojaId.Value != Guid.Empty) 
                ? lojaId.Value 
                : oticaId;

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Otica)
                .Include(os => os.Cliente)
                .Include(os => os.Vendedor)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                .Include(os => os.Parcelas)
                .Include(os => os.PedidoLentePor)
                .Where(os => os.Ativo);

            if (lojaAlvoId.HasValue)
            {
                query = query.Where(os => os.OticaId == lojaAlvoId.Value);
            }
            else if (!EhSistema() && idsRede.Any())
            {
                query = query.Where(os => idsRede.Contains(os.OticaId));
            }
            else if (oticaId != Guid.Empty)
            {
                query = query.Where(os => os.OticaId == oticaId);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();
                bool apenasDigitos = termo.All(char.IsDigit);

                if (apenasDigitos)
                {
                    query = query.Where(os => 
                        os.NumeroOS == termo || 
                        os.NumeroOS.EndsWith(termo) || 
                        os.NumeroOS.Contains(termo));
                }
                else
                {
                    var termoLower = termo.ToLower();
                    query = query.Where(os => os.Cliente != null && os.Cliente.Nome.ToLower().Contains(termoLower));
                }
            }

            if (vendedorId.HasValue && vendedorId.Value != Guid.Empty && !ehVendedor)
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

            // FILTRO DE LENTES NÃO PEDIDAS
            if (filtroLentePedida == "pendente")
            {
                query = query.Where(os =>
                    !os.LentePedida &&
                    os.Status != "CANCELADO" &&
                    os.Status != "CANCELADA" &&
                    ((os.Financeiro != null && os.Financeiro.ValorLente > 0) ||
                     !string.IsNullOrEmpty(os.LenteDescricaoManual)));
            }

            // UNIFICAÇÃO DE ATRASOS: "Serviços atrasados"
            if (filtroAtraso == "atrasados")
            {
                DateTime hoje = DateTime.UtcNow.Date;
                query = query.Where(os =>
                    os.DataPrevistaEntrega.Date < hoje &&
                    os.Status != "ENTREGUE" &&
                    os.Status != "CANCELADO" &&
                    os.Status != "CANCELADA");
            }

            // FILTRO POR STATUS
            if (!string.IsNullOrWhiteSpace(filtroStatus))
            {
                query = query.Where(os => os.Status.ToUpper() == filtroStatus.ToUpper());
            }

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
                    os.LojaVenda,
                    os.UltimaEdicaoLog,
                    Medico = os.MedicoNome,
                    os.MedicoCrm,
                    os.MedicoTipo,
                    os.Observacoes,
                    os.IsRetroativa,

                    // Controle de Lentes Pedidas e Novo Fluxo
                    os.LentePedida,
                    os.DataPedidoLente,
                    PedidoLentePorNome = os.PedidoLentePor != null ? os.PedidoLentePor.Nome : null,
                    os.DataPrevisaoLente,
                    os.LenteChegou,
                    os.DataChegadaLente,
                    os.WhatsAppNotificadoCriacao,
                    os.WhatsAppNotificadoPronto,
                    LenteDescricao = os.Financeiro != null && os.Financeiro.LentePreco != null && os.Financeiro.LentePreco.Lente != null
                        ? os.Financeiro.LentePreco.Lente.Tipo
                        : (!string.IsNullOrEmpty(os.LenteDescricaoManual) ? os.LenteDescricaoManual : ""),
                    ArmacaoDescricao = os.Financeiro != null && os.Financeiro.Armacao != null
                        ? (os.Financeiro.Armacao.ModeloReferencia + (!string.IsNullOrEmpty(os.Financeiro.Armacao.CodigoSku) ? " (" + os.Financeiro.Armacao.CodigoSku + ")" : ""))
                        : (!string.IsNullOrEmpty(os.ArmacaoModeloManual) ? os.ArmacaoModeloManual : ""),

                    VendedorNome = (!ehVendedor || (os.VendedorId.HasValue && os.VendedorId.Value == vendedorLogadoId))
                        ? (os.Vendedor != null ? os.Vendedor.Nome : "Não informado")
                        : "Outro Vendedor",

                    VendedorId = (!ehVendedor || (os.VendedorId.HasValue && os.VendedorId.Value == vendedorLogadoId))
                        ? os.VendedorId
                        : null,

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

                    ClienteTelefone = os.Cliente != null
                        ? os.Cliente.Telefone
                        : "",

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

                            OdEsfericoPerto = os.Receita.OdEsferico + (os.Receita.Adicao ?? 0),
                            OeEsfericoPerto = os.Receita.OeEsferico + (os.Receita.Adicao ?? 0),
                            OdCilindricoPerto = os.Receita.OdCilindrico,
                            OeCilindricoPerto = os.Receita.OeCilindrico,
                            OdEixoPerto = os.Receita.OdEixo,
                            OeEixoPerto = os.Receita.OeEixo,

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

            IQueryable<Otica> queryLojas = _context.Oticas.AsNoTracking();
            if (!EhSistema() && idsRede.Any())
            {
                queryLojas = queryLojas.Where(o => idsRede.Contains(o.Id));
            }
            var lojas = await queryLojas.OrderBy(l => l.Nome).Select(l => new { l.Id, l.Nome }).ToListAsync();

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo && (idsRede.Contains(u.OticaId) || u.OticaId == oticaId))
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
                Lojas = lojas,
                FiltroAtivo = filtroComposicao ?? "total",
                FiltroLentePedida = filtroLentePedida ?? "",
                VendedorFiltro = vendedorId,
                TotalFiltroAtivo = totalFiltroAtivo,
                Busca = busca ?? "",
                LojaId = lojaAlvoId
            });
        }

        // 1.1 Painel Kanban de Acompanhamento de Produção e Status de OSs
        [HttpGet("/ordens/kanban")]
        public async Task<IActionResult> Kanban(
            [FromQuery] Guid? vendedorId, 
            [FromQuery] string? busca)
        {
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
            bool ehVendedor = string.Equals(perfilClaim, nameof(PerfilUsuario.Vendedor), StringComparison.OrdinalIgnoreCase) || (!EhAdministrador() && !EhDono() && !EhSistema());
            bool ehAdminOuSistema = EhSistema() || EhAdministrador() || EhDono();

            var oticaIdEfetivo = ObterOticaId();
            var oticaAtual = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == oticaIdEfetivo);

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(usuarioIdClaim, out Guid vendedorLogadoId);

            IQueryable<OrdemServico> query = _context.OrdensServico
                .Include(os => os.Otica)
                .Include(os => os.Cliente)
                .Include(os => os.Vendedor)
                .Include(os => os.Receita)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.Armacao)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.LentePreco)
                        .ThenInclude(lp => lp!.Lente)
                .Include(os => os.PedidoLentePor)
                .Where(os => os.Ativo && os.OticaId == oticaIdEfetivo);

            if (vendedorId.HasValue && vendedorId.Value != Guid.Empty && !ehVendedor)
            {
                query = query.Where(os => os.VendedorId == vendedorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(os =>
                    os.NumeroOS.ToLower().Contains(termo) ||
                    (os.Cliente != null && os.Cliente.Nome.ToLower().Contains(termo)) ||
                    (os.Cliente != null && os.Cliente.CPF != null && os.Cliente.CPF.Contains(termo)));
            }

            var todasOrdens = await query
                .OrderByDescending(os => os.DataEntrada)
                .Select(os => new
                {
                    os.Id,
                    os.NumeroOS,
                    os.DataEntrada,
                    os.DataPrevistaEntrega,
                    os.DataEntregaReal,
                    os.Status,
                    os.LojaVenda,
                    os.Observacoes,

                    os.LentePedida,
                    os.DataPedidoLente,
                    PedidoLentePorNome = os.PedidoLentePor != null ? os.PedidoLentePor.Nome : null,
                    os.DataPrevisaoLente,
                    os.LenteChegou,
                    os.DataChegadaLente,
                    os.WhatsAppNotificadoCriacao,
                    os.WhatsAppNotificadoPronto,

                    LenteDescricao = os.Financeiro != null && os.Financeiro.LentePreco != null && os.Financeiro.LentePreco.Lente != null
                        ? os.Financeiro.LentePreco.Lente.Tipo
                        : (!string.IsNullOrEmpty(os.LenteDescricaoManual) ? os.LenteDescricaoManual : ""),

                    ArmacaoDescricao = os.Financeiro != null && os.Financeiro.Armacao != null
                        ? (os.Financeiro.Armacao.ModeloReferencia + (!string.IsNullOrEmpty(os.Financeiro.Armacao.CodigoSku) ? " (" + os.Financeiro.Armacao.CodigoSku + ")" : ""))
                        : (!string.IsNullOrEmpty(os.ArmacaoModeloManual) ? os.ArmacaoModeloManual : ""),

                    VendedorNome = (!ehVendedor || (os.VendedorId.HasValue && os.VendedorId.Value == vendedorLogadoId))
                        ? (os.Vendedor != null ? os.Vendedor.Nome : "Não informado")
                        : "Outro Vendedor",

                    VendedorId = (!ehVendedor || (os.VendedorId.HasValue && os.VendedorId.Value == vendedorLogadoId))
                        ? os.VendedorId
                        : null,

                    ClienteId = os.Cliente != null ? (Guid?)os.Cliente.Id : null,
                    ClienteNome = os.Cliente != null ? os.Cliente.Nome : "Cliente não identificado",
                    ClienteTelefone = os.Cliente != null ? os.Cliente.Telefone : "",
                    ClienteCpf = os.Cliente != null ? os.Cliente.CPF : "",

                    ValorTotal = os.Financeiro != null ? os.Financeiro.ValorTotalLiquido : 0m,
                    ValorRestante = os.Financeiro != null ? os.Financeiro.ValorRestante : 0m,
                    FormaPagamento = os.Financeiro != null ? os.Financeiro.FormaPagamento : "",
                    ValorEntrada = os.Financeiro != null ? os.Financeiro.ValorEntrada : 0m,
                    DataQuitacao = os.Financeiro != null ? os.Financeiro.DataQuitacao : null,

                    Receita = os.Receita != null ? new
                    {
                        os.Receita.OdEsferico,
                        os.Receita.OdCilindrico,
                        os.Receita.OdEixo,
                        os.Receita.OeEsferico,
                        os.Receita.OeCilindrico,
                        os.Receita.OeEixo,
                        os.Receita.Adicao
                    } : null
                })
                .ToListAsync();

            var hoje = DateTime.UtcNow.Date;

            // Categorização do Kanban
            var lancadas = todasOrdens
                .Where(os => os.Status == "LANCADA" || os.Status == "EM_ABERTO")
                .ToList();

            var confirmadas = todasOrdens
                .Where(os => os.Status == "CONFIRMADA")
                .ToList();

            var aguardandoLente = todasOrdens
                .Where(os => os.Status == "AGUARDANDO_LENTE")
                .ToList();

            var emMontagem = todasOrdens
                .Where(os => os.Status == "EM_MONTAGEM" || os.Status == "EM_LABORATORIO")
                .ToList();

            var prontas = todasOrdens
                .Where(os => os.Status == "PRONTA" || os.Status == "PRONTO")
                .ToList();

            var entregues = todasOrdens
                .Where(os => os.Status == "ENTREGUE")
                .Take(25)
                .ToList();

            // Métricas e Alertas
            int totalLentesHoje = todasOrdens.Count(os =>
                (os.Status == "AGUARDANDO_LENTE" || !os.LenteChegou) &&
                os.DataPrevisaoLente.HasValue &&
                os.DataPrevisaoLente.Value.Date == hoje &&
                os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Status != "ENTREGUE");

            int totalLentesAtrasadas = todasOrdens.Count(os =>
                (os.Status == "AGUARDANDO_LENTE" || !os.LenteChegou) &&
                os.DataPrevisaoLente.HasValue &&
                os.DataPrevisaoLente.Value.Date < hoje &&
                os.Status != "CANCELADO" && os.Status != "CANCELADA" && os.Status != "ENTREGUE");

            var config = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaIdEfetivo);

            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo && u.OticaId == oticaIdEfetivo)
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            return Inertia.Render("OrdensServico/Kanban", new
            {
                Lancadas = lancadas,
                Confirmadas = confirmadas,
                AguardandoLente = aguardandoLente,
                EmMontagem = emMontagem,
                Prontas = prontas,
                Entregues = entregues,
                Estatisticas = new
                {
                    TotalLancadas = lancadas.Count,
                    TotalConfirmadas = confirmadas.Count,
                    TotalAguardandoLente = aguardandoLente.Count,
                    TotalEmMontagem = emMontagem.Count,
                    TotalProntas = prontas.Count,
                    TotalEntregues = entregues.Count,
                    LentesChegamHoje = totalLentesHoje,
                    LentesAtrasadas = totalLentesAtrasadas,
                    TotalAtivas = lancadas.Count + confirmadas.Count + aguardandoLente.Count + emMontagem.Count + prontas.Count
                },
                Vendedores = vendedores,
                VendedorFiltro = vendedorId,
                BuscaFiltro = busca ?? "",
                LojaWhatsapp = config?.WhatsappNumero ?? "",
                OticaIdAtual = oticaIdEfetivo,
                NomeOticaAtual = oticaAtual?.Nome ?? "Ótica RETSYS",
                EhAdminOuSistema = ehAdminOuSistema
            });
        }

        // 2. Tela de cadastro de nova OS (Gera e sugere o próximo número sequencial).
        [HttpGet("/ordens/nova")]
        public async Task<IActionResult> Criar()
        {
            var oticaId = ObterOticaId();
            var matrizId = ObterMatrizIdEfetivo(oticaId, _context);
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
            bool ehVendedor = string.Equals(perfilClaim, nameof(PerfilUsuario.Vendedor), StringComparison.OrdinalIgnoreCase) || (!EhAdministrador() && !EhDono() && !EhSistema());

            IQueryable<Usuario> queryVendedores = _context.Usuarios
                .Where(u => u.Ativo && u.OticaId == oticaId);

            if (ehVendedor && Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId))
            {
                queryVendedores = queryVendedores.Where(u => u.Id == usuarioLogadoId);
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
                .Where(lp => lp.Ativo && lp.Lente != null && lp.Lente.Ativo && (lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId))
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
            var usuarioNomeClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? "Colaborador(a)";
            Guid.TryParse(usuarioIdClaim, out Guid idLogado);
            var usuarioLogado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == idLogado);
            string nomeVendedoraExibicao = usuarioLogado?.Nome ?? usuarioNomeClaim;

            return Inertia.Render("OrdensServico/Create", new
            {
                ProximoNumeroOS = proximoNumeroOS,
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes,
                UsuarioLogadoId = idLogado != Guid.Empty ? idLogado.ToString() : null,
                UsuarioLogadoNome = nomeVendedoraExibicao
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
                var idsRede = ObterIdsOticasDaRede(oticaId, _context);
                var perfilClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "VENDEDOR";
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);

                bool ehAdminOuGerente =
                    string.Equals(perfilClaim, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(perfilClaim, "GERENTE", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(perfilClaim, "SISTEMA", StringComparison.OrdinalIgnoreCase);

                Guid vendedorId = Guid.Empty;
                if (!ehAdminOuGerente)
                {
                    // Vendedora padrão SEMPRE emite com seu próprio usuário (Seção 5.3)
                    vendedorId = usuarioLogadoId;
                }
                else if (Guid.TryParse(formCollection["vendedorId"].ToString(), out Guid vIdParsed) && vIdParsed != Guid.Empty)
                {
                    vendedorId = vIdParsed;
                }
                else
                {
                    vendedorId = usuarioLogadoId;
                }

                if (vendedorId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        mensagem = "Selecione uma vendedora válida."
                    });
                }

                var vendedor = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == vendedorId && (u.OticaId == oticaId || idsRede.Contains(u.OticaId)));

                if (vendedor == null || !vendedor.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "Vendedora não localizada ou inativa."
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

                string? cpfFinalOs = string.IsNullOrWhiteSpace(cpfInformado) ? null : cpfInformado;
                string nomeClienteInformado = formCollection["nome"].ToString().Trim();

                if (string.IsNullOrWhiteSpace(nomeClienteInformado))
                {
                    return BadRequest(new
                    {
                        mensagem = "Informe o nome do cliente."
                    });
                }

                Cliente? cliente = null;
                if (!string.IsNullOrEmpty(cpfFinalOs))
                {
                    cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c =>
                            c.OticaId == oticaId &&
                            c.CPF != null &&
                            c.CPF.Replace(".", "").Replace("-", "") == cpfFinalOs);
                }

                if (cliente == null && !string.IsNullOrWhiteSpace(nomeClienteInformado))
                {
                    var nomeLower = nomeClienteInformado.Trim().ToLower();
                    cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c =>
                            c.OticaId == oticaId &&
                            c.Nome.ToLower() == nomeLower);
                }

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Id = Guid.NewGuid(),
                        OticaId = oticaId,
                        CPF = cpfFinalOs,
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

                var oticaObj = await _context.Oticas.AsNoTracking().FirstOrDefaultAsync(o => o.Id == oticaId);
                string oticaNomeAtual = oticaObj?.Nome ?? "Matriz";

                string lojaVenda = formCollection.ContainsKey("lojaVenda") && !string.IsNullOrWhiteSpace(formCollection["lojaVenda"].ToString())
                    ? formCollection["lojaVenda"].ToString()
                    : (!string.IsNullOrWhiteSpace(vendedor.FilialLoja) ? vendedor.FilialLoja : oticaNomeAtual);

                DateTime dataEntradaCalculada = DateTime.UtcNow;
                if (ehAdminOuGerente)
                {
                    string? rawData = formCollection.ContainsKey("dataEmissao") && !string.IsNullOrWhiteSpace(formCollection["dataEmissao"].ToString())
                        ? formCollection["dataEmissao"].ToString()
                        : (formCollection.ContainsKey("dataEntrada") && !string.IsNullOrWhiteSpace(formCollection["dataEntrada"].ToString())
                            ? formCollection["dataEntrada"].ToString()
                            : null);

                    if (!string.IsNullOrWhiteSpace(rawData) && DateTime.TryParse(rawData, out var dtParsed))
                    {
                        var horaAtual = DateTime.UtcNow.TimeOfDay;
                        dataEntradaCalculada = DateTime.SpecifyKind(dtParsed.Date.Add(horaAtual), DateTimeKind.Utc);
                    }
                }

                var novaOS = new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
                    NumeroOS = numeroOsDigitado,
                    ClienteId = cliente.Id,
                    VendedorId = vendedor.Id,
                    LojaVenda = lojaVenda,
                    DataEntrada = dataEntradaCalculada,

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

                if (!adicao.HasValue && formCollection.ContainsKey("odEsfericoPerto") && decimal.TryParse(formCollection["odEsfericoPerto"].ToString(), out var odPerto) && odPerto != 0)
                {
                    adicao = Math.Max(0m, odPerto - odEsferico);
                }

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
                    id = novaOS.Id,
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
        [HttpPost("/ordens/{id:guid}/marcar-lente-pedida")]
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
        [HttpPost("/ordens/{id:guid}/entregar")]
        [HttpPost("/ordens/quitar-e-entregar/{id:guid}")]
        public async Task<IActionResult> QuitarEEntregar(Guid id, [FromBody] QuitarEEntregarRequest? jsonModel)
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

            string opcaoQuitacao = "PAGO_RETIRADA";
            string formaRetirada = "DINHEIRO";
            int? parcelasRetirada = null;

            if (jsonModel != null && !string.IsNullOrWhiteSpace(jsonModel.OpcaoQuitacao))
            {
                opcaoQuitacao = jsonModel.OpcaoQuitacao.Trim();
                formaRetirada = jsonModel.FormaPagamentoRetirada?.ToUpper() ?? "DINHEIRO";
                parcelasRetirada = jsonModel.ParcelasRetirada;
            }
            else if (Request.HasFormContentType)
            {
                opcaoQuitacao = Request.Form["opcaoQuitacao"].ToString().Trim();
                formaRetirada = Request.Form["formaPagamentoRetirada"].ToString().ToUpper();
                if (int.TryParse(Request.Form["parcelasRetirada"].ToString(), out int pRet))
                {
                    parcelasRetirada = pRet;
                }
            }

            if (ordem.Financeiro != null)
            {
                if (opcaoQuitacao == "PAGO_RETIRADA")
                {
                    if (string.IsNullOrWhiteSpace(formaRetirada) || formaRetirada == "CONVENIO")
                    {
                        return BadRequest(new
                        {
                            mensagem = "Selecione uma forma de pagamento válida para a retirada."
                        });
                    }

                    if (formaRetirada != "CARTAO_CREDITO")
                    {
                        parcelasRetirada = null;
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

        // --- NOVO FLUXO DE PRODUÇÃO E ACOMPANHAMENTO ---

        // Ação 1: Confirmar OS (após aprovação do pagamento ou conferência)
        [HttpPost("/ordens/confirmar/{id:guid}")]
        public async Task<IActionResult> ConfirmarOrdem(Guid id)
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null) return NotFound();

            ordem.Status = "CONFIRMADA";
            await _context.SaveChangesAsync();

            return Ok(new { sucesso = true, novoStatus = "CONFIRMADA" });
        }

        // Ação 2: Confirmar pedido de lente com data prevista de entrega do laboratório
        [HttpPost("/ordens/pedir-lente/{id:guid}")]
        public async Task<IActionResult> PedirLente(Guid id, [FromBody] PedirLenteRequest? dados)
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico
                .Include(os => os.Financeiro)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null) return NotFound();

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);

            ordem.LentePedida = true;
            ordem.DataPedidoLente = DateTime.UtcNow;
            ordem.PedidoLentePorId = usuarioLogadoId != Guid.Empty ? usuarioLogadoId : null;

            if (dados != null && dados.DataPrevisaoLente.HasValue)
            {
                ordem.DataPrevisaoLente = DateTime.SpecifyKind(dados.DataPrevisaoLente.Value.Date, DateTimeKind.Utc);
            }
            else if (Request.HasFormContentType && DateTime.TryParse(Request.Form["dataPrevisaoLente"].ToString(), out DateTime dt))
            {
                ordem.DataPrevisaoLente = DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
            }

            ordem.Status = "AGUARDANDO_LENTE";
            await _context.SaveChangesAsync();

            return Ok(new { sucesso = true, novoStatus = "AGUARDANDO_LENTE", dataPrevisaoLente = ordem.DataPrevisaoLente });
        }

        // Ação 3: Marcar que a lente chegou e avançar para fila de Montagem
        [HttpPost("/ordens/lente-chegou/{id:guid}")]
        public async Task<IActionResult> LenteChegou(Guid id)
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null) return NotFound();

            ordem.LenteChegou = true;
            ordem.DataChegadaLente = DateTime.UtcNow;
            ordem.Status = "EM_MONTAGEM";

            await _context.SaveChangesAsync();

            return Ok(new { sucesso = true, novoStatus = "EM_MONTAGEM" });
        }

        // Ação 4: Concluir a montagem dos óculos e avançar para OS Pronta
        [HttpPost("/ordens/concluir-montagem/{id:guid}")]
        public async Task<IActionResult> ConcluirMontagem(Guid id)
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null) return NotFound();

            ordem.Status = "PRONTA";
            await _context.SaveChangesAsync();

            return Ok(new { sucesso = true, novoStatus = "PRONTA" });
        }

        // --- INTEGRAÇÃO WHATSAPP ---
        [HttpGet("/api/ordens/{id:guid}/whatsapp-info")]
        public async Task<IActionResult> ObterInfoWhatsApp(Guid id, [FromQuery] string tipo = "cadastro")
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico
                .Include(os => os.Cliente)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.LentePreco)
                        .ThenInclude(lp => lp!.Lente)
                .Include(os => os.Financeiro)
                    .ThenInclude(f => f!.Armacao)
                .Include(os => os.Otica)
                .FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId && os.Ativo);

            if (ordem == null) return NotFound(new { mensagem = "Ordem de serviço não localizada." });

            var config = await _context.ConfiguracoesLoja.FirstOrDefaultAsync(c => c.OticaId == oticaId);

            string nomeCliente = ordem.Cliente?.Nome ?? "Cliente";
            string primeiroNome = nomeCliente.Split(' ')[0];
            string numeroOs = ordem.NumeroOS;
            string nomeOtica = !string.IsNullOrWhiteSpace(config?.NomeLoja) ? config.NomeLoja : (ordem.Otica?.Nome ?? "Ótica RETSYS");

            string armacaoTexto = !string.IsNullOrWhiteSpace(ordem.Financeiro?.Armacao?.ModeloReferencia)
                ? ordem.Financeiro.Armacao.ModeloReferencia
                : (!string.IsNullOrWhiteSpace(ordem.ArmacaoModeloManual) ? ordem.ArmacaoModeloManual : "Armação Própria / Balcão");

            string lenteTexto = !string.IsNullOrWhiteSpace(ordem.Financeiro?.LentePreco?.Lente?.Tipo)
                ? ordem.Financeiro.LentePreco.Lente.Tipo
                : (!string.IsNullOrWhiteSpace(ordem.LenteDescricaoManual) ? ordem.LenteDescricaoManual : "Lentes Graduadas");

            string resumoPedido = $"👓 *Armação:* {armacaoTexto}\n🔬 *Lentes:* {lenteTexto}";

            string previsaoEntrega = ordem.DataPrevistaEntrega != default
                ? ordem.DataPrevistaEntrega.ToString("dd/MM/yyyy")
                : "A combinar";

            decimal valorRestante = ordem.Financeiro?.ValorRestante ?? 0m;
            string statusPagamento = valorRestante <= 0
                ? "Totalmente Quitado ✅"
                : $"Pendente (Saldo de R$ {valorRestante:N2}) ⏳";

            string saldoDevedorTexto = valorRestante > 0
                ? $"*Valor a acertar na retirada:* R$ {valorRestante:N2}"
                : "Nenhum valor pendente. Seu pedido já está 100% quitado!";

            string template;
            if (string.Equals(tipo, "pronto", StringComparison.OrdinalIgnoreCase))
            {
                template = !string.IsNullOrWhiteSpace(config?.WhatsappMsgProntoTemplate)
                    ? config.WhatsappMsgProntoTemplate
                    : ConfiguracoesController.TEMPLATE_PRONTO_PADRAO;
            }
            else
            {
                template = !string.IsNullOrWhiteSpace(config?.WhatsappMsgCadastroTemplate)
                    ? config.WhatsappMsgCadastroTemplate
                    : ConfiguracoesController.TEMPLATE_CADASTRO_PADRAO;
            }

            string mensagem = template
                .Replace("{cliente}", primeiroNome)
                .Replace("{cliente_completo}", nomeCliente)
                .Replace("{numero_os}", numeroOs)
                .Replace("{otica}", nomeOtica)
                .Replace("{resumo_pedido}", resumoPedido)
                .Replace("{previsao_entrega}", previsaoEntrega)
                .Replace("{status_pagamento}", statusPagamento)
                .Replace("{saldo_devedor}", saldoDevedorTexto);

            string telefoneLimpo = FormatarTelefoneWhatsApp(ordem.Cliente?.Telefone);
            string urlWhatsApp = !string.IsNullOrEmpty(telefoneLimpo)
                ? $"https://api.whatsapp.com/send?phone={telefoneLimpo}&text={Uri.EscapeDataString(mensagem)}"
                : "";

            return Ok(new
            {
                telefone = telefoneLimpo,
                telefoneFormatado = ordem.Cliente?.Telefone ?? "",
                clienteNome = nomeCliente,
                numeroOS = numeroOs,
                mensagem,
                urlWhatsApp,
                notificadoCriacao = ordem.WhatsAppNotificadoCriacao,
                notificadoPronto = ordem.WhatsAppNotificadoPronto
            });
        }

        [HttpPost("/api/ordens/{id:guid}/marcar-whatsapp-enviado")]
        public async Task<IActionResult> MarcarWhatsAppEnviado(Guid id, [FromBody] MarcarWhatsAppRequest? request)
        {
            var oticaId = ObterOticaId();
            var ordem = await _context.OrdensServico.FirstOrDefaultAsync(os => os.Id == id && os.OticaId == oticaId);
            if (ordem == null) return NotFound();

            string tipo = request?.Tipo ?? "cadastro";
            if (string.Equals(tipo, "pronto", StringComparison.OrdinalIgnoreCase))
            {
                ordem.WhatsAppNotificadoPronto = true;
            }
            else
            {
                ordem.WhatsAppNotificadoCriacao = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { sucesso = true });
        }

        private static string FormatarTelefoneWhatsApp(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return string.Empty;
            var digitos = new string(tel.Where(char.IsDigit).ToArray());
            if (digitos.Length == 10 || digitos.Length == 11)
            {
                digitos = "55" + digitos;
            }
            return digitos;
        }

        // 5. Alteração de status padrão.
        [HttpPost("/ordens/{id:guid}/status")]
        [HttpPost("/ordens/alterar-status/{id:guid}")]
        public async Task<IActionResult> AlterarStatus(
            Guid id,
            [FromQuery] string? novoStatus,
            [FromBody] AlterarStatusRequest? bodyModel)
        {
            var oticaId = ObterOticaId();

            string statusAlvo = novoStatus ?? bodyModel?.NovoStatus ?? bodyModel?.Status ?? string.Empty;
            if (string.IsNullOrWhiteSpace(statusAlvo) && Request.HasFormContentType)
            {
                statusAlvo = Request.Form["novoStatus"].ToString();
            }
            statusAlvo = statusAlvo.Trim().ToUpper();

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
                "LANCADA",
                "EM_ABERTO",
                "CONFIRMADA",
                "AGUARDANDO_LENTE",
                "EM_MONTAGEM",
                "EM_LABORATORIO",
                "PRONTA",
                "PRONTO",
                "ENTREGUE",
                "CANCELADA",
                "CANCELADO"
            };

            if (!statusValidos.Contains(statusAlvo) || statusAnterior == statusAlvo)
            {
                return RedirectToAction(nameof(Index));
            }

            if ((statusAlvo == "CANCELADO" || statusAlvo == "CANCELADA") &&
                await OrdemPossuiComissaoPagaAsync(ordem.Id))
            {
                Inertia.Share(
                    "erro",
                    "Esta OS possui comissão já paga. O cancelamento exige um ajuste financeiro manual e auditável."
                );

                return RedirectToAction(nameof(Index));
            }

            if ((statusAlvo == "CANCELADO" || statusAlvo == "CANCELADA") &&
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

            if (statusAlvo == "CANCELADO" || statusAlvo == "CANCELADA")
            {
                await EstornarComissoesDaOrdemAsync(ordem.Id);
            }

            ordem.Status = statusAlvo;

            if (statusAlvo == "ENTREGUE")
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
        [HttpPost("/ordens/{id:guid}/cancelar")]
        [HttpPost("/ordens/cancelar/{id:guid}")]
        [HttpPost("/ordens/excluir/{id:guid}")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            if (!EhAdministrador())
            {
                return Forbid();
            }

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

        // 9. Edição Administrativa Completa da OS pós-emissão (Exclusivo Admin / Gerente - Seção 1 do PDF)
                // 8.1 Tela de EdiÃ§Ã£o Completa da OS pÃ³s-emissÃ£o
        [HttpGet("/ordens/{id:guid}/editar")]
        [HttpGet("/ordens-servico/{id:guid}/editar")]
        public async Task<IActionResult> Editar(Guid id)
        {
            var oticaId = ObterOticaId();
            var idsRede = ObterIdsOticasDaRede(oticaId, _context);

            var os = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Vendedor)
                .Include(o => o.Financeiro)
                    .ThenInclude(f => f!.Armacao)
                .Include(o => o.Financeiro)
                    .ThenInclude(f => f!.LentePreco)
                        .ThenInclude(lp => lp!.Lente)
                .Include(o => o.Receita)
                .Include(o => o.Parcelas)
                .Include(o => o.Otica)
                .FirstOrDefaultAsync(o => o.Id == id && o.Ativo);

            if (os == null)
            {
                return NotFound();
            }

            if (!EhSistema() && os.OticaId != oticaId && !idsRede.Contains(os.OticaId))
            {
                return Forbid();
            }

            var matrizId = ObterMatrizIdEfetivo(oticaId, _context);
            var vendedores = await _context.Usuarios
                .Where(u => u.Ativo && (u.OticaId == oticaId || idsRede.Contains(u.OticaId)))
                .OrderBy(u => u.Nome)
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            var armacoes = await _context.Armacoes
                .Include(a => a.Marca)
                .Where(a => a.OticaId == oticaId && a.Ativo)
                .Select(a => new
                {
                    a.Id,
                    MarcaNome = a.Marca != null ? a.Marca.Nome : "Sem marca",
                    a.ModeloReferencia,
                    a.Cor,
                    a.QuantidadeEstoque,
                    a.PrecoVenda
                })
                .ToListAsync();

            var lentes = await _context.LentesTabelaPrecos
                .Include(lp => lp.Lente)
                .Where(lp => lp.Ativo && lp.Lente != null && lp.Lente.Ativo && (lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId))
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

            var oticas = await _context.Oticas
                .Where(o => o.Id == matrizId || o.MatrizId == matrizId)
                .OrderBy(o => o.Nome)
                .Select(o => new { o.Id, o.Nome })
                .ToListAsync();

            var auditoriaLogs = await _context.OsAuditoriaLogs
                .Include(l => l.Usuario)
                .Where(l => l.OrdemServicoId == os.Id)
                .OrderByDescending(l => l.DataAlteracao)
                .Take(20)
                .Select(l => new
                {
                    l.Id,
                    DataAlteracao = l.DataAlteracao.ToString("dd/MM/yyyy HH:mm"),
                    UsuarioNome = l.Usuario != null ? l.Usuario.Nome : "Sistema",
                    l.CampoAlterado,
                    l.ValorAntigo,
                    l.ValorNovo,
                    l.Descricao
                })
                .ToListAsync();

            var osModel = new
            {
                os.Id,
                os.NumeroOS,
                DataEntradaFormatada = os.DataEntrada.ToString("yyyy-MM-dd"),
                DataPrevistaEntregaFormatada = os.DataPrevistaEntrega != default ? os.DataPrevistaEntrega.ToString("yyyy-MM-dd") : "",
                DataEntregaRealFormatada = os.DataEntregaReal.HasValue ? os.DataEntregaReal.Value.ToString("yyyy-MM-dd") : "",
                os.Status,
                os.LojaVenda,
                os.OticaId,
                os.VendedorId,
                VendedorNome = os.Vendedor?.Nome,
                os.MedicoNome,
                os.MedicoCrm,
                os.MedicoTipo,
                os.Observacoes,
                os.ArmacaoModeloManual,
                os.LenteDescricaoManual,
                os.LaboratorioParceiro,
                os.NumeroPedidoLaboratorio,
                os.EtapaLaboratorio,
                os.LentePedida,

                // Cliente
                Cliente = new
                {
                    os.Cliente.Id,
                    os.Cliente.Nome,
                    os.Cliente.CPF,
                    os.Cliente.Telefone,
                    os.Cliente.Email,
                    os.Cliente.Convenio,
                    os.Cliente.Logradouro,
                    os.Cliente.Numero,
                    os.Cliente.Bairro,
                    os.Cliente.Cidade,
                    os.Cliente.Estado,
                    os.Cliente.Cep,
                    DataNascimentoFormatada = os.Cliente.DataNascimento.HasValue ? os.Cliente.DataNascimento.Value.ToString("yyyy-MM-dd") : ""
                },

                // Receita com Longe e Perto
                Receita = os.Receita != null ? new
                {
                    os.Receita.OdEsferico,
                    os.Receita.OdCilindrico,
                    os.Receita.OdEixo,
                    os.Receita.OeEsferico,
                    os.Receita.OeCilindrico,
                    os.Receita.OeEixo,
                    os.Receita.Adicao,
                    OdEsfericoPerto = os.Receita.OdEsfericoPerto,
                    OeEsfericoPerto = os.Receita.OeEsfericoPerto,
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
                    os.Receita.ObsReceita
                } : null,

                // Financeiro
                Financeiro = os.Financeiro != null ? new
                {
                    os.Financeiro.ArmacaoId,
                    os.Financeiro.LentePrecoId,
                    os.Financeiro.ValorTotalBruto,
                    os.Financeiro.DescontoReais,
                    os.Financeiro.DescontoPercentual,
                    os.Financeiro.ValorTotalLiquido,
                    os.Financeiro.ValorArmacao,
                    os.Financeiro.ValorLente,
                    os.Financeiro.ValorEntrada,
                    os.Financeiro.ValorRestante,
                    os.Financeiro.FormaPagamento,
                    os.Financeiro.Parcelas
                } : null,

                // Parcelas
                Parcelas = os.Parcelas.OrderBy(p => p.NumeroParcela).Select(p => new
                {
                    p.Id,
                    p.NumeroParcela,
                    p.Valor,
                    DataVencimento = p.DataVencimento.ToString("dd/MM/yyyy"),
                    Pago = p.Paga,
                    DataPagamento = p.DataPagamento.HasValue ? p.DataPagamento.Value.ToString("dd/MM/yyyy") : "",
                    FormaPagamento = p.Metodo.HasValue ? p.Metodo.Value.ToString() : (p.Paga ? "PAGO" : "PENDENTE")
                }).ToList()
            };

            return Inertia.Render("OrdensServico/Edit", new
            {
                Ordem = osModel,
                Vendedores = vendedores,
                Armacoes = armacoes,
                Lentes = lentes,
                Oticas = oticas,
                AuditoriaLogs = auditoriaLogs
            });
        }

        // 9. EdiÃ§Ã£o Completa da OS pÃ³s-emissÃ£o
        [HttpPost("/ordens/editar-admin/{id:guid}")]
        [HttpPost("/ordens/{id:guid}/editar")]
        [HttpPost("/ordens-servico/{id:guid}/editar")]
        public async Task<IActionResult> EditarAdmin(
            Guid id,
            [FromBody] EditarAdminRequest? jsonModel)
        {
            var oticaId = ObterOticaId();
            var idsRede = ObterIdsOticasDaRede(oticaId, _context);

            if (!EhAdministrador())
            {
                Inertia.Share("erro", "Apenas Administradores ou Gerentes podem editar dados de OS já emitidas.");
                return RedirectToAction(nameof(Index));
            }

            var os = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Vendedor)
                .Include(o => o.Financeiro)
                .Include(o => o.Receita)
                .Include(o => o.Parcelas)
                .FirstOrDefaultAsync(o => o.Id == id && o.Ativo);

            if (os == null)
            {
                return NotFound();
            }

            if (!EhSistema() && os.OticaId != oticaId && !idsRede.Contains(os.OticaId))
            {
                return Forbid();
            }

            var model = jsonModel ?? new EditarAdminRequest();

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuarioNomeClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
            Guid.TryParse(usuarioIdClaim, out Guid usuarioLogadoId);
            string motivoLog = !string.IsNullOrWhiteSpace(model.Motivo) ? model.Motivo.Trim() : "Edição Administrativa pós-emissão";

            // 1. TRAVA DE PERÍODO FECHADO (Regra 5 do PDF)
            string periodoAntigo = os.DataEntrada.ToString("yyyy-MM");
            string? periodoNovo = model.NovaDataEntrada.HasValue ? model.NovaDataEntrada.Value.ToString("yyyy-MM") : null;

            bool periodoBloqueado = await _context.FechamentosComissao
                .Include(f => f.Vendedor)
                .AnyAsync(f => (f.Vendedor.OticaId == os.OticaId || (os.VendedorId.HasValue && f.VendedorId == os.VendedorId.Value))
                            && (f.PeriodoReferencia == periodoAntigo || (periodoNovo != null && f.PeriodoReferencia == periodoNovo))
                            && (f.Status == "FECHADO" || f.Status == "PAGO"));

            bool alterouData = model.NovaDataEntrada.HasValue && model.NovaDataEntrada.Value.Date != os.DataEntrada.Date;
            bool alterouValor = model.ValorTotalLiquido.HasValue && os.Financeiro != null && Math.Abs(model.ValorTotalLiquido.Value - os.Financeiro.ValorTotalLiquido) > 0.01m;

            if (periodoBloqueado && (alterouData || alterouValor))
            {
                Inertia.Share("erro", "Alteração bloqueada: Esta Ordem de Serviço pertence a um período de comissões já FECHADO ou PAGO! Alterações de data e valor são proibidas.");
                return RedirectToAction(nameof(Index));
            }

            int logsCriados = 0;
            void RegistrarLog(string campo, string? antigo, string? novo)
            {
                string strAntigo = antigo?.Trim() ?? "";
                string strNovo = novo?.Trim() ?? "";
                if (!string.Equals(strAntigo, strNovo, StringComparison.OrdinalIgnoreCase))
                {
                    _context.OsAuditoriaLogs.Add(new OsAuditoriaLog
                    {
                        Id = Guid.NewGuid(),
                        OticaId = os.OticaId,
                        OrdemServicoId = os.Id,
                        UsuarioId = usuarioLogadoId != Guid.Empty ? usuarioLogadoId : (os.VendedorId ?? Guid.Empty),
                        DataAlteracao = DateTime.UtcNow,
                        CampoAlterado = campo,
                        ValorAntigo = string.IsNullOrEmpty(strAntigo) ? "(vazio)" : strAntigo,
                        ValorNovo = string.IsNullOrEmpty(strNovo) ? "(vazio)" : strNovo,
                        Descricao = $"{campo} alterado de '{strAntigo}' para '{strNovo}' por {usuarioNomeClaim}. Motivo: {motivoLog}"
                    });
                    logsCriados++;
                }
            }

            // DADOS GERAIS
            if (!string.IsNullOrWhiteSpace(model.NumeroOS) && model.NumeroOS.Trim() != os.NumeroOS)
            {
                string novoNumero = model.NumeroOS.Trim();
                bool existe = await _context.OrdensServico.AnyAsync(o => o.Id != os.Id && o.NumeroOS == novoNumero && o.OticaId == os.OticaId && o.Ativo);
                if (!existe)
                {
                    RegistrarLog("Número da OS", os.NumeroOS, novoNumero);
                    os.NumeroOS = novoNumero;
                }
            }

            if (model.NovaDataEntrada.HasValue && model.NovaDataEntrada.Value.Date != os.DataEntrada.Date)
            {
                RegistrarLog("Data de Lançamento", os.DataEntrada.ToString("dd/MM/yyyy"), model.NovaDataEntrada.Value.ToString("dd/MM/yyyy"));
                if (!os.DataEntradaOriginal.HasValue)
                {
                    os.DataEntradaOriginal = os.DataEntrada;
                }
                var horaOriginal = os.DataEntrada.TimeOfDay;
                os.DataEntrada = DateTime.SpecifyKind(model.NovaDataEntrada.Value.Date.Add(horaOriginal), DateTimeKind.Utc);
                os.DataAjustadaLog = $"data ajustada em {DateTime.UtcNow.AddHours(-3):dd/MM} por {usuarioNomeClaim}";
            }

            if (model.DataPrevistaEntrega.HasValue && model.DataPrevistaEntrega.Value.Date != os.DataPrevistaEntrega.Date)
            {
                RegistrarLog("Data Prevista de Entrega", os.DataPrevistaEntrega.ToString("dd/MM/yyyy"), model.DataPrevistaEntrega.Value.ToString("dd/MM/yyyy"));
                os.DataPrevistaEntrega = DateTime.SpecifyKind(model.DataPrevistaEntrega.Value.Date, DateTimeKind.Utc);
            }

            if (model.DataEntregaReal.HasValue && (!os.DataEntregaReal.HasValue || model.DataEntregaReal.Value.Date != os.DataEntregaReal.Value.Date))
            {
                RegistrarLog("Data de Entrega Real", os.DataEntregaReal?.ToString("dd/MM/yyyy"), model.DataEntregaReal.Value.ToString("dd/MM/yyyy"));
                os.DataEntregaReal = DateTime.SpecifyKind(model.DataEntregaReal.Value.Date, DateTimeKind.Utc);
            }

            if (!string.IsNullOrWhiteSpace(model.LojaVenda) && model.LojaVenda.Trim() != os.LojaVenda)
            {
                RegistrarLog("Loja da Venda", os.LojaVenda, model.LojaVenda.Trim());
                os.LojaVenda = model.LojaVenda.Trim();
            }

            if (model.OticaId.HasValue && model.OticaId.Value != Guid.Empty && model.OticaId.Value != os.OticaId)
            {
                var novaOtica = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == model.OticaId.Value);
                if (novaOtica != null)
                {
                    RegistrarLog("Unidade/Loja Vínculo", os.Otica?.Nome ?? os.LojaVenda, novaOtica.Nome);
                    os.OticaId = novaOtica.Id;
                    os.LojaVenda = novaOtica.Nome;
                }
            }

            Usuario? novoVendedor = null;
            if (model.VendedorId.HasValue && model.VendedorId.Value != Guid.Empty && model.VendedorId.Value != os.VendedorId)
            {
                novoVendedor = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == model.VendedorId.Value);
                if (novoVendedor != null)
                {
                    RegistrarLog("Vendedora Responsável", os.Vendedor?.Nome, novoVendedor.Nome);
                    os.VendedorId = novoVendedor.Id;
                }
            }

            if (model.MedicoNome != null)
            {
                RegistrarLog("Médico", os.MedicoNome, model.MedicoNome);
                os.MedicoNome = model.MedicoNome;
            }

            if (model.MedicoCrm != null)
            {
                RegistrarLog("CRM", os.MedicoCrm, model.MedicoCrm);
                os.MedicoCrm = model.MedicoCrm;
            }

            if (model.MedicoTipo != null)
            {
                RegistrarLog("Tipo de Profissional", os.MedicoTipo, model.MedicoTipo);
                os.MedicoTipo = model.MedicoTipo;
            }

            if (model.Observacoes != null)
            {
                RegistrarLog("Observações", os.Observacoes, model.Observacoes);
                os.Observacoes = model.Observacoes;
            }

            // STATUS
            if (!string.IsNullOrWhiteSpace(model.Status) && !string.Equals(model.Status, os.Status, StringComparison.OrdinalIgnoreCase))
            {
                RegistrarLog("Status", os.Status, model.Status.ToUpper().Trim());
                os.Status = model.Status.ToUpper().Trim();
                if ((os.Status == "ENTREGUE") && !os.DataEntregaReal.HasValue)
                {
                    os.DataEntregaReal = DateTime.UtcNow;
                }
            }

            // DADOS DO CLIENTE
            if (os.Cliente != null)
            {
                if (model.ClienteNome != null) { RegistrarLog("Cliente: Nome", os.Cliente.Nome, model.ClienteNome); os.Cliente.Nome = model.ClienteNome; }
                if (model.ClienteCpf != null) { RegistrarLog("Cliente: CPF", os.Cliente.CPF, model.ClienteCpf); os.Cliente.CPF = model.ClienteCpf; }
                if (model.ClienteTelefone != null) { RegistrarLog("Cliente: Telefone", os.Cliente.Telefone, model.ClienteTelefone); os.Cliente.Telefone = model.ClienteTelefone; }
                if (model.ClienteEmail != null) { RegistrarLog("Cliente: Email", os.Cliente.Email, model.ClienteEmail); os.Cliente.Email = model.ClienteEmail; }
                if (model.ClienteConvenio != null) { RegistrarLog("Cliente: Convênio", os.Cliente.Convenio, model.ClienteConvenio); os.Cliente.Convenio = model.ClienteConvenio; }
                if (model.ClienteLogradouro != null) { RegistrarLog("Cliente: Logradouro", os.Cliente.Logradouro, model.ClienteLogradouro); os.Cliente.Logradouro = model.ClienteLogradouro; }
                if (model.ClienteNumero != null) { RegistrarLog("Cliente: Número", os.Cliente.Numero, model.ClienteNumero); os.Cliente.Numero = model.ClienteNumero; }
                if (model.ClienteBairro != null) { RegistrarLog("Cliente: Bairro", os.Cliente.Bairro, model.ClienteBairro); os.Cliente.Bairro = model.ClienteBairro; }
                if (model.ClienteCidade != null) { RegistrarLog("Cliente: Cidade", os.Cliente.Cidade, model.ClienteCidade); os.Cliente.Cidade = model.ClienteCidade; }
                if (model.ClienteEstado != null) { RegistrarLog("Cliente: Estado", os.Cliente.Estado, model.ClienteEstado); os.Cliente.Estado = model.ClienteEstado; }
                if (model.ClienteCep != null) { RegistrarLog("Cliente: CEP", os.Cliente.Cep, model.ClienteCep); os.Cliente.Cep = model.ClienteCep; }
                if (model.ClienteDataNascimento.HasValue && os.Cliente.DataNascimento?.Date != model.ClienteDataNascimento.Value.Date)
                {
                    RegistrarLog("Cliente: Data Nascimento", os.Cliente.DataNascimento?.ToString("dd/MM/yyyy"), model.ClienteDataNascimento.Value.ToString("dd/MM/yyyy"));
                    os.Cliente.DataNascimento = DateTime.SpecifyKind(model.ClienteDataNascimento.Value.Date, DateTimeKind.Utc);
                }
            }

            // RECEITA
            if (os.Receita != null)
            {
                if (model.OdEsferico.HasValue) { RegistrarLog("Receita: OD Esférico", os.Receita.OdEsferico.ToString("F2"), model.OdEsferico.Value.ToString("F2")); os.Receita.OdEsferico = model.OdEsferico.Value; }
                if (model.OdCilindrico.HasValue) { RegistrarLog("Receita: OD Cilíndrico", os.Receita.OdCilindrico.ToString("F2"), model.OdCilindrico.Value.ToString("F2")); os.Receita.OdCilindrico = model.OdCilindrico.Value; }
                if (model.OdEixo.HasValue) { RegistrarLog("Receita: OD Eixo", os.Receita.OdEixo.ToString(), model.OdEixo.Value.ToString()); os.Receita.OdEixo = model.OdEixo.Value; }
                if (model.OeEsferico.HasValue) { RegistrarLog("Receita: OE Esférico", os.Receita.OeEsferico.ToString("F2"), model.OeEsferico.Value.ToString("F2")); os.Receita.OeEsferico = model.OeEsferico.Value; }
                if (model.OeCilindrico.HasValue) { RegistrarLog("Receita: OE Cilíndrico", os.Receita.OeCilindrico.ToString("F2"), model.OeCilindrico.Value.ToString("F2")); os.Receita.OeCilindrico = model.OeCilindrico.Value; }
                if (model.OeEixo.HasValue) { RegistrarLog("Receita: OE Eixo", os.Receita.OeEixo.ToString(), model.OeEixo.Value.ToString()); os.Receita.OeEixo = model.OeEixo.Value; }
                if (model.Adicao.HasValue) 
                { 
                    RegistrarLog("Receita: Adição", os.Receita.Adicao?.ToString("F2"), model.Adicao.Value.ToString("F2")); 
                    os.Receita.Adicao = model.Adicao.Value; 
                }
                else if (model.OdEsfericoPerto.HasValue)
                {
                    decimal esfLonge = model.OdEsferico ?? os.Receita.OdEsferico;
                    decimal adCalc = Math.Max(0m, model.OdEsfericoPerto.Value - esfLonge);
                    RegistrarLog("Receita: Adição", os.Receita.Adicao?.ToString("F2"), adCalc.ToString("F2"));
                    os.Receita.Adicao = adCalc;
                }
                if (model.DnpOd.HasValue) { RegistrarLog("Receita: DNP OD", os.Receita.DnpOd.ToString("F1"), model.DnpOd.Value.ToString("F1")); os.Receita.DnpOd = model.DnpOd.Value; }
                if (model.DnpOe.HasValue) { RegistrarLog("Receita: DNP OE", os.Receita.DnpOe.ToString("F1"), model.DnpOe.Value.ToString("F1")); os.Receita.DnpOe = model.DnpOe.Value; }
                if (model.AlturaMontagemOd.HasValue) { RegistrarLog("Receita: Altura OD", os.Receita.AlturaMontagemOd?.ToString("F1"), model.AlturaMontagemOd.Value.ToString("F1")); os.Receita.AlturaMontagemOd = model.AlturaMontagemOd.Value; }
                if (model.AlturaMontagemOe.HasValue) { RegistrarLog("Receita: Altura OE", os.Receita.AlturaMontagemOe?.ToString("F1"), model.AlturaMontagemOe.Value.ToString("F1")); os.Receita.AlturaMontagemOe = model.AlturaMontagemOe.Value; }
                if (model.Aro.HasValue) { RegistrarLog("Receita: ARO", os.Receita.Aro?.ToString("F1"), model.Aro.Value.ToString("F1")); os.Receita.Aro = model.Aro.Value; }
                if (model.Dm.HasValue) { RegistrarLog("Receita: DM", os.Receita.Dm?.ToString("F1"), model.Dm.Value.ToString("F1")); os.Receita.Dm = model.Dm.Value; }
                if (model.Vert.HasValue) { RegistrarLog("Receita: VERT", os.Receita.Vert?.ToString("F1"), model.Vert.Value.ToString("F1")); os.Receita.Vert = model.Vert.Value; }
                if (model.Po.HasValue) { RegistrarLog("Receita: PO", os.Receita.Po?.ToString("F1"), model.Po.Value.ToString("F1")); os.Receita.Po = model.Po.Value; }
                if (model.CoOd.HasValue) { RegistrarLog("Receita: CO OD", os.Receita.CoOd?.ToString("F1"), model.CoOd.Value.ToString("F1")); os.Receita.CoOd = model.CoOd.Value; }
                if (model.CoOe.HasValue) { RegistrarLog("Receita: CO OE", os.Receita.CoOe?.ToString("F1"), model.CoOe.Value.ToString("F1")); os.Receita.CoOe = model.CoOe.Value; }
                if (model.ObsReceita != null) { RegistrarLog("Receita: Observações", os.Receita.ObsReceita, model.ObsReceita); os.Receita.ObsReceita = model.ObsReceita; }
            }

            // PRODUTOS
            if (model.ArmacaoDescricaoManual != null)
            {
                RegistrarLog("Armação", os.ArmacaoModeloManual, model.ArmacaoDescricaoManual);
                os.ArmacaoModeloManual = model.ArmacaoDescricaoManual;
            }
            if (model.LenteDescricaoManual != null)
            {
                RegistrarLog("Lente", os.LenteDescricaoManual, model.LenteDescricaoManual);
                os.LenteDescricaoManual = model.LenteDescricaoManual;
            }

            // FINANCEIRO E RECÁLCULO AUTOMÁTICO (Regra 3 do PDF)
            decimal valLiquidoFinal = os.Financeiro?.ValorTotalLiquido ?? 0m;
            if (os.Financeiro != null)
            {
                decimal armacaoVal = model.ValorArmacao ?? os.Financeiro.ValorArmacao;
                decimal lenteVal = model.ValorLente ?? os.Financeiro.ValorLente;
                decimal brutoVal = armacaoVal + lenteVal;

                decimal descontoVal = model.DescontoReais ?? os.Financeiro.DescontoReais;
                if (descontoVal > brutoVal) descontoVal = brutoVal;

                valLiquidoFinal = Math.Max(0m, brutoVal - descontoVal);

                decimal entradaVal = model.ValorEntrada ?? os.Financeiro.ValorEntrada ?? 0m;
                if (entradaVal > valLiquidoFinal) entradaVal = valLiquidoFinal;

                decimal restanteVal = Math.Max(0m, valLiquidoFinal - entradaVal);
                decimal descontoPerc = brutoVal > 0 ? Math.Round((descontoVal / brutoVal) * 100m, 2) : 0m;

                RegistrarLog("Valor Armação", os.Financeiro.ValorArmacao.ToString("C2"), armacaoVal.ToString("C2"));
                RegistrarLog("Valor Lente", os.Financeiro.ValorLente.ToString("C2"), lenteVal.ToString("C2"));
                RegistrarLog("Valor Bruto", os.Financeiro.ValorTotalBruto.ToString("C2"), brutoVal.ToString("C2"));
                RegistrarLog("Desconto (R$)", os.Financeiro.DescontoReais.ToString("C2"), descontoVal.ToString("C2"));
                RegistrarLog("Valor Líquido", os.Financeiro.ValorTotalLiquido.ToString("C2"), valLiquidoFinal.ToString("C2"));
                RegistrarLog("Valor Entrada", (os.Financeiro.ValorEntrada ?? 0m).ToString("C2"), entradaVal.ToString("C2"));
                RegistrarLog("Valor Restante", os.Financeiro.ValorRestante.ToString("C2"), restanteVal.ToString("C2"));

                if (!string.IsNullOrWhiteSpace(model.FormaPagamento) && model.FormaPagamento != os.Financeiro.FormaPagamento)
                {
                    RegistrarLog("Forma de Pagamento", os.Financeiro.FormaPagamento, model.FormaPagamento);
                    os.Financeiro.FormaPagamento = model.FormaPagamento;
                }

                if (model.Parcelas.HasValue && model.Parcelas.Value != os.Financeiro.Parcelas)
                {
                    RegistrarLog("Parcelas", os.Financeiro.Parcelas.ToString(), model.Parcelas.Value.ToString());
                    os.Financeiro.Parcelas = model.Parcelas.Value;
                }

                os.Financeiro.ValorArmacao = armacaoVal;
                os.Financeiro.ValorLente = lenteVal;
                os.Financeiro.ValorTotalBruto = brutoVal;
                os.Financeiro.DescontoReais = descontoVal;
                os.Financeiro.DescontoPercentual = descontoPerc;
                os.Financeiro.ValorTotalLiquido = valLiquidoFinal;
                os.Financeiro.ValorEntrada = entradaVal;
                os.Financeiro.ValorRestante = restanteVal;
            }

            // RECÁLCULO DE COMISSÃO (Regra 4 do PDF)
            var comissoes = await _context.Comissoes.Where(c => c.OrdemServicoId == os.Id).ToListAsync();
            foreach (var c in comissoes)
            {
                if (novoVendedor != null && novoVendedor.Id != c.VendedorId)
                {
                    c.VendedorId = novoVendedor.Id;
                    if (novoVendedor.PercentualComissao > 0)
                    {
                        c.PercentualAplicado = novoVendedor.PercentualComissao;
                    }
                }

                c.ValorBase = valLiquidoFinal;
                c.ValorComissao = Math.Round(valLiquidoFinal * c.PercentualAplicado / 100m, 2, MidpointRounding.AwayFromZero);
                c.PeriodoReferencia = os.DataEntrada.ToString("yyyy-MM");

                if (os.Status == "CANCELADO" || os.Status == "CANCELADA")
                {
                    c.Status = "CANCELADO";
                }
                else if (c.Status == "CANCELADO")
                {
                    c.Status = "PENDENTE";
                }
            }

            // MARCAÇÃO NA OS (Regra 2 do PDF)
            string dataHoraFormatada = DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm");
            os.UltimaEdicaoLog = $"editada em {dataHoraFormatada} por {usuarioNomeClaim}";

            await _context.SaveChangesAsync();

            Inertia.Share("sucesso", $"OS {os.NumeroOS} atualizada com sucesso pelo administrador ({logsCriados} alteração(ões) auditada(s)).");
            return RedirectToAction(nameof(Index));
        }

        // Endpoint para carregar logs de auditoria da OS para exibição na prancheta
        [HttpGet("/api/ordens/{id:guid}/auditoria")]
        public async Task<IActionResult> ObterAuditoriaLogs(Guid id)
        {
            var logs = await _context.OsAuditoriaLogs
                .Include(l => l.Usuario)
                .Where(l => l.OrdemServicoId == id)
                .OrderByDescending(l => l.DataAlteracao)
                .Select(l => new
                {
                    l.Id,
                    l.DataAlteracao,
                    DataFormatada = l.DataAlteracao.AddHours(-3).ToString("dd/MM/yyyy HH:mm"),
                    UsuarioNome = l.Usuario != null ? l.Usuario.Nome : "Sistema",
                    l.CampoAlterado,
                    l.ValorAntigo,
                    l.ValorNovo,
                    l.Descricao
                })
                .ToListAsync();

            return Ok(logs);
        }
    }

    public class QuitarEEntregarRequest
    {
        public string OpcaoQuitacao { get; set; } = "PAGO_RETIRADA";
        public string FormaPagamentoRetirada { get; set; } = "DINHEIRO";
        public int ParcelasRetirada { get; set; } = 1;
    }

    public class EditarAdminRequest
    {
        public string? NumeroOS { get; set; }
        public DateTime? NovaDataEntrada { get; set; }
        public DateTime? DataPrevistaEntrega { get; set; }
        public DateTime? DataEntregaReal { get; set; }
        public string? LojaVenda { get; set; }
        public Guid? OticaId { get; set; }
        public Guid? VendedorId { get; set; }
        public string? MedicoNome { get; set; }
        public string? MedicoCrm { get; set; }
        public string? MedicoTipo { get; set; }
        public string? Observacoes { get; set; }
        public string? Motivo { get; set; }
        public string? Status { get; set; }

        public string? ClienteNome { get; set; }
        public string? ClienteCpf { get; set; }
        public string? ClienteTelefone { get; set; }
        public DateTime? ClienteDataNascimento { get; set; }
        public string? ClienteEmail { get; set; }
        public string? ClienteConvenio { get; set; }
        public string? ClienteLogradouro { get; set; }
        public string? ClienteNumero { get; set; }
        public string? ClienteBairro { get; set; }
        public string? ClienteCidade { get; set; }
        public string? ClienteEstado { get; set; }
        public string? ClienteCep { get; set; }

        public decimal? OdEsferico { get; set; }
        public decimal? OdCilindrico { get; set; }
        public int? OdEixo { get; set; }
        public decimal? OeEsferico { get; set; }
        public decimal? OeCilindrico { get; set; }
        public int? OeEixo { get; set; }
        public decimal? Adicao { get; set; }
        public decimal? OdEsfericoPerto { get; set; }
        public decimal? OeEsfericoPerto { get; set; }
        public string? ArmacaoModeloManual { get; set; }
        public string? LaboratorioParceiro { get; set; }
        public string? NumeroPedidoLaboratorio { get; set; }
        public bool? LentePedida { get; set; }
        public decimal? DnpOd { get; set; }
        public decimal? DnpOe { get; set; }
        public decimal? AlturaMontagemOd { get; set; }
        public decimal? AlturaMontagemOe { get; set; }
        public decimal? Aro { get; set; }
        public decimal? Dm { get; set; }
        public decimal? Vert { get; set; }
        public decimal? Po { get; set; }
        public decimal? CoOd { get; set; }
        public decimal? CoOe { get; set; }
        public string? ObsReceita { get; set; }

        public string? ArmacaoDescricaoManual { get; set; }
        public string? LenteDescricaoManual { get; set; }

        public decimal? ValorArmacao { get; set; }
        public decimal? ValorLente { get; set; }
        public decimal? DescontoReais { get; set; }
        public decimal? ValorTotalLiquido { get; set; }
        public decimal? ValorEntrada { get; set; }
        public string? FormaPagamento { get; set; }
        public int? Parcelas { get; set; }
    }

    public class AlterarStatusRequest
    {
        public string? NovoStatus { get; set; }
        public string? Status { get; set; }
    }

    public class PedirLenteRequest
    {
        public DateTime? DataPrevisaoLente { get; set; }
    }

    public class MarcarWhatsAppRequest
    {
        public string? Tipo { get; set; } // "cadastro" ou "pronto"
    }
}
