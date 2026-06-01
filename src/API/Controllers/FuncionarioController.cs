using GestaoRH.Application.Features.Funcionarios.Commands.AtualizarFuncionario;
using GestaoRH.Application.Features.Funcionarios.Commands.CadastrarFuncionario;
using GestaoRH.Application.Features.Funcionarios.Commands.DesativarFuncionario;
using GestaoRH.Application.Features.Funcionarios.Commands.Login;
using GestaoRH.Application.Features.Funcionarios.Commands.TrocarSenha;
using GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionarios;
using GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionariosPorSetor;
using GestaoRH.Application.Features.Funcionarios.Queries.ObterFuncionarioPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuncionarioController : ControllerBase
{
    private readonly IMediator _mediator;

    public FuncionarioController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarFuncionarioCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id:int}/trocar-senha")]
    public async Task<IActionResult> TrocarSenha(int id, [FromBody] TrocarSenhaCommand command)
    {
        // Ensure the ID in the URL matches the one in the command or set it
        var finalCommand = command with { Id = id };
        await _mediator.Send(finalCommand);
        return Ok(new { mensagem = "Senha alterada com sucesso." });
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivos = false)
    {
        var result = await _mediator.Send(new ListarFuncionariosQuery(apenasAtivos));
        return Ok(result);
    }

    [HttpGet("setor/{setorId:int}")]
    public async Task<IActionResult> ListarPorSetor(int setorId)
    {
        var result = await _mediator.Send(new ListarFuncionariosPorSetorQuery(setorId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new ObterFuncionarioPorIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarFuncionarioCommand command)
    {
        var finalCommand = command with { Id = id };
        var result = await _mediator.Send(finalCommand);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        await _mediator.Send(new DesativarFuncionarioCommand(id));
        return NoContent();
    }
}
