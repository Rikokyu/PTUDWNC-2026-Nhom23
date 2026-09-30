using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ApplicationNotFoundException = CulinaryBlog.Application.Common.Exceptions.NotFoundException;
using ApplicationValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;
using DomainNotFoundException = CulinaryBlog.Domain.Exceptions.NotFoundException;
using DomainValidationException = CulinaryBlog.Domain.Exceptions.ValidationException;
using ForbiddenException = CulinaryBlog.Application.Common.Exceptions.ForbiddenException;
using ApplicationUnauthorizedException = CulinaryBlog.Application.Common.Exceptions.UnauthorizedException;

namespace CulinaryBlog.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IProblemDetailsService problemDetailsService)
    {
        _next = next;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
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
        catch (ApplicationUnauthorizedException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message);
        }
        catch (ApplicationValidationException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ex.Message);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
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
            catch (ArgumentException ex)
            {
                await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid request",
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

    private async Task WriteProblemAsync(
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
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path.ToString()
        };

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem
            });
    }
}