using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarDocumentosPorSetor;

public record ListarDocumentosPorSetorQuery(int SetorId) : IRequest<IEnumerable<InstanciaListagemDto>>;
