using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Queries.ListarEmpresas;

public record ListarEmpresasQuery : IRequest<IEnumerable<EmpresaResponseDto>>;
