namespace EnvironmentsService.Src.Application.Validator;

using EnvironmentsService.Src.Application.DTOs.Create;
using FluentValidation;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.EnvironmentId)
            .NotEmpty().WithMessage("El ID del entorno es obligatorio.");

        RuleFor(x => x.TimeRanges)
            .NotEmpty().WithMessage("Se requiere al menos un rango de tiempo.")
            .Must(trs => trs.All(tr => tr.StartDate < tr.EndDate))
                .WithMessage("Cada rango debe tener una fecha de inicio anterior a la de fin.")
            .Must(trs => trs.All(tr => tr.StartDate > DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                .WithMessage("Ningún rango puede comenzar en el pasado.");
        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("El precio total debe ser mayor a 0.");
    }
}
