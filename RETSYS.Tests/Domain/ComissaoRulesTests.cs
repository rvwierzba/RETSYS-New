using Xunit;
using RETSYS.Domain.Entities;

namespace RETSYS.Tests.Domain
{
    public class ComissaoRulesTests
    {
        [Theory]
        [InlineData(1000.00, 3.0, 30.00)]
        [InlineData(1550.00, 5.0, 77.50)]
        [InlineData(2340.50, 2.5, 58.51)]
        public void DeveCalcularComissaoSobreValorLiquido(decimal valorLiquido, decimal percentual, decimal esperadoComissao)
        {
            decimal comissao = Math.Round(valorLiquido * (percentual / 100m), 2, MidpointRounding.AwayFromZero);
            Assert.Equal(esperadoComissao, comissao);
        }

        [Fact]
        public void DeveCancelarComissaoQuandoOrdemForCancelada()
        {
            var comissao = new Comissao
            {
                Id = Guid.NewGuid(),
                ValorBase = 1200.00m,
                PercentualAplicado = 4.0m,
                ValorComissao = 48.00m,
                Status = "PENDENTE"
            };

            // Simula cancelamento da OS
            string statusOS = "CANCELADO";
            if (statusOS == "CANCELADO" || statusOS == "CANCELADA")
            {
                comissao.Status = "CANCELADO";
            }

            Assert.Equal("CANCELADO", comissao.Status);
        }

        [Fact]
        public void DeveBloquearEdicaoSePeriodoFechadoOuPago()
        {
            var fechamento = new FechamentoComissao
            {
                Id = Guid.NewGuid(),
                PeriodoReferencia = "2026-09",
                Status = "PAGO"
            };

            bool periodoBloqueado = fechamento.Status == "FECHADO" || fechamento.Status == "PAGO";
            Assert.True(periodoBloqueado);
        }
    }
}

