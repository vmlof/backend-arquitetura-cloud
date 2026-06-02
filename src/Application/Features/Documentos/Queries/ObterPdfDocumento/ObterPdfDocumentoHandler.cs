using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterPdfDocumento;

public class ObterPdfDocumentoHandler : IRequestHandler<ObterPdfDocumentoQuery, string>
{
    private readonly IUnitOfWork _uof;

    public ObterPdfDocumentoHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<string> Handle(ObterPdfDocumentoQuery request, CancellationToken cancellationToken)
    {
        var inst = await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(request.Id);
        if (inst == null)
            throw new KeyNotFoundException("Documento nao encontrado.");

        if (string.IsNullOrEmpty(inst.PdfBase64))
            throw new KeyNotFoundException("PDF ainda nao gerado.");

        return inst.PdfBase64;
    }
}
