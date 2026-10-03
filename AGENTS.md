# AGENTS.md

## Overview
Carnitas ("TACO") is a .NET 10 solution for automating Terraform across repositories/organisations. Ships a Blazor Server web app and a native-AOT CLI worker, which talk to each other over a shared gRPC contract. An Aspire AppHost orchestrates Web + CLI locally. The web UI covers organisations, repositories, modules and queued tasks/operations; the core workflow engine (init/plan/apply/source-discovery stages) lives in `Carnitas`.

## Projects (Carnitas.sln)
- `Carnitas` — core domain/workflow library. Workflow orchestration and Terraform stages (`Workflow/`), job dispatch (`Job/`), repo checkout via LibGit2Sharp (`Source/`), OpenTelemetry tracing/baggage (`Observability/`), and options (`Options/`). References the vendored `Sarsoo.Terraform` project and its nested `TerraformDotnet` / `TerraformDotnet.Hcl` projects.
- `Carnitas.Model` — EF Core + ASP.NET Identity + Npgsql (PostgreSQL). `ApplicationDbContext`, `ApplicationUser`, governance (`Organisation`, policy), source entities (`Module`, `Repository`, `Checkout`, `GitHubApp`), the task queue (`QueuedTask`, `TaskQueueService`), and EF migrations. References `Carnitas`.
- `Carnitas.CLI` — console worker, assembly name `cns`, native AOT. System.CommandLine commands: `start` (worker loop), `dep`, `rdep`, `plan`, `init`. Hosted workers poll the backend over gRPC. References `Carnitas.Grpc` + `Carnitas`.
- `Carnitas.Grpc` — shared protobuf contract (`proto/agent.proto`, namespace `Carnitas.Grpc`) plus Google.Protobuf / Grpc.Net.Client. Used by both client (CLI) and server (Web). References `Carnitas`.
- `Carnitas.Web` — Blazor Server (InteractiveServer) + MudBlazor + Identity, gRPC server (`Grpc/AgentService.cs`) with health checks + reflection, GitHub webhook processing (Octokit), OpenTelemetry (OTLP). References `Carnitas.Grpc` + `Carnitas.Model`. Docker image bundles the `terraform` binary.
- `Carnitas.AppHost` — Aspire AppHost (`Aspire.AppHost.Sdk` 13.5.4) orchestrating `Carnitas.Web` and `Carnitas.CLI`; CLI waits for web.
- `Carnitas.Test` — xunit.v3 tests on Microsoft.Testing.Platform (MTP). References CLI, Grpc, Model, Web and `Carnitas`. `OperationRunLogEntryModelTests` builds the EF model without a DB (so it runs in CI); `TaskQueueTests` skip unless `CARNITAS_TEST_DB` is set.
- `Sarsoo.Terraform` — **git submodule** (vendored orchestration lib), itself containing nested submodule `TerraformDotnet` (HCL parser/emitter). Referenced by ProjectReference, not NuGet.

## Prerequisites
- .NET 10 SDK (CI pins 10.0.x).
- Init submodules recursively or restore/build fails:
  `git submodule update --init --recursive`
- Local web dev / migrations need PostgreSQL. Connection strings:
  - Web: `Carnitas.Web/appsettings.Development.json` → `ConnectionStrings:DefaultConnection` (`host=localhost; database=carnitas; username=andy`).
  - EF design-time factory: `Carnitas.Model/Design/Factory.cs` (same string).
  - Queue integration tests read env var `CARNITAS_TEST_DB`; when unset those tests skip (model-only tests still run without a DB).

