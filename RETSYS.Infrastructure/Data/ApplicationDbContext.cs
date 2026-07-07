using Microsoft.EntityFrameworkCore;
using RETSYS.Domain.Entities;

namespace RETSYS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Marca> Marcas => Set<Marca>();
        public DbSet<Armacao> Armacoes => Set<Armacao>();
        public DbSet<Lente> Lentes => Set<Lente>();
        public DbSet<LentePreco> LentesTabelaPrecos => Set<LentePreco>();
        // ❌ REMOVIDO: public DbSet<LenteTratamento> LentesTratamentos => Set<LenteTratamento>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
        public DbSet<OsReceita> OsReceitas => Set<OsReceita>();
        public DbSet<OsFinanceiro> OsFinanceiros => Set<OsFinanceiro>();
        public DbSet<ParcelaPagamento> ParcelasPagamento => Set<ParcelaPagamento>();
        public DbSet<ConfiguracaoLoja> ConfiguracoesLoja => Set<ConfiguracaoLoja>();

        public DbSet<ConfiguracaoComissao> ConfiguracoesComissao => Set<ConfiguracaoComissao>();
        public DbSet<Comissao> Comissoes => Set<Comissao>();
        public DbSet<FechamentoComissao> FechamentosComissao => Set<FechamentoComissao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USUARIOS
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

            // MARCAS
            modelBuilder.Entity<Marca>(b =>
            {
                b.ToTable("marcas");
                b.HasKey(m => m.Id);
                b.Property(m => m.Nome).IsRequired().HasMaxLength(100);
                b.Property(m => m.Descricao).HasMaxLength(250);
            });

            // ARMACOES
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

            // LENTES (catálogo base, sem preço/estoque)
            modelBuilder.Entity<Lente>(b =>
            {
                b.ToTable("lentes");
                b.HasKey(l => l.Id);
                b.Property(l => l.CodigoSku).IsRequired().HasMaxLength(50);
                b.Property(l => l.Laboratorio).IsRequired().HasMaxLength(100);
                b.Property(l => l.Tipo).IsRequired().HasMaxLength(50);
                b.Property(l => l.GraduacaoMin).HasPrecision(5, 2);
                b.Property(l => l.GraduacaoMax).HasPrecision(5, 2);

                // Uma Lente base -> Várias variações de preço/tratamento
                b.HasMany(l => l.Precos)
                 .WithOne(lp => lp.Lente)
                 .HasForeignKey(lp => lp.LenteId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // LENTE_PRECO (variações: índice + tratamento (texto livre) + preço)
            modelBuilder.Entity<LentePreco>(b =>
            {
                b.ToTable("lentes_tabela_precos");
                b.HasKey(lp => lp.Id);
                b.Property(lp => lp.IndiceRefracao).HasPrecision(4, 2);
                b.Property(lp => lp.PrecoCusto).HasPrecision(10, 2);
                b.Property(lp => lp.PrecoVenda).HasPrecision(10, 2);

                // ✅ NOVO: Tratamento agora é apenas um rótulo de texto livre
                //    (sem FK, sem tabela lentes_tratamentos, sem acrescimo_valor)
                b.Property(lp => lp.Tratamento).HasMaxLength(100);

                // ❌ REMOVIDO: relacionamento HasOne(lp => lp.Tratamento) com LenteTratamento
            });

            // ❌ REMOVIDO POR COMPLETO: bloco de configuração de LenteTratamento
            // modelBuilder.Entity<LenteTratamento>(b => { ... });

            // CLIENTES (com campos de migração)
            modelBuilder.Entity<Cliente>(b =>
            {
                b.ToTable("clientes");
                b.HasKey(c => c.Id);
                b.Property(c => c.Nome).IsRequired().HasMaxLength(150);
                b.Property(c => c.CPF).IsRequired().HasMaxLength(14);
                b.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
                b.Property(c => c.Logradouro).IsRequired().HasMaxLength(150);
                b.Property(c => c.Numero).IsRequired().HasMaxLength(10);
                b.Property(c => c.Complemento).HasMaxLength(60);
                b.Property(c => c.Bairro).IsRequired().HasMaxLength(80);
                b.Property(c => c.Cidade).IsRequired().HasMaxLength(80);
                b.Property(c => c.Estado).IsRequired().HasMaxLength(2);
                b.Property(c => c.Cep).IsRequired().HasMaxLength(9);
                b.Property(c => c.Convenio).HasMaxLength(100);
                b.Property(c => c.Email).HasMaxLength(150);
                b.Property(c => c.Observacoes).HasColumnType("text");

                // Campos de migração (legado, opcionais)
                b.Property(c => c.ValorGasto).HasPrecision(10, 2);
                b.Property(c => c.ProdutoAdquirido).HasMaxLength(150);

                b.HasIndex(c => c.CPF).IsUnique();
            });

            // ORDENS_SERVICO
            modelBuilder.Entity<OrdemServico>(b =>
            {
                b.ToTable("ordens_servico");
                b.HasKey(os => os.Id);
                b.Property(os => os.NumeroOS).IsRequired().HasMaxLength(20);
                b.Property(os => os.MedicoNome).HasMaxLength(100);
                b.Property(os => os.MedicoCrm).HasMaxLength(20);
                b.Property(os => os.Status).HasMaxLength(50).IsRequired();
                b.Property(os => os.Observacoes).HasColumnType("text");
                b.Property(os => os.MedicoTipo).HasMaxLength(30).IsRequired().HasDefaultValue("NAO_ESPECIFICADO");

                // Limites de tamanho para os campos manuais de OS retroativa
                b.Property(os => os.ArmacaoModeloManual).HasMaxLength(150);
                b.Property(os => os.LenteDescricaoManual).HasMaxLength(200);

                b.HasOne(os => os.Cliente)
                 .WithMany(c => c.OrdensServico)
                 .HasForeignKey(os => os.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(os => os.Vendedor)
                 .WithMany()
                 .HasForeignKey(os => os.VendedorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // OS_RECEITA (1:1)
            modelBuilder.Entity<OsReceita>(b =>
            {
                b.ToTable("os_receita");
                b.HasKey(r => r.OsId);

                b.Property(r => r.OdEsferico).HasPrecision(5, 2);
                b.Property(r => r.OdCilindrico).HasPrecision(5, 2);
                b.Property(r => r.OeEsferico).HasPrecision(5, 2);
                b.Property(r => r.OeCilindrico).HasPrecision(5, 2);
                b.Property(r => r.Adicao).HasPrecision(4, 2);

                b.Property(r => r.DnpOd).HasPrecision(4, 1);
                b.Property(r => r.DnpOe).HasPrecision(4, 1);
                b.Property(r => r.AlturaMontagem).HasPrecision(4, 1);
                b.Property(r => r.ObsReceita).HasColumnType("text");

                // Garante que o EF nunca tente mapear as propriedades computadas
                b.Ignore(r => r.OdEsfericoPerto);
                b.Ignore(r => r.OeEsfericoPerto);
                b.Ignore(r => r.OdCilindricoPerto);
                b.Ignore(r => r.OeCilindricoPerto);
                b.Ignore(r => r.OdEixoPerto);
                b.Ignore(r => r.OeEixoPerto);

                b.HasOne(r => r.OrdemServico)
                 .WithOne(os => os.Receita)
                 .HasForeignKey<OsReceita>(r => r.OsId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // OS_FINANCEIRO (1:1) — referenciando LentePreco
            modelBuilder.Entity<OsFinanceiro>(b =>
            {
                b.ToTable("os_financeiro");
                b.HasKey(f => f.OsId);

                b.Property(f => f.ValorTotalBruto).HasPrecision(10, 2);
                b.Property(f => f.DescontoReais).HasPrecision(10, 2);
                b.Property(f => f.DescontoPercentual).HasPrecision(5, 2);
                b.Property(f => f.ValorTotalLiquido).HasPrecision(10, 2);
                b.Property(f => f.FormaPagamento).HasMaxLength(50).IsRequired();
                b.Property(f => f.ValorEntrada).HasPrecision(10, 2);
                b.Property(f => f.ValorArmacao).HasPrecision(10, 2).IsRequired();
                b.Property(f => f.ValorLente).HasPrecision(10, 2).IsRequired();
                b.Property(f => f.Parcelas).IsRequired(false);

                b.HasOne(f => f.OrdemServico)
                 .WithOne(os => os.Financeiro)
                 .HasForeignKey<OsFinanceiro>(f => f.OsId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(f => f.Armacao)
                 .WithMany()
                 .HasForeignKey(f => f.ArmacaoId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Aponta para a variação exata (LentePreco), não mais para Lente
                b.HasOne(f => f.LentePreco)
                 .WithMany()
                 .HasForeignKey(f => f.LentePrecoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // PARCELAS_PAGAMENTO
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

            // CONFIGURACOES_LOJA
            modelBuilder.Entity<ConfiguracaoLoja>(b =>
            {
                b.ToTable("configuracoes_loja");
                b.HasKey(c => c.Id);
                b.Property(c => c.NomeLoja).HasMaxLength(100).IsRequired();
                b.Property(c => c.PixApiKey).HasMaxLength(500);
            });

            // MÓDULO DE COMISSIONAMENTO
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
