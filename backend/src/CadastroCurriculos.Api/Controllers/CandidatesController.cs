using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;
using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidates;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;
using Microsoft.AspNetCore.Mvc;

namespace CadastroCurriculos.Api.Controllers;

[ApiController]
[Route("api/candidates")]
public sealed class CandidatesController : ControllerBase
{
    private readonly ISender _sender;

    public CandidatesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Cadastra um candidato. Usado tanto pelo fluxo manual quanto pelo fluxo com PDF
    /// (após o usuário revisar/completar os dados extraídos) — mesmo endpoint, mesma validação.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CandidateDto>> Create(
        [FromBody] CreateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCandidateCommand(
            request.FullName,
            request.Email,
            request.Phone,
            request.AreaOfInterest,
            request.ProfessionalSummary,
            request.Source,
            request.ResumeFileName);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<CandidateListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCandidatesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCandidateByIdQuery(id), cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Recebe um PDF de currículo, extrai o texto no backend e tenta identificar nome,
    /// e-mail e telefone. Não persiste nada: o resultado apenas pré-preenche o formulário
    /// no frontend, que reaproveita o endpoint POST /api/candidates para salvar.
    /// </summary>
    [HttpPost("extract-resume")]
    [RequestSizeLimit(ExtractResumeDataCommandValidator.MaxFileSizeBytes)]
    public async Task<ActionResult<ExtractedResumeDataDto>> ExtractResume(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Nenhum arquivo foi enviado." });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var command = new ExtractResumeDataCommand(memoryStream.ToArray(), file.FileName);
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateCandidateRequest(
    string FullName,
    string Email,
    string? Phone,
    string? AreaOfInterest,
    string? ProfessionalSummary,
    CandidateSource Source = CandidateSource.Manual,
    string? ResumeFileName = null);
