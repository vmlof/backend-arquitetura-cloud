using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.AtualizarEmpresa;

public record AtualizarEmpresaCommand(
    int Id,
    string RazaoSocial,
    string Endereco,
    string Telefone,
    string? LogoBase64,
    string ResponsavelNome,
    string ResponsavelSobrenome
) : IRequest<EmpresaResponseDto>;
