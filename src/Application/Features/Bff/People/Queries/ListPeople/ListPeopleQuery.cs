using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.People.Queries.ListPeople;

public record ListPeopleQuery : IRequest<IReadOnlyCollection<PersonSummaryDto>>;
