using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.PublicarModelo;

public record PublicarModeloCommand(int Id) : IRequest;
