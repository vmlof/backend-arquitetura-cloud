namespace GestaoRH.Models.DTOs;

public class EmpresaCadastroDto
{
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string ResponsavelSobrenome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class EmpresaAtualizarDto
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string ResponsavelSobrenome { get; set; } = string.Empty;
}

public class EmpresaLoginDto
{
    public string Cnpj { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class EmpresaResponseDto
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string ResponsavelSobrenome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
