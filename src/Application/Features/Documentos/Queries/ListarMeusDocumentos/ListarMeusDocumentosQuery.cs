using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarMeusDocumentos;

public record ListarMeusDocumentosQuery(int FuncionarioId) : IRequest<IEnumerable<InstanciaListagemDto>>;
