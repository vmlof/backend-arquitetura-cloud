using FluentValidation;

namespace GestaoRH.Application.Features.Funcionarios.Commands.TrocarSenha;

public class TrocarSenhaValidator : AbstractValidator<TrocarSenhaCommand>
{
    public TrocarSenhaValidator()
    {
        RuleFor(x => x.SenhaAtual).NotEmpty().WithMessage("Senha atual e obrigatoria.");
        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("Nova senha e obrigatoria.")
            .MinimumLength(6).WithMessage("A nova senha deve ter pelo menos 6 caracteres.");
    }
}
