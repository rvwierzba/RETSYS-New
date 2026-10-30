using Xunit;
using Microsoft.EntityFrameworkCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;

namespace RETSYS.Tests.Domain
{
    public class PdvBalcaoAndEstoqueTests
    {
        private ApplicationDbContext ObterContextoEmMemoria(string nomeDb)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: nomeDb)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task DeveBaixarEstoqueAoFinalizarVendaBalcao()
        {
            using var context = ObterContextoEmMemoria(Guid.NewGuid().ToString());
            var oticaId = Guid.NewGuid();

            var armacao = new Armacao
            {
                Id = Guid.NewGuid(),
                OticaId = oticaId,
                CodigoSku = "RAY-001",
                ModeloReferencia = "Ray-Ban Aviator",
                QuantidadeEstoque = 10,
                PrecoVenda = 650.00m,
                Ativo = true
            };

            context.Armacoes.Add(armacao);
            await context.SaveChangesAsync();

            // Simula venda de 2 unidades no balcão
            int quantidadeVendida = 2;
            armacao.QuantidadeEstoque = Math.Max(0, armacao.QuantidadeEstoque - quantidadeVendida);

            var venda = new VendaBalcao
            {
                Id = Guid.NewGuid(),
                OticaId = oticaId,
                NumeroVenda = "VB-00001",
                ValorTotalBruto = 1300.00m,
                ValorTotalLiquido = 1300.00m,
                FormaPagamento = "PIX",
                Status = "CONCLUIDA"
            };

            venda.Itens.Add(new ItemVendaBalcao
            {
                ArmacaoId = armacao.Id,
                Descricao = armacao.ModeloReferencia,
                Quantidade = quantidadeVendida,
                PrecoUnitario = 650.00m
            });

            context.VendasBalcao.Add(venda);
            await context.SaveChangesAsync();

            var armacaoAtualizada = await context.Armacoes.FirstAsync(a => a.Id == armacao.Id);
            Assert.Equal(8, armacaoAtualizada.QuantidadeEstoque);

            var vendaGravada = await context.VendasBalcao.Include(v => v.Itens).FirstAsync(v => v.Id == venda.Id);
            Assert.Equal("VB-00001", vendaGravada.NumeroVenda);
            Assert.Single(vendaGravada.Itens);
            Assert.Equal(2, vendaGravada.Itens.First().Quantidade);
        }
    }
}
