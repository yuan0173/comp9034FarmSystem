# Repository Guidelines

## Project Structure & Modules
- `frontendWebsite/`: Vite + React + TypeScript SPA.
  - `src/pages`, `src/components`, `src/api`, `src/hooks`, `src/utils`, `public/`.
- `backend/`: ASP.NET Core Web API.
  - `src/Controllers`, `src/Services`, `src/Models`, `src/DTOs`, `src/Middlewares`, `src/Repositories`, `src/Data`, `src/Common/`.
  - `Database/` (SQL + migration scripts), `tests/` (utility scripts, manual checks).
- `docs/`: Project analysis and backend integration notes.
- CI & scripts: `azure-pipelines.yml`, `test-login-*.sh` for targeted manual checks.

## Build, Test, and Development Commands
- Frontend: `cd frontendWebsite && npm install && npm run dev` (dev server).
  - Prod build: `npm run build && npm run preview`.
  - Env: `cp env.example .env` then set `VITE_API_BASE_URL`.
- Backend: `cd backend && dotnet restore && dotnet run` (starts API on configured port).
  - Optional: `backend/tests/start-backend.sh` to start with defaults.
- Database: apply SQL in `backend/Database/` (or `backend/Database/execute_migration.sh`).

## Coding Style & Naming Conventions
- TypeScript/React: Prettier + ESLint enforced.
  - 2‑space indent, single quotes, no semicolons, width 80 (`.prettierrc`).
  - ESLint with TypeScript + React Hooks rules (`.eslintrc.cjs`).
  - Components: PascalCase in `src/components`; pages in `src/pages`; hooks `use*` in `src/hooks`.
- C#: PascalCase for types/methods, camelCase for locals/params.
  - Keep layers separated under `src/Controllers|Services|Repositories|Models|DTOs|Middlewares|Data`.

## Testing Guidelines
- Current: No formal unit tests checked in; shell scripts `test-login-*.sh` cover key flows.
- Frontend: prefer Vitest + React Testing Library under `frontendWebsite/src/__tests__/`.
- Backend: xUnit under `backend/tests/`; focus on Services and Controllers.

## Commit & Pull Request Guidelines
- Commits: follow Conventional Commits where possible (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`). Optional scope, e.g., `feat(frontend): ...`.
- PRs must include: problem/solution summary, linked issues, screenshots/GIFs for UI changes, steps to validate (incl. API base URL), and affected modules (`frontendWebsite/`, `backend/`).

## Security & Configuration Tips
- Never commit secrets. Use `frontendWebsite/.env` and `backend/appsettings.*.json`.
- Keep CORS origins and JWT settings aligned between frontend and backend.
- CI: set required variables in Azure Pipelines before enabling deploys.
