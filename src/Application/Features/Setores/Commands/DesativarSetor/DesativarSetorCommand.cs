using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.DesativarSetor;

public record DesativarSetorCommand(int Id) : IRequest;
