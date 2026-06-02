using GestaoRH.Application.Features.Empresas.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.LoginEmpresa;

public record LoginEmpresaCommand(string Cnpj, string Senha) : IRequest<EmpresaLoginResponseDto>;
