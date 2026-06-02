using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Queries.ListarModelos;

public class ListarModelosHandler : IRequestHandler<ListarModelosQuery, IEnumerable<ModeloListagemDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarModelosHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ModeloListagemDto>> Handle(ListarModelosQuery request, CancellationToken cancellationToken)
    {
        var modelos = await _uof.ModeloRepository.ListarAsync();
        return _mapper.Map<IEnumerable<ModeloListagemDto>>(modelos);
    }
}
