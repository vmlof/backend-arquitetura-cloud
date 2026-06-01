using FluentValidation;

namespace GestaoRH.Application.Features.Funcionarios.Commands.AtualizarFuncionario;

public class AtualizarFuncionarioValidator : AbstractValidator<AtualizarFuncionarioCommand>
{
    private static readonly HashSet<string> GenerosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "masculino", "feminino", "sem_genero" };
    private static readonly HashSet<string> TurnosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "matutino", "vespertino", "noturno" };

    public AtualizarFuncionarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome e obrigatorio.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail valido e obrigatorio.");
        RuleFor(x => x.SetorId).GreaterThan(0).WithMessage("Setor e obrigatorio.");
        
        RuleFor(x => x.Genero)
            .Must(g => GenerosValidos.Contains(g))
            .WithMessage("Genero invalido. Use: masculino, feminino ou sem_genero.");
            
        RuleFor(x => x.Turno)
            .Must(t => TurnosValidos.Contains(t))
            .WithMessage("Turno invalido. Use: matutino, vespertino ou noturno.");
    }
}
