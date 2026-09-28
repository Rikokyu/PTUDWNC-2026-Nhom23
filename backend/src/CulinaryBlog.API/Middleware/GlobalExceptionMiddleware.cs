using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status403Forbidden,
                ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                ex.Message);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode =
            statusCode;

        context.Response.ContentType =
            "application/json";

        var response =
            new
            {
                statusCode,
                message
            };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}