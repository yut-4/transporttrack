using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Application.Services;

internal static class ServiceValidation
{
    public static void Validate<T>(T model)
    {
        var context = new ValidationContext(model!);
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model!, context, errors, validateAllProperties: true))
        {
            throw new ValidationException(string.Join(" ", errors.Select(error => error.ErrorMessage)));
        }
    }

    public static void ValidateId(int id)
    {
        if (id <= 0) throw new ValidationException("El identificador debe ser mayor que cero.");
    }
}
