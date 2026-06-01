using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.AtualizarFuncionario;

public class AtualizarFuncionarioHandler : IRequestHandler<AtualizarFuncionarioCommand, FuncionarioResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public AtualizarFuncionarioHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<FuncionarioResponseDto> Handle(AtualizarFuncionarioCommand request, CancellationToken cancellationToken)
    {
        var func = await _uof.FuncionarioRepository.ObterPorIdAsync(request.Id)
                   ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

        if (!string.Equals(func.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var dup = await _uof.FuncionarioRepository.ObterPorEmailAtivoAsync(request.Email);
            if (dup != null && dup.Id != request.Id)
                throw new InvalidOperationException("E-mail ja em uso por outro funcionario ativo.");
        }

        func.Nome     = request.Nome;
        func.Telefone = request.Telefone ?? string.Empty;
        func.Email    = request.Email;
        func.Genero   = request.Genero.ToLower();
        func.Turno    = request.Turno.ToLower();
        func.SetorId  = request.SetorId;
        func.IsChefe  = request.IsChefe;
        func.Ativo    = request.Ativo;

        await _uof.FuncionarioRepository.AtualizarAsync(func);
        await _uof.CommitAsync();

        return _mapper.Map<FuncionarioResponseDto>(func);
    }
}
