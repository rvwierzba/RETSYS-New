using RETSYS.Domain.Entities;
using RETSYS.Domain.Enums;
using RETSYS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RETSYS.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicoCriptografia _criptografia;

        public DatabaseSeeder(ApplicationDbContext context, IServicoCriptografia criptografia)
        {
            _context = context;
            _criptografia = criptografia;
        }

        public async Task SemearDadosAsync()
        {
            // Executa automaticamente as Migrations pendentes caso o banco tenha acabado de subir
            await _context.Database.MigrateAsync();

            // 1. Garantir que exista pelo menos uma Ótica matriz no sistema com GUID válido (diferente de Guid.Empty)
            var oticaPadrao = await _context.Oticas.FirstOrDefaultAsync(o => o.Id != Guid.Empty);
            if (oticaPadrao == null)
            {
                // Se só existe a ótica com Guid.Empty, recuperamos o nome dela se houver
                var oticaZero = await _context.Oticas.FirstOrDefaultAsync(o => o.Id == Guid.Empty);
                string nome = (oticaZero != null && !string.IsNullOrWhiteSpace(oticaZero.Nome) && !oticaZero.Nome.Equals("Ótica Padrão", StringComparison.OrdinalIgnoreCase)) 
                    ? oticaZero.Nome 
                    : "Ótica Matriz";

                if (nome.Equals("Matriz", StringComparison.OrdinalIgnoreCase) || nome.Equals("Ótica RETSYS", StringComparison.OrdinalIgnoreCase))
                {
                    nome = "Ótica Matriz";
                }

                oticaPadrao = new Otica
                {
                    Id = Guid.NewGuid(),
                    Nome = nome,
                    CriadoEm = DateTime.UtcNow
                };
                _context.Oticas.Add(oticaPadrao);
                await _context.SaveChangesAsync();
            }

            // 2. Normalização de nomes e aglutinação de duplicatas de 'Matriz' para 'Ótica Matriz'
            try
            {
                var oticaMatrizSemPrefixo = await _context.Oticas.FirstOrDefaultAsync(o => o.Nome == "Matriz");
                var oticaMatrizComPrefixo = await _context.Oticas.FirstOrDefaultAsync(o => o.Nome == "Ótica Matriz");

                if (oticaMatrizSemPrefixo != null && oticaMatrizComPrefixo != null && oticaMatrizSemPrefixo.Id != oticaMatrizComPrefixo.Id)
                {
                    string idOrigem = oticaMatrizSemPrefixo.Id.ToString();
                    string idDestino = oticaMatrizComPrefixo.Id.ToString();

                    using var comandoMerge = _context.Database.GetDbConnection().CreateCommand();
                    await _context.Database.OpenConnectionAsync();
                    comandoMerge.CommandText = $"""
                        UPDATE "usuarios" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "marcas" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "armacoes" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "lentes" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "clientes" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "ordens_servico" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "configuracoes_loja" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        UPDATE "os_auditoria_logs" SET "OticaId" = '{idDestino}' WHERE "OticaId" = '{idOrigem}';
                        DELETE FROM "oticas" WHERE "Id" = '{idOrigem}';
                    """;
                    await comandoMerge.ExecuteNonQueryAsync();
                }
                else if (oticaMatrizSemPrefixo != null && oticaMatrizComPrefixo == null)
                {
                    oticaMatrizSemPrefixo.Nome = "Ótica Matriz";
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auto-Sync Deduplicação Ótica Matriz]: {ex.Message}");
            }

            // 3. Sincronização e aglutinação automática de todos os dados legados com Guid.Empty para a Ótica válida
            try
            {
                using var comando = _context.Database.GetDbConnection().CreateCommand();
                await _context.Database.OpenConnectionAsync();

                string novoOticaId = oticaPadrao.Id.ToString();

                comando.CommandText = $"""
                    UPDATE "usuarios" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "marcas" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "armacoes" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "lentes" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "clientes" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "ordens_servico" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "configuracoes_loja" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    UPDATE "os_auditoria_logs" SET "OticaId" = '{novoOticaId}' WHERE "OticaId" = '00000000-0000-0000-0000-000000000000';
                    DELETE FROM "oticas" WHERE "Id" = '00000000-0000-0000-0000-000000000000';
                """;
                await comando.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auto-Sync MultiTenant Ótica Aviso]: {ex.Message}");
            }

            // 3. Verificar se a tabela de Usuários está vazia
            if (!await _context.Usuarios.AnyAsync())
            {
                // Criando o perfil do Dono da Ótica (Administrador geral)
                var admin = new Usuario
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaPadrao.Id,
                    Nome = "Gerente Geral RETSYS",
                    Email = "admin@otica.com",
                    SenhaHash = _criptografia.CriptografarSenha("Admin@2026"),
                    FilialLoja = "Matriz",
                    Perfil = PerfilUsuario.Admin,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                // Criando um vendedor padrão para testes operacionais
                var vendedor = new Usuario
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaPadrao.Id,
                    Nome = "Vendedor Balcão",
                    Email = "vendedor@otica.com",
                    SenhaHash = _criptografia.CriptografarSenha("Venda@2026"),
                    FilialLoja = "Matriz",
                    Perfil = PerfilUsuario.Vendedor,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                _context.Usuarios.AddRange(admin, vendedor);
                await _context.SaveChangesAsync();
            }

            // 4. Re-vincular registros de Filiais (Travessa Itália, Parque, etc.) às Óticas corretas com base no criador / filial do usuário
            try
            {
                var nomesFiliais = await _context.Usuarios
                    .Select(u => u.FilialLoja)
                    .Union(_context.OrdensServico.Select(os => os.LojaVenda))
                    .Union(_context.Armacoes.Select(a => a.LojaUnidade))
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToListAsync();

                var oticasExistentes = await _context.Oticas.ToListAsync();

                foreach (var nomeFilial in nomesFiliais)
                {
                    if (string.IsNullOrWhiteSpace(nomeFilial)) continue;

                    var oticaExiste = oticasExistentes.FirstOrDefault(o => o.Nome.Trim().Equals(nomeFilial.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (oticaExiste == null)
                    {
                        var novaOtica = new Otica
                        {
                            Id = Guid.NewGuid(),
                            Nome = nomeFilial.Trim(),
                            CriadoEm = DateTime.UtcNow
                        };
                        _context.Oticas.Add(novaOtica);
                        oticasExistentes.Add(novaOtica);
                    }
                }
                await _context.SaveChangesAsync();

                // Sincroniza o OticaId do usuário com a Ótica correspondente à sua FilialLoja
                var todosUsuarios = await _context.Usuarios.ToListAsync();
                foreach (var u in todosUsuarios)
                {
                    if (!string.IsNullOrWhiteSpace(u.FilialLoja))
                    {
                        var oticaFilial = oticasExistentes.FirstOrDefault(o =>
                            o.Nome.Trim().Equals(u.FilialLoja.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            u.FilialLoja.Contains(o.Nome, StringComparison.OrdinalIgnoreCase) ||
                            o.Nome.Contains(u.FilialLoja, StringComparison.OrdinalIgnoreCase));

                        if (oticaFilial != null && u.OticaId != oticaFilial.Id)
                        {
                            u.OticaId = oticaFilial.Id;
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseSeeder Info]: {ex.Message}");
            }
        }
    }
}