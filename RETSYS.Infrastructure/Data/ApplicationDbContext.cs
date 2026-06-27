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
        public DbSet<Lente> Lentes => Set<Lente>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
        public DbSet<ParcelaPagamento> ParcelasPagamento => Set<ParcelaPagamento>();
        public DbSet<ConfiguracaoLoja> ConfiguracoesLoja => Set<ConfiguracaoLoja>();
        
        // Novas tabelas do Módulo de Comissionamento
        public DbSet<ConfiguracaoComissao> ConfiguracoesComissao => Set<ConfiguracaoComissao>();
        public DbSet<Comissao> Comissoes => Set<Comissao>();
        public DbSet<FechamentoComissao> FechamentosComissao => Set<FechamentoComissao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento da Tabela USUARIOS
            modelBuilder.Entity<Usuario>(b =>
            {
                b.ToTable("usuarios");
                b.HasKey(u => u.Id);
                b.Property(u => u.Nome).IsRequired().HasMaxLength(100);
                b.Property(u => u.Email).IsRequired().HasMaxLength(150);
                b.Property(u => u.SenhaHash).IsRequired();
                b.Property(u => u.FilialLoja).HasMaxLength(100);
                b.Property(u => u.Perfil).HasConversion<int>();
                b.Property(u => u.LimiteDesconto).HasPrecision(5, 2);
                b.Property(u => u.MetaMensal).HasPrecision(18, 2);

                b.HasIndex(u => u.Email).IsUnique();
            });

            // Mapeamento da Tabela MARCAS
            modelBuilder.Entity<Marca>(b =>
            {
                b.ToTable("marcas");
                b.HasKey(m => m.Id);
                b.Property(m => m.Nome).IsRequired().HasMaxLength(100);
                b.Property(m => m.Descricao).HasMaxLength(250);
            });

            // Mapeamento da Tabela ARMACOES
            modelBuilder.Entity<Armacao>(b =>
            {
                b.ToTable("armacoes");
                b.HasKey(a => a.Id);
                b.Property(a => a.CodigoSku).IsRequired().HasMaxLength(50);
                b.Property(a => a.ModeloReferencia).IsRequired().HasMaxLength(100);
                b.Property(a => a.Cor).HasMaxLength(50);
                b.Property(a => a.Tamanho).HasMaxLength(50);
                b.Property(a => a.Material).HasMaxLength(100);
                b.Property(a => a.Fornecedor).HasMaxLength(100);
                b.Property(a => a.PrecoCusto).HasPrecision(18, 2);
                b.Property(a => a.PrecoVenda).HasPrecision(18, 2);

                b.HasOne(a => a.Marca)
                 .WithMany(m => m.Armacoes)
                 .HasForeignKey(a => a.MarcaId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Mapeamento da Tabela LENTES
            modelBuilder.Entity<Lente>(b =>
            {
                b.ToTable("lentes");
                b.HasKey(l => l.Id);
                b.Property(l => l.CodigoSku).IsRequired().HasMaxLength(50);
                b.Property(l => l.Laboratorio).IsRequired().HasMaxLength(100);
                b.Property(l => l.Tipo).IsRequired().HasMaxLength(50);
                b.Property(l => l.Tratamento).HasMaxLength(100);
                b.Property(l => l.IndiceRefracao).HasPrecision(4, 2);
                b.Property(l => l.GraduacaoMin).HasPrecision(5, 2);
                b.Property(l => l.GraduacaoMax).HasPrecision(5, 2);
                b.Property(l => l.PrecoCusto).HasPrecision(18, 2);
                b.Property(l => l.PrecoVenda).HasPrecision(18, 2);
            });

            // Mapeamento da Tabela CLIENTES
            modelBuilder.Entity<Cliente>(b =>
            {
                b.ToTable("clientes");
                b.HasKey(c => c.Id);
                b.Property(c => c.Nome).IsRequired().HasMaxLength(250);
                b.Property(c => c.CPF).IsRequired().HasMaxLength(14);
                b.Property(c => c.Celular).IsRequired().HasMaxLength(20);
                
                b.HasIndex(c => c.CPF).IsUnique();
            });

            // Mapeamento da Tabela ORDENS_SERVICO
            modelBuilder.Entity<OrdemServico>(b =>
            {
                b.ToTable("ordens_servico");
                b.HasKey(os => os.Id);
                b.Property(os => os.NumeroOS).IsRequired().HasMaxLength(50);
                b.Property(os => os.Medico).HasMaxLength(100);
                b.Property(os => os.MedicoCrm).HasMaxLength(50);
                b.Property(os => os.TipoLente).HasMaxLength(100);
                b.Property(os => os.FormaPagamento).HasMaxLength(50);
                b.Property(os => os.Status).HasMaxLength(50);
                
                b.Property(os => os.EsfericoLongeDireito).HasPrecision(5, 2);
                b.Property(os => os.EsfericoLongeEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.CilindricoLongeDireito).HasPrecision(5, 2);
                b.Property(os => os.CilindricoLongeEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.Adicao).HasPrecision(5, 2);
                b.Property(os => os.EsfericoPertoDireito).HasPrecision(5, 2);
                b.Property(os => os.EsfericoPertoEsquerdo).HasPrecision(5, 2);
                b.Property(os => os.CilindricoPertoDireito).HasPrecision(5, 2);
                b.Property(os => os.CilindricoPertoEsquerdo).HasPrecision(5, 2);
                
                b.Property(os => os.DnpOd).HasPrecision(4, 1);
                b.Property(os => os.DnpOe).HasPrecision(4, 1);
                b.Property(os => os.AlturaMontagem).HasPrecision(4, 1);
                
                b.Property(os => os.ValorTotalBruto).HasPrecision(18, 2);
                b.Property(os => os.DescontoReais).HasPrecision(18, 2);
                b.Property(os => os.DescontoPercentual).HasPrecision(5, 2);
                b.Property(os => os.ValorTotal).HasPrecision(18, 2);
                b.Property(os => os.ValorEntrada).HasPrecision(18, 2);

                b.HasOne(os => os.Cliente)
                 .WithMany(c => c.OrdensServico)
                 .HasForeignKey(os => os.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(os => os.Vendedor)
                 .WithMany()
                 .HasForeignKey(os => os.VendedorId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(os => os.Armacao)
                 .WithMany()
                 .HasForeignKey(os => os.ArmacaoId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(os => os.Lente)
                 .WithMany()
                 .HasForeignKey(os => os.LenteId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Mapeamento da Tabela PARCELAS_PAGAMENTO
            modelBuilder.Entity<ParcelaPagamento>(b =>
            {
                b.ToTable("parcelas_pagamento");
                b.HasKey(p => p.Id);
                b.Property(p => p.DescricaoParcela).IsRequired().HasMaxLength(150);
                b.Property(p => p.Valor).HasPrecision(18, 2);
                b.Property(p => p.Metodo).HasConversion<string>().HasMaxLength(50);

                b.HasOne(p => p.OrdemServico)
                 .WithMany(os => os.Parcelas)
                 .HasForeignKey(p => p.OrdemServicoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Mapeamento das Configurações da Loja
            modelBuilder.Entity<ConfiguracaoLoja>(b =>
            {
                b.ToTable("configuracoes_loja");
                b.HasKey(c => c.Id);
                b.Property(c => c.NomeLoja).HasMaxLength(100).IsRequired();
                b.Property(c => c.PixApiKey).HasMaxLength(500);
            });

            // =======================================================
            // MAPEAMENTO DOS NOVOS MODELOS DE COMISSIONAMENTO (MÓDULO 5)
            // =======================================================

            // 1. Configuração de Comissão Global
            modelBuilder.Entity<ConfiguracaoComissao>(b =>
            {
                b.ToTable("configuracao_comissao");
                b.HasKey(cc => cc.Id);
                b.Property(cc => cc.PercentualComissao).HasPrecision(5, 2);
                b.Property(cc => cc.BaseCalculo).HasMaxLength(50).IsRequired();
                b.Property(cc => cc.MomentoGeracao).HasMaxLength(50).IsRequired();

                b.HasOne(cc => cc.UpdatedBy)
                 .WithMany()
                 .HasForeignKey(cc => cc.UpdatedById)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. Registro de Comissões por OS
            modelBuilder.Entity<Comissao>(b =>
            {
                b.ToTable("comissoes");
                b.HasKey(c => c.Id);
                b.Property(c => c.ValorBase).HasPrecision(18, 2);
                b.Property(c => c.PercentualAplicado).HasPrecision(5, 2);
                b.Property(c => c.ValorComissao).HasPrecision(18, 2);
                b.Property(c => c.Status).HasMaxLength(30).IsRequired();
                b.Property(c => c.PeriodoReferencia).HasMaxLength(7).IsRequired();
                b.Property(c => c.Observacoes).HasMaxLength(250);

                b.HasOne(c => c.OrdemServico)
                 .WithMany()
                 .HasForeignKey(c => c.OrdemServicoId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(c => c.Vendedor)
                 .WithMany()
                 .HasForeignKey(c => c.VendedorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Fechamento Mensal de Comissões
            modelBuilder.Entity<FechamentoComissao>(b =>
            {
                b.ToTable("fechamentos_comissao");
                b.HasKey(fc => fc.Id);
                b.Property(fc => fc.PeriodoReferencia).HasMaxLength(7).IsRequired();
                b.Property(fc => fc.TotalVendasBrutas).HasPrecision(18, 2);
                b.Property(fc => fc.TotalComissao).HasPrecision(18, 2);
                b.Property(fc => fc.Status).HasMaxLength(30).IsRequired();

                b.HasOne(fc => fc.Vendedor)
                 .WithMany()
                 .HasForeignKey(fc => fc.VendedorId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(fc => fc.FechadoPor)
                 .WithMany()
                 .HasForeignKey(fc => fc.FechadoPorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}