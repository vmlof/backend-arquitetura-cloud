using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Queries.ObterEmpresaPorId;

public record ObterEmpresaPorIdQuery(int Id) : IRequest<EmpresaResponseDto>;
