using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.AtualizarModelo;

public class AtualizarModeloHandler : IRequestHandler<AtualizarModeloCommand, ModeloResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public Microsoft.Extensions.Logging.ILogger<AtualizarModeloHandler>? Logger { get; }

    public AtualizarModeloHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<ModeloResponseDto> Handle(AtualizarModeloCommand request, CancellationToken cancellationToken)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(request.Id);
        if (modelo == null)
            throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status == "arquivado")
            throw new InvalidOperationException("Modelos arquivados nao podem ser editados.");

        modelo.Nome         = request.Nome.Trim();
        modelo.Descricao    = request.Descricao?.Trim() ?? string.Empty;
        modelo.Categoria    = request.Categoria;
        modelo.TipoUso      = request.TipoUso;
        modelo.AtualizadoEm = DateTime.UtcNow;

        await _uof.ModeloRepository.AtualizarAsync(modelo);

        // Recria seções, campos e assinantes (estratégia delete+insert)
        await _uof.ModeloRepository.DeletarSecoesPorModeloAsync(request.Id);
        await _uof.ModeloRepository.DeletarAssinantesPorModeloAsync(request.Id);

        // Salva seções
        for (int i = 0; i < request.Secoes.Count; i++)
        {
            var secDto = request.Secoes[i];
            var secao  = new DocumentoModeloSecao
            {
                ModeloId = request.Id,
                Titulo   = secDto.Titulo?.Trim() ?? $"Seção {i + 1}",
                Tipo     = secDto.Tipo,
                Conteudo = secDto.Conteudo ?? string.Empty,
                Ordem    = i,
            };
            var secaoId = await _uof.ModeloRepository.CriarSecaoAsync(secao);

            // Salva campos da seção
            for (int j = 0; j < secDto.Campos.Count; j++)
            {
                var campoDto = secDto.Campos[j];
                var campo    = new DocumentoModeloCampo
                {
                    SecaoId     = secaoId,
                    Label       = campoDto.Label?.Trim() ?? string.Empty,
                    TipoCampo   = campoDto.TipoCampo,
                    Obrigatorio = campoDto.Obrigatorio,
                    Ordem       = j,
                };
                await _uof.ModeloRepository.CriarCampoAsync(campo);
            }
        }

        // Salva assinantes
        for (int i = 0; i < request.Assinantes.Count; i++)
        {
            var aDto     = request.Assinantes[i];
            var assinante = new DocumentoModeloAssinante
            {
                ModeloId    = request.Id,
                Papel       = aDto.Papel,
                Rotulo      = aDto.Rotulo?.Trim() ?? string.Empty,
                Obrigatorio = aDto.Obrigatorio,
                Ordem       = i + 1,
                ExibirPdf   = aDto.ExibirPdf,
            };
            await _uof.ModeloRepository.CriarAssinanteAsync(assinante);
        }

        await _uof.CommitAsync();

        var atualizado = await _uof.ModeloRepository.ObterPorIdAsync(request.Id)
                         ?? throw new Exception("Falha ao recuperar modelo apos atualizacao.");

        return _mapper.Map<ModeloResponseDto>(atualizado);
    }
}
