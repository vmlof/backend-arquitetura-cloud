using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Funcionarios.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public LoginHandler(IUnitOfWork uof, IMapper mapper, IJwtService jwtService)
    {
        _uof = uof;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var func = await _uof.FuncionarioRepository.ObterPorCpfAtivoAsync(request.Cpf);

        if (func == null || !BCrypt.Net.BCrypt.Verify(request.Senha, func.Senha))
            throw new UnauthorizedAccessException("CPF ou senha invalidos.");

        return new LoginResponseDto
        {
            Funcionario = _mapper.Map<FuncionarioResponseDto>(func),
            SenhaTrocada = func.SenhaTrocada,
            Jwt = _jwtService.GenerateFuncionarioToken(func)
        };
    }
}
