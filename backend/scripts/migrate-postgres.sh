#!/usr/bin/env bash
set -euo pipefail

# EF Core migration helper for PostgreSQL
# Usage:
#   export DATABASE_URL=postgres://user:pass@host:5432/db
#   ./scripts/migrate-postgres.sh "InitPgTan"

MIGRATION_NAME=${1:-InitPgTan}

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found; please install .NET 8 SDK" >&2
  exit 1
fi

if ! command -v dotnet-ef >/dev/null 2>&1; then
  dotnet tool install --global dotnet-ef || true
fi

echo "==> Restoring packages"
dotnet restore

echo "==> Generating migration: $MIGRATION_NAME"
dotnet ef migrations add "$MIGRATION_NAME" --context COMP9034.Backend.Data.ApplicationDbContext

echo "==> Applying database update"
dotnet ef database update --context COMP9034.Backend.Data.ApplicationDbContext

echo "✅ Migration completed"

