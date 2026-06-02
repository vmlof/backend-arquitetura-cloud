using FluentValidation;
using GestaoRH.Application.Common.Interfaces;
using GestaoRH.API.Middlewares;
using GestaoRH.Domain.Interfaces;
using GestaoRH.Infrastructure.Clients;
using GestaoRH.Infrastructure.Configuration;
using GestaoRH.Infrastructure.Data;
using GestaoRH.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

const string MyCors = "_myCors";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(MyCors, p =>
        p.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("127.0.0.1"))
            && uri.Port is >= 3000 and <= 5999)
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddOpenApi();

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(GestaoRH.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.Configure<DownstreamServicesOptions>(
    builder.Configuration.GetSection(DownstreamServicesOptions.SectionName));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtService, GestaoRH.Infrastructure.Security.JwtService>();
builder.Services.AddSingleton<IPdfService, PdfService>();
builder.Services.AddHttpClient<IBffPeopleClient, PeopleBffClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
    if (Uri.TryCreate(options.PeopleBaseUrl, UriKind.Absolute, out var baseUri))
    {
        client.BaseAddress = baseUri;
    }
});
builder.Services.AddHttpClient<IBffDocumentsClient, DocumentsBffClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
    if (Uri.TryCreate(options.DocumentsBaseUrl, UriKind.Absolute, out var baseUri))
    {
        client.BaseAddress = baseUri;
    }
});
builder.Services.AddHttpClient<IBffFunctionClient, FunctionBffClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
    if (Uri.TryCreate(options.FunctionBaseUrl, UriKind.Absolute, out var baseUri))
    {
        client.BaseAddress = baseUri;
    }
});

var app = builder.Build();

_ = Task.Run(async () =>
{
    try { await PdfService.InicializarAsync(); }
    catch (Exception ex)
    {
        Console.WriteLine($"[PdfService] Aviso ao baixar Chromium: {ex.Message}");
    }
});

app.UseHttpsRedirection();
app.UseCors(MyCors);
app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();

app.UseRouting();

// Global Error Handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<Auth>();
app.UseAuthorization();
app.MapControllers();

await PdfService.InicializarAsync();
app.Run();
