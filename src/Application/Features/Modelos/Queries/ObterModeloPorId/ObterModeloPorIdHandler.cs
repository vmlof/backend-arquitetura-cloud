using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Queries.ObterModeloPorId;

public class ObterModeloPorIdHandler : IRequestHandler<ObterModeloPorIdQuery, ModeloResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ObterModeloPorIdHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<ModeloResponseDto> Handle(ObterModeloPorIdQuery request, CancellationToken cancellationToken)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(request.Id);
        if (modelo == null)
            throw new KeyNotFoundException("Modelo nao encontrado.");

        return _mapper.Map<ModeloResponseDto>(modelo);
    }
}
