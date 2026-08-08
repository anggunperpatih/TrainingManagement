# Training Management

ASP.NET Core 10 Razor Pages request-management application using SQL Server and ASP.NET Core Identity.

## Security and workflow
- Identity cookie authentication; roles and organisation assignments are persisted in SQL Server.
- Backend scope checks use the authenticated user only; client role/user IDs are never trusted.
- `rowversion` optimistic concurrency plus a filtered unique final-decision index prevent duplicate final outcomes.
- Inactive users are rejected at cookie validation. Platform Admin can create and activate/deactivate users.
- Required notes: decline, information request, requester response, resolution. Self-approval is rejected.

## Run
Set development secret (never commit it): `dotnet user-secrets set Demo:Password "<strong password>" --project src/OperationsRequests`.
Run `dotnet ef database update --project src/OperationsRequests` then `dotnet run --project src/OperationsRequests`.

## Operations
Use environment variables/secrets for connection strings. Apply migrations during deployment. Application logs go to standard ASP.NET Core logging sinks. Roll back application by redeploying last image/revision; restore SQL Server from a pre-migration backup before rolling back destructive schema changes. Tear down by deleting hosting resource and database after the review period.

## Deployment
GitHub is source control only; deploy the application container/App Service to a host with SQL Server (for example Azure App Service + Azure SQL) using repository secrets for the connection string. Do not commit passwords, tokens, or production connection strings.

## Known deferred work
Automated integration tests and a hosting provider configuration still require completion before an assessment submission. A public deployment also requires a hosting account/resource and GitHub authentication via PAT/SSH/CLI, not an account password.
