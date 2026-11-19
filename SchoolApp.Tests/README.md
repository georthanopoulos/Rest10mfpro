# SchoolApp.Tests

Integration tests that start the real application in memory (`WebApplicationFactory`) and call it over HTTP.
SQL Server is replaced by an in-memory SQLite database, so **no Docker and no SQL Server are needed**.

## Run

From the repository root (the folder that contains `Rest10mfpro.slnx`):

```powershell
dotnet test SchoolApp.Tests
```

## First-time setup

Add the project to the solution once:

```powershell
dotnet sln Rest10mfpro.slnx add SchoolApp.Tests/SchoolApp.Tests.csproj
```

## Notes

- Each test class gets its own empty database (created from the EF model, with the seed data).
- Settings such as the JWT secret are supplied by `ApiFactory`; nothing is read from `.env` or user-secrets.
- SQLite compares text case-sensitively and does not enforce every SQL Server type rule, so these tests check
  the API behaviour, not the SQL Server schema. The SQL scripts in `SchoolApp/Db` are checked separately, in DBeaver.
- `AuthTests.Login_stays_reachable_with_a_restricted_token` is expected to fail until `MustChangePasswordMiddleware`
  also exempts `/api/v1/auth/login`.
