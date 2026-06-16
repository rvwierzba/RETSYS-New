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

            // 1. Verificar se a tabela de Usuários está vazia
            if (!await _context.Usuarios.AnyAsync())
            {
                // Criando o perfil do Dono da Ótica (Administrador geral)
                var admin = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = "Gerente Geral RETSYS",
                    Email = "admin@otica.com",
                    SenhaHash = _criptografia.CriptografarSenha("Admin@2026"),
                    FilialLoja = "Matriz",
                    Perfil = PerfilUsuario.Admin,
                    Ativo = true
                };

                // Criando um vendedor padrão para testes operacionais
                var vendedor = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = "Vendedor Balcão",
                    Email = "vendedor@otica.com",
                    SenhaHash = _criptografia.CriptografarSenha("Venda@2026"),
                    FilialLoja = "Matriz",
                    Perfil = PerfilUsuario.Vendedor,
                    Ativo = true
                };

                _context.Usuarios.AddRange(admin, vendedor);
                await _context.SaveChangesAsync();
            }
        }
    }
}