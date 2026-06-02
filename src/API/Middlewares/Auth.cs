using GestaoRH.Infrastructure.Security;

namespace GestaoRH.API.Middlewares;

public class Auth
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> RotasPublicas = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/empresa/cadastrar",
        "/api/empresa/login",
        "/api/funcionario/login",
        "/aggregated-data",
        "/people",
        "/documents"
    };

    public Auth(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = (context.Request.Path.Value ?? string.Empty).TrimEnd('/').ToLowerInvariant();
        var protegerRotasBff = _config.GetValue("BffSecurity:ProtectRoutes", false);

        if (RotasPublicas.Contains(path) || path.StartsWith("/openapi") || path.StartsWith("/scalar"))
        {
            await _next(context);
            return;
        }

        var deveAutenticar = path.StartsWith("/api/")
            || (protegerRotasBff && (
                path == "/aggregated-data"
                || path.StartsWith("/people/")
                || path.StartsWith("/documents/")));

        if (deveAutenticar)
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();
            var token = (header?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false)
                ? header["Bearer ".Length..].Trim()
                : null;

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token nao informado.");
                return;
            }

            var principal = Jwt.ValidateToken(token, _config);
            if (principal is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token invalido ou expirado.");
                return;
            }

            context.Items["Claims"] = principal;
        }

        await _next(context);
    }
}
