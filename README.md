# Agency CRM

A multi-tenant CRM and Invoicing web application for digital agencies.

## Tech Stack
- **Backend:** .NET 8 (ASP.NET Core Web API)
- **Database:** PostgreSQL with EF Core
- **Frontend:** Next.js (App Router, TypeScript)
- **Orchestration:** Docker + docker-compose

## Local Development Setup

### Option 1: Full Stack via Docker (Preview Mode)
Best for a complete, production-like preview.
```bash
docker compose up --build
```
- **Web (Frontend)**: [http://localhost:3000](http://localhost:3000)
- **API Health**: [http://localhost:8000/health](http://localhost:8000/health)
- **Hangfire Dashboard**: [http://localhost:8000/hangfire](http://localhost:8000/hangfire)

### Option 2: Fast Dev Mode (No Docker)
Best for rapid iteration. You must have .NET 8 and Node 20+ installed.

1. **Start Database only**:
   ```bash
   docker-compose up db -d
   ```
2. **Start Backend**:
   ```bash
   cd backend/Crm.Api
   dotnet run
   ```
   *Binds to: http://localhost:5177*
3. **Start Frontend**:
   ```bash
   cd web
   npm run dev
   ```
   *Binds to: http://localhost:3000*

**Note**: When switching to Fast Dev Mode, ensure `web/.env.local` uses `NEXT_PUBLIC_API_BASE_URL=http://localhost:5177`.

### Database Migrations
When Domain entities are updated (e.g. Phase 1 Hardening), apply migrations manually:

1. Bring up your local database:
```bash
docker-compose up db -d
```
2. Create the Migration (from the root folder):
```bash
cd backend/Crm.Api
dotnet ef migrations add <MigrationName> --project ../Crm.Infrastructure
```
3. Update the Database:
```bash
dotnet ef database update --project ../Crm.Infrastructure
```

### Authentication Flow (Production-Grade)
The system uses a highly secure authentication pattern:
- **Access Tokens**: Short-lived (15 min) JWTs stored in `httpOnly`, `Secure`, `SameSite=Strict` cookies.
- **Refresh Tokens**: Long-lived (7 days) tokens stored in the database with automatic rotation and revocation on logout.
- **Frontend Interaction**: JavaScript cannot access the tokens. The browser sends them automatically. The API client (`api.ts`) automatically handles silent token refreshes on `401 Unauthorized` responses.

---

## Multi-Tenant Verification & Seeding


The database is automatically seeded with two tenants for isolation testing:

### Tenant A: Tech Corp
- **Admin**: `admin@tenanta.com` / `Admin123!`
- **Data**: "Tech Solutions Ltd" (Client), "Website Redesign" (Lead).

### Tenant B: Creative Agency
- **Admin**: `admin@tenantb.com` / `Admin123!`
- **Data**: "Creative Studio" (Client), "Mobile App Design" (Lead).

### Isolation Verification Guide
1. Log in as `admin@tenanta.com`.
2. Navigate to `/clients` and `/leads`. Confirm only Tenant A data is visible.
3. Log out and log in as `admin@tenantb.com`.
4. Confirm only Tenant B data is visible.
5. Create a new client as Tenant B and verify it does NOT appear for Tenant A.

## Background Jobs ⚙️
The system uses **Hangfire** for recurring background tasks.
- **Dashboard**: Accessible at `http://localhost:8000/hangfire` in development.
- **Monitoring**: You can manually trigger, retry, or delete jobs from the dashboard.
- **Configuration**: Job intervals (CRON expressions) are managed in `appsettings.json` under `BackgroundJobs`.
- **Development**: Jobs run automatically if the API is active. To disable, set the interval to a value that never triggers or comment out the registration in `Program.cs`.


## Configuration & Secrets 🔐

The project uses a hierarchy of configuration to ensure security and flexibility:

### 1. Local Development (`.env`)
Copy the provided templates to create your local environment files:
```bash
cp .env.example .env
cp web/.env.example web/.env
```
The root `.env` file is automatically read by `docker-compose`.

### 2. Environment Variables
All sensitive values can be overridden via environment variables. Key variables include:
- `DB_PASSWORD`: PostgreSQL password.
- `JWT_KEY`: Secret key for signing JWT tokens (min 32 characters).
- `NEXT_PUBLIC_API_BASE_URL`: The full URL to the backend API.

### 3. AppSettings
Non-sensitive defaults are stored in `appsettings.json`. In production, these should be supplemented with `appsettings.Production.json` or Environment Variables following the `Section__Key` pattern (e.g., `ConnectionStrings__Default`).

## Database Configuration 🛢️

The application uses an environment-driven database configuration to support both local development and cloud platforms like Railway.

### 1. Connection String Priority
The backend resolves the PostgreSQL connection string in this order:
1. `DATABASE_URL` environment variable (Supports URIs like `postgres://user:password@host:port/db`).
2. `ConnectionStrings:Default` as defined in `appsettings.Development.json` (Local fallback).
3. `ConnectionStrings:Default` as defined in `appsettings.json`.

### 2. Using Railway Postgres
To use a Railway Postgres instance (locally or in production):
- Set the `DATABASE_URL` environment variable to the **External Connection String** provided by Railway.
- The application automatically parses this URI into the format required by EF Core.

## Data Migration to Railway 🚀

If you have existing data in your local Docker Postgres and want to move it to Railway:

1. **Dump local data**:
   ```bash
   pg_dump -h localhost -p 5433 -U postgres agency_crm > backup.sql
   ```
2. **Restore to Railway**:
   Use the Railway CLI or pure `psql` (replace `<RAILWAY_URL>` with your external connection string):
   ```bash
   psql "<RAILWAY_URL>" < backup.sql
   ```

> [!IMPORTANT]
> Never commit a `.env` file containing real secrets to version control. Always use the `.env.example` patterns for documentation.

## Testing 🧪

For the current demo phase, we focus on **Unit Tests** for maximum stability.

- **Run Unit Tests**:
  ```bash
  dotnet test backend/Crm.UnitTests
  ```
- **Integration Tests**: These remain in the codebase (`Crm.IntegrationTests`) but are currently excluded from the default CI run to ensure faster, reliable builds.

## CI/CD 🚀

The project uses **GitHub Actions** for continuous integration.
- **Workflow**: `.github/workflows/ci.yml`
- **Actions**:
  - Builds Backend (.NET 8) and Frontend (Next.js).
  - Runs Backend **Unit Tests** (Integration tests are skipped for demo mode).
  - Lints and builds the Frontend.
  - Runs Frontend unit tests using Vitest.
- **Triggers**: Runs on every push to `main` and all Pull Requests.


<!-- Trigger Railway Redeploy: 2026-03-26 T15:40 -->
