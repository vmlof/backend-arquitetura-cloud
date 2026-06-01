using System.Security.Claims;
using GestaoRH.Domain.Entities;

namespace GestaoRH.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(Empresa empresa, int expireMinutes = 60);
    string GenerateFuncionarioToken(Funcionario funcionario, int expireMinutes = 60);
    ClaimsPrincipal? ValidateToken(string token);
}
