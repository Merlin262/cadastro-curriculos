using CadastroCurriculos.Application.Candidates.Dtos;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed record GetCandidatesQuery : IRequest<List<CandidateListItemDto>>;
