using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.CadastrarSetor;

public record CadastrarSetorCommand(string Nome, string Descricao) : IRequest<SetorResponseDto>;
