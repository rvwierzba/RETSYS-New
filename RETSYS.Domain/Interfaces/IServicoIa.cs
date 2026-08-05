namespace RETSYS.Domain.Interfaces;

public interface IServicoIa
{
    Task<ResultadoLeituraReceitaDto?> ProcessarFotoReceitaAsync(Stream imagemStream);
}

public class ResultadoLeituraReceitaDto
{
    public decimal? EsfericoLongeDireito { get; set; }
    public decimal? CilindricoLongeDireito { get; set; }
    public int? EixoLongeDireito { get; set; }

    public decimal? EsfericoLongeEsquerdo { get; set; }
    public decimal? CilindricoLongeEsquerdo { get; set; }
    public int? EixoLongeEsquerdo { get; set; }

    public decimal? Adicao { get; set; }
    public string? Medico { get; set; }
}