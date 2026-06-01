using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.DesativarFuncionario;

public record DesativarFuncionarioCommand(int Id) : IRequest;
