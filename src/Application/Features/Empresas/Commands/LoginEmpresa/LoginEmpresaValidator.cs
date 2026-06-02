using FluentValidation;

namespace GestaoRH.Application.Features.Empresas.Commands.LoginEmpresa;

public class LoginEmpresaValidator : AbstractValidator<LoginEmpresaCommand>
{
    public LoginEmpresaValidator()
    {
        RuleFor(x => x.Cnpj).NotEmpty().WithMessage("CNPJ é obrigatório.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
