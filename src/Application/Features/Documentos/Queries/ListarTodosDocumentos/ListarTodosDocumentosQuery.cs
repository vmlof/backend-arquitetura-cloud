using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarTodosDocumentos;

public record ListarTodosDocumentosQuery : IRequest<IEnumerable<InstanciaListagemDto>>;
