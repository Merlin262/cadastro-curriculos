using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Common;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed record GetCandidatesQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<CandidateListItemDto>>;
