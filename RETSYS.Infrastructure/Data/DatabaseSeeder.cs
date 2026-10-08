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

            // 4. Configurar a Hierarquia Matriz e Filiais (Travessa Itália, Parque, etc. como filiais da Ótica Matriz)
            try
            {
                var oticasExistentes = await _context.Oticas.ToListAsync();
                var matriz = oticasExistentes.FirstOrDefault(o => o.Nome.Equals("Ótica Matriz", StringComparison.OrdinalIgnoreCase)) 
                             ?? oticaPadrao;

                if (matriz != null)
                {
                    matriz.MatrizId = null; // A Matriz não tem MatrizId

                    foreach (var otica in oticasExistentes)
                    {
                        if (otica.Id != matriz.Id)
                        {
                            // Filiais apontam para a Matriz
                            if (!otica.MatrizId.HasValue || otica.MatrizId == Guid.Empty)
                            {
                                otica.MatrizId = matriz.Id;
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

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

                // Sincroniza o OticaId das Ordens de Serviço com a Ótica correspondente à sua LojaVenda
                var todasOS = await _context.OrdensServico.ToListAsync();
                foreach (var os in todasOS)
                {
                    if (!string.IsNullOrWhiteSpace(os.LojaVenda))
                    {
                        var oticaFilial = oticasExistentes.FirstOrDefault(o =>
                            o.Nome.Trim().Equals(os.LojaVenda.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            os.LojaVenda.Contains(o.Nome, StringComparison.OrdinalIgnoreCase) ||
                            o.Nome.Contains(os.LojaVenda, StringComparison.OrdinalIgnoreCase));

                        if (oticaFilial != null && os.OticaId != oticaFilial.Id)
                        {
                            os.OticaId = oticaFilial.Id;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Garantir que exista um usuário com perfil Sistema para testes globais
                var usuarioSistema = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "sistema@otica.com" || u.Perfil == PerfilUsuario.Sistema);
                if (usuarioSistema == null && matriz != null)
                {
                    usuarioSistema = new Usuario
                    {
                        Id = Guid.NewGuid(),
                        OticaId = matriz.Id,
                        Nome = "Administrador do Sistema (Deus)",
                        Email = "sistema@otica.com",
                        SenhaHash = _criptografia.CriptografarSenha("Sistema@2026"),
                        FilialLoja = matriz.Nome,
                        Perfil = PerfilUsuario.Sistema,
                        Ativo = true,
                        CriadoEm = DateTime.UtcNow
                    };
                    _context.Usuarios.Add(usuarioSistema);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseSeeder Info]: {ex.Message}");
            }
        }
    }
}