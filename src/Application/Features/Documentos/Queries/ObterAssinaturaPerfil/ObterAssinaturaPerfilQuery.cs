using MediatR;

namespace GestaoRH.Application.Features.Documentos.Queries.ObterAssinaturaPerfil;

public record ObterAssinaturaPerfilQuery(int FuncionarioId) : IRequest<string?>;
