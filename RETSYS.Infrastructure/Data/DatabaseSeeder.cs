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
                    : "Ótica RETSYS";

                oticaPadrao = new Otica
                {
                    Id = Guid.NewGuid(),
                    Nome = nome,
                    CriadoEm = DateTime.UtcNow
                };
                _context.Oticas.Add(oticaPadrao);
                await _context.SaveChangesAsync();
            }

            // 2. Sincronização e aglutinação automática de todos os dados legados com Guid.Empty para a Ótica válida
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
        }
    }
}