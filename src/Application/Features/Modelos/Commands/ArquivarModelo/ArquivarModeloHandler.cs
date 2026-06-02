using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.ArquivarModelo;

public class ArquivarModeloHandler : IRequestHandler<ArquivarModeloCommand>
{
    private readonly IUnitOfWork _uof;

    public ArquivarModeloHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(ArquivarModeloCommand request, CancellationToken cancellationToken)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(request.Id);
        if (modelo == null)
            throw new KeyNotFoundException("Modelo nao encontrado.");

        await _uof.ModeloRepository.ArquivarAsync(request.Id);
        await _uof.CommitAsync();
    }
}
