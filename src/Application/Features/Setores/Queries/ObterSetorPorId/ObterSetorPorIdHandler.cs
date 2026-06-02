using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Queries.ObterSetorPorId;

public class ObterSetorPorIdHandler : IRequestHandler<ObterSetorPorIdQuery, SetorResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ObterSetorPorIdHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<SetorResponseDto> Handle(ObterSetorPorIdQuery request, CancellationToken cancellationToken)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(request.Id);
        if (setor == null)
            throw new KeyNotFoundException("Setor nao encontrado.");

        return _mapper.Map<SetorResponseDto>(setor);
    }
}
