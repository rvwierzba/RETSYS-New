using System;
using System.Text.Json;
using RETSYS.Web.Json;
using Xunit;

namespace RETSYS.Tests.Web
{
    public class ConversorJsonToleranteTests
    {
        private class DtoFormulario
        {
            public Guid? VendedorId { get; set; }
            public Guid MarcaId { get; set; }
            public DateTime? DataEntregaReal { get; set; }
            public decimal? ValorArmacao { get; set; }
            public decimal PrecoCusto { get; set; }
            public int QuantidadeEstoque { get; set; }
            public bool LentePedida { get; set; }
            public string? Observacoes { get; set; }
        }

        private static readonly JsonSerializerOptions Opcoes = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new ConversorJsonTolerante() }
        };

        [Fact]
        public void CamposVaziosDoFormularioNaoDevemInvalidarODtoInteiro()
        {
            var json = """{"vendedorId":"","marcaId":"","dataEntregaReal":"","valorArmacao":"","precoCusto":"","quantidadeEstoque":"","lentePedida":"","observacoes":"mantida"}""";

            var dto = JsonSerializer.Deserialize<DtoFormulario>(json, Opcoes);

            Assert.NotNull(dto);
            Assert.Null(dto!.VendedorId);
            Assert.Equal(Guid.Empty, dto.MarcaId);
            Assert.Null(dto.DataEntregaReal);
            Assert.Null(dto.ValorArmacao);
            Assert.Equal(0m, dto.PrecoCusto);
            Assert.Equal(0, dto.QuantidadeEstoque);
            Assert.False(dto.LentePedida);
            Assert.Equal("mantida", dto.Observacoes);
        }

        [Fact]
        public void DeveAceitarValoresValidosENumerosComoTexto()
        {
            var id = Guid.NewGuid();
            var json = $$"""{"vendedorId":"{{id}}","dataEntregaReal":"2026-11-03","valorArmacao":"149,90","precoCusto":45.5,"quantidadeEstoque":"3","lentePedida":true}""";

            var dto = JsonSerializer.Deserialize<DtoFormulario>(json, Opcoes);

            Assert.Equal(id, dto!.VendedorId);
            Assert.Equal(new DateTime(2026, 11, 3), dto.DataEntregaReal!.Value.Date);
            Assert.Equal(149.90m, dto.ValorArmacao);
            Assert.Equal(45.5m, dto.PrecoCusto);
            Assert.Equal(3, dto.QuantidadeEstoque);
            Assert.True(dto.LentePedida);
        }

        [Fact]
        public void ValorInvalidoNaoVazioDeveContinuarSendoRejeitado()
        {
            var json = """{"vendedorId":"nao-e-um-guid"}""";

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DtoFormulario>(json, Opcoes));
        }
    }
}
