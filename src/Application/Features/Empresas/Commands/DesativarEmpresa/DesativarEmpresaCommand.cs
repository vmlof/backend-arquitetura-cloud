using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.DesativarEmpresa;

public record DesativarEmpresaCommand(int Id) : IRequest;
