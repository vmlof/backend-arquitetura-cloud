using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Modelos.Commands.ArquivarModelo;
using GestaoRH.Application.Features.Modelos.Commands.AtualizarModelo;
using GestaoRH.Application.Features.Modelos.Commands.CriarModelo;
using GestaoRH.Application.Features.Modelos.Commands.DuplicarModelo;
using GestaoRH.Application.Features.Modelos.Commands.PublicarModelo;
using GestaoRH.Application.Features.Modelos.Queries.ListarModelos;
using GestaoRH.Application.Features.Modelos.Queries.ObterModeloPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModeloController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModeloController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista todos os modelos (tabela simplificada)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _mediator.Send(new ListarModelosQuery());
        return Ok(result);
    }

    /// <summary>Retorna modelo completo (com seções/campos/assinantes)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new ObterModeloPorIdQuery(id));
        return Ok(result);
    }

    /// <summary>Cria novo modelo como rascunho</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ModeloCadastroDto dto)
    {
        var command = new CriarModeloCommand(
            dto.Nome,
            dto.Descricao,
            dto.Categoria,
            dto.TipoUso,
            dto.Secoes,
            dto.Assinantes
        );
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    /// <summary>Atualiza modelo (re-salva seções e assinantes)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ModeloCadastroDto dto)
    {
        var command = new AtualizarModeloCommand(
            id,
            dto.Nome,
            dto.Descricao,
            dto.Categoria,
            dto.TipoUso,
            dto.Secoes,
            dto.Assinantes
        );
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Publica modelo (valida assinante obrigatório)</summary>
    [HttpPatch("{id:int}/publicar")]
    public async Task<IActionResult> Publicar(int id)
    {
        await _mediator.Send(new PublicarModeloCommand(id));
        return Ok(new { mensagem = "Modelo publicado com sucesso." });
    }

    /// <summary>Arquiva modelo</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Arquivar(int id)
    {
        await _mediator.Send(new ArquivarModeloCommand(id));
        return NoContent();
    }

    /// <summary>Duplica modelo como novo rascunho</summary>
    [HttpPost("{id:int}/duplicar")]
    public async Task<IActionResult> Duplicar(int id)
    {
        var result = await _mediator.Send(new DuplicarModeloCommand(id));
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }
}
