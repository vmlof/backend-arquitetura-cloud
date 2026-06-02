using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterAssinaturaPerfil;

public class ObterAssinaturaPerfilHandler : IRequestHandler<ObterAssinaturaPerfilQuery, string?>
{
    private readonly IUnitOfWork _uof;

    public ObterAssinaturaPerfilHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<string?> Handle(ObterAssinaturaPerfilQuery request, CancellationToken cancellationToken)
    {
        return await _uof.DocumentoRepository.ObterAssinaturaPerfilAsync(request.FuncionarioId);
    }
}
