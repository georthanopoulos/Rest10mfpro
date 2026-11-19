using Microsoft.AspNetCore.Mvc;

namespace SchoolApp.Middleware;

public class MustChangePasswordMiddleware
{
                                                        // Endpoints that must stay reachable while the temporary password is still in use:
                                                             // the password change itself, and login (a client may still hold the old restricted token).
    private static readonly string[] AllowedPaths =
    {
        "/api/v1/auth/change-password",
        "/api/v1/auth/login"
    };

    private readonly RequestDelegate _next;

    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool mustChangePassword = context.User.FindFirst("MustChangePassword")?.Value == "True";

        bool isAllowedEndpoint = AllowedPaths.Any(path =>
            context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

        if (context.User.Identity?.IsAuthenticated == true && mustChangePassword && !isAllowedEndpoint)
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