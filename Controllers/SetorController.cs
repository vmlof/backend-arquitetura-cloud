using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetorController : ControllerBase
{
    private readonly SetorService _setorService;
    private readonly IUnitOfWork  _uof;

    public SetorController(SetorService setorService, IUnitOfWork uof)
    {
        _setorService = setorService;
        _uof = uof;
    }

    /// <summary>Lista setores ATIVOS (para selects do front)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _setorService.Listar();
        return Ok(lista.Select(SetorService.ToResponse));
    }

    /// <summary>Lista TODOS os setores (ativos e inativos) — tela de gestão</summary>
    [HttpGet("todos")]
    public async Task<IActionResult> ListarTodos()
    {
        var lista = await _setorService.ListarTodos();
        return Ok(lista.Select(SetorService.ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var setor = await _setorService.ObterPorId(id);
            return Ok(SetorService.ToResponse(setor));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] SetorCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var setor = await _setorService.Cadastrar(dto);
            await _uof.CommitAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = setor.Id }, SetorService.ToResponse(setor));
        }
        catch (ArgumentException ex)        { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] SetorAtualizarDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var setor = await _setorService.Atualizar(id, dto);
            await _uof.CommitAsync();
            return Ok(SetorService.ToResponse(setor));
        }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _setorService.Desativar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }
}
