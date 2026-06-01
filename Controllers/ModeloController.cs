using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModeloController : ControllerBase
{
    private readonly ModeloService _modeloService;
    private readonly IUnitOfWork   _uof;

    public ModeloController(ModeloService modeloService, IUnitOfWork uof)
    {
        _modeloService = modeloService;
        _uof           = uof;
    }

    /// <summary>Lista todos os modelos (tabela simplificada)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _modeloService.Listar();
        return Ok(lista);
    }

    /// <summary>Retorna modelo completo (com seções/campos/assinantes)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var modelo = await _modeloService.ObterPorId(id);
            return Ok(modelo);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Cria novo modelo como rascunho</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ModeloCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var modelo = await _modeloService.Criar(dto);
            await _uof.CommitAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = modelo.Id }, modelo);
        }
        catch (ArgumentException ex)        { return BadRequest(ex.Message); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    /// <summary>Atualiza modelo (re-salva seções e assinantes)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ModeloCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var modelo = await _modeloService.Atualizar(id, dto);
            await _uof.CommitAsync();
            return Ok(modelo);
        }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex)         { return BadRequest(ex.Message); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    /// <summary>Publica modelo (valida assinante obrigatório)</summary>
    [HttpPatch("{id:int}/publicar")]
    public async Task<IActionResult> Publicar(int id)
    {
        try
        {
            await _modeloService.Publicar(id);
            await _uof.CommitAsync();
            return Ok(new { mensagem = "Modelo publicado com sucesso." });
        }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    /// <summary>Arquiva modelo</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Arquivar(int id)
    {
        try
        {
            await _modeloService.Arquivar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }

    /// <summary>Duplica modelo como novo rascunho</summary>
    [HttpPost("{id:int}/duplicar")]
    public async Task<IActionResult> Duplicar(int id)
    {
        try
        {
            var modelo = await _modeloService.Duplicar(id);
            await _uof.CommitAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = modelo.Id }, modelo);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }
}
