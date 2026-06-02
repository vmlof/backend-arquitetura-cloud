using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.Documents.Queries.ListDocuments;

public class ListDocumentsHandler : IRequestHandler<ListDocumentsQuery, IReadOnlyCollection<DocumentSummaryDto>>
{
    private readonly IBffDocumentsClient _documentsClient;

    public ListDocumentsHandler(IBffDocumentsClient documentsClient)
    {
        _documentsClient = documentsClient;
    }

    public Task<IReadOnlyCollection<DocumentSummaryDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        return _documentsClient.ListAsync(cancellationToken);
    }
}
