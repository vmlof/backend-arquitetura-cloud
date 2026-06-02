using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Setores.Commands.AtualizarSetor;
using GestaoRH.Application.Features.Setores.Commands.CadastrarSetor;
using GestaoRH.Application.Features.Setores.Commands.DesativarSetor;
using GestaoRH.Application.Features.Setores.Queries.ListarSetores;
using GestaoRH.Application.Features.Setores.Queries.ListarTodosSetores;
using GestaoRH.Application.Features.Setores.Queries.ObterSetorPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetorController : ControllerBase
{
    private readonly IMediator _mediator;

    public SetorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista setores ATIVOS (para selects do front)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _mediator.Send(new ListarSetoresQuery());
        return Ok(result);
    }

    /// <summary>Lista TODOS os setores (ativos e inativos) — tela de gestão</summary>
    [HttpGet("todos")]
    public async Task<IActionResult> ListarTodos()
    {
        var result = await _mediator.Send(new ListarTodosSetoresQuery());
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new ObterSetorPorIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarSetorCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] SetorAtualizarDto dto)
    {
        var command = new AtualizarSetorCommand(id, dto.Nome, dto.Descricao, dto.Ativo);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        await _mediator.Send(new DesativarSetorCommand(id));
        return NoContent();
    }
}
