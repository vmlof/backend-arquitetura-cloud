using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.ArquivarModelo;

public record ArquivarModeloCommand(int Id) : IRequest;
