using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Queries.ObterEmpresaPorId;

public class ObterEmpresaPorIdHandler : IRequestHandler<ObterEmpresaPorIdQuery, EmpresaResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ObterEmpresaPorIdHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<EmpresaResponseDto> Handle(ObterEmpresaPorIdQuery request, CancellationToken cancellationToken)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(request.Id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        return _mapper.Map<EmpresaResponseDto>(empresa);
    }
}
