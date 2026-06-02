using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarMeusDocumentos;

public class ListarMeusDocumentosHandler : IRequestHandler<ListarMeusDocumentosQuery, IEnumerable<InstanciaListagemDto>>
{
    private readonly IUnitOfWork _uof;

    public ListarMeusDocumentosHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<IEnumerable<InstanciaListagemDto>> Handle(ListarMeusDocumentosQuery request, CancellationToken cancellationToken)
    {
        var list = await _uof.DocumentoRepository.ListarInstanciasPorFuncionarioAsync(request.FuncionarioId);
        return list.Select(i => DocumentoHelpers.ToListagemDto(i));
    }
}
