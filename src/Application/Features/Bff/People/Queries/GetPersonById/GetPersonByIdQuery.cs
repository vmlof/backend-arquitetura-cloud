using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.People.Queries.GetPersonById;

public record GetPersonByIdQuery(int Id) : IRequest<PersonDetailDto?>;
