using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterDocumentoPorId;

public record ObterDocumentoPorIdQuery(int Id, int SignerId, string SignerTipo) : IRequest<InstanciaDetalheDto>;
