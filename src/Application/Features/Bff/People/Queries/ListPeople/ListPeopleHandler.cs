using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.People.Queries.ListPeople;

public class ListPeopleHandler : IRequestHandler<ListPeopleQuery, IReadOnlyCollection<PersonSummaryDto>>
{
    private readonly IBffPeopleClient _peopleClient;

    public ListPeopleHandler(IBffPeopleClient peopleClient)
    {
        _peopleClient = peopleClient;
    }

    public Task<IReadOnlyCollection<PersonSummaryDto>> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
    {
        return _peopleClient.ListAsync(cancellationToken);
    }
}
