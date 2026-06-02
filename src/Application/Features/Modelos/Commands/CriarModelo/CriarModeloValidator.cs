using FluentValidation;

namespace GestaoRH.Application.Features.Modelos.Commands.CriarModelo;

public class CriarModeloValidator : AbstractValidator<CriarModeloCommand>
{
    public CriarModeloValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome do modelo é obrigatório.");
        RuleFor(x => x.Categoria).NotEmpty().WithMessage("Categoria é obrigatória.");
        RuleFor(x => x.TipoUso).NotEmpty().WithMessage("Tipo de uso é obrigatório.");
    }
}
