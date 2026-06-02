using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ListarTodosSetores;

public record ListarTodosSetoresQuery : IRequest<IEnumerable<SetorResponseDto>>;
