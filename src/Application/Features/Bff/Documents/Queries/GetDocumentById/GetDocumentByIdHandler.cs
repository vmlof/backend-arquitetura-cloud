using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.Documents.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto?>
{
    private readonly IBffDocumentsClient _documentsClient;

    public GetDocumentByIdHandler(IBffDocumentsClient documentsClient)
    {
        _documentsClient = documentsClient;
    }

    public Task<DocumentDetailDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        return _documentsClient.GetByIdAsync(request.Id, cancellationToken);
    }
}
