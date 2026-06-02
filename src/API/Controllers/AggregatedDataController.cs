using GestaoRH.Application.Features.Bff.AggregatedData.GetAggregatedData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("aggregated-data")]
public class AggregatedDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public AggregatedDataController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAggregatedDataQuery(), cancellationToken);
        return Ok(response);
    }
}
