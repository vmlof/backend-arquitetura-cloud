using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.People.Queries.GetPersonById;

public class GetPersonByIdHandler : IRequestHandler<GetPersonByIdQuery, PersonDetailDto?>
{
    private readonly IBffPeopleClient _peopleClient;

    public GetPersonByIdHandler(IBffPeopleClient peopleClient)
    {
        _peopleClient = peopleClient;
    }

    public Task<PersonDetailDto?> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
    {
        return _peopleClient.GetByIdAsync(request.Id, cancellationToken);
    }
}
