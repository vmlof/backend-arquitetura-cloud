using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ObterFuncionarioPorId;

public class ObterFuncionarioPorIdHandler : IRequestHandler<ObterFuncionarioPorIdQuery, FuncionarioRhResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ObterFuncionarioPorIdHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<FuncionarioRhResponseDto> Handle(ObterFuncionarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var funcionario = await _uof.FuncionarioRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Funcionario nao encontrado.");
            
        return _mapper.Map<FuncionarioRhResponseDto>(funcionario);
    }
}
