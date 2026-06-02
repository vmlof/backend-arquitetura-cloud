using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.CadastrarEmpresa;

public record CadastrarEmpresaCommand(
    string Cnpj,
    string RazaoSocial,
    string Endereco,
    string Telefone,
    string? LogoBase64,
    string ResponsavelNome,
    string ResponsavelSobrenome,
    string Senha
) : IRequest<EmpresaResponseDto>;
