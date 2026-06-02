using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery(int Id) : IRequest<DocumentDetailDto?>;
