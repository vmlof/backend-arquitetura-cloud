using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Queries.ListarFuncionariosPorSetor;

public class ListarFuncionariosPorSetorHandler : IRequestHandler<ListarFuncionariosPorSetorQuery, IEnumerable<FuncionarioResponseDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarFuncionariosPorSetorHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FuncionarioResponseDto>> Handle(ListarFuncionariosPorSetorQuery request, CancellationToken cancellationToken)
    {
        var lista = await _uof.FuncionarioRepository.ListarPorSetorAsync(request.SetorId);
        return _mapper.Map<IEnumerable<FuncionarioResponseDto>>(lista);
    }
}
