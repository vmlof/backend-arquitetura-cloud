using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.DesativarSetor;

public class DesativarSetorHandler : IRequestHandler<DesativarSetorCommand>
{
    private readonly IUnitOfWork _uof;

    public DesativarSetorHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(DesativarSetorCommand request, CancellationToken cancellationToken)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(request.Id);
        if (setor == null)
            throw new KeyNotFoundException("Setor nao encontrado.");

        await _uof.SetorRepository.DesativarAsync(request.Id);
        await _uof.CommitAsync();
    }
}