## Commands
- Build: `dotnet build`
- Test: `dotnet test` — runs `Carnitas.Test` (xunit.v3 via Microsoft.Testing.Platform; `global.json`'s `test.runner` setting selects the MTP runner). DB-backed queue tests are skipped unless `CARNITAS_TEST_DB` is set.
- Run web (dev): `dotnet run --project Carnitas.Web`
  - HTTP/1 (Blazor) → http://localhost:5000
  - HTTP/2 (gRPC) → http://localhost:5001
  - Docker image exposes 8080/8081 instead.
- Run the full local stack (Aspire): `dotnet run --project Carnitas.AppHost` (or `aspire start`); dashboard endpoints are in `Carnitas.AppHost/Properties/launchSettings.json`.
- Run CLI worker: `dotnet run --project Carnitas.CLI -- start`
- EF migrations (DbContext lives in Carnitas.Model; design-time factory targets local postgres):
  `dotnet ef migrations add <Name> --project Carnitas.Model`
  `dotnet ef database update --project Carnitas.Model`
- Containers: `DevOps/Container/build.sh` wraps `docker buildx bake` with `DevOps/Container/docker-bake.hcl`.

## CodeGraph indexes (local dev)
- Root `.codegraph/` auto-syncs and is configured by root `codegraph.json` (`"include": ["MudBlazor/", "libgit2sharp/"]`). It covers the whole solution **plus** loose dependency checkouts (MudBlazor, libgit2sharp) and the `Sarsoo.Terraform`/`TerraformDotnet` submodule sources — use `codegraph explore` / `codegraph_explore` **without** `projectPath` for cross-repo questions.
- Nested, separately queryable indexes: `MudBlazor/`, `libgit2sharp/`, `Sarsoo.Terraform/`, `Sarsoo.Terraform/TerraformDotnet/` — pass `projectPath` (MCP) or `-p <path>` (CLI) for library-focused queries.
- Nested `projectPath`/`-p` indexes are not live-watched (project convention) — run `codegraph sync` inside them after a MudBlazor / libgit2sharp / submodule version bump (root auto-syncs).
- `libgit2sharp/` is an untracked loose checkout with its own `.git` and is **not** in `.gitignore` (unlike `MudBlazor/`), so be careful not to stage it.
- Dependency sources are deliberately kept inside the root index for unified context (`MudBlazor/`, `libgit2sharp/` are listed under `include`, not `exclude`). If generic dependency names outrank first-party code, prefer `deprioritize` over excluding them.
- `MudBlazor/` is a loose, gitignored checkout of tag **v9.4.0** (sparse: `src/MudBlazor`, `src/MudBlazor.Docs`), while `Carnitas.Web` pins MudBlazor **9.10.0** — the checkout is stale, so don't treat it as API truth for this repo.

## Gotchas / conventions
- Don't edit submodule code (`Sarsoo.Terraform`, `TerraformDotnet`) or loose dependency checkouts (`MudBlazor`, `libgit2sharp`) for Carnitas work — separate pinned repos.
- CLI and Sarsoo.Terraform are AOT with `JsonSerializerIsReflectionEnabledByDefault=false` — new JSON must use source-generated contexts (`EnableAotAnalyzer` is on).
- CI is mirrored in `.github/workflows/ci.yml` and `.gitea/workflows/ci.yml` (both: build + `dotnet test`; container build only on main/tags). `.github` pushes to DockerHub; `.gitea` pushes to `cr.lab.sarsoo.xyz`. In `.github` the CLI image is published as `carnitas-web` and the Web image as `carnitas-cli`; `DevOps/Container/docker-bake.hcl` similarly maps `cli`→`myapp/frontend`, `webapp`→`myapp/backend`. Verify with `docker buildx bake --print --file ./DevOps/Container/docker-bake.hcl`.
- Keep package versions aligned across the csproj files (the authoritative source): EF Core / Identity 10.0.12, Npgsql 10.0.3, MudBlazor 9.10.0, LibGit2Sharp 0.32.0, System.CommandLine 2.0.12, Aspire AppHost SDK 13.5.4, Grpc 2.83.0.
- Terraform binary resolution differs by target: CLI dev appsettings points at a Nix path (`Terraform.BinaryPath`), CLI Docker installs via `tfenv`, Web Docker copies `hashicorp/terraform`.
