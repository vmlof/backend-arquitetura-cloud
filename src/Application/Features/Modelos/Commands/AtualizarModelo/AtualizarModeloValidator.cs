using FluentValidation;

namespace GestaoRH.Application.Features.Modelos.Commands.AtualizarModelo;

public class AtualizarModeloValidator : AbstractValidator<AtualizarModeloCommand>
{
    public AtualizarModeloValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id do modelo deve ser maior que 0.");
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome do modelo é obrigatório.");
        RuleFor(x => x.Categoria).NotEmpty().WithMessage("Categoria é obrigatória.");
        RuleFor(x => x.TipoUso).NotEmpty().WithMessage("Tipo de uso é obrigatório.");
    }
}
