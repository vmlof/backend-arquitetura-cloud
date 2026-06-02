using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.SalvarAssinaturaPerfil;

public record SalvarAssinaturaPerfilCommand(int FuncionarioId, string Base64) : IRequest;
