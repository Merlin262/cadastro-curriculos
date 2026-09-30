using FluentValidation;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesQueryValidator : AbstractValidator<GetCandidatesQuery>
{
    public GetCandidatesQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100).WithMessage("O tamanho da página deve estar entre 1 e 100.");

        RuleFor(q => q.Search)
            .MaximumLength(200).WithMessage("O termo de busca deve ter no máximo 200 caracteres.");
    }
}
