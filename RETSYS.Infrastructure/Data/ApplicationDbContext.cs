using Microsoft.EntityFrameworkCore;
using RETSYS.Domain.Entities;

namespace RETSYS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Declaração dos DbSets (As tabelas do banco de dados)
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Marca> Marcas => Set<Marca>();
        public DbSet<Armacao> Armacoes => Set<Armacao>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
        public DbSet<ParcelaPagamento> ParcelasPagamento => Set<ParcelaPagamento>();
        public DbSet<ConfiguracaoLoja> ConfiguracoesLoja => Set<ConfiguracaoLoja>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento da Tabela USUARIOS (Tabela 2 do TCC)
            modelBuilder.Entity<Usuario>(b =>
            {
                b.ToTable("usuarios");
                b.HasKey(u => u.Id);
                b.Property(u => u.Nome).IsRequired().HasMaxLength(100);
                b.Property(u => u.Email).IsRequired().HasMaxLength(150);
                b.Property(u => u.SenhaHash).IsRequired();
                b.Property(u => u.FilialLoja).HasMaxLength(100);
                
                // Converte o Enum PerfilUsuario para inteiro no banco de dados
                b.Property(u => u.Perfil).HasConversion<int>();

                // Índice único para garantir que não existam dois usuários com o mesmo e-mail
                b.HasIndex(u => u.Email).IsUnique();
            });

            // Mapeamento da Tabela MARCAS (Nova funcionalidade do MVP)
            modelBuilder.Entity<Marca>(b =>
            {
                b.ToTable("marcas");
                b.HasKey(m => m.Id);
                b.Property(m => m.Nome).IsRequired().HasMaxLength(100);
                b.Property(m => m.Descricao).HasMaxLength(250);
            });

            // Mapeamento da Tabela ARMACOES (Tabela 5 do TCC)
            modelBuilder.Entity<Armacao>(b =>
            {
                b.ToTable("armacoes");
                b.HasKey(a => a.Id);
                b.Property(a => a.Codigo).IsRequired().HasMaxLength(50);
                b.Property(a => a.Modelo).IsRequired().HasMaxLength(100);
                b.Property(a => a.PrecoFinal).HasPrecision(18, 2); // Define precisão monetária correta

                // Relacionamento 1 para Muitos (Uma Marca -> Muitas Armações)
                b.HasOne(a => a.Marca)
                 .WithMany(m => m.Armacoes)
                 .HasForeignKey(a => a.MarcaId)
                 .OnDelete(DeleteBehavior.Restrict); // Impede deletar marca se houver armações vinculadas
            });

            // Mapeamento da Tabela CLIENTES (Tabela 1 do TCC)
            modelBuilder.Entity<Cliente>(b =>
            {
                b.ToTable("clientes");
                b.HasKey(c => c.Id);
                b.Property(c => c.Nome).IsRequired().HasMaxLength(250);
                b.Property(c => c.CPF).IsRequired().HasMaxLength(14);
                b.Property(c => c.Celular).IsRequired().HasMaxLength(20);
                
                // Índice para buscas rápidas por CPF e garantia de unicidade
                b.HasIndex(c => c.CPF).IsUnique();
            });

            // Mapeamento da Tabela ORDENS_SERVICO (Tabela 7 do TCC)
            modelBuilder.Entity<OrdemServico>(b =>
            {
                b.ToTable("ordens_servico");
                b.HasKey(os => os.Id);
                b.Property(os => os.NumeroOS).IsRequired().HasMaxLength(50);
                
                // Mapeamento de precisão decimal para os graus das lentes (essencial no PostgreSQL)
                b.Property(os => os.EsfericoLongeDireito).HasPrecision(5, 2);
                b.Property(os => os.EsfericoLongeEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.CilindricoLongeDireito).HasPrecision(5, 2);
                b.Property(os => os.CilindricoLongeEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.Adicao).HasPrecision(5, 2);
                
                b.Property(os => os.EsfericoPertoDireito).HasPrecision(5, 2);
                b.Property(os => os.EsfericoPertoEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.CilindricoPertoDireito).HasPrecision(5, 2);
                b.Property(os => os.CilindricoPertoEsquerdo).HasPrecision(5, 2);
                
                b.Property(os => os.ValorTotal).HasPrecision(18, 2);

                // Relacionamento Cliente -> Ordens de Serviço
                b.HasOne(os => os.Cliente)
                 .WithMany(c => c.OrdensServico)
                 .HasForeignKey(os => os.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento Vendedor (Usuario) -> Ordens de Serviço
                b.HasOne(os => os.Usuario)
                 .WithMany()
                 .HasForeignKey(os => os.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Mapeamento da Tabela PARCELAS_PAGAMENTO (Tabela 3 do TCC)
            modelBuilder.Entity<ParcelaPagamento>(b =>
            {
                b.ToTable("parcelas_pagamento");
                b.HasKey(p => p.Id);
                b.Property(p => p.DescricaoParcela).IsRequired().HasMaxLength(150);
                b.Property(p => p.Valor).HasPrecision(18, 2);
                
                // Armazena o Enum MetodoPagamento como String no banco (melhor legibilidade gerencial)
                b.Property(p => p.Metodo).HasConversion<string>().HasMaxLength(50);

                // Relacionamento OrdemServico -> Parcelas
                b.HasOne(p => p.OrdemServico)
                 .WithMany(os => os.Parcelas)
                 .HasForeignKey(p => p.OrdemServicoId)
                 .OnDelete(DeleteBehavior.Cascade); // Se a OS for deletada, as parcelas vão junto
            });

            // Mapeamento das Configurações Autônomas da Loção/Ótica (Módulo PIX Opcional)
            modelBuilder.Entity<ConfiguracaoLoja>(b =>
            {
                b.ToTable("configuracoes_loja");
                b.HasKey(c => c.Id);
                b.Property(c => c.NomeLoja).HasMaxLength(100).IsRequired();
                b.Property(c => c.PixApiKey).HasMaxLength(500); // Espaço seguro para o token criptografado
            });
        }
    }
}