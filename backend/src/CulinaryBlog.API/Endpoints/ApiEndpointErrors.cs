namespace CulinaryBlog.API.Endpoints;

internal static class ApiEndpointErrors
{
    public static IResult Unprocessable(
        string message,
        string field)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [field] = new[] { message }
            },
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Dữ liệu không hợp lệ");
    }
}
