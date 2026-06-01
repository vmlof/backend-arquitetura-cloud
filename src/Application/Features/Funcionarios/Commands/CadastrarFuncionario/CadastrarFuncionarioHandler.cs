using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.CadastrarFuncionario;

public class CadastrarFuncionarioHandler : IRequestHandler<CadastrarFuncionarioCommand, FuncionarioRhResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public CadastrarFuncionarioHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<FuncionarioRhResponseDto> Handle(CadastrarFuncionarioCommand request, CancellationToken cancellationToken)
    {
        if (await _uof.FuncionarioRepository.ObterPorCpfAtivoAsync(request.Cpf) != null)
            throw new InvalidOperationException("CPF ja cadastrado por um funcionario ativo.");
        
        if (await _uof.FuncionarioRepository.ObterPorEmailAtivoAsync(request.Email) != null)
            throw new InvalidOperationException("E-mail ja cadastrado por um funcionario ativo.");
        
        if (await _uof.SetorRepository.ObterPorIdAsync(request.SetorId) == null)
            throw new KeyNotFoundException("Setor informado nao encontrado.");

        var senhaTemp = GerarSenhaTemporaria(request.Cpf);
        
        var func = new Funcionario
        {
            Cpf             = request.Cpf,
            Nome            = request.Nome,
            Telefone        = request.Telefone ?? string.Empty,
            Email           = request.Email,
            Genero          = request.Genero.ToLower(),
            Turno           = request.Turno.ToLower(),
            SetorId         = request.SetorId,
            SenhaTemporaria = senhaTemp,
            Senha           = BCrypt.Net.BCrypt.HashPassword(senhaTemp),
            SenhaTrocada    = false,
            IsChefe         = request.IsChefe,
            Ativo           = true,
            CriadoEm       = DateTime.UtcNow
        };

        var id = await _uof.FuncionarioRepository.CriarAsync(func);
        await _uof.CommitAsync();
        
        var createdFunc = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
               ?? throw new Exception("Falha ao recuperar funcionario apos cadastro.");

        return _mapper.Map<FuncionarioRhResponseDto>(createdFunc);
    }

    private static string GerarSenhaTemporaria(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return $"{(digits.Length >= 4 ? digits[..4] : digits)}senha#";
    }
}
