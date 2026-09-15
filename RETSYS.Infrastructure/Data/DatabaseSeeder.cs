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

            // 1. Garantir que exta pelo menos uma Ótica matriz no sistema
            var oticaPadrao = await _context.Oticas.FirstOrDefaultAsync();
            if (oticaPadrao == null)
            {
                oticaPadrao = new Otica
                {
                    Id = Guid.NewGuid(),
                    Nome = "Ótica RETSYS Demonstrativa",
                    CriadoEm = DateTime.UtcNow
                };
                _context.Oticas.Add(oticaPadrao);
                await _context.SaveChangesAsync();
            }

            // 2. Verificar se a tabela de Usuários está vazia
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
            else
            {
                // Corrigir usuários legados eventualmente sem OticaId
                var usuariosSemOtica = await _context.Usuarios.Where(u => u.OticaId == Guid.Empty).ToListAsync();
                if (usuariosSemOtica.Any())
                {
                    foreach (var u in usuariosSemOtica)
                    {
                        u.OticaId = oticaPadrao.Id;
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}