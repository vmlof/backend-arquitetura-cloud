using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ListarSetores;

public record ListarSetoresQuery : IRequest<IEnumerable<SetorResponseDto>>;
