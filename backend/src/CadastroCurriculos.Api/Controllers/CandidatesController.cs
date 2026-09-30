using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;
using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Candidates.Queries.CheckEmailExists;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidateResume;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidates;
using CadastroCurriculos.Application.Common;
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
    /// O PDF original (quando enviado) é reanexado aqui para ser salvo junto do cadastro.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(PdfFileRules.MaxFileSizeBytes + 1_000_000)]
    public async Task<ActionResult<CandidateDto>> Create(
        [FromForm] CreateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        byte[]? resumeFileContent = null;
        var resumeFileName = request.ResumeFileName;

        if (request.ResumeFile is not null && request.ResumeFile.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await request.ResumeFile.CopyToAsync(memoryStream, cancellationToken);
            resumeFileContent = memoryStream.ToArray();
            resumeFileName = request.ResumeFile.FileName;
        }

        var command = new CreateCandidateCommand(
            request.FullName,
            request.Email,
            request.Phone,
            request.AreaOfInterest,
            request.ProfessionalSummary,
            request.Source,
            resumeFileName,
            resumeFileContent);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CandidateListItemDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetCandidatesQuery(search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCandidateByIdQuery(id), cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Baixa o PDF original enviado pelo candidato, quando houver um armazenado.
    /// </summary>
    [HttpGet("{id:guid}/resume")]
    public async Task<IActionResult> GetResume(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _sender.Send(new GetCandidateResumeQuery(id), cancellationToken);

        return resume is null ? NotFound() : File(resume.Content, "application/pdf", resume.FileName);
    }

    /// <summary>
    /// Verificação leve e não bloqueante usada pelo formulário para avisar (sem impedir o
    /// cadastro) quando já existe um candidato com o mesmo e-mail.
    /// </summary>
    [HttpGet("check-email")]
    public async Task<ActionResult<CheckEmailExistsResponse>> CheckEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var exists = await _sender.Send(new CheckEmailExistsQuery(email), cancellationToken);
        return Ok(new CheckEmailExistsResponse(exists));
    }

    /// <summary>
    /// Recebe um PDF de currículo, extrai o texto no backend e tenta identificar nome,
    /// e-mail e telefone. Não persiste nada: o resultado apenas pré-preenche o formulário
    /// no frontend, que reaproveita o endpoint POST /api/candidates para salvar.
    /// </summary>
    [HttpPost("extract-resume")]
    [RequestSizeLimit(PdfFileRules.MaxFileSizeBytes)]
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

public sealed class CreateCandidateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AreaOfInterest { get; set; }
    public string? ProfessionalSummary { get; set; }
    public CandidateSource Source { get; set; } = CandidateSource.Manual;
    public string? ResumeFileName { get; set; }
    public IFormFile? ResumeFile { get; set; }
}

public sealed record CheckEmailExistsResponse(bool Exists);
