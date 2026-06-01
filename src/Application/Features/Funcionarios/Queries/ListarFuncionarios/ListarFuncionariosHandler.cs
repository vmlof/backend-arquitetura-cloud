using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionarios;

public class ListarFuncionariosHandler : IRequestHandler<ListarFuncionariosQuery, IEnumerable<FuncionarioRhResponseDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarFuncionariosHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FuncionarioRhResponseDto>> Handle(ListarFuncionariosQuery request, CancellationToken cancellationToken)
    {
        var funcionarios = request.ApenasAtivos 
            ? await _uof.FuncionarioRepository.ListarAsync() 
            : await _uof.FuncionarioRepository.ListarTodosAsync();
            
        return _mapper.Map<IEnumerable<FuncionarioRhResponseDto>>(funcionarios);
    }
}
