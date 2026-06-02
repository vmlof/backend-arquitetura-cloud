using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.AtualizarSetor;

public class AtualizarSetorHandler : IRequestHandler<AtualizarSetorCommand, SetorResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public AtualizarSetorHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<SetorResponseDto> Handle(AtualizarSetorCommand request, CancellationToken cancellationToken)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(request.Id);
        if (setor == null)
            throw new KeyNotFoundException("Setor nao encontrado.");

        if (!string.Equals(setor.Nome, request.Nome, StringComparison.OrdinalIgnoreCase))
        {
            var duplicado = await _uof.SetorRepository.ObterPorNomeAtivoAsync(request.Nome);
            if (duplicado != null && duplicado.Id != request.Id)
                throw new InvalidOperationException($"Ja existe um setor ativo com o nome '{request.Nome}'.");
        }

        setor.Nome = request.Nome.Trim();
        setor.Descricao = request.Descricao?.Trim() ?? string.Empty;
        setor.Ativo = request.Ativo;

        await _uof.SetorRepository.AtualizarAsync(setor);
        await _uof.CommitAsync();

        return _mapper.Map<SetorResponseDto>(setor);
    }
}
