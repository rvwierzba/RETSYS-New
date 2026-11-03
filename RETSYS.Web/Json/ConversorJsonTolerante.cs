using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RETSYS.Web.Json
{
    /// <summary>
    /// Os formulários Vue enviam "" para campos vazios (datas, ids, números). O System.Text.Json
    /// rejeita "" para Guid/DateTime/decimal/int e, nesse caso, o DTO inteiro do [FromBody] chega nulo
    /// (ex.: a edição de OS não salvava nada). Este conversor trata "" como null/valor padrão e aceita
    /// números enviados como texto (inclusive com vírgula decimal).
    /// </summary>
    public class ConversorJsonTolerante : JsonConverterFactory
    {
        private static readonly Type[] TiposSuportados =
        {
            typeof(Guid), typeof(DateTime), typeof(decimal), typeof(int), typeof(long), typeof(double), typeof(bool)
        };

        public override bool CanConvert(Type typeToConvert)
        {
            var tipoBase = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return Array.IndexOf(TiposSuportados, tipoBase) >= 0;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var tipoBase = Nullable.GetUnderlyingType(typeToConvert);
            if (tipoBase != null)
            {
                return (JsonConverter)Activator.CreateInstance(typeof(ConversorAnulavel<>).MakeGenericType(tipoBase))!;
            }
            return (JsonConverter)Activator.CreateInstance(typeof(ConversorValor<>).MakeGenericType(typeToConvert))!;
        }

        private static bool TentarLer<T>(ref Utf8JsonReader reader, out T valor)
        {
            valor = default!;
            object? lido = null;
            var tipo = typeof(T);

            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return false;
                case JsonTokenType.True:
                case JsonTokenType.False:
                    if (tipo == typeof(bool)) lido = reader.GetBoolean();
                    break;
                case JsonTokenType.Number:
                    if (tipo == typeof(decimal)) lido = reader.GetDecimal();
                    else if (tipo == typeof(int)) lido = reader.TryGetInt32(out var i) ? i : (int)Math.Round(reader.GetDecimal());
                    else if (tipo == typeof(long)) lido = reader.TryGetInt64(out var l) ? l : (long)Math.Round(reader.GetDecimal());
                    else if (tipo == typeof(double)) lido = reader.GetDouble();
                    else if (tipo == typeof(bool)) lido = reader.GetDecimal() != 0;
                    break;
                case JsonTokenType.String:
                    var texto = reader.GetString()?.Trim();
                    if (string.IsNullOrEmpty(texto)) return false;
                    lido = ConverterTexto(texto, tipo);
                    break;
            }

            if (lido == null)
            {
                throw new JsonException($"Valor inválido para o tipo {tipo.Name}.");
            }

            valor = (T)lido;
            return true;
        }

        private static object? ConverterTexto(string texto, Type tipo)
        {
            var normalizado = texto.Contains(',') && !texto.Contains('.') ? texto.Replace(',', '.') : texto;

            if (tipo == typeof(Guid)) return Guid.TryParse(texto, out var g) ? g : null;
            if (tipo == typeof(DateTime))
            {
                return DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var d)
                    || DateTime.TryParse(texto, new CultureInfo("pt-BR"), DateTimeStyles.None, out d) ? d : null;
            }
            if (tipo == typeof(decimal)) return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var m) ? m : null;
            if (tipo == typeof(int)) return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var mi) ? (int)Math.Round(mi) : null;
            if (tipo == typeof(long)) return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var ml) ? (long)Math.Round(ml) : null;
            if (tipo == typeof(double)) return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out var db) ? db : null;
            if (tipo == typeof(bool))
            {
                if (bool.TryParse(texto, out var b)) return b;
                return texto == "1" || texto.Equals("on", StringComparison.OrdinalIgnoreCase) ? true
                    : texto == "0" || texto.Equals("off", StringComparison.OrdinalIgnoreCase) ? false : null;
            }
            return null;
        }

        private class ConversorAnulavel<T> : JsonConverter<T?> where T : struct
        {
            public override bool HandleNull => true;

            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return TentarLer<T>(ref reader, out var valor) ? valor : null;
            }

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                if (value.HasValue) JsonSerializer.Serialize(writer, value.Value);
                else writer.WriteNullValue();
            }
        }

        private class ConversorValor<T> : JsonConverter<T> where T : struct
        {
            public override bool HandleNull => true;

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return TentarLer<T>(ref reader, out var valor) ? valor : default;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value);
            }
        }
    }
}
