using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ObterSetorPorId;

public record ObterSetorPorIdQuery(int Id) : IRequest<SetorResponseDto>;
