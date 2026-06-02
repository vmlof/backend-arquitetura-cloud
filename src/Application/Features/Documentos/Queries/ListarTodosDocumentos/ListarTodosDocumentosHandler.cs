using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarTodosDocumentos;

public class ListarTodosDocumentosHandler : IRequestHandler<ListarTodosDocumentosQuery, IEnumerable<InstanciaListagemDto>>
{
    private readonly IUnitOfWork _uof;

    public ListarTodosDocumentosHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<IEnumerable<InstanciaListagemDto>> Handle(ListarTodosDocumentosQuery request, CancellationToken cancellationToken)
    {
        var list = await _uof.DocumentoRepository.ListarTodasInstanciasAsync();
        return list.Select(i => DocumentoHelpers.ToListagemDto(i));
    }
}
