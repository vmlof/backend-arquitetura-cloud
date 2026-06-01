namespace GestaoRH.Models.DTOs;

public class SetorCadastroDto
{
    public string Nome      { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}

public class SetorAtualizarDto
{
    public string Nome      { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool   Ativo     { get; set; } = true;   // permite ativar/desativar via edição
}

public class SetorResponseDto
{
    public int      Id        { get; set; }
    public string   Nome      { get; set; } = string.Empty;
    public string   Descricao { get; set; } = string.Empty;
    public bool     Ativo     { get; set; }
    public DateTime CriadoEm  { get; set; }
}
