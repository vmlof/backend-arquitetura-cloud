using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.Documents.Queries.ListDocuments;

public record ListDocumentsQuery : IRequest<IReadOnlyCollection<DocumentSummaryDto>>;
