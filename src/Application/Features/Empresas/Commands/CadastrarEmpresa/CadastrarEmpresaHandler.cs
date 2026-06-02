using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.CadastrarEmpresa;

public class CadastrarEmpresaHandler : IRequestHandler<CadastrarEmpresaCommand, EmpresaResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public CadastrarEmpresaHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<EmpresaResponseDto> Handle(CadastrarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var jaExiste = await _uof.EmpresaRepository.ObterPorCnpjAsync(request.Cnpj);
        if (jaExiste != null)
            throw new InvalidOperationException("CNPJ já cadastrado no sistema.");

        var empresa = new Empresa
        {
            Cnpj = request.Cnpj,
            RazaoSocial = request.RazaoSocial,
            Endereco = request.Endereco,
            Telefone = request.Telefone,
            LogoBase64 = request.LogoBase64,
            ResponsavelNome = request.ResponsavelNome,
            ResponsavelSobrenome = request.ResponsavelSobrenome,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _uof.EmpresaRepository.CriarAsync(empresa);
        await _uof.CommitAsync();

        var criada = await _uof.EmpresaRepository.ObterPorIdAsync(id)
            ?? throw new Exception("Falha ao recuperar empresa após cadastro.");

        return _mapper.Map<EmpresaResponseDto>(criada);
    }
}
