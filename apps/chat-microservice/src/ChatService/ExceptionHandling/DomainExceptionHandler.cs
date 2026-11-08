using ChatService.DomainModel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.ExceptionHandling
{
    // Global exception handler (ADR 0011): ét sted, hvor exceptions bliver oversat til HTTP-statuskoder.
    // Domænet og handlerne kaster bare NotFoundException, ForbiddenException osv. og kender intet til HTTP.
    // ASP.NET kalder TryHandleAsync for hver exception, der slipper ud af en request (kræver app.UseExceptionHandler()).
    public class DomainExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        public DomainExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // 1) Vælg statuskode og titel ud fra exceptionens TYPE (tabellen i ADR 0011 og 0013)
            (int status, string title) = exception switch
            {
                NotFoundException  => (StatusCodes.Status404NotFound, "Findes ikke"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Ingen adgang"),
                ConflictException  => (StatusCodes.Status409Conflict, "Konflikt med en regel"),
                DomainValidationException => (StatusCodes.Status400BadRequest, "Ugyldigt input"),
                _                  => (StatusCodes.Status500InternalServerError, "Der skete en uventet fejl")
            };

            // 2) Sæt statuskoden og skriv et ProblemDetails-svar (samme format som 400-valideringsfejlene).
            httpContext.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = exception.Message
                }
            });
        }
    }
}
