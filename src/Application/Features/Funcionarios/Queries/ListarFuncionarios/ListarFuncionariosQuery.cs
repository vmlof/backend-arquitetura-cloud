using MediatR;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionarios;

public record ListarFuncionariosQuery(bool ApenasAtivos = false) : IRequest<IEnumerable<FuncionarioRhResponseDto>>;
