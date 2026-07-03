---
name: qfc227-coverage-tooling
description: How to get numeric per-class C# coverage in this legacy VSTO repo (issue #227 cycle-2) — the reliable mechanism after several tooling dead-ends
metadata:
  type: project
---

Getting numeric per-class line coverage for the legacy `packages.config` VSTO projects (QuickFiler.Test etc.) in this repo.

**Why:** The plan's canonical `vstest.console.exe <dll> /EnableCodeCoverage` passes but emits a binary `.coverage` that could NOT be offline-converted in this environment — `dotnet-coverage v18.5 merge` and `Microsoft.CodeCoverage.Console v18.7 merge` both produce empty Cobertura packages. `dotnet-coverage collect` (profiler-attach) breaks 135/233 tests, so its numbers are unrepresentative.

**How to apply (the working mechanism):** run vstest with a Cobertura-format runsettings — same environment, still 233/233 pass, and it emits a real Cobertura XML directly:
- Runsettings: DataCollector friendlyName="Code Coverage", `<Configuration>` with `<Format>Cobertura</Format>` placed DIRECTLY under `<Configuration>` (NOT nested in `<CodeCoverage>` — nesting silently reverts to `.coverage`), and a sibling `<CodeCoverage><Attributes><Exclude>` block listing `System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute` (plus DebuggerHidden/DebuggerNonUserCode/CompilerGenerated/GeneratedCode) so exempt members are excluded from the denominator. Without that exclude block, `[ExcludeFromCodeCoverage]` members show as uncovered lines and deflate the number.
- Invoke: `MSYS_NO_PATHCONV=1 vstest.console.exe <dll> /EnableCodeCoverage "/Settings:C:/.../cov.runsettings" "/ResultsDirectory:C:\...\out"`. In Git Bash, MSBuild/vstest args need `MSYS_NO_PATHCONV=1` or the leading-`/` flags get mangled to paths; MSBuild flags also work with `-` prefix (`-t:Build -p:...`).
- Parse the emitted `*.cobertura.xml`: repo-wide from root `lines-covered`/`lines-valid`; per-class by summing `<line hits>` over `<class>` entries whose `name` contains `QfcItemController` and `filename` under `\QuickFiler\Controllers\`.

Tools present: MSBuild.exe and vstest.console.exe under `C:\Program Files\Microsoft Visual Studio\18\Community\...`; `dotnet-coverage` and `csharpier` on PATH. A reusable `cov.runsettings` is kept in the session scratchpad.

**Baseline #227 cycle-2 (2026-07-01):** 233/233 tests; QfcItemController non-exempt denominator 226/239 = 94.56%; repo-wide 13.68% (under #197 authority-scoped exception). Starting `[ExcludeFromCodeCoverage]` count = 103.

**Update (2026-07-02, Phase 6-8 run):** `dotnet-coverage collect --output-format cobertura --settings coverage.config` (not the runsettings-Cobertura path above) was the mechanism actually used for the final P6-P8 gates; it does **NOT** honor `[ExcludeFromCodeCoverage]` — exempt member lines still appear as `hits=0` in the Cobertura output, so the raw per-file line-rate understates non-exempt coverage. Compute the "affected non-exempt denominator" by excluding the annotated member source-line spans from the per-line hit data via a brace-matching source parse, not by trusting the tool's exclude filter. Other environment facts: run vstest from Bash with `MSYS2_ARG_CONV_EXCL="*"` (or `MSYS_NO_PATHCONV=1`) or git-bash mangles `/InIsolation`/`/Settings:` into Windows paths; `dotnet-coverage collect -- vstest.console.exe ...` needs `vstest.console.exe` on PATH (prepend the VS `Common7/IDE/Extensions/TestPlatform` dir) — the full quoted path after `--` fails with "cannot find the file specified"; `scripts/vscode/Invoke-MSTest.ps1` throws a StrictMode `.Count` error with only one `*.Test.dll` discovered, so invoke `vstest.console.exe` directly for a single assembly; MSBuild is not on the git-bash PATH, use `scripts/vscode/Invoke-VSBuild.ps1` (resolves via vswhere); csharpier needs the `format` subcommand (`dotnet tool run csharpier format .` — bare `.` errors, `check` is read-only).
