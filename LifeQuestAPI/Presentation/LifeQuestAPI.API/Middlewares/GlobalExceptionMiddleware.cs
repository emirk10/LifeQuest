using System.Net;
using System.Text.Json;

namespace LifeQuestAPI.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sunucuda beklenmeyen bir hata oluştu: {Message}", ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is FluentValidation.ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var validationResponse = new
            {
                IsSuccess = false,
                Message = "Doğrulama hataları oluştu.",
                Errors = validationException.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                })
            };

            var validationJsonResponse = JsonSerializer.Serialize(validationResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(validationJsonResponse);
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            IsSuccess = false,
            Message = $"Sunucuda beklenmeyen bir hata oluştu." // Avoid exposing raw exception messages to the client
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(jsonResponse);
    }
}