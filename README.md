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

## Tests\nRun unit and SQL Server integration tests with `TEST_SQL_CONNECTION` set in the environment. The suite covers scope isolation, mandatory workflow notes, self-approval, priority routing, audit creation, SQL Server rowversion concurrency, and unique final-decision enforcement.\n\n## AI use and verification\nAI assisted scaffolding and review; generated changes were compiled, exercised against SQL Server, and tested. Remaining risk: full browser/Identity integration test coverage and public deployment are outside this local solution.
# Scenario coverage

| Requirement | Status | Evidence |
|---|---|---|
| Identity, roles and backend scopes | Implemented | `ScopeService`, Identity roles, tests |
| Hierarchy and live user assignment | Implemented | Admin Users page + validation |
| Request lifecycle, audit, messages | Implemented | Workflow service + details page |
| SQL concurrency/final decision | Implemented | SQL Server integration tests |
| Dashboard scope/reporting | Implemented locally | Dashboard page |
| Automated tests | Partial | 12 tests; extend Identity browser cases before external assessment |
| Public deployment | Deferred | Requires hosting runtime and managed SQL Server |

## Test commands

```powershell
$env:TEST_SQL_CONNECTION = $env:TEST_SQL_CONNECTION
dotnet test OperationsRequests.sln -c Release
```

