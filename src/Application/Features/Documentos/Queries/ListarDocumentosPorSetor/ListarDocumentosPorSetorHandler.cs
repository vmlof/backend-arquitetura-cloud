using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ListarDocumentosPorSetor;

public class ListarDocumentosPorSetorHandler : IRequestHandler<ListarDocumentosPorSetorQuery, IEnumerable<InstanciaListagemDto>>
{
    private readonly IUnitOfWork _uof;

    public ListarDocumentosPorSetorHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<IEnumerable<InstanciaListagemDto>> Handle(ListarDocumentosPorSetorQuery request, CancellationToken cancellationToken)
    {
        var list = await _uof.DocumentoRepository.ListarInstanciasPorSetorAsync(request.SetorId);
        return list.Select(i => DocumentoHelpers.ToListagemDto(i));
    }
}
