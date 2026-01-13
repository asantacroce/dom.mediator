using Dom.Mediator.Abstractions;
using FluentValidation;

public record CreateTaskCommand(
    string Title, 
    string Description, 
    DateTime? DueDate) : ICommand<string>;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage($"{nameof(CreateTaskCommand.Title).ToLower()} is required.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage($"{nameof(CreateTaskCommand.Description).ToLower()} is required.");

    }
}