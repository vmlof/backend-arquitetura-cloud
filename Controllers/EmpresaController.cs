using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using GestaoRH.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresaController : ControllerBase
{
    private readonly EmpresaService _empresaService;
    private readonly IUnitOfWork _uof;
    private readonly IConfiguration _config;

    public EmpresaController(EmpresaService empresaService, IUnitOfWork uof, IConfiguration config)
    {
        _empresaService = empresaService;
        _uof = uof;
        _config = config;
    }

    /// <summary>Cadastra uma nova empresa (público)</summary>
    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] EmpresaCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");

            var empresa = await _empresaService.Cadastrar(dto);
            await _uof.CommitAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = empresa.Id },
                EmpresaService.ToResponse(empresa));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>Login da empresa — retorna JWT (público)</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] EmpresaLoginDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");

            var empresa = await _empresaService.Login(dto.Cnpj, dto.Senha);

            return Ok(new
            {
                Empresa = EmpresaService.ToResponse(empresa),
                Jwt     = Jwt.GenerateToken(empresa, _config)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>Retorna dados da empresa por ID (requer token)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var empresa = await _empresaService.ObterPorId(id);
            return Ok(EmpresaService.ToResponse(empresa));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Lista todas as empresas (requer token)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _empresaService.Listar();
        return Ok(lista.Select(EmpresaService.ToResponse));
    }

    /// <summary>Atualiza dados da empresa (requer token)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EmpresaAtualizarDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");

            var empresa = await _empresaService.Atualizar(id, dto);
            await _uof.CommitAsync();

            return Ok(EmpresaService.ToResponse(empresa));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>Desativa a empresa (requer token)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _empresaService.Desativar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
