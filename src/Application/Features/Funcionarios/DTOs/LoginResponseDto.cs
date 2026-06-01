using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.DTOs;

public class LoginResponseDto
{
    public FuncionarioResponseDto Funcionario { get; set; } = null!;
    public bool SenhaTrocada { get; set; }
    public string Jwt { get; set; } = string.Empty;
}
