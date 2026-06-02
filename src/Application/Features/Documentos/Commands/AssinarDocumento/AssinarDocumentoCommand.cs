using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.AssinarDocumento;

public record AssinarDocumentoCommand(
    int InstanciaId,
    int AssinaturaId,
    AssinarDocumentoDto Dto,
    int SignerId,
    string SignerTipo
) : IRequest<InstanciaDetalheDto>;
