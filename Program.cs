using GestaoRH.Middlewares;
using GestaoRH.Repositories;
using GestaoRH.Services;
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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<SetorService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<ModeloService>();
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddSingleton<PdfService>();   // singleton 

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
app.UseMiddleware<Auth>();
app.UseAuthorization();
app.MapControllers();

await PdfService.InicializarAsync();
app.Run();
