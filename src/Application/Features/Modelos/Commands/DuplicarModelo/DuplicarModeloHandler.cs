using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.DuplicarModelo;

public class DuplicarModeloHandler : IRequestHandler<DuplicarModeloCommand, ModeloResponseDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public DuplicarModeloHandler(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    public async Task<ModeloResponseDto> Handle(DuplicarModeloCommand request, CancellationToken cancellationToken)
    {
        var original = await _uof.ModeloRepository.ObterPorIdAsync(request.Id);
        if (original == null)
            throw new KeyNotFoundException("Modelo nao encontrado.");

        var modelo = new DocumentoModelo
        {
            Nome         = $"{original.Nome} (cópia)",
            Descricao    = original.Descricao,
            Categoria    = original.Categoria,
            TipoUso      = original.TipoUso,
            Status       = "rascunho",
            Versao       = 1,
            CriadoEm    = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        var modeloId = await _uof.ModeloRepository.CriarAsync(modelo);

        // Salva seções
        for (int i = 0; i < original.Secoes.Count; i++)
        {
            var secOriginal = original.Secoes[i];
            var secao  = new DocumentoModeloSecao
            {
                ModeloId = modeloId,
                Titulo   = secOriginal.Titulo,
                Tipo     = secOriginal.Tipo,
                Conteudo = secOriginal.Conteudo,
                Ordem    = secOriginal.Ordem,
            };
            var secaoId = await _uof.ModeloRepository.CriarSecaoAsync(secao);

            // Salva campos da seção
            for (int j = 0; j < secOriginal.Campos.Count; j++)
            {
                var campoOriginal = secOriginal.Campos[j];
                var campo    = new DocumentoModeloCampo
                {
                    SecaoId     = secaoId,
                    Label       = campoOriginal.Label,
                    TipoCampo   = campoOriginal.TipoCampo,
                    Obrigatorio = campoOriginal.Obrigatorio,
                    Ordem       = campoOriginal.Ordem,
                };
                await _uof.ModeloRepository.CriarCampoAsync(campo);
            }
        }

        // Salva assinantes
        for (int i = 0; i < original.Assinantes.Count; i++)
        {
            var aOriginal     = original.Assinantes[i];
            var assinante = new DocumentoModeloAssinante
            {
                ModeloId    = modeloId,
                Papel       = aOriginal.Papel,
                Rotulo      = aOriginal.Rotulo,
                Obrigatorio = aOriginal.Obrigatorio,
                Ordem       = aOriginal.Ordem,
                ExibirPdf   = aOriginal.ExibirPdf,
            };
            await _uof.ModeloRepository.CriarAssinanteAsync(assinante);
        }

        await _uof.CommitAsync();

        var duplicado = await _uof.ModeloRepository.ObterPorIdAsync(modeloId)
                       ?? throw new Exception("Falha ao recuperar modelo duplicado.");

        return _mapper.Map<ModeloResponseDto>(duplicado);
    }
}
