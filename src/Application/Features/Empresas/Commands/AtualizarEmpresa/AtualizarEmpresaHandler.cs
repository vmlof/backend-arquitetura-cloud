using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.AtualizarEmpresa;

public class AtualizarEmpresaHandler : IRequestHandler<AtualizarEmpresaCommand, EmpresaResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public AtualizarEmpresaHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<EmpresaResponseDto> Handle(AtualizarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(request.Id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        empresa.RazaoSocial = request.RazaoSocial;
        empresa.Endereco = request.Endereco;
        empresa.Telefone = request.Telefone;
        empresa.LogoBase64 = request.LogoBase64;
        empresa.ResponsavelNome = request.ResponsavelNome;
        empresa.ResponsavelSobrenome = request.ResponsavelSobrenome;

        await _uof.EmpresaRepository.AtualizarAsync(empresa);
        await _uof.CommitAsync();

        return _mapper.Map<EmpresaResponseDto>(empresa);
    }
}
