using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Empresas.DTOs;

public class EmpresaLoginResponseDto
{
    public EmpresaResponseDto Empresa { get; set; } = null!;
    public string Jwt { get; set; } = string.Empty;
}
