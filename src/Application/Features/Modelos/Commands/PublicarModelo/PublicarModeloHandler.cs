using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.PublicarModelo;

public class PublicarModeloHandler : IRequestHandler<PublicarModeloCommand>
{
    private readonly IUnitOfWork _uof;

    public PublicarModeloHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(PublicarModeloCommand request, CancellationToken cancellationToken)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(request.Id);
        if (modelo == null)
            throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status == "arquivado")
            throw new InvalidOperationException("Modelos arquivados nao podem ser publicados.");

        if (!modelo.Assinantes.Any(a => a.Obrigatorio))
            throw new InvalidOperationException("O modelo precisa ter pelo menos 1 assinante obrigatorio para ser publicado.");

        await _uof.ModeloRepository.PublicarAsync(request.Id);
        await _uof.CommitAsync();
    }
}
