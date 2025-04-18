using FluentValidation;
using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Validators
{
    public class MovieUpdateDTOValidator : AbstractValidator<MovieUpdateDTO>
    {
        public MovieUpdateDTOValidator()
        {
            RuleFor(x => x.IdMovie)
                .NotEmpty().WithMessage("IdMovie is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Synopsis)
                .MaximumLength(500).WithMessage("Synopsis cannot exceed 500 characters");

            RuleFor(x => x.categoryId)
                .NotEmpty().WithMessage("Category is required");

            RuleFor(x => x.ImageFile)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Image file size must be less than 5MB")
                .Must(file => file == null ||
                    new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                .WithMessage("Only .jpg, .jpeg, and .png files are allowed");
        }
    }
}
