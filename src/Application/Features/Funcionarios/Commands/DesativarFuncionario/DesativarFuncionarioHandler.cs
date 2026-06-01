using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.DesativarFuncionario;

public class DesativarFuncionarioHandler : IRequestHandler<DesativarFuncionarioCommand>
{
    private readonly IUnitOfWork _uof;

    public DesativarFuncionarioHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(DesativarFuncionarioCommand request, CancellationToken cancellationToken)
    {
        _ = await _uof.FuncionarioRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

        await _uof.FuncionarioRepository.DesativarAsync(request.Id);
        await _uof.CommitAsync();
    }
}
