# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution layout

The solution file is `MakanPergi/MakanPergi.sln`. It contains 4 projects:

- **`MakanPergi/`** — ASP.NET Core MVC front end (Razor views + controllers). Runs on `http://localhost:5214` (see `Properties/launchSettings.json`). It has **no direct database access** — all data operations go through HTTP calls to `MakanPergiWebApi` via Refit typed clients.
- **`MakanPergiWebApi/`** — ASP.NET Core Web API backend. Runs on `http://localhost:5179`, Swagger UI at `/swagger`. Owns the EF Core `DbContext`, repository/unit-of-work layer, AutoMapper profiles, and Hangfire background jobs.
- **`MakanPergi.Data/`** — EF Core `DbContext` (`MakanPergiDbContext`) and migrations only.
- **`MakanPergi.Entities/`** — POCO entity classes (`Food`, `Cuisine`, `Category`, `Shop`, join entities `FoodCategory`/`FoodFromShop`, and DB view mapping `VW_FoodFullView`).
- **`Common/`** (separate `Common.csproj`, referenced by both `MakanPergi` and `MakanPergiWebApi`) — shared DTOs (DataTables request/response models, Excel upload models, `WebSocketMsg`), the generic Hangfire/MediatR backend-job plumbing (`BackendJobs/`), TagHelpers, and misc helpers (`MyHttpUtility`, `StringExtensions`).

**Naming trap:** `MakanPergi/Common/` (namespace `MakanPergi.Common.*`) is a plain folder *inside* the MVC project — unrelated to the top-level `Common/` project above, despite the identical name. It holds MVC-only concerns: `AppConfig/ApiSetting.cs` (the `ApiSetting`/`UrlItems` config model used by `DependencyRegistry`), `Constants/Constants.cs`, `Helpers/GlobalProfile.cs` (the MVC app's own AutoMapper profile, distinct from the WebApi's `GlobalProfile`), `ExcelGeneralMethods.cs`, and `ApiResponseActionResult.cs`. When a path or namespace says `MakanPergi.Common`, it's this folder, not `Common.csproj`.

There is no test project in the solution.

## Commands

Note the nesting: the repo root (this file's directory) contains `MakanPergi/`, `MakanPergiWebApi/`, `MakanPergi.Data/`, `MakanPergi.Entities/`, and `Common/` as siblings, but the `.sln` itself lives one level down at `MakanPergi/MakanPergi.sln`. Paths below are relative to the repo root.

```powershell
# restore + build the whole solution
dotnet build MakanPergi/MakanPergi.sln

# run the MVC front end (must also run the Web API for anything data-related to work)
dotnet run --project MakanPergi/MakanPergi.csproj

# run the Web API
dotnet run --project MakanPergiWebApi/MakanPergiWebApi.csproj

# EF Core migrations (-p points at the project owning the DbContext, -s at the startup project
# that has the connection string / DI configuration used to generate the migration)
dotnet ef migrations add <Name> -p MakanPergi.Data/MakanPergi.Data.csproj -s MakanPergiWebApi/MakanPergiWebApi.csproj
dotnet ef database update -p MakanPergi.Data/MakanPergi.Data.csproj -s MakanPergiWebApi/MakanPergiWebApi.csproj
```

Both apps must be running simultaneously for the MVC app's CRUD pages to work, since it proxies everything to the Web API over HTTP (see below). The Web API's SQL Server connection string is in `MakanPergiWebApi/appsettings.json` / `appsettings.Development.json` (`ConnectionStrings:DefaultConnection`, targets a local SQL Express instance).

**Config gotcha:** the base `appsettings.json` in each project has placeholder/stale values for `ApiSetting` (e.g. MVC's base file points `MakanPergiWebApi`'s URL at `http://localhost:8001`, which nothing listens on). The `Development` variant is what's actually used when running via `dotnet run`/`launchSettings.json` (`ASPNETCORE_ENVIRONMENT=Development`), and its ports correctly match `launchSettings.json` (`https://localhost:7127` for the Web API, `https://localhost:7214` for the MVC app's Portal webhook target). When changing cross-service URLs, edit the `.Development.json` files, not the base ones.

## Architecture

### Two-tier HTTP split (MVC ➜ WebApi), not MVC ➜ DB directly

`MakanPergi` (MVC) never touches EF Core. Instead:

1. `MakanPergi/Interfaces/*.cs` define Refit interfaces (`ICuisineService`, `ICategoryService`, `IFoodService`, `IShopService`) whose methods map to `MakanPergiWebApi` endpoints (e.g. `[Post("/api/cuisine/search")]`).
2. `MakanPergi/DependencyRegistry.cs` (`AddDependencies`) registers each Refit interface with `AddRefitClient<T>()`, pointed at the base URL configured in `appsettings.json` under `ApiSetting:UrlItems` (matched by `Name == Constants.MakanPergiWebApi`, a constant defined in `MakanPergi/Common/Constants/Constants.cs` — the MVC-internal `Common` folder, see naming trap above).
3. MVC controllers (`MakanPergi/Controllers/*`) inject these Refit interfaces directly and call them like local services. `MakanPergi/Services/CuisineService.cs` is a stub/vestigial class — don't use it as a reference pattern; the Refit interfaces injected directly into controllers are the real pattern.

