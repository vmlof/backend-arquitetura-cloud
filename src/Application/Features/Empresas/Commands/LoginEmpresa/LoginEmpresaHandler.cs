using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Empresas.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.LoginEmpresa;

public class LoginEmpresaHandler : IRequestHandler<LoginEmpresaCommand, EmpresaLoginResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public LoginEmpresaHandler(IUnitOfWork uof, IMapper mapper, IJwtService jwtService)
    {
        _uof = uof;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<EmpresaLoginResponseDto> Handle(LoginEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorCnpjAsync(request.Cnpj);

        if (empresa == null || !empresa.Ativo)
            throw new UnauthorizedAccessException("CNPJ ou senha inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, empresa.Senha))
            throw new UnauthorizedAccessException("CNPJ ou senha inválidos.");

        var jwt = _jwtService.GenerateToken(empresa);

        return new EmpresaLoginResponseDto
        {
            Empresa = _mapper.Map<EmpresaResponseDto>(empresa),
            Jwt = jwt
        };
    }
}
