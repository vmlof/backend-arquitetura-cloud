using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Documents.Queries.GetDocumentById;
using GestaoRH.Application.Features.Bff.Documents.Queries.ListDocuments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("documents")]
public class DocumentsBffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBffDocumentsClient _documentsClient;

    public DocumentsBffController(IMediator mediator, IBffDocumentsClient documentsClient)
    {
        _mediator = mediator;
        _documentsClient = documentsClient;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ListDocumentsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetDocumentByIdQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JsonObject payload, CancellationToken cancellationToken)
    {
        var response = await _documentsClient.CreateAsync(payload, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] JsonObject payload, CancellationToken cancellationToken)
    {
        var response = await _documentsClient.UpdateAsync(id, payload, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _documentsClient.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
