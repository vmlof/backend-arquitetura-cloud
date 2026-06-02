using FluentValidation;

namespace GestaoRH.Application.Features.Setores.Commands.CadastrarSetor;

public class CadastrarSetorValidator : AbstractValidator<CadastrarSetorCommand>
{
    public CadastrarSetorValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome do setor é obrigatório.");
    }
}
