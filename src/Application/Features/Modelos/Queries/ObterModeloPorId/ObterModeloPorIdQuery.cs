using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Queries.ObterModeloPorId;

public record ObterModeloPorIdQuery(int Id) : IRequest<ModeloResponseDto>;
