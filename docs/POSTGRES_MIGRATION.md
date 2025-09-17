PostgreSQL Migration Guide (Unified Tan Schema)

Prerequisites
- .NET 8 SDK
- PostgreSQL (local Docker or cloud)

Environment
- Set `DATABASE_URL` to a Postgres URL: `postgres://user:pass@host:5432/db`.
- Production (Render): also set `ASPNETCORE_ENVIRONMENT=Production`.

Steps
1) Install EF tool (once): `dotnet tool install --global dotnet-ef`
2) Backend restore: `cd backend && dotnet restore`
3) Create migration: `dotnet ef migrations add InitPgTan`
4) Apply migration: `dotnet ef database update`
5) Run backend: `ASPNETCORE_URLS=http://localhost:4000 dotnet run`

Notes
- Default timestamps use `CURRENT_TIMESTAMP AT TIME ZONE 'UTC'`.
- Partial index for `Username` uses Postgres syntax: `Username IS NOT NULL`.
- Events includes MANUAL_OVERRIDE and optional AdminId (FK to Staff).
- BiometricData security fields: `Salt`, `TemplateHash` (indexed), `DeviceEnrollment`.
- BiometricVerification includes optional `BiometricId` FK and normalized result values.

Data migration (SQLite → Postgres)
- Export CSV from SQLite; map fields (Notes→reason, OccurredAt→timeStamp handled at API/DTO level).
- Import with `\copy` in psql; respect FK order: Staff → Device → Events → BiometricData → WorkSchedule → Salary → BiometricVerification.

