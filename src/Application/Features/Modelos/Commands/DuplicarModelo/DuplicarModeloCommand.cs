using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.DuplicarModelo;

public record DuplicarModeloCommand(int Id) : IRequest<ModeloResponseDto>;
