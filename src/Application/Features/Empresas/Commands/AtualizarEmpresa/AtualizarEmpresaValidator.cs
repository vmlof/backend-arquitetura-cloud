using FluentValidation;

namespace GestaoRH.Application.Features.Empresas.Commands.AtualizarEmpresa;

public class AtualizarEmpresaValidator : AbstractValidator<AtualizarEmpresaCommand>
{
    public AtualizarEmpresaValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id deve ser maior que 0.");
        RuleFor(x => x.RazaoSocial).NotEmpty().WithMessage("Razão Social é obrigatória.");
        RuleFor(x => x.ResponsavelNome).NotEmpty().WithMessage("Nome do responsável é obrigatório.");
        RuleFor(x => x.ResponsavelSobrenome).NotEmpty().WithMessage("Sobrenome do responsável é obrigatório.");
    }
}
