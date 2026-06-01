namespace GestaoRH.Models;

public class Funcionario
{
    public int    Id              { get; set; }
    public string Cpf             { get; set; } = string.Empty;
    public string Nome            { get; set; } = string.Empty;
    public string Telefone        { get; set; } = string.Empty;
    public string Email           { get; set; } = string.Empty;
    public string Genero          { get; set; } = string.Empty;
    public string Turno           { get; set; } = string.Empty;
    public int    SetorId         { get; set; }
    public string? NomeSetor      { get; set; }
    public string SenhaTemporaria { get; set; } = string.Empty;
    public string Senha           { get; set; } = string.Empty;
    public bool   SenhaTrocada    { get; set; } = false;
    public bool   IsChefe         { get; set; } = false;   // chefe de setor
    public bool   Ativo           { get; set; } = true;
    public DateTime CriadoEm     { get; set; } = DateTime.UtcNow;
}
