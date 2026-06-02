using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.CadastrarSetor;

public class CadastrarSetorHandler : IRequestHandler<CadastrarSetorCommand, SetorResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public CadastrarSetorHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<SetorResponseDto> Handle(CadastrarSetorCommand request, CancellationToken cancellationToken)
    {
        var jaExiste = await _uof.SetorRepository.ObterPorNomeAtivoAsync(request.Nome);
        if (jaExiste != null)
            throw new InvalidOperationException($"Ja existe um setor ativo com o nome '{request.Nome}'.");

        var setor = new Setor
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim() ?? string.Empty,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _uof.SetorRepository.CriarAsync(setor);
        await _uof.CommitAsync();

        var criado = await _uof.SetorRepository.ObterPorIdAsync(id)
            ?? throw new Exception("Falha ao recuperar setor apos cadastro.");

        return _mapper.Map<SetorResponseDto>(criado);
    }
}
