using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Queries.ListarModelos;

public record ListarModelosQuery : IRequest<IEnumerable<ModeloListagemDto>>;
