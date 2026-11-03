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

            // Obs.: a antiga fusão automática de óticas chamadas "Matriz" foi removida: com o cadastro público,
            // duas empresas diferentes podem ter o mesmo nome e seriam fundidas (dados de uma visíveis na outra).

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
                    FilialLoja = oticaPadrao.Nome,
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
                    FilialLoja = oticaPadrao.Nome,
                    Perfil = PerfilUsuario.Vendedor,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                // Usuário Sistema (acesso a todas as óticas) criado somente em instalação nova
                var usuarioSistema = new Usuario
                {
                    Id = Guid.NewGuid(),
                    OticaId = oticaPadrao.Id,
                    Nome = "Administrador do Sistema (Deus)",
                    Email = "sistema@otica.com",
                    SenhaHash = _criptografia.CriptografarSenha("Sistema@2026"),
                    FilialLoja = oticaPadrao.Nome,
                    Perfil = PerfilUsuario.Sistema,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                _context.Usuarios.AddRange(admin, vendedor, usuarioSistema);
                await _context.SaveChangesAsync();
            }

            // 4. A hierarquia Matriz/Filiais (Otica.MatrizId) e o vínculo de usuários e OSs com a loja (OticaId)
            //    são mantidos apenas pelas telas do sistema. As antigas "sincronizações" executadas a cada
            //    inicialização foram removidas porque:
            //    - anexavam TODA ótica sem matriz à primeira matriz do banco (empresas distintas viravam uma rede);
            //    - reatribuíam o OticaId de usuários e OSs por semelhança de nome (FilialLoja/LojaVenda "contém"
            //      o nome da ótica), movendo registros entre lojas a cada deploy.
        }
    }
}