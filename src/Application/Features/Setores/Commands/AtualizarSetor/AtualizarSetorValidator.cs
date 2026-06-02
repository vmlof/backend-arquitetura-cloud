using FluentValidation;

namespace GestaoRH.Application.Features.Setores.Commands.AtualizarSetor;

public class AtualizarSetorValidator : AbstractValidator<AtualizarSetorCommand>
{
    public AtualizarSetorValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id do setor deve ser maior que 0.");
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome do setor é obrigatório.");
    }
}
