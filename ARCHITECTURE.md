# Architecture Overview

## Tech Stack
- **Backend:** .NET 8 ASP.NET Core Web API
- **Data Access:** Entity Framework Core + Npgsql (PostgreSQL)
- **Frontend:** Next.js (App Router, TypeScript, Tailwind CSS)
- **Orchestration:** Docker + docker-compose

## High-Level Modules
1. **Auth & Tenants:** Identity management and multi-tenant context (`tenant_id`).
2. **CRM:** Client, Contact, and Lead management.
3. **Offers / Deals:** Sales pipelines, multi-channel deal stages, and template-based quote generation.
4. **Projects & Tasks:** Post-approval workflow, task tracking, resource management, and time tracking.
5. **Contracts:** Template-based contract generation, document versioning, and internal electronic signatures.
6. **Invoicing:** Multi-currency, success-fee, and time-based invoicing.
7. **Ads Metrics:** Native API integrations for Google, Meta, and TikTok to fetch performance metrics and calculate ROI.
8. **Automation Engine:** Rule-based background jobs and webhooks for data ingestion.

## Data Flow
1. **Frontend:** React Server Components for data fetching; Client Components for interactivity.
2. **API Layer:** RESTful Controllers handle requests and DTO mapping.
3. **Application Layer:** Services contain business logic, validation, and orchestration.
4. **Infrastructure Layer:** Repositories handle data access and EF Core `DbContext` interactions.
5. **Persistence:** PostgreSQL for relational data, scoped by `TenantId`.

## Frontend Data Access
The Next.js application uses a standardized data access layer:
- **API Client**: A typed wrapper around `fetch` (`web/src/lib/api.ts`) that handles base URL, auto-auth, and silent token refresh.
- **TanStack Query**: Used for all server state management (`web/src/hooks/queries`):
  - **Caching**: Entities are cached per tenant for instant transitions.
  - **Mutations**: Data is automatically refetched (invalidated) after create/update mutations.
- **Typed Hooks**: Domain-specific hooks (e.g., `useClients`, `useLeads`) provide a clean, type-safe interface for components.

## Multi-Tenancy Implementation
- Multi-tenancy is enforced at the database level using a `TenantId` column on all business entities.
- EF Core Global Query Filters are applied in `AppDbContext` to automatically scope every query to the current user's `TenantId`.
- The `TenantId` is extracted from the JWT claims in the backend and propagates through the `CurrentUserContext`.

## Background Jobs
The system uses **Hangfire** for durable background processing:
- **Persistence**: Job state is stored in PostgreSQL.
- **Server**: An `IHostedService` Hangfire server runs within the API process.
- **Jobs**:
  - `AdMetricsSyncJob`: Recurring sync of marketing metrics.
  - `RemindersJob`: Daily check for overdue items.
- **Dashboard**: Provides real-time monitoring and manual intervention at `/hangfire`.

## Observability
The system implements a multi-layered observability strategy:
- **Structured Logging (Serilog)**:
  - **Context Enrichment**: All logs are enriched with `TenantId`, `UserId`, and `RequestId`.
  - **Sinks**: Logs are written to the Console (stdout) and can be easily routed to cloud providers (e.g., CloudWatch, ELK).
  - **Conventions**: Use `Information` for lifecycle events, `Warning` for business rule violations, and `Error` for exceptions.
- **Metrics (.NET Metrics)**:
  - **Counters**: `crm_requests_total`, `crm_errors_total`.
  - **Histograms**: `crm_request_duration_seconds`.
- **Frontend Feedback**: Centralized error handling in `api.ts` coupled with `sonner` toast notifications for immediate user feedback.



