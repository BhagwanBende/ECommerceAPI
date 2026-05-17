using System.Net;
using System.Text.Json;
using ECommerceAPI.DTOs;

namespace ECommerceAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            ArgumentException =>
                (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized access."),
            KeyNotFoundException =>
                (HttpStatusCode.NotFound, ex.Message),
            _ =>
                (HttpStatusCode.InternalServerError,
                "Something went wrong. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>(
            false, message);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}