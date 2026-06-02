using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.CriarModelo;

public class CriarModeloHandler : IRequestHandler<CriarModeloCommand, ModeloResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public CriarModeloHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<ModeloResponseDto> Handle(CriarModeloCommand request, CancellationToken cancellationToken)
    {
        var modelo = new DocumentoModelo
        {
            Nome         = request.Nome.Trim(),
            Descricao    = request.Descricao?.Trim() ?? string.Empty,
            Categoria    = request.Categoria,
            TipoUso      = request.TipoUso,
            Status       = "rascunho",
            Versao       = 1,
            CriadoEm    = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        var modeloId = await _uof.ModeloRepository.CriarAsync(modelo);

        // Salva seções
        for (int i = 0; i < request.Secoes.Count; i++)
        {
            var secDto = request.Secoes[i];
            var secao  = new DocumentoModeloSecao
            {
                ModeloId = modeloId,
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
                ModeloId    = modeloId,
                Papel       = aDto.Papel,
                Rotulo      = aDto.Rotulo?.Trim() ?? string.Empty,
                Obrigatorio = aDto.Obrigatorio,
                Ordem       = i + 1,
                ExibirPdf   = aDto.ExibirPdf,
            };
            await _uof.ModeloRepository.CriarAssinanteAsync(assinante);
        }

        await _uof.CommitAsync();

        var criado = await _uof.ModeloRepository.ObterPorIdAsync(modeloId)
                     ?? throw new Exception("Falha ao recuperar modelo apos cadastro.");

        return _mapper.Map<ModeloResponseDto>(criado);
    }
}
