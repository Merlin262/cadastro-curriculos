using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.CheckEmailExists;

public sealed record CheckEmailExistsQuery(string Email) : IRequest<bool>;
