using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Empresas.Commands.AtualizarEmpresa;
using GestaoRH.Application.Features.Empresas.Commands.CadastrarEmpresa;
using GestaoRH.Application.Features.Empresas.Commands.DesativarEmpresa;
using GestaoRH.Application.Features.Empresas.Commands.LoginEmpresa;
using GestaoRH.Application.Features.Empresas.Queries.ListarEmpresas;
using GestaoRH.Application.Features.Empresas.Queries.ObterEmpresaPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresaController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmpresaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cadastra uma nova empresa (público)</summary>
    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarEmpresaCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    /// <summary>Login da empresa — retorna JWT (público)</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginEmpresaCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Retorna dados da empresa por ID (requer token)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new ObterEmpresaPorIdQuery(id));
        return Ok(result);
    }

    /// <summary>Lista todas as empresas (requer token)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _mediator.Send(new ListarEmpresasQuery());
        return Ok(result);
    }

    /// <summary>Atualiza dados da empresa (requer token)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EmpresaAtualizarDto dto)
    {
        var command = new AtualizarEmpresaCommand(
            id,
            dto.RazaoSocial,
            dto.Endereco,
            dto.Telefone,
            dto.LogoBase64,
            dto.ResponsavelNome,
            dto.ResponsavelSobrenome
        );
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Desativa a empresa (requer token)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        await _mediator.Send(new DesativarEmpresaCommand(id));
        return NoContent();
    }
}
