using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using GestaoRH.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuncionarioController : ControllerBase
{
    private readonly FuncionarioService _funcionarioService;
    private readonly IUnitOfWork        _uof;
    private readonly IConfiguration     _config;

    public FuncionarioController(FuncionarioService funcionarioService, IUnitOfWork uof, IConfiguration config)
    {
        _funcionarioService = funcionarioService;
        _uof                = uof;
        _config             = config;
    }

    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] FuncionarioCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Cadastrar(dto);
            await _uof.CommitAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = funcionario.Id },
                FuncionarioService.ToRhResponse(funcionario));
        }
        catch (ArgumentException ex)         { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] FuncionarioLoginDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Login(dto.Cpf, dto.Senha);
            return Ok(new
            {
                Funcionario  = FuncionarioService.ToResponse(funcionario),
                SenhaTrocada = funcionario.SenhaTrocada,
                Jwt          = Jwt.GenerateFuncionarioToken(funcionario, _config)
            });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        catch (Exception ex)                   { return StatusCode(500, ex.Message); }
    }

    [HttpPatch("{id:int}/trocar-senha")]
    public async Task<IActionResult> TrocarSenha(int id, [FromBody] FuncionarioTrocarSenhaDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            await _funcionarioService.TrocarSenha(id, dto);
            await _uof.CommitAsync();
            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }
        catch (KeyNotFoundException ex)        { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        catch (ArgumentException ex)           { return BadRequest(ex.Message); }
        catch (Exception ex)                   { return StatusCode(500, ex.Message); }
    }

    /// <summary>Lista TODOS os funcionários (ativos + inativos) — tela de gestão RH</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _funcionarioService.ListarTodos();
        return Ok(lista.Select(FuncionarioService.ToRhResponse));
    }

    [HttpGet("setor/{setorId:int}")]
    public async Task<IActionResult> ListarPorSetor(int setorId)
    {
        var lista = await _funcionarioService.ListarPorSetor(setorId);
        return Ok(lista.Select(FuncionarioService.ToRhResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var funcionario = await _funcionarioService.ObterPorId(id);
            return Ok(FuncionarioService.ToRhResponse(funcionario));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] FuncionarioAtualizarDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Atualizar(id, dto);
            await _uof.CommitAsync();
            return Ok(FuncionarioService.ToRhResponse(funcionario));
        }
        catch (KeyNotFoundException ex)       { return NotFound(ex.Message); }
        catch (InvalidOperationException ex)  { return Conflict(ex.Message); }
        catch (ArgumentException ex)          { return BadRequest(ex.Message); }
        catch (Exception ex)                  { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _funcionarioService.Desativar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }
}
