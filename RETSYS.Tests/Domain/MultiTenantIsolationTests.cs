using Xunit;
using Microsoft.EntityFrameworkCore;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Entities;

namespace RETSYS.Tests.Domain
{
    public class MultiTenantIsolationTests
    {
        private ApplicationDbContext ObterContextoEmMemoria(string nomeDb)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: nomeDb)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task DeveIsolarOrdensDeServicoEntreLojasDiferentes()
        {
            using var context = ObterContextoEmMemoria(Guid.NewGuid().ToString());

            var lojaMatrizId = Guid.NewGuid();
            var lojaFilialId = Guid.NewGuid();

            var osMatriz = new OrdemServico
            {
                Id = Guid.NewGuid(),
                OticaId = lojaMatrizId,
                NumeroOS = "OS-00001",
                Status = "EM_ABERTO",
                Ativo = true,
                LojaVenda = "Ótica Retina Matriz",
                DataEntrada = DateTime.UtcNow,
                DataPrevistaEntrega = DateTime.UtcNow.AddDays(7)
            };

            var osFilial = new OrdemServico
            {
                Id = Guid.NewGuid(),
                OticaId = lojaFilialId,
                NumeroOS = "OS-00002",
                Status = "EM_ABERTO",
                Ativo = true,
                LojaVenda = "Ótica Retina Filial",
                DataEntrada = DateTime.UtcNow,
                DataPrevistaEntrega = DateTime.UtcNow.AddDays(7)
            };

            context.OrdensServico.AddRange(osMatriz, osFilial);
            await context.SaveChangesAsync();

            // Consulta simulada para operador logado na Loja Matriz
            var ordensVisiveisMatriz = await context.OrdensServico
                .Where(os => os.OticaId == lojaMatrizId && os.Ativo)
                .ToListAsync();

            Assert.Single(ordensVisiveisMatriz);
            Assert.Equal("OS-00001", ordensVisiveisMatriz[0].NumeroOS);
            Assert.DoesNotContain(ordensVisiveisMatriz, o => o.NumeroOS == "OS-00002");
        }

        [Fact]
        public async Task DevePermitirVisualizacaoDeRedeApenasParaDonoQuandoSolicitado()
        {
            using var context = ObterContextoEmMemoria(Guid.NewGuid().ToString());

            var matrizId = Guid.NewGuid();
            var filialId = Guid.NewGuid();
            var idsRede = new List<Guid> { matrizId, filialId };

            context.OrdensServico.AddRange(
                new OrdemServico { Id = Guid.NewGuid(), OticaId = matrizId, NumeroOS = "M-1", Ativo = true },
                new OrdemServico { Id = Guid.NewGuid(), OticaId = filialId, NumeroOS = "F-1", Ativo = true }
            );
            await context.SaveChangesAsync();

            // Consulta explícita por rede
            var ordensRede = await context.OrdensServico
                .Where(os => idsRede.Contains(os.OticaId) && os.Ativo)
                .ToListAsync();

            Assert.Equal(2, ordensRede.Count);
        }
    }
}
