using System.Text.Json;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ApplicationNotFoundException = CulinaryBlog.Application.Common.Exceptions.NotFoundException;
using ApplicationValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;
using DomainNotFoundException = CulinaryBlog.Domain.Exceptions.NotFoundException;
using DomainValidationException = CulinaryBlog.Domain.Exceptions.ValidationException;
using ForbiddenException = CulinaryBlog.Application.Common.Exceptions.ForbiddenException;

namespace CulinaryBlog.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationNotFoundException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message);
        }
        catch (DomainNotFoundException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                ex.Message);
        }
        catch (ApplicationValidationException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                ex.Message);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "Business rule conflict",
                ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Domain error",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception occurred while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path.ToString()
        };

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            JsonSerializerOptions.Web,
            context.RequestAborted);
    }
}