On the `MakanPergiWebApi` side, the same interface names exist again but as real implementations: `Interfaces/Services/*` + `Services/*.cs`, wired up to `IUnitOfWork`/`IRepository<T>` in `Repository/`. When adding a new entity/feature end-to-end, you generally touch matching files in both `MakanPergi/Interfaces` (Refit contract) and `MakanPergiWebApi/Interfaces/Services` + `MakanPergiWebApi/Services` (implementation), plus the controller on each side.

### Repository / Unit of Work (WebApi only)

- `IUnitOfWork.Repository<T>()` (`MakanPergiWebApi/Repository/UnitOfWork.cs`) lazily creates and caches one `Repository<T>` per entity type per unit-of-work instance.
- `Repository<T>` (`MakanPergiWebApi/Repository/Repository.cs`) wraps `DbSet<T>`, exposing `Table` (tracked), `TableNoTracking` (`AsNoTracking`), and standard CRUD methods. Entities must derive from `AbstractBaseEntity` (`MakanPergi.Entities/Base`).
- Services inherit from `BaseService` and call `WorkContext.Repository<T>()` — see `CuisineService` in `MakanPergiWebApi/Services/CuisineService.cs` as the canonical example (search/list/get-by-id/create/update/delete/excel-upload methods).
- `UnitOfWork.CompleteAsync()` wraps `SaveChangesAsync` and rethrows `DbUpdateException`s with friendlier messages (e.g. detects SQL Server duplicate-key errors 2601/2607).

### AutoMapper

Each app registers its **own separate** `GlobalProfile` class — `MakanPergiWebApi/Helpers/GlobalProfile.cs` (entity ⇄ DataModel/ViewModel mappings) and `MakanPergi/Common/Helpers/GlobalProfile.cs` (Dto ⇄ ViewModel mappings for the MVC layer). They are not the same file and don't share mappings. `MakanPergi`'s `Program.cs` additionally builds a temporary service provider at startup and calls `mapper.ConfigurationProvider.AssertConfigurationIsValid()` — a broken mapping profile will crash the MVC app immediately at boot with a printed error, not just at request time. `MakanPergiWebApi` has no such startup check.

### Real-time progress via a webhook + SignalR relay

Long-running work in `MakanPergiWebApi` (e.g. Excel bulk upload in `CuisineService.CuisineExcelUploadProcessAsync`) reports progress by POSTing a `WebSocketMsg` to the MVC app's `PortalNotifyApiController` (`api/PortalNotifyApi/sendMessage`), which then re-broadcasts it to connected browser clients over a SignalR hub (`MainHub`, mapped at `/MainHub`). This indirection exists because the Web API itself has no SignalR hub/browser connections — only the MVC app does. `IPortalService`/`PortalService` (`MakanPergiWebApi/Controllers/PortalService.cs`) is the client side of this webhook; the target URL comes from `ApiSetting:PortalUrl` in `MakanPergiWebApi/appsettings.json`.

Per-row excel upload processing creates a new DI scope (`_serviceProvider.CreateScope()`) to get a fresh `IUnitOfWork`/repository per row rather than reusing the request-scoped one — follow this pattern for any similar batch/loop processing that needs isolated DbContext state per iteration.

### Background jobs (Hangfire + MediatR), WebApi only

- `Common/BackendJobs/` is a generic Hangfire wrapper: `HangfireJob` (implements `IBackendJob`) reads `BackendJobOptions.JobItems` (name + cron) from config, resolves each job name to a MediatR `IRequest` via an injected `Func<string, IRequest>` accessor, and calls `_recurringJobManager.AddOrUpdate` for each.
- To add a new recurring job: create a MediatR `IRequest` + `IRequestHandler<TRequest>` (see `MakanPergiWebApi/Infrastructure/Jobs/DemoBatchJob.cs`), add an entry to the `hangFireJobsCollection` dictionary in `MakanPergiWebApi/Program.cs` (maps job name string ➜ request type), and add a matching `BackendJobOptions:JobItems` entry (`Name` + `Cron`) in `appsettings.json`.
- `Program.cs` forces the `HangfireJob` singleton to construct eagerly right after `app.Build()` (`app.Services.GetRequiredService<IBackendJob>()`) so recurring jobs get registered even though nothing else resolves it directly.
- Hangfire uses in-memory storage (`AddHangfire(c => c.UseInMemoryStorage())`) — jobs are lost on restart, dashboard is at `/hangfire` (read-only, local-requests-only).

### Response wrapping

`MakanPergiWebApi` uses `AutoWrapper` (`app.UseApiResponseAndExceptionWrapper`) to wrap all controller responses/exceptions into a consistent envelope. Controllers return `ApiResponse` directly (not raw DTOs) and throw `ApiException(message, statusCode)` for error cases — AutoWrapper unwraps these instead of double-wrapping. Note the versioning caveat in `Program.cs`: this only works because of the AutoWrapper version pinned in the `.csproj`; upgrading AutoWrapper may require removing the manual `ApiResponse` returns.

### Logging

Both apps configure Serilog to file only (`Log.Logger` in `Program.cs`), writing to `Logs/log-.txt` with daily rolling and a 14-day retention, independently of each other (each app has its own `Logs/` folder).

### DbContext model notes

`MakanPergiDbContext` (`MakanPergi.Data/MakanPergiDbContext.cs`) defines many-to-many relationships via explicit join entities (`FoodCategory`, `FoodFromShop`) rather than implicit EF Core skip-navigation tables, and maps a keyless entity (`VW_FoodFullView`) to a SQL view (`FoodFullView`) via `HasNoKey().ToView(...)`.
