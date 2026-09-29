using CadastroCurriculos.Application.Candidates.Dtos;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;

public sealed record GetCandidateByIdQuery(Guid Id) : IRequest<CandidateDto?>;
