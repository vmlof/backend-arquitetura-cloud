using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.People.Queries.GetPersonById;
using GestaoRH.Application.Features.Bff.People.Queries.ListPeople;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("people")]
public class PeopleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBffPeopleClient _peopleClient;

    public PeopleController(IMediator mediator, IBffPeopleClient peopleClient)
    {
        _mediator = mediator;
        _peopleClient = peopleClient;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ListPeopleQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPersonByIdQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JsonObject payload, CancellationToken cancellationToken)
    {
        var response = await _peopleClient.CreateAsync(payload, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JsonObject payload, CancellationToken cancellationToken)
    {
        var response = await _peopleClient.UpdateAsync(id, payload, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _peopleClient.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
