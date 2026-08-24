---
name: utilitiescs-test-parallelism-flakiness
description: UtilitiesCS.Test full-suite has timing tests that time out (~22s) under default 24-worker parallelism + coverage instrumentation; lower MSTest Workers via runsettings for a deterministic pass
metadata:
  type: project
---

Running the full `UtilitiesCS.Test` + `QuickFiler.Test` suite via `vstest.console.exe` produces non-deterministic failures: a small set of timing-sensitive UtilitiesCS.Test cases (e.g. `TryAddValuesAsync_UpdatesExistingValue`, `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`, OneDrive download tests) time out at ~22s and fail. Each PASSES in isolation at ~65ms. The suite runs MSTest class-level parallelization at Workers=0 (= processor count, 24 on <host>) via an assembly `[Parallelize]` attribute.

- Raw `/EnableCodeCoverage` (built-in collector): ~1-2 flaky failures per run, failing set changes run to run.
- `dotnet-coverage collect` instrumentation: amplifies to ~20 flaky failures regardless of worker count (instrumentation overhead is the dominant cause, not just parallelism).

**Why:** The failures are parallel CPU-starvation timeouts, not defects; they are pre-existing and unrelated to whatever code is under test.

**How to apply:** For a deterministic green gate run, pass a runsettings that overrides `<MSTest><Parallelize><Workers>4</Workers><Scope>ClassLevel</Scope>` via vstest `/Settings:`. Under the built-in Code Coverage collector at 4 workers the full suite is deterministically clean (4661 tests, 0 failed). This is a settings adjustment only — it does not weaken assertions or add retries/sleeps. For numeric coverage, still use `dotnet-coverage collect --output-format cobertura` (the `.coverage` binary is not offline-convertible here — see [[project-qfc227-coverage-tooling]]); a few flaky failures in that instrumented run do not invalidate the coverage numbers. `dotnet-coverage` is a GLOBAL tool (`<user-profile>\.dotnet\tools\dotnet-coverage.exe`), not in the repo tool manifest, so `dotnet tool run dotnet-coverage` fails — call the exe directly, and pass the wrapped vstest.console.exe path in Windows form (`C:\...`) or dotnet-coverage's process launcher cannot find it.

CSharpier: the repo `dotnet tool run csharpier` resolves v1.2.6 which uses `check`/`format` subcommands (`csharpier check .`, `csharpier format .`); the legacy `--check` flag is rejected. See [[project-repo-sdk-and-nullable-rebuild]].
