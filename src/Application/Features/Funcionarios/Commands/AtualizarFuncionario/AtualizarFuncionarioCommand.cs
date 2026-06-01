using MediatR;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Commands.AtualizarFuncionario;

public record AtualizarFuncionarioCommand(
    int Id,
    string Nome,
    string? Telefone,
    string Email,
    string Genero,
    string Turno,
    int SetorId,
    bool IsChefe,
    bool Ativo
) : IRequest<FuncionarioResponseDto>;
