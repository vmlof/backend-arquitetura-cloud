using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.AggregatedData.GetAggregatedData;

public class GetAggregatedDataHandler : IRequestHandler<GetAggregatedDataQuery, AggregatedDataResponseDto>
{
    private readonly IBffPeopleClient _peopleClient;
    private readonly IBffDocumentsClient _documentsClient;
    private readonly IBffFunctionClient _functionClient;

    public GetAggregatedDataHandler(
        IBffPeopleClient peopleClient,
        IBffDocumentsClient documentsClient,
        IBffFunctionClient functionClient)
    {
        _peopleClient = peopleClient;
        _documentsClient = documentsClient;
        _functionClient = functionClient;
    }

    public async Task<AggregatedDataResponseDto> Handle(GetAggregatedDataQuery request, CancellationToken cancellationToken)
    {
        var peopleTask = _peopleClient.ListAsync(cancellationToken);
        var documentsTask = _documentsClient.ListAsync(cancellationToken);

        await Task.WhenAll(peopleTask, documentsTask);

        var people = await peopleTask;
        var documents = await documentsTask;
        var functionData = await _functionClient.GetSummaryAsync(people.Count, documents.Count, cancellationToken);

        return new AggregatedDataResponseDto(
            people,
            documents,
            functionData,
            DateTimeOffset.UtcNow,
            "web");
    }
}
