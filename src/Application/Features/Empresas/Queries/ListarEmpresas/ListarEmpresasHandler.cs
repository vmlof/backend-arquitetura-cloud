using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Queries.ListarEmpresas;

public class ListarEmpresasHandler : IRequestHandler<ListarEmpresasQuery, IEnumerable<EmpresaResponseDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarEmpresasHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmpresaResponseDto>> Handle(ListarEmpresasQuery request, CancellationToken cancellationToken)
    {
        var empresas = await _uof.EmpresaRepository.ListarAsync();
        return _mapper.Map<IEnumerable<EmpresaResponseDto>>(empresas);
    }
}
