using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.SalvarAssinaturaPerfil;

public class SalvarAssinaturaPerfilHandler : IRequestHandler<SalvarAssinaturaPerfilCommand>
{
    private readonly IUnitOfWork _uof;

    public SalvarAssinaturaPerfilHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task Handle(SalvarAssinaturaPerfilCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Base64))
            throw new ArgumentException("Assinatura nao pode ser vazia.");

        await _uof.DocumentoRepository.SalvarAssinaturaPerfilAsync(request.FuncionarioId, request.Base64);
        await _uof.CommitAsync();
    }
}
