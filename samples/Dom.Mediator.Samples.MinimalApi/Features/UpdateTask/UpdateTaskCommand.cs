using Dom.Mediator.Abstractions;
using FluentValidation;

public record UpdateTaskCommand(
    string Id,
    string Comment,
    Status? Status
    ) : ICommand;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage($"A comment is always required when updating a task.");

    }
}