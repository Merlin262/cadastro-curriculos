using CadastroCurriculos.Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using LiteMediator;
using ApplicationValidationException = CadastroCurriculos.Application.Common.Exceptions.ValidationException;

namespace CadastroCurriculos.Tests.Common;

public class ValidationBehaviorTests
{
    public sealed record SampleRequest(string Name) : IRequest<string>;

    private sealed class AlwaysInvalidValidator : AbstractValidator<SampleRequest>
    {
        public AlwaysInvalidValidator()
        {
            RuleFor(r => r.Name).Must(_ => false).WithMessage("Nome inválido.");
        }
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenValidatorFails()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { new AlwaysInvalidValidator() });
        var request = new SampleRequest("qualquer");

        await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            behavior.Handle(request, _ => Task.FromResult("nao deveria chegar aqui"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallsNext_WhenThereAreNoValidators()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(Array.Empty<IValidator<SampleRequest>>());
        var request = new SampleRequest("qualquer");

        var result = await behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", result);
    }
}
