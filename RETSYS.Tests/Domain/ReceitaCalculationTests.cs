using Xunit;
using RETSYS.Domain.Entities;

namespace RETSYS.Tests.Domain
{
    public class ReceitaCalculationTests
    {
        [Theory]
        [InlineData(1.50, 2.00, 3.50)] // Longe +2.00 -> Perto +3.50
        [InlineData(-2.00, 2.50, 0.50)] // Longe -2.00 + Ad 2.50 -> Perto +0.50
        [InlineData(0.00, 1.75, 1.75)] // Plana + Ad 1.75 -> Perto +1.75
        public void DeveCalcularEsfericoPertoCorretamente(decimal longe, decimal adicao, decimal esperadoPerto)
        {
            decimal perto = longe + adicao;
            Assert.Equal(esperadoPerto, perto);
        }

        [Theory]
        [InlineData(3.50, 1.50, 2.00)]
        [InlineData(0.50, -2.00, 2.50)]
        [InlineData(2.00, 0.00, 2.00)]
        public void DeveCalcularAdicaoAPartirDoPertoELonge(decimal perto, decimal longe, decimal esperadaAdicao)
        {
            decimal adicao = Math.Max(0m, perto - longe);
            Assert.Equal(esperadaAdicao, adicao);
        }

        [Fact]
        public void DeveDividirDpTotalEmDnpSimetrico()
        {
            decimal dpTotal = 64.0m;
            decimal dnpOd = Math.Round(dpTotal / 2m, 1);
            decimal dnpOe = Math.Round(dpTotal / 2m, 1);

            Assert.Equal(32.0m, dnpOd);
            Assert.Equal(32.0m, dnpOe);
        }

        [Fact]
        public void DeveSomarDnpEmDpTotal()
        {
            decimal dnpOd = 31.5m;
            decimal dnpOe = 32.0m;
            decimal dpTotal = dnpOd + dnpOe;

            Assert.Equal(63.5m, dpTotal);
        }

        [Fact]
        public void DevePersistirMedidasLaboratoriaisNaReceita()
        {
            var receita = new OsReceita
            {
                OsId = Guid.NewGuid(),
                OdEsferico = -1.75m,
                OdCilindrico = -0.50m,
                OdEixo = 180,
                OeEsferico = -2.00m,
                OeCilindrico = -0.75m,
                OeEixo = 90,
                Adicao = 2.00m,
                DnpOd = 32.0m,
                DnpOe = 32.0m,
                AlturaMontagemOd = 19.5m,
                AlturaMontagemOe = 19.5m,
                Aro = 54.0m,
                Po = 18.0m,
                Dm = 58.0m,
                Vert = 38.0m,
                CoOd = 32.0m,
                CoOe = 32.0m,
                ObsReceita = "Montagem digital com chanfro fino"
            };

            Assert.Equal(54.0m, receita.Aro);
            Assert.Equal(18.0m, receita.Po);
            Assert.Equal(58.0m, receita.Dm);
            Assert.Equal(38.0m, receita.Vert);
            Assert.Equal("Montagem digital com chanfro fino", receita.ObsReceita);
        }
    }
}
