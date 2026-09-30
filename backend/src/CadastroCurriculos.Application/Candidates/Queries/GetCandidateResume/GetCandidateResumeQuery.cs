using CadastroCurriculos.Application.Candidates.Dtos;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidateResume;

public sealed record GetCandidateResumeQuery(Guid Id) : IRequest<CandidateResumeDto?>;
