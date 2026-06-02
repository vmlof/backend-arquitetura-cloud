using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ListarSetores;

public class ListarSetoresHandler : IRequestHandler<ListarSetoresQuery, IEnumerable<SetorResponseDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarSetoresHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SetorResponseDto>> Handle(ListarSetoresQuery request, CancellationToken cancellationToken)
    {
        var setores = await _uof.SetorRepository.ListarAsync();
        return _mapper.Map<IEnumerable<SetorResponseDto>>(setores);
    }
}
