using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GestaoRH.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    private string GetSecretKey()
        => _config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey nao configurada.");

    public string GenerateToken(Empresa empresa, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey());
        var claims = new[]
        {
            new Claim("Id",          empresa.Id.ToString()),
            new Claim("Cnpj",        empresa.Cnpj),
            new Claim("RazaoSocial", empresa.RazaoSocial),
            new Claim("Responsavel", $"{empresa.ResponsavelNome} {empresa.ResponsavelSobrenome}"),
            new Claim("Perfil",      "empresa")
        };
        return BuildToken(claims, key, expireMinutes);
    }

    public string GenerateFuncionarioToken(Funcionario f, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey());
        var perfil = f.IsChefe ? "chefe" : "funcionario";

        var claims = new[]
        {
            new Claim("Id",       f.Id.ToString()),
            new Claim("Cpf",      f.Cpf),
            new Claim("Nome",     f.Nome),
            new Claim("Email",    f.Email),
            new Claim("SetorId",  f.SetorId.ToString()),
            new Claim("IsChefe",  f.IsChefe.ToString().ToLower()),
            new Claim("Perfil",   perfil)
        };
        return BuildToken(claims, key, expireMinutes);
    }

    private static string BuildToken(Claim[] claims, byte[] key, int expireMinutes)
    {
        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey());
        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false, ValidateAudience = false,
                ValidateLifetime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch { return null; }
    }
}
