# CRM Capabilities Matrix vs. Terms of Reference (ToR)

This document tracks our current system capabilities against the ["CRM + invoicing study and terms of reference for digital agencies"](./phase2_upgrade_plan.md) text.

| Feature Area | Study / ToR Requirement | Current MVP Status | Phase 2 Upgrade Plan |
| :--- | :--- | :--- | :--- |
| **Contact & Leads** | Centralized file for each customer, interaction tracking, categorized/segmented records. | ✅ **Done**. `Clients`, `Contacts`, and `Leads` tables exist. Multi-tenant scoping works. | No major changes needed. Expand webhooks to ingest leads automatically. |
| **Multi-channel Pipeline** | Support multiple deal pipelines simultaneously (sales, retainers, etc.) | ⚠️ **Partial**. We have a basic Deals/Offers structure, but strictly a single pipeline. | Support custom quoting and multiple stages/pipelines in `OffersController`. |
| **Quoting (Offers)** | Auto-generate from templates, prepopulated with CRM data, notifications, status tracking. | ⚠️ **Partial**. Offers exist and require approval, but lack template generation and tracking. | Add `QuoteTemplateId`, build quote view UI, track approval state changes. |
| **Contracts** | Template-derived, version management, e-signatures mapped to deals. | ⚠️ **Partial**. Basic contract endpoints exist placeholder. | Add versioning and an internal electronic signature capability (timestamped approval). |
| **Projects & Tasks** | Auto-convert deals to projects. Gantt/Kanban. Resource allocation, file sharing. | ⚠️ **Partial**. Projects and tasks exist, kanban works. No resource allocation to teams yet. | Add `ProjectTeam` junction. Display user avatars on Kanban. Auto-convert via Hangfire. |
| **Time Tracking** | Track billable hours per client/project mapping to rates. | ❌ **Missing**. Not implemented. | Add `TimeEntry` system to tasks. Use aggregated hours for invoicing. |
| **Invoicing** | Issue based on milestones/hours, success-fee based on Ad metrics. | ⚠️ **Partial**. Basic invoicing exists without multi-currency or success fees. | Build multi-currency logic and `GenerateSuccessFeeInvoiceAsync` using ROA logic. |
| **Ad Integrations** | Native Webhooks and APIs for Meta, Google Ads, TikTok to fetch CPL/ROI. | ❌ **Missing**. Fully stubbed. | Implemented via Native API ingestion (Webhooks + OAuth logic) without CSV intermediaries. |
| **Automation** | Automatic routing of tasks, email follow-ups, and dashboard KPIs. | ⚠️ **Partial**. Basic robust backend exists (`Hangfire`) but limited pulse rules. | Expand rules engine. Auto-create projects upon quote approval. |

## Executive Summary
**Where we are**: The system is structurally sound for standard CRUD operations and handles multi-tenant DB schemas perfectly. The auth, routing, and Next.js frontend are functioning. 

**Where we are going (Phase 2)**: We are transforming a standard CRM into an "Agency Intelligence" platform. By strictly pursuing Native APIs for Ad Platforms and creating deep linkage between Ad performance (ROI) and **Invoicing (Success-Fee)**, the CRM aligns with enterprise platforms like HubSpot and HighLevel, perfectly optimized for Digital Agencies.
