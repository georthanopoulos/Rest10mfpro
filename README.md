# Rest10mfpro

REST API for a school registry, built with ASP.NET Core (.NET 10). Employees and admins pre-register teachers and students. Each person then activates their own account with a one-time link and sets their own password.

## Features

- Pre-registration of teachers and students by an employee or an admin
- Account activation with a one-time token (valid for 72 hours, stored only as a SHA-256 hash)
- JWT authentication with role and capability based authorization
- View all users, with filters for teachers and students, with pagination
- Courses and student enrollment
- Teachers and students can see only their own profile and courses
- Soft delete, unique constraints and consistent error responses (ProblemDetails)

## Tech stack

- .NET 10, ASP.NET Core Web API
- Entity Framework Core (Code First) with SQL Server
- JWT (Bearer) and BCrypt password hashing
- AutoMapper
- xUnit and `WebApplicationFactory` for integration tests (SQLite in-memory)
- Docker and Docker Compose

## Getting started

### Requirements

- Docker Desktop
- A SQL client such as DBeaver (to run the database scripts)
- .NET 10 SDK (only to run the tests or to run the API outside Docker)

### 1. Configure the environment

Copy `.env.example` to `.env` and fill in every value. The `.env` file is ignored by git and must never be committed.

```powershell
copy .env.example .env
```

| Variable | Meaning |
| --- | --- |
| `JWT_ISSUER`, `JWT_AUDIENCE` | JWT issuer and audience |
| `JWT_SECRET` | Long random signing key (use at least 32 random characters) |
| `CORS_ORIGIN` | Allowed frontend origin |
| `ALLOWED_HOSTS` | Allowed host names |
| `ASPNETCORE_ENVIRONMENT` | `Development` enables Swagger, `Production` disables it |
| `SEED_ADMIN_USERNAME`, `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD` | The first admin, created at startup with a temporary password |
| `SA_PASSWORD` | Password of the SQL Server `sa` login |
| `DB_HOST`, `DB_PORT`, `DB_NAME` | Database host (`sqlserver` inside Docker), port and name |
| `DB_USER`, `DB_USER_PASSWORD` | Application login with limited rights (read and write only) |

### 2. Start SQL Server

```powershell
docker compose up -d sqlserver
```

### 3. Create the database

The schema is managed with EF Core Code First migrations, but it is applied manually, using the generated SQL scripts in `SchoolApp/Db`. The application never changes the schema by itself.

Run the scripts in this order, with a SQL client connected to `localhost` and the port from `DB_PORT`:

1. `V0__create_database_and_users.sql`: run as `sa`. First replace the placeholders `<GEORGE_PASSWORD>` (schema owner) and `<APP_PASSWORD>` (must match `DB_USER_PASSWORD`).
2. `V1__schema_and_seed.sql`, `V2__account_status_activation_tokens.sql`, `V3__enrollment_capabilities.sql`: run as the schema owner (`george`).

### 4. Start the application

```powershell
docker compose up -d --build
```

Swagger UI (when `ASPNETCORE_ENVIRONMENT=Development`): http://localhost:8081/swagger

To apply code changes later, run `docker compose up -d --build`. To restart only the web app, run `docker compose restart webapp`. Do not use `docker compose down -v`, because it deletes the database volume.

## Authentication and first login

The first admin is created at startup from the `SEED_ADMIN_*` variables, with a **temporary password**.

1. Log in with `POST /api/v1/auth/login`. The response contains the JWT token and `mustChangePassword: true`.
2. While `mustChangePassword` is true, the API allows only `POST /api/v1/auth/login` and `POST /api/v1/auth/change-password`. Every other endpoint answers `403` with a message that points to the password change.
3. Call `POST /api/v1/auth/change-password`, then log in again to get a normal token.

In Swagger, press **Authorize** and paste the token to call protected endpoints.

Tokens are valid for 3 hours.

## Roles and capabilities

Access is controlled by capabilities stored in the token (for example `INSERT_TEACHER`). Each endpoint requires one capability.

| Role | Capabilities |
| --- | --- |
| `ADMIN` | All 21 capabilities |
| `EMPLOYEE` | Insert and view teachers and students, view courses, view all users, enroll and unenroll students |
| `TEACHER` | View only their own profile and courses |
| `STUDENT` | View only their own profile and courses |

Teachers and students are checked with resource-based authorization: they can read only data that belongs to them, even if they know another id.

The full list of endpoints, with their required capability and possible error responses, is documented in Swagger.

## Registration and activation flow

1. An employee or an admin registers a teacher or a student. The server sets the role, and the registrar does not choose a password.
2. The new user receives a one-time activation token, valid for 72 hours. Only its SHA-256 hash is stored in the database.
3. The user calls `POST /api/v1/auth/activate` with the token and chooses their own password. The account becomes active and can log in.
4. Employees and admins can list all users and filter them by teachers or students, with pagination.

## Running the tests

The integration tests use `WebApplicationFactory` with an in-memory SQLite database, so they need neither Docker nor SQL Server. From the repository root:

```powershell
dotnet test SchoolApp.Tests
```

## Security notes

- Secrets (JWT key, database passwords, admin password) live only in `.env` or in user-secrets, never in the repository.
- Passwords are hashed with BCrypt. Login takes the same time for existing and non-existing users, to avoid revealing which usernames exist.
- The application database user has read and write rights only. It cannot create or drop tables.
- Errors are returned as ProblemDetails with consistent status codes (400, 401, 403, 404, 409).

## Known limitations

- JWT tokens cannot be revoked. A token stays valid for 3 hours, and its claims are a snapshot taken at login.
- There is no rate limiting or account lockout on login.
- `GET /teachers/{id}` answers `404` for a missing id and `403` for an existing one that the caller cannot see, so an authenticated user can learn which ids exist.
- Two simultaneous registrations or enrollments of the same item may return `500` instead of `409`, because the unique index rejects the second one.
- The capabilities `VIEW_ONLY_TEACHER`, `VIEW_ONLY_STUDENT` and `INSERT_ADMIN` are seeded but not used by any endpoint yet.
- If the only admin is soft-deleted, the seeder will fail at startup on the unique username.
- The database must be created manually with the scripts `V0` to `V3` before the first start (see Getting started).