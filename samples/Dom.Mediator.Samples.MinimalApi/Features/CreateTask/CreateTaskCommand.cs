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
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty();
    }
}