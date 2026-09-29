using System.Net;
using ApplicationValidationException = CadastroCurriculos.Application.Common.Exceptions.ValidationException;
using Microsoft.AspNetCore.Mvc;

namespace CadastroCurriculos.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ValidationProblemDetails(validationException.Errors)
            {
                Title = "Um ou mais erros de validação ocorreram.",
                Status = (int)HttpStatusCode.BadRequest,
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro não tratado ao processar a requisição {Path}.", context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Ocorreu um erro inesperado ao processar sua solicitação.",
                Status = (int)HttpStatusCode.InternalServerError,
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
