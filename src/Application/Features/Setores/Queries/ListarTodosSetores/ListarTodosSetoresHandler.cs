using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ListarTodosSetores;

public class ListarTodosSetoresHandler : IRequestHandler<ListarTodosSetoresQuery, IEnumerable<SetorResponseDto>>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ListarTodosSetoresHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SetorResponseDto>> Handle(ListarTodosSetoresQuery request, CancellationToken cancellationToken)
    {
        var setores = await _uof.SetorRepository.ListarTodosAsync();
        return _mapper.Map<IEnumerable<SetorResponseDto>>(setores);
    }
}
