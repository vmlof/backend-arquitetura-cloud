using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Funcionarios.Commands.TrocarSenha;

public class TrocarSenhaHandler : IRequestHandler<TrocarSenhaCommand>
{
    private readonly IUnitOfWork _uof;

    public TrocarSenhaHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(TrocarSenhaCommand request, CancellationToken cancellationToken)
    {
        var func = await _uof.FuncionarioRepository.ObterPorIdAsync(request.Id)
                   ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, func.Senha))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        await _uof.FuncionarioRepository.AtualizarSenhaAsync(request.Id, BCrypt.Net.BCrypt.HashPassword(request.NovaSenha));
        await _uof.CommitAsync();
    }
}
