using MediatR;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Commands.CadastrarFuncionario;

public record CadastrarFuncionarioCommand(
    string Cpf,
    string Nome,
    string? Telefone,
    string Email,
    string Genero,
    string Turno,
    int SetorId,
    bool IsChefe
) : IRequest<FuncionarioRhResponseDto>;
