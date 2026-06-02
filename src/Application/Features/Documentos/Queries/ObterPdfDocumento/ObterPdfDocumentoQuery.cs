using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterPdfDocumento;

public record ObterPdfDocumentoQuery(int Id) : IRequest<string>;
