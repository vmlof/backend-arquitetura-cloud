using MediatR;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionariosPorSetor;

public record ListarFuncionariosPorSetorQuery(int SetorId) : IRequest<IEnumerable<FuncionarioResponseDto>>;
