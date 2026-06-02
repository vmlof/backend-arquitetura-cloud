using FluentValidation;

namespace GestaoRH.Application.Features.Empresas.Commands.CadastrarEmpresa;

public class CadastrarEmpresaValidator : AbstractValidator<CadastrarEmpresaCommand>
{
    public CadastrarEmpresaValidator()
    {
        RuleFor(x => x.Cnpj).NotEmpty().WithMessage("CNPJ é obrigatório.");
        RuleFor(x => x.RazaoSocial).NotEmpty().WithMessage("Razão Social é obrigatória.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha é obrigatória.");
        RuleFor(x => x.ResponsavelNome).NotEmpty().WithMessage("Nome do responsável é obrigatório.");
        RuleFor(x => x.ResponsavelSobrenome).NotEmpty().WithMessage("Sobrenome do responsável é obrigatório.");
    }
}
