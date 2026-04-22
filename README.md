# Police Smart Hub

## Deploy to Render

This repo contains a static frontend and an ASP.NET Core backend in `webserver/`. The backend also serves the frontend files, so the simplest Render deployment is one Web Service for the backend plus one Render PostgreSQL database.

### Recommended: Render Blueprint

1. Push the repo to GitHub.
2. In Render, create a new Blueprint from `render.yaml`.
3. Render will create:
   - `police-webserver` Docker Web Service
   - `police-postgres` PostgreSQL database
4. Fill the secret env vars when Render asks for them.
5. After deploy, test:
   - `https://<your-service>.onrender.com/health`
   - `https://<your-service>.onrender.com/api/auth/me`

The service runs EF Core migrations on startup. Check Render logs for `Database startup completed successfully` or the detailed database error message.

### Manual Render Web Service

If you do not use the blueprint:

- Service type: Web Service
- Environment/runtime: Docker
- Root Directory: repo root
- Dockerfile Path: `./Dockerfile`
- Health Check Path: `/health`

For local non-Docker testing:

```powershell
cd webserver
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls "http://0.0.0.0:5055"
```

For a native .NET host, the equivalent commands are:

- Build Command: `dotnet publish webserver/PoliceWebServer.csproj -c Release -o out`
- Start Command: `dotnet out/PoliceWebServer.dll`
- Root Directory: repo root

### Required Render env vars

Set these on the backend Web Service:

- `ASPNETCORE_ENVIRONMENT=Production`
- `POLICE_DATABASE_PROVIDER=postgres`
- `DATABASE_URL=<Render Postgres connection string>`
- `ADMIN_USERNAME=admin`
- `ADMIN_PASSWORD=<strong password>`
- `ADMIN_EMAIL=<admin email>`
- `ADMIN_DISPLAY_NAME=<display name>`

Optional:

- `PORT` is provided by Render automatically. The app binds to `http://0.0.0.0:$PORT` when present.
- `FRONTEND_URL=https://your-frontend.example.com` only if a separate frontend origin calls this backend with cookies.
- `POSTGRESQL_ADDON_URI` or `POLICE_POSTGRES_CONNECTION` can be used instead of `DATABASE_URL`.

Connection string priority for PostgreSQL:

1. `DATABASE_URL`
2. `POSTGRESQL_ADDON_URI`
3. `POLICE_POSTGRES_CONNECTION`
4. `ConnectionStrings:Postgres` in appsettings

`postgres://...` and `postgresql://...` URLs are converted to Npgsql connection strings automatically with SSL required.

### Migrations

The backend runs migrations automatically at startup for relational databases. If you want to run them manually on another host:

```powershell
cd webserver
dotnet ef database update
```

If `dotnet ef` is not installed, install the EF tool locally or use the automatic startup migration.

### Frontend API URL

The public frontend is configured to call the Render backend through `app-config.js`:

```text
https://police-otit.onrender.com
```

If you host the frontend separately, set `FRONTEND_URL` on the backend to the frontend origin. Do not point API or SignalR calls at the frontend domain.
