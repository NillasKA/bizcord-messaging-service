using Bizcord.Messaging.Api.Application;
using Bizcord.Messaging.Api.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bizcord.Messaging.Api.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            DomainException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest, Title = "Invalid message", Detail = exception.Message
            },
            MessagePublicationException publication => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Event publication not confirmed",
                Detail = "The message is saved. Retrieve it using messageId; blindly retrying POST can create duplicates.",
                Extensions = { ["messageId"] = publication.MessageId, ["persisted"] = true }
            },
            _ => null
        };
        if (problem is null) return false;
        if (exception is MessagePublicationException) logger.LogError(exception, "A saved message could not be published");
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
