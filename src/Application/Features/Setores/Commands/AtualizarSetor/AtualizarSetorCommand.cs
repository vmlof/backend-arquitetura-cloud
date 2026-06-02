using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Setores.Commands.AtualizarSetor;

public record AtualizarSetorCommand(int Id, string Nome, string Descricao, bool Ativo) : IRequest<SetorResponseDto>;
