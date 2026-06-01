using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoRH.Models;
using Microsoft.IdentityModel.Tokens;

namespace GestaoRH.Utils;

public static class Jwt
{
    private static string GetSecretKey(IConfiguration config)
        => config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey nao configurada.");

    public static string GenerateToken(Empresa empresa, IConfiguration config, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
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

    public static string GenerateFuncionarioToken(Funcionario f, IConfiguration config, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
        // Perfil: "chefe" se is_chefe=true, senão "funcionario"
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

    public static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
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
