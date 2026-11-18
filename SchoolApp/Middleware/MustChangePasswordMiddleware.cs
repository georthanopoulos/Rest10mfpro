using Microsoft.AspNetCore.Mvc;

namespace SchoolApp.Middleware;

public class MustChangePasswordMiddleware
{
    private readonly RequestDelegate _next;

    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool mustChangePassword = context.User.FindFirst("MustChangePassword")?.Value == "True";

        bool isChangePasswordEndpoint = context.Request.Path.Equals(
            "/api/v1/auth/change-password", StringComparison.OrdinalIgnoreCase);

        if (context.User.Identity?.IsAuthenticated == true && mustChangePassword && !isChangePasswordEndpoint)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Password change required",
                Detail = "You must change your temporary password before using the API. Use POST /api/v1/auth/change-password."
            });
            return;
        }

        await _next(context);
    }
}