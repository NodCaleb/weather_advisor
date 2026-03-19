# Quickstart: Weather Advisor — Aspire Orchestration

**Branch**: `002-aspire-orchestration-discovery` | **Date**: 2026-03-19

This guide replaces the manual multi-terminal startup described in `001-weather-advisor-app/quickstart.md`. With this feature, all components start from a single command using .NET Aspire orchestration.

> **Manual startup still works.** If you only need to run one component (e.g., the API with no frontend for API-only testing), follow the instructions in `001-weather-advisor-app/quickstart.md`. Aspire orchestration is additive — it does not replace or break the existing standalone workflows (FR-007).

---

## Prerequisites

| Tool | Minimum Version | Install / check |
|------|----------------|-----------------|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 20 LTS | `node --version` |
| npm | 10+ | `npm --version` |
| Aspire CLI | 13.x | `aspire --version` |

> **Docker is not required.** This topology only orchestrates .NET projects and npm apps — no containers.

### Install Aspire CLI (one-time, per machine)

```powershell
# PowerShell (Windows)
irm https://aspire.dev/install.ps1 | iex
```

```bash
# bash (macOS/Linux)
curl -sSL https://aspire.dev/install.sh | bash
```

Verify:
```bash
aspire --version
# → 13.x.x+{commit-sha}
```

---

## 1. Clone and Navigate

```bash
git clone <repo-url>
cd weather_advisor
```

---

## 2. Configure User Secrets (one-time, per machine)

The WeatherAdvisor API requires runtime secrets that must **not** be committed to source control. These are supplied via .NET User Secrets, which Aspire forwards automatically when starting the API in Development mode.

```bash
cd backend/WeatherAdvisor.Api

# If UserSecretsId is not yet in the .csproj:
dotnet user-secrets init

# Set required secrets (replace placeholders with real values):
dotnet user-secrets set "OpenMeteo:ApiKey" "<your-api-key-if-required>"
```

> **If Open-Meteo currently requires no API key** (free tier): the `OpenMeteo:ApiKey` secret may be omitted. Check `appsettings.json` for required configuration keys.

Copy the development settings example (first-time only):

```bash
cp backend/WeatherAdvisor.Api/appsettings.Development.example.json \
   backend/WeatherAdvisor.Api/appsettings.Development.json
```

> `appsettings.Development.json` is git-ignored. The example file is safe to commit and contains no secrets.

Return to the repository root when done:

```bash
cd ../..
```

---

## 3. Start the Full Stack (single command)

```bash
aspire run --project backend/WeatherAdvisor.AppHost
```

Or using `dotnet run` from the AppHost directory:

```bash
cd backend/WeatherAdvisor.AppHost
dotnet run
```

**What happens**:
1. Aspire assigns dynamic ports to all components.
2. The API (`WeatherAdvisor.Api`) starts with `ASPNETCORE_ENVIRONMENT=Development`, loading user secrets automatically.
3. The Vite dev server starts with `VITE_API_BASE_URL` set to the dynamically assigned API URL — no hardcoded ports.
4. The Aspire dashboard URL is printed to the terminal (click or paste into browser to open).

**Expected terminal output** (ports will differ):
```
Dashboard:  https://localhost:17068/login?t=ea559845...
AppHost:    Started
weatheradvisor-api:  http://localhost:34521
frontend:            http://localhost:43017
```

Open `http://localhost:43017` in your browser to use the application.

---

## 4. Verify the Stack

### Via the Aspire Dashboard

Open the dashboard URL printed in the terminal. Confirm:
- `weatheradvisor-api` shows state **Running**
- `frontend` shows state **Running**

### Via curl / PowerShell

```powershell
# Replace 34521 with the actual API port shown in terminal/dashboard
$weather = Invoke-RestMethod -Uri 'http://localhost:34521/weather?city=London'
Write-Output "City: $($weather.city) | Condition: $($weather.conditionLabel)"
```

---

## 5. Stop the Stack

Press `Ctrl+C` in the terminal where Aspire is running. All child processes (API + Vite dev server) terminate automatically.

---

## 6. Run Tests (unchanged)

Tests are not affected by Aspire orchestration. Run them independently as before:

### Backend tests

```bash
cd backend
dotnet test
```

### Frontend tests

```bash
cd frontend
npm run test
```

---

## Troubleshooting

### "aspire: command not found"

Install the Aspire CLI (see Prerequisites above). Ensure `~/.aspire/bin` is on your `PATH`. Close and reopen your terminal after installation.

### API starts but returns errors about missing configuration

Confirm user secrets are set:
```bash
cd backend/WeatherAdvisor.Api
dotnet user-secrets list
```
Set any missing values with `dotnet user-secrets set "<key>" "<value>"`.

### Vite dev server fails to start

Ensure Node.js and npm are installed (`node --version`, `npm --version`). Aspire runs `npm install` automatically before starting the dev server — if this fails, run `npm install` manually in `frontend/` and inspect the error output.

### CORS errors in browser console

This should not occur under Aspire orchestration (the API's Development CORS policy uses `AllowAnyOrigin()`). If seen, verify that `ASPNETCORE_ENVIRONMENT=Development` is set for the API process (it is set automatically by Aspire, but confirm in the Aspire dashboard environment tab for the `weatheradvisor-api` resource).

### Component shows "Failed" in dashboard

Click the component name in the dashboard to view its log output and identify the startup failure reason.

---

## Appendix: Standalone Startup (without Aspire)

If you need to run components individually (e.g., for isolated debugging), use the original startup procedure from `001-weather-advisor-app/quickstart.md`:

```bash
# Terminal 1 — Backend
cd backend/WeatherAdvisor.Api
dotnet run --no-launch-profile --urls http://localhost:5000

# Terminal 2 — Frontend
cd frontend
# Ensure VITE_API_BASE_URL is set in .env.local
echo "VITE_API_BASE_URL=http://localhost:5000" > .env.local
npm run dev
```
