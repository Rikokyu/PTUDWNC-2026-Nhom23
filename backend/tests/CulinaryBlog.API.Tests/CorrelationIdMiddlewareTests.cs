using CulinaryBlog.API.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PreservesSafeClientCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "client-trace-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            "client-trace-123",
            context.Response.Headers[CorrelationIdMiddleware.HeaderName]);
        Assert.Equal(
            "client-trace-123",
            CorrelationIdMiddleware.GetCorrelationId(context));
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationIdWhenClientDidNotSendOne()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName]
            .ToString();
        Assert.True(Guid.TryParseExact(correlationId, "N", out _));
        Assert.Equal(correlationId, CorrelationIdMiddleware.GetCorrelationId(context));
    }

    [Fact]
    public async Task InvokeAsync_ReplacesUnsafeClientCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "bad\r\nvalue";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName]
            .ToString();
        Assert.NotEqual("bad\r\nvalue", correlationId);
        Assert.True(Guid.TryParseExact(correlationId, "N", out _));
    }
}
