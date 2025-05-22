namespace EnvironmentsService.src.Application.Validator;

using EnvironmentsService.Src.Application.DTOs.Create;
using FluentValidation;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.EnvironmentId)
            .NotEmpty().WithMessage("El ID del entorno es obligatorio.");

        RuleFor(x => x.RenterId)
            .NotEmpty().WithMessage("El ID del arrendatario es obligatorio.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("La fecha de inicio debe ser anterior a la de fin.")
            .GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).WithMessage("La fecha de inicio no puede estar en el pasado.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior a la de inicio.");
    }
}
