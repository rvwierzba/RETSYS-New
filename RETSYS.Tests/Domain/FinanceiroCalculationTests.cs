using Xunit;
using RETSYS.Domain.Entities;

namespace RETSYS.Tests.Domain
{
    public class FinanceiroCalculationTests
    {
        [Fact]
        public void DeveCalcularTotaisComEntradaEDesconto()
        {
            decimal valorArmacao = 450.00m;
            decimal valorLente = 850.00m;
            decimal bruto = valorArmacao + valorLente;
            decimal desconto = 100.00m;
            decimal liquido = Math.Max(0m, bruto - desconto);
            decimal entrada = 300.00m;
            decimal restante = Math.Max(0m, liquido - entrada);

            var financeiro = new OsFinanceiro
            {
                OsId = Guid.NewGuid(),
                ValorArmacao = valorArmacao,
                ValorLente = valorLente,
                ValorTotalBruto = bruto,
                DescontoReais = desconto,
                ValorTotalLiquido = liquido,
                ValorEntrada = entrada,
                ValorRestante = restante,
                FormaPagamento = "CARTAO_CREDITO",
                Parcelas = 3
            };

            Assert.Equal(1300.00m, financeiro.ValorTotalBruto);
            Assert.Equal(1200.00m, financeiro.ValorTotalLiquido);
            Assert.Equal(900.00m, financeiro.ValorRestante);
            Assert.Equal(300.00m, financeiro.ValorRestante / financeiro.Parcelas);
        }

        [Fact]
        public void NaoDevePermitirDescontoMaiorQueValorBruto()
        {
            decimal bruto = 500.00m;
            decimal descontoSolicitado = 650.00m;
            decimal descontoAplicado = Math.Min(bruto, descontoSolicitado);
            decimal liquido = Math.Max(0m, bruto - descontoAplicado);

            Assert.Equal(500.00m, descontoAplicado);
            Assert.Equal(0.00m, liquido);
        }

        [Fact]
        public void NaoDevePermitirEntradaMaiorQueValorLiquido()
        {
            decimal liquido = 800.00m;
            decimal entradaSolicitada = 1000.00m;
            decimal entradaAplicada = Math.Min(liquido, entradaSolicitada);
            decimal restante = Math.Max(0m, liquido - entradaAplicada);

            Assert.Equal(800.00m, entradaAplicada);
            Assert.Equal(0.00m, restante);
        }
    }
}
