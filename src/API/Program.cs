using FluentValidation;
using GestaoRH.API.Middlewares;
using GestaoRH.Application.Common.Services;
using GestaoRH.Domain.Interfaces;
using GestaoRH.Infrastructure.Data;
using GestaoRH.Infrastructure.Services;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

const string MyCors = "_myCors";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(MyCors, p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddOpenApi();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtService, GestaoRH.Infrastructure.Security.JwtService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<SetorService>();
builder.Services.AddScoped<ModeloService>();
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddSingleton<PdfService>();

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
