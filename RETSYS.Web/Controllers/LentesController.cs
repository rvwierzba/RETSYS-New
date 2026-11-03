using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RETSYS.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RETSYS.Web.Controllers
{
    public class LentesController : TenantController
    {
        private readonly ApplicationDbContext _context;

        public LentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 0. PÁGINA PRINCIPAL (INERTIA)
        // =========================================================================

        [HttpGet("/lentes")]
        public async Task<IActionResult> Index()
        {
            var oticaId = ObterOticaId();
            var matrizId = ObterMatrizIdEfetivo(oticaId, _context);

            if (oticaId != Guid.Empty)
            {
                await GarantirLentesIniciais(matrizId != Guid.Empty ? matrizId : oticaId);
            }

            var lentes = await _context.Lentes
                .Where(l => l.OticaId == oticaId || l.OticaId == matrizId)
                .OrderBy(l => l.Laboratorio)
                .ThenBy(l => l.Tipo)
                .ToListAsync();

            var precos = await _context.LentesTabelaPrecos
                .Include(p => p.Lente)
                .Where(p => p.Ativo && p.Lente != null && (p.Lente.OticaId == oticaId || p.Lente.OticaId == matrizId))
                .OrderBy(p => p.Lente!.Laboratorio)
                .ToListAsync();

            var tratamentosSugeridos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente != null && (lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            bool isAdmin = EhAdministrador();

            return Inertia.Render("Lentes/Index", new
            {
                Lentes = lentes.Select(l => new
                {
                    l.Id,
                    l.Laboratorio,
                    l.Tipo,
                    l.Surfacada
                }),
                Precos = precos.Select(p => new
                {
                    p.Id,
                    p.LenteId,
                    Lente = new
                    {
                        Laboratorio = p.Lente!.Laboratorio,
                        Tipo = p.Lente.Tipo
                    },
                    p.Tipo,
                    p.IndiceRefracao,
                    p.Tratamento,
                    PrecoCusto = isAdmin ? p.PrecoCusto : 0m,
                    p.PrecoVenda
                }),
                TratamentosSugeridos = tratamentosSugeridos,
                IsAdmin = isAdmin
            });
        }

        // =========================================================================
        // 1. ENDPOINTS DE CONSULTA
        // =========================================================================

        [HttpGet("/api/lentes/calcular-preco")]
        public async Task<IActionResult> CalcularPreco(
            [FromQuery] Guid lenteId,
            [FromQuery] string tipo,
            [FromQuery] decimal indiceRefracao,
            [FromQuery] string? tratamento)
        {
            try
            {
                var oticaId = ObterOticaId();
                var matrizId = ObterMatrizIdEfetivo(oticaId, _context);

                var lente = await _context.Lentes
                    .FirstOrDefaultAsync(l => l.Id == lenteId && (l.OticaId == oticaId || l.OticaId == matrizId));

                if (lente == null)
                {
                    return NotFound(new { mensagem = "Lente não cadastrada no sistema." });
                }

                if (lente.Surfacada)
                {
                    return Ok(new
                    {
                        surfacada = true,
                        precoVenda = 0.00m,
                        mensagem = "Lente surfaçada detetada. O preço pode ser editado manualmente."
                    });
                }

                var query = _context.LentesTabelaPrecos
                    .Where(lp => lp.LenteId == lenteId &&
                                 lp.Tipo == tipo &&
                                 lp.IndiceRefracao == indiceRefracao &&
                                 lp.Ativo);

                query = string.IsNullOrEmpty(tratamento)
                    ? query.Where(lp => string.IsNullOrEmpty(lp.Tratamento))
                    : query.Where(lp => lp.Tratamento == tratamento);

                var precoMatriz = await query.FirstOrDefaultAsync();

                if (precoMatriz == null)
                {
                    return BadRequest(new { mensagem = "Não há preço configurado para este Tipo, Índice de refração e Tratamento selecionados." });
                }

                return Ok(new
                {
                    surfacada = false,
                    precoVenda = precoMatriz.PrecoVenda
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Falha ao processar o cálculo da lente.", detalhes = ex.Message });
            }
        }

        [HttpGet("/api/lentes/tratamentos")]
        public async Task<IActionResult> ListarTratamentos()
        {
            var oticaId = ObterOticaId();
            var matrizId = ObterMatrizIdEfetivo(oticaId, _context);

            var tratamentos = await _context.LentesTabelaPrecos
                .Where(lp => lp.Ativo && !string.IsNullOrEmpty(lp.Tratamento) && lp.Lente != null && (lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId))
                .Select(lp => lp.Tratamento)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tratamentos);
        }

        [HttpGet("/api/lentes/{lenteId:guid}/opcoes-matriz")]
        public async Task<IActionResult> ObterOpcoesMatriz(Guid lenteId)
        {
            var oticaId = ObterOticaId();
            var matrizId = ObterMatrizIdEfetivo(oticaId, _context);

            var opcoes = await _context.LentesTabelaPrecos
                .Where(lp => lp.LenteId == lenteId && lp.Ativo && lp.Lente != null && (lp.Lente.OticaId == oticaId || lp.Lente.OticaId == matrizId))
                .Select(lp => new { lp.Tipo, lp.IndiceRefracao, lp.Tratamento })
                .Distinct()
                .ToListAsync();

            return Ok(opcoes);
        }

        // =========================================================================
        // 2. ENDPOINTS DE ESCRITA (CADASTRO DE LENTE BASE)
        // =========================================================================

        [HttpPost("/lentes")]
        public async Task<IActionResult> CriarLenteBase([FromBody] NovaLenteInput input)
        {
            if (!EhAdministrador())
            {
                MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem cadastrar lentes base.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.Laboratorio) || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Laboratório e Tipo de Bloco são campos obrigatórios.");
                    return RedirectToAction(nameof(Index));
                }

                var oticaId = ObterOticaId();

                if (oticaId == Guid.Empty)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Não foi possível identificar a ótica do usuário logado. Faça login novamente.");
                    return RedirectToAction(nameof(Index));
                }

                var novaLente = new Lente
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaId,
                    CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Laboratorio = input.Laboratorio.Trim(),
                    Tipo = input.Tipo.Trim(),
                    Surfacada = input.Surfacada,
                    GraduacaoMin = -20.00m,
                    GraduacaoMax = 20.00m,
                    Ativo = true
                };

                _context.Lentes.Add(novaLente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao salvar lente: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================================
        // 3. ENDPOINTS DE ESCRITA (MATRIZ DE PREÇOS - LentePreco)
        // =========================================================================

        [HttpPost("/lentes/precos")]
        public async Task<IActionResult> CriarPreco([FromBody] NovoLentePrecoInput input)
        {
            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.Tipo))
                {
                    MensagemFlash.Definir(HttpContext, "erro", "O tipo da variação é obrigatório.");
                    return RedirectToAction(nameof(Index));
                }

                var oticaId = ObterOticaId();
                if (oticaId == Guid.Empty)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Ótica não identificada. Faça login novamente.");
                    return RedirectToAction(nameof(Index));
                }

                Guid targetLenteId = Guid.Empty;

                if (input.LenteId.HasValue && input.LenteId.Value != Guid.Empty)
                {
                    var lenteExiste = await _context.Lentes.AnyAsync(l => l.Id == input.LenteId.Value && l.OticaId == oticaId);
                    if (lenteExiste)
                    {
                        targetLenteId = input.LenteId.Value;
                    }
                }

                if (targetLenteId == Guid.Empty)
                {
                    string lab = string.IsNullOrWhiteSpace(input.Laboratorio) ? "Genérico" : input.Laboratorio.Trim();
                    string bloco = string.IsNullOrWhiteSpace(input.NomeBloco) ? "Padrão" : input.NomeBloco.Trim();

                    var lenteExistente = await _context.Lentes
                        .FirstOrDefaultAsync(l => l.OticaId == oticaId &&
                                                 l.Laboratorio.ToLower() == lab.ToLower() &&
                                                 l.Tipo.ToLower() == bloco.ToLower());

                    if (lenteExistente != null)
                    {
                        targetLenteId = lenteExistente.Id;
                    }
                    else
                    {
                        var novaLente = new Lente
                        {
                            Id = Guid.NewGuid(),
                            OticaId = oticaId,
                            CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                            Laboratorio = lab,
                            Tipo = bloco,
                            Surfacada = input.Surfacada,
                            GraduacaoMin = -20.00m,
                            GraduacaoMax = 20.00m,
                            Ativo = true
                        };

                        _context.Lentes.Add(novaLente);
                        await _context.SaveChangesAsync();
                        targetLenteId = novaLente.Id;
                    }
                }

                var novoPreco = new LentePreco
                {
                    Id = Guid.NewGuid(),
                    LenteId = targetLenteId,
                    Tipo = input.Tipo.Trim(),
                    IndiceRefracao = input.IndiceRefracao,
                    Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim(),
                    PrecoCusto = input.PrecoCusto,
                    PrecoVenda = input.PrecoVenda,
                    Ativo = true
                };

                _context.LentesTabelaPrecos.Add(novoPreco);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao salvar preço na matriz: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpDelete("/lentes/precos/{id:guid}")]
        public async Task<IActionResult> RemoverPreco(Guid id)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = "Apenas administradores podem remover preços da matriz." });
            }

            try
            {
                var oticaId = ObterOticaId();

                var preco = await _context.LentesTabelaPrecos
                    .Include(p => p.Lente)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Lente != null && p.Lente.OticaId == oticaId);

                if (preco == null)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Preço não encontrado na matriz.");
                    return RedirectToAction(nameof(Index));
                }

                // Preço já usado em OS não pode ser apagado (FK Restrict): apenas desativa
                if (await _context.OsFinanceiros.AnyAsync(f => f.LentePrecoId == preco.Id))
                {
                    preco.Ativo = false;
                    MensagemFlash.Definir(HttpContext, "sucesso", "Preço possui OSs vinculadas e foi desativado.");
                }
                else
                {
                    _context.LentesTabelaPrecos.Remove(preco);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao remover preço da matriz: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPut("/lentes/{id:guid}")]
        [HttpPost("/lentes/editar/{id:guid}")]
        public async Task<IActionResult> EditarLenteBase(Guid id, [FromBody] NovaLenteInput input)
        {
            if (!EhAdministrador())
            {
                MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem editar lentes base.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var oticaId = ObterOticaId();
                var lente = await _context.Lentes.FirstOrDefaultAsync(l => l.Id == id && l.OticaId == oticaId);
                if (lente == null)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Lente base não encontrada.");
                    return RedirectToAction(nameof(Index));
                }

                lente.Laboratorio = input.Laboratorio.Trim();
                lente.Tipo = input.Tipo.Trim();
                lente.Surfacada = input.Surfacada;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao editar lente base: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpDelete("/lentes/{id:guid}")]
        [HttpPost("/lentes/excluir/{id:guid}")]
        public async Task<IActionResult> RemoverLenteBase(Guid id)
        {
            if (!EhAdministrador())
            {
                MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem remover lentes base.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var oticaId = ObterOticaId();
                var lente = await _context.Lentes.FirstOrDefaultAsync(l => l.Id == id && l.OticaId == oticaId);
                if (lente == null)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Lente base não encontrada.");
                    return RedirectToAction(nameof(Index));
                }

                // Lente com preços já usados em OS: desativa para preservar o histórico
                if (await _context.OsFinanceiros.AnyAsync(f => f.LentePreco != null && f.LentePreco.LenteId == lente.Id))
                {
                    lente.Ativo = false;
                    MensagemFlash.Definir(HttpContext, "sucesso", "Lente possui OSs vinculadas e foi desativada.");
                }
                else
                {
                    _context.Lentes.Remove(lente);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao remover lente base: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPut("/lentes/precos/{id:guid}")]
        [HttpPost("/lentes/precos/editar/{id:guid}")]
        public async Task<IActionResult> EditarPreco(Guid id, [FromBody] NovoLentePrecoInput input)
        {
            if (!EhAdministrador())
            {
                MensagemFlash.Definir(HttpContext, "erro", "Apenas administradores podem editar preços na matriz.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var oticaId = ObterOticaId();
                var preco = await _context.LentesTabelaPrecos
                    .Include(p => p.Lente)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Lente != null && p.Lente.OticaId == oticaId);

                if (preco == null)
                {
                    MensagemFlash.Definir(HttpContext, "erro", "Preço não encontrado na matriz.");
                    return RedirectToAction(nameof(Index));
                }

                if (input.LenteId.HasValue && input.LenteId.Value != Guid.Empty && input.LenteId.Value != preco.LenteId)
                {
                    // Só pode mover o preço para outra lente da mesma loja
                    bool lenteDaLoja = await _context.Lentes.AnyAsync(l => l.Id == input.LenteId.Value && l.OticaId == oticaId);
                    if (lenteDaLoja)
                    {
                        preco.LenteId = input.LenteId.Value;
                    }
                }
                preco.Tipo = input.Tipo.Trim();
                preco.IndiceRefracao = input.IndiceRefracao;
                preco.Tratamento = string.IsNullOrWhiteSpace(input.Tratamento) ? null : input.Tratamento.Trim();
                preco.PrecoCusto = input.PrecoCusto;
                preco.PrecoVenda = input.PrecoVenda;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemFlash.Definir(HttpContext, "erro", $"Erro ao editar preço da matriz: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================================
        // 4. IMPORTADOR DE TABELA DE FORNECEDOR (texto colado, uma variação por linha)
        // Ex.: "Monofocal 1.56 Antirreflexo - Custo 45,00 Venda 135,00"
        //      "PROGRESSIVA;1.67;Crizal;230;690"
        // =========================================================================

        [HttpPost("/api/admin/lentes/importar-ia")]
        public async Task<IActionResult> ImportarTabela([FromBody] ImportarTabelaLentesInput input)
        {
            if (!EhAdministrador())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { erro = "Apenas administradores podem importar tabelas de lentes." });
            }

            if (input == null || string.IsNullOrWhiteSpace(input.Laboratorio) || string.IsNullOrWhiteSpace(input.TextoBruto))
            {
                return BadRequest(new { erro = "Informe o laboratório e cole as linhas da tabela." });
            }

            var oticaId = ObterOticaId();
            if (oticaId == Guid.Empty)
            {
                return BadRequest(new { erro = "Ótica não identificada. Faça login novamente." });
            }

            var laboratorio = input.Laboratorio.Trim();
            var lentesDoLaboratorio = await _context.Lentes
                .Where(l => l.OticaId == oticaId && l.Laboratorio.ToLower() == laboratorio.ToLower())
                .ToListAsync();

            int importadas = 0;
            var ignoradas = new List<string>();

            foreach (var linhaOriginal in input.TextoBruto.Split('\n'))
            {
                var linha = linhaOriginal.Trim();
                if (linha.Length == 0) continue;

                var variacao = InterpretarLinhaTabela(linha);
                if (variacao == null)
                {
                    ignoradas.Add(linha);
                    continue;
                }

                var nomeBloco = System.Globalization.CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(variacao.Tipo.ToLower());
                var lente = lentesDoLaboratorio.FirstOrDefault(l => l.Tipo.Equals(nomeBloco, StringComparison.OrdinalIgnoreCase));
                if (lente == null)
                {
                    lente = new Lente
                    {
                        Id = Guid.NewGuid(),
                        OticaId = oticaId,
                        CodigoSku = $"LNT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Laboratorio = laboratorio,
                        Tipo = nomeBloco,
                        Surfacada = variacao.Tipo is "PROGRESSIVA" or "BIFOCAL" or "OCUPACIONAL",
                        GraduacaoMin = -20.00m,
                        GraduacaoMax = 20.00m,
                        Ativo = true
                    };
                    _context.Lentes.Add(lente);
                    lentesDoLaboratorio.Add(lente);
                }

                _context.LentesTabelaPrecos.Add(new LentePreco
                {
                    Id = Guid.NewGuid(),
                    LenteId = lente.Id,
                    Tipo = variacao.Tipo,
                    IndiceRefracao = variacao.Indice,
                    Tratamento = variacao.Tratamento,
                    PrecoCusto = variacao.Custo,
                    PrecoVenda = variacao.Venda,
                    Ativo = true
                });
                importadas++;
            }

            if (importadas == 0)
            {
                return BadRequest(new { erro = "Nenhuma linha reconhecida. Use uma variação por linha com tipo (Monofocal, Bifocal, Progressiva, Ocupacional), índice (ex.: 1.56) e o custo (e opcionalmente a venda)." });
            }

            await _context.SaveChangesAsync();

            var mensagem = $"{importadas} variação(ões) importada(s) para {laboratorio}.";
            if (ignoradas.Count > 0) mensagem += $" {ignoradas.Count} linha(s) não reconhecida(s) foram ignoradas.";
            return Ok(new { mensagem, importadas, ignoradas });
        }

        private record VariacaoImportada(string Tipo, decimal Indice, string? Tratamento, decimal Custo, decimal Venda);

        private static VariacaoImportada? InterpretarLinhaTabela(string linha)
        {
            var upper = linha.ToUpperInvariant();
            string? tipo = upper.Contains("PROGRESS") || upper.Contains("MULTIFOCAL") ? "PROGRESSIVA"
                : upper.Contains("BIFOCAL") ? "BIFOCAL"
                : upper.Contains("OCUPACIONAL") ? "OCUPACIONAL"
                : upper.Contains("MONOFOCAL") || upper.Contains("VISAO SIMPLES") || upper.Contains("VISÃO SIMPLES") ? "MONOFOCAL"
                : null;
            if (tipo == null) return null;

            var indiceMatch = Regex.Match(linha, @"\b1[.,](\d{2})\b");
            if (!indiceMatch.Success) return null;
            var indice = decimal.Parse("1." + indiceMatch.Groups[1].Value, CultureInfo.InvariantCulture);

            // Valores monetários depois do índice (aceita 1.234,56 / 1234.56 / 45,00 / 45)
            var resto = linha.Substring(indiceMatch.Index + indiceMatch.Length);
            var valores = Regex.Matches(resto, @"\d{1,3}(?:\.\d{3})+,\d{2}|\d+(?:[.,]\d{1,2})?")
                .Select(m => ConverterValorMonetario(m.Value))
                .Where(v => v.HasValue && v.Value > 0)
                .Select(v => v!.Value)
                .ToList();
            if (valores.Count == 0) return null;

            var custo = valores[0];
            var venda = valores.Count > 1 ? valores[1] : valores[0];

            // Tratamento: texto entre o índice e o primeiro número, sem rótulos e separadores
            var primeiroNumero = Regex.Match(resto, @"\d");
            var tratamento = primeiroNumero.Success ? resto[..primeiroNumero.Index] : resto;
            tratamento = Regex.Replace(tratamento, @"(?i)\b(custo|venda|pre[cç]o)\b|R\$", " ");
            tratamento = Regex.Replace(tratamento, @"\s+", " ").Trim(' ', '-', ';', ':', '|', '\t', ',');

            return new VariacaoImportada(tipo, indice, string.IsNullOrWhiteSpace(tratamento) ? null : tratamento, custo, venda);
        }

        private static decimal? ConverterValorMonetario(string texto)
        {
            var normalizado = texto.Contains(',') ? texto.Replace(".", "").Replace(',', '.') : texto;
            return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
                ? Math.Round(valor, 2)
                : null;
        }

        // =========================================================================
        // AUXILIAR
        // =========================================================================

        private async Task GarantirLentesIniciais(Guid oticaId)
        {
            if (oticaId == Guid.Empty) return;

            bool jaExiste = await _context.Lentes.AnyAsync(l => l.OticaId == oticaId);
            if (jaExiste) return;

            var l1 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ESS-01", Laboratorio = "Essilor", Tipo = "Monofocal Orma", Surfacada = false, GraduacaoMin = -10.00m, GraduacaoMax = 10.00m, Ativo = true };
            var l2 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ESS-02", Laboratorio = "Essilor", Tipo = "Varilux Comfort", Surfacada = true, GraduacaoMin = -12.00m, GraduacaoMax = 12.00m, Ativo = true };
            var l3 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-HOY-01", Laboratorio = "Hoya", Tipo = "Miyosmart BlueControl", Surfacada = false, GraduacaoMin = -10.00m, GraduacaoMax = 10.00m, Ativo = true };
            var l4 = new Lente { Id = Guid.NewGuid(), OticaId = oticaId, CodigoSku = "LNT-ZEI-01", Laboratorio = "Zeiss", Tipo = "Progressiva Light D", Surfacada = true, GraduacaoMin = -15.00m, GraduacaoMax = 15.00m, Ativo = true };

            _context.Lentes.AddRange(l1, l2, l3, l4);

            _context.LentesTabelaPrecos.AddRange(
                new LentePreco { Id = Guid.NewGuid(), LenteId = l1.Id, Tipo = "MONOFOCAL", IndiceRefracao = 1.50m, Tratamento = "Antirreflexo Crizal", PrecoCusto = 45.00m, PrecoVenda = 135.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l2.Id, Tipo = "PROGRESSIVA", IndiceRefracao = 1.56m, Tratamento = "Antirreflexo Premium", PrecoCusto = 160.00m, PrecoVenda = 480.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l3.Id, Tipo = "MONOFOCAL", IndiceRefracao = 1.60m, Tratamento = "Filtro Azul (BlueCut)", PrecoCusto = 85.00m, PrecoVenda = 270.00m, Ativo = true },
                new LentePreco { Id = Guid.NewGuid(), LenteId = l4.Id, Tipo = "PROGRESSIVA", IndiceRefracao = 1.67m, Tratamento = "DuraVision Platinum", PrecoCusto = 230.00m, PrecoVenda = 690.00m, Ativo = true }
            );

            await _context.SaveChangesAsync();
        }
    }

    public class ImportarTabelaLentesInput
    {
        public string Laboratorio { get; set; } = string.Empty;
        public string TextoBruto { get; set; } = string.Empty;
    }

    public class NovaLenteInput
    {
        public string Laboratorio { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Surfacada { get; set; }
    }

    public class NovoLentePrecoInput
    {
        public Guid? LenteId { get; set; }
        public string? Laboratorio { get; set; }
        public string? NomeBloco { get; set; }
        public bool Surfacada { get; set; }
        public string Tipo { get; set; } = "MONOFOCAL";
        public decimal IndiceRefracao { get; set; }
        public string? Tratamento { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
    }
}
