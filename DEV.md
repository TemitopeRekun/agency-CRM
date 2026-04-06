# Developer Guide: Agency CRM Local Workflow (Docker-Free)

This guide describes how to run the Agency CRM locally without requiring Docker Desktop or a local PostgreSQL instance.

## 1. Prerequisites
- **.NET 8 SDK**
- **Node.js 18+ & npm**
- **SQLite** (Optional, for browsing the local DB file)

## 2. Backend Setup (API)

The backend is configured to automatically fall back to **SQLite** if no `DATABASE_URL` (Postgres) is provided.

### Configuration
1. Navigate to `backend/Crm.Api`.
2. Review `appsettings.Development.json`. The system uses `crm_local.db` by default.

### Run the API
```bash
cd backend/Crm.Api
dotnet run
```
- **API URL**: `http://localhost:5177`
- **Hangfire Dashboard**: `http://localhost:5177/hangfire`
- **Swagger/OpenAPI**: `http://localhost:5177/swagger`

### Database Management
- The database is automatically created and seeded on the first run.
- To reset the database, simply delete `Crm.Api/crm_local.db` and restart the app.

---

## 3. Frontend Setup (Web)

The frontend is a Next.js application that communicates with the backend API.

### Configuration
1. Navigate to `web/`.
2. Copy `.env.local.example` to `.env.local`:
   ```bash
   cp .env.local.example .env.local
   ```
3. Ensure `NEXT_PUBLIC_API_BASE_URL` is set to `http://localhost:5177`.

### Run the Web App
```bash
cd web
npm install
npm run dev
```
- **Web URL**: `http://localhost:3000`

---

## 4. Testing

### Backend Integration Tests (SQLite)
Integration tests now run in-memory for maximum speed and isolation. No Docker is required.
```bash
dotnet test backend/Crm.IntegrationTests/Crm.IntegrationTests.csproj
```

### Frontend Tests
```bash
cd web
npm test
```

---

## 5. Troubleshooting

### File Locking Issues
If you encounter "File is locked by another process" errors during `dotnet test`, run the following in PowerShell:
```powershell
Get-Process -Name "testhost" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### Next.js Proxy Errors
If you see `ERR_CONNECTION_REFUSED` in the browser console, ensure the backend API is actually running at the port specified in `.env.local`.
