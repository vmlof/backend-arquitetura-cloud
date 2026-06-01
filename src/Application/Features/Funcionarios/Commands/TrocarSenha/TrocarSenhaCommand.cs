using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.TrocarSenha;

public record TrocarSenhaCommand(int Id, string SenhaAtual, string NovaSenha) : IRequest;
