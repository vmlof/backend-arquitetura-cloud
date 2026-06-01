namespace GestaoRH.Models;

public class Empresa
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string ResponsavelSobrenome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
