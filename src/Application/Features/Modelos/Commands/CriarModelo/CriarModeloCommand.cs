using GestaoRH.Application.Common.DTOs;
using MediatR;

namespace GestaoRH.Application.Features.Modelos.Commands.CriarModelo;

public record CriarModeloCommand(
    string Nome,
    string Descricao,
    string Categoria,
    string TipoUso,
    List<SecaoCadastroDto> Secoes,
    List<AssinanteCadastroDto> Assinantes
) : IRequest<ModeloResponseDto>;
