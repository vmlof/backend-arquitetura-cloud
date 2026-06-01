namespace GestaoRH.Models.DTOs;

public class FuncionarioCadastroDto
{
    public string Cpf      { get; set; } = string.Empty;
    public string Nome     { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Genero   { get; set; } = string.Empty;
    public string Turno    { get; set; } = string.Empty;
    public int    SetorId  { get; set; }
    public bool   IsChefe  { get; set; } = false;
}

public class FuncionarioAtualizarDto
{
    public string Nome     { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Genero   { get; set; } = string.Empty;
    public string Turno    { get; set; } = string.Empty;
    public int    SetorId  { get; set; }
    public bool   IsChefe  { get; set; } = false;
    public bool   Ativo    { get; set; } = true;
}

public class FuncionarioLoginDto
{
    public string Cpf   { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class FuncionarioTrocarSenhaDto
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha  { get; set; } = string.Empty;
}

public class FuncionarioResponseDto
{
    public int      Id           { get; set; }
    public string   Cpf          { get; set; } = string.Empty;
    public string   Nome         { get; set; } = string.Empty;
    public string   Telefone     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
    public string   Genero       { get; set; } = string.Empty;
    public string   Turno        { get; set; } = string.Empty;
    public int      SetorId      { get; set; }
    public string?  NomeSetor    { get; set; }
    public bool     SenhaTrocada { get; set; }
    public bool     IsChefe      { get; set; }
    public bool     Ativo        { get; set; }
    public DateTime CriadoEm    { get; set; }
}

public class FuncionarioRhResponseDto : FuncionarioResponseDto
{
    public string SenhaTemporaria { get; set; } = string.Empty;
}
