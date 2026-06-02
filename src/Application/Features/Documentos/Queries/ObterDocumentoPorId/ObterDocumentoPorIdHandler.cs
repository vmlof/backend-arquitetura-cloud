using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterDocumentoPorId;

public class ObterDocumentoPorIdHandler : IRequestHandler<ObterDocumentoPorIdQuery, InstanciaDetalheDto>
{
    private readonly IUnitOfWork _uof;

    public ObterDocumentoPorIdHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<InstanciaDetalheDto> Handle(ObterDocumentoPorIdQuery request, CancellationToken cancellationToken)
    {
        var inst = await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(request.Id);
        if (inst == null)
            throw new KeyNotFoundException("Documento nao encontrado.");

        var conteudoHtml = inst.Status == "concluido"
            ? DocumentoHelpers.InjetarAssinaturasNoHtml(inst.ConteudoHtml, inst.Assinaturas)
            : inst.ConteudoHtml;

        return DocumentoHelpers.ToDetalheDto(inst, request.SignerId, request.SignerTipo, conteudoHtml);
    }
}
