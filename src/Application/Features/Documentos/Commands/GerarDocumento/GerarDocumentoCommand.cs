using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.GerarDocumento;

public record GerarDocumentoCommand(GerarDocumentoDto Dto, int EmpresaId) : IRequest<object>;
