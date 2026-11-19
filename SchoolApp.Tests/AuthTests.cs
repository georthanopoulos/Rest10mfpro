using System.Net;
using System.Net.Http.Json;

namespace SchoolApp.Tests;

public class AuthTests : ApiTestBase
{
    public AuthTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_with_unknown_user_returns_401()
    {
        var response = await LoginRawAsync("nobody-here", Password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_looks_exactly_like_login_with_unknown_user()
    {
        var username = await CreateUserAsync("EMPLOYEE");

        var wrongPassword = await LoginRawAsync(username, "Wr0ng-Passw0rd!");
        var unknownUser = await LoginRawAsync("nobody-here", "Wr0ng-Passw0rd!");

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.Equal(await DetailAsync(wrongPassword), await DetailAsync(unknownUser));
    }

    [Fact]
    public async Task Login_with_malformed_body_returns_400()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/v1/auth/login", new { username = "a", password = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Pending_account_cannot_log_in()
    {
        var admin = await AdminAsync();
        var pre = await PreRegisterTeacherAsync(admin);

        var response = await LoginRawAsync(pre.User.Username, Password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_admin_must_change_the_temporary_password_before_anything_else()
    {
        // 1. login works and tells the client what to do
        var login = await LoginRawAsync(ApiFactory.AdminUsername, ApiFactory.AdminTempPassword);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var result = await Read<LoginResult>(login);
        Assert.True(result.MustChangePassword);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));

        // 2. every other endpoint is blocked and the error says how to fix it
        var client = WithBearer(Anonymous(), result.Token);

        var blocked = await client.GetAsync("/api/v1/teachers");
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Contains("change-password", await DetailAsync(blocked));

        // 3. bad attempts are rejected
        var wrongCurrent = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = "Wr0ng-Passw0rd!", newPassword = "New-Passw0rd!9" });
        Assert.Equal(HttpStatusCode.BadRequest, wrongCurrent.StatusCode);

        var samePassword = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = ApiFactory.AdminTempPassword, newPassword = ApiFactory.AdminTempPassword });
        Assert.Equal(HttpStatusCode.BadRequest, samePassword.StatusCode);

        var weakPassword = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = ApiFactory.AdminTempPassword, newPassword = "weak" });
        Assert.Equal(HttpStatusCode.BadRequest, weakPassword.StatusCode);

        // 4. the real change succeeds
        var changed = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { currentPassword = ApiFactory.AdminTempPassword, newPassword = "New-Passw0rd!9" });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        // 5. the old password is dead, the new one gives an unrestricted token
        var oldLogin = await LoginRawAsync(ApiFactory.AdminUsername, ApiFactory.AdminTempPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await LoginRawAsync(ApiFactory.AdminUsername, "New-Passw0rd!9");
        var newResult = await Read<LoginResult>(newLogin);
        Assert.False(newResult.MustChangePassword);
        Assert.Null(newResult.Message);

        var allowed = await WithBearer(Anonymous(), newResult.Token).GetAsync("/api/v1/teachers");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    /// <summary>
    /// A client that still holds the restricted token (for example the Swagger "Authorize" button)
    /// must still be able to call login again. At the time of writing this FAILS with 403, because
    /// MustChangePasswordMiddleware only exempts /auth/change-password (see review item 1).
    /// </summary>
    [Fact]
    public async Task Login_stays_reachable_with_a_restricted_token()
    {
        var username = await CreateUserAsync("EMPLOYEE", mustChangePassword: true);

        var first = await Read<LoginResult>(await LoginRawAsync(username, Password));
        Assert.True(first.MustChangePassword);

        var staleClient = WithBearer(Anonymous(), first.Token);
        var second = await LoginRawAsync(username, Password, staleClient);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task Activation_rejects_bad_tokens_and_a_token_works_only_once()
    {
        var admin = await AdminAsync();
        var pre = await PreRegisterTeacherAsync(admin);

        var unknownToken = await Anonymous().PostAsJsonAsync("/api/v1/auth/activate",
            new { token = new string('A', 64), password = Password });
        Assert.Equal(HttpStatusCode.BadRequest, unknownToken.StatusCode);

        var weakPassword = await Anonymous().PostAsJsonAsync("/api/v1/auth/activate",
            new { token = pre.ActivationToken, password = "weak" });
        Assert.Equal(HttpStatusCode.BadRequest, weakPassword.StatusCode);

        var ok = await Anonymous().PostAsJsonAsync("/api/v1/auth/activate",
            new { token = pre.ActivationToken, password = Password });
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);

        var reused = await Anonymous().PostAsJsonAsync("/api/v1/auth/activate",
            new { token = pre.ActivationToken, password = "Other-Passw0rd!1" });
        Assert.Equal(HttpStatusCode.BadRequest, reused.StatusCode);

        var login = await LoginRawAsync(pre.User.Username, Password);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False((await Read<LoginResult>(login)).MustChangePassword);
    }
}
