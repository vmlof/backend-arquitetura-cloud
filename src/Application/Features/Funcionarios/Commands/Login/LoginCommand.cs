using MediatR;
using GestaoRH.Application.Features.Funcionarios.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Commands.Login;

public record LoginCommand(string Cpf, string Senha) : IRequest<LoginResponseDto>;
