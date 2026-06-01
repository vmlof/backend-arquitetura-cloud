using MediatR;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ObterFuncionarioPorId;

public record ObterFuncionarioPorIdQuery(int Id) : IRequest<FuncionarioRhResponseDto>;
