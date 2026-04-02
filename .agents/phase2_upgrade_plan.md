# Phase 2 Upgrade Plan: Agency CRM (From 5 to 9/10)

## Overview & Current State
The application is currently at a **5.5 / 10** state. 
- **The Core Foundation is Solid**: Multi-tenant architecture, robust authentication (JWT), role-based access, and basic CI/CD are built and functioning correctly.
- **The MVP Workflow works**: We have the fundamental "Lead → Offer → Project & Tasks" pipeline fully implemented.
- **What's Missing (The Gap to 9/10 based on ToR)**: The highly specialized agency features required for a complete solution. According to the terms of reference for digital agencies, we lack: Multi-channel pipelines, template-based quoting, document e-signatures, time tracking, success-fee invoicing based on ad metrics, deep integrations (APIs/Webhooks for external platforms), a process automation engine, and a customer portal.

**Primary Directive for Next Session**: To minimize risk, development must happen in *small, isolated chunks*. After each chunk completes, a strict sanity check and full integration test suite run MUST pass before proceeding to the next chunk.

---

## Terms of Reference (Goal Scope)
This upgrade aims to fulfill the following requirements specified by the client and the Agency CRM study:
1. **Advanced Leads & Multi-Channel Pipeline**: Support for multiple pipelines (sales, execution, retainer) and direct ingestion via webhooks.
2. **Quoting (Offers)**: Automated generation from templates, notifications on opening, and tracking.
3. **Projects, Tasks & Time Tracking**: Task management with assigned members, resource allocation, Gantt/Kanban, and built-in time tracking mapped to billable rates.
4. **Contracts**: Template-derived contracts, version tracking, and electronic signature readiness.
5. **Invoicing & Payments**: Deal-based, time-tracked, and "success-fee" invoicing. Multi-currency support.
6. **Ads & Analytics Integrations (CRITICAL)**: Importing metrics (Meta, Google, TikTok via REST API or CSV) to calculate Return on Investment (ROI) and invoice clients based on actual performance.
7. **Automation & Reporting**: Rule-based workflows (e.g., auto-creating projects from approved quotes), alerts for overdue statuses, and centralized metric dashboards linking costs to ROI.

---

## Step-by-Step Implementation Roadmap (The "Chunks")

When triggering this upgrade, execute the following chunks *one at a time*. 

### Chunk 1: Advanced Quoting, Multi-Channel Pipelines & Contracts
*Goal: Close the loop on sales with professional quoting and contract management.*
* **Backend**: Update `OfferService` to support template generation and status tracking. Expand `ContractService` for document versioning and an `IsWaitingSignature` flag. Support multiple pipelines in the deals module.
* **Frontend**: Add customizable quoting UI. Add contract stage indicators detailing their specific stage in the approval process.

### Chunk 2: The Ad Metrics Intelligence & Integrations
*Goal: Allow the CRM to ingest campaign performance (Meta, Google, TikTok).*
* **Backend**: 
  * Update `AdMetricService` to handle incoming performance reports via CSV or API. 
  * Expose Webhooks (e.g., `/api/webhooks/facebook-lead`, `/api/webhooks/ad-performance`) to allow real-time updates.
  * Build algorithms to calculate **CPL (Cost Per Lead)**, **Conversion Rates**, and total **ROI**.
* **Frontend**: Build a "Profitability/ROI Chart" widget for the Dashboard. Connect it to the new endpoint.

### Chunk 3: Projects, Task Resource Allocation & Time Tracking
*Goal: Full project visibility and tracking billable hours.*
* **Backend**: 
  * Introduce a `ProjectTeam` mapping entity to assign multiple `User` (Team Members) to specific `Project` records.
  * Add a `TimeEntry` entity for task-based time tracking.
* **Frontend**: Update `tasks/page.tsx` and the Kanban board to display assigned avatars. Add a "Log Time" UI module with Gantt visualization.

### Chunk 4: Success-Fee & Multi-currency Invoicing
*Goal: Modern agencies bill based on success and time.*
* **Backend**: Update `InvoiceService` and domain entities.
  * Add multi-currency toggles with basic exchange rate handling.
  * Implement `GenerateSuccessFeeInvoiceAsync()`, which queries `AdMetrics` tables to calculate final invoice amount based on a percentage.
  * Implement invoicing based on accumulated tracked time.
* **Frontend**: Update the Invoice generation modal to allow selecting "Success Fee Billing" or "Hourly Billing".

### Chunk 5: Process Automation Engine
*Goal: Reduce manual work via automatic workflows.*
* **Backend**: Setup rule-based background jobs/hooks. For example, automatic conversion of Approved Offer -> Project & Tasks. Automatic notifications for overdue invoices or stalled leads.
* **Frontend**: Automation configuration UI (simple rules engine).

---

## Execution Protocol for the Agent

When instructed to begin this plan, follow this strict protocol:
1. **Verify Baseline**: Ensure the system builds (`npm run build`, `dotnet build`) and all tests currently pass.
2. **Execute Chunk**: Implement one chunk entirely (Backend + Frontend).
3. **Strict Validation**: Run backend integration tests (`dotnet test`). Run frontend linting and component tests. If anything fails, **revert or fix immediately** before moving on.
4. **Review Checkpoint**: Summarize the chunk's completion for the human developer, verify the database schema migrations were applied cleanly, and wait for human go-ahead before starting the next chunk.
