using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Empresas.Commands.DesativarEmpresa;

public class DesativarEmpresaHandler : IRequestHandler<DesativarEmpresaCommand>
{
    private readonly IUnitOfWork _uof;

    public DesativarEmpresaHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(DesativarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(request.Id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        await _uof.EmpresaRepository.DesativarAsync(request.Id);
        await _uof.CommitAsync();
    }
}
