using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.AtualizarModelo;

public record AtualizarModeloCommand(
    int Id,
    string Nome,
    string Descricao,
    string Categoria,
    string TipoUso,
    List<SecaoCadastroDto> Secoes,
    List<AssinanteCadastroDto> Assinantes
) : IRequest<ModeloResponseDto>;
