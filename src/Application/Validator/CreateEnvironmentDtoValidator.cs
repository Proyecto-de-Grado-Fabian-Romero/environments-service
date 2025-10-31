namespace EnvironmentsService.src.Application.Validator;

using EnvironmentsService.Src.Application.DTOs.Create;
using FluentValidation;

public class CreateEnvironmentDtoValidator : AbstractValidator<CreateEnvironmentDto>
{
    public CreateEnvironmentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("El título es obligatorio.")
            .MinimumLength(3)
            .WithMessage("El título debe tener al menos 3 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es obligatoria.")
            .MinimumLength(10)
            .WithMessage("La descripción debe tener al menos 10 caracteres.");

        RuleFor(x => x.Location).NotEmpty().WithMessage("La ubicación es obligatoria.");

        RuleFor(x => x.TypePublicKey).NotEmpty().WithMessage("El tipo de entorno es obligatorio.");

        RuleFor(x => x.RentalUnit)
            .Must(unit => unit == "Horas" || unit == "Días")
            .WithMessage("La unidad de alquiler debe ser 'Horas' o 'Días'.");

        RuleFor(x => x.MinRentalTime)
            .GreaterThan(0)
            .WithMessage("El tiempo mínimo debe ser mayor a 0.");

        RuleFor(x => x.MaxRentalTime)
            .GreaterThan(0)
            .WithMessage("El tiempo máximo debe ser mayor a 0.")
            .GreaterThanOrEqualTo(x => x.MinRentalTime)
            .WithMessage("El tiempo máximo debe ser mayor o igual al mínimo.");

        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("La capacidad debe ser mayor a 0.");

        RuleFor(x => x.Images)
            .Must(images => images.Count <= 10)
            .WithMessage("Se pueden subir como máximo 10 imágenes.");
    }
}
