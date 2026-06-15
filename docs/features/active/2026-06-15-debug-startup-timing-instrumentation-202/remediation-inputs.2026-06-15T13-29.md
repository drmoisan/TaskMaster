# Remediation Inputs: debug-startup-timing-instrumentation (Issue #202)

**Generated:** 2026-06-15T13-29
**Feature Folder:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
**Base Branch:** `main` (`a21d09e18dfebb9e3450c1b3322e7715c09d91e6`)
**Head Branch:** `feature/debug-startup-timing-instrumentation-202` (`1d193d90dba55eec0a739ff13f5ecb5e3d218b99`)

## Source Audit Artifacts

- `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/policy-audit.2026-06-15T13-29.md`
- `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/code-review.2026-06-15T13-29.md`
- `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/feature-audit.2026-06-15T13-29.md`

## Remediation-Required Findings

### Finding 1 — Test file exceeds 500-line limit (BLOCKING)

- **Severity:** Major / FAIL (policy violation; remediation trigger)
- **Policy:** General Code Change Policy §4 file-size limit (CLAUDE.md and `.claude/rules/general-code-change.md`): no production or test code file may exceed 500 lines.
- **File:** `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
- **Detail:** The file grew from 440 lines (baseline `a21d09e1`) to 687 lines (head `1d193d90`) when the four startup-timing wiring tests and their helpers (`AttachMemoryAppender`, `DetachMemoryAppender`, `SetEnginesMock`, and the timing-related additions to `TestableApplicationGlobals`) were added. 687 > 500.
- **Required remediation:** Extract the startup-timing wiring tests and their supporting helpers into a new test file (suggested: `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`), add the `<Compile Include="...">` entry to `TaskMaster.Test.csproj`, and ensure both the original and new files are each under 500 lines. Preserve the `[DoNotParallelize]` markers and the `Settings.Default.StartupTimingEnabled` save/restore in the new file.
- **Verification after fix:** `awk 'END{print NR}'` on both files must report < 500; then re-run the full C# toolchain in order (CSharpier -> analyzer build -> nullable/TreatWarningsAsErrors build -> `vstest.console.exe ... /EnableCodeCoverage`) and confirm EXIT_CODE 0 with no test loss (>= 4194 tests passing).
- **Evidence:** `git show a21d09e1:TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | awk 'END{print NR}'` = 440; `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` = 687.

### Finding 2 — Canonical C# coverage artifact absent (NON-BLOCKING / process)

- **Severity:** Minor (process; does not change any coverage verdict)
- **Detail:** The feature-review-workflow names `artifacts/csharp/coverage.xml` as the C# coverage artifact. That path does not exist. Equivalent merged Cobertura data exists at `TestResults/final-full.cobertura.xml` and was parsed directly; all coverage thresholds (new-code 100%, no regression) were verifiable.
- **Recommended remediation:** Emit or copy the merged Cobertura output to `artifacts/csharp/coverage.xml` so the workflow's artifact contract is satisfied on future reviews.
- **Verification after fix:** `artifacts/csharp/coverage.xml` exists and parses to the same repo-wide and per-class figures.

## Non-Blocking Confirmations (no remediation needed)

- All five acceptance criteria evaluate PASS (see `feature-audit.2026-06-15T13-29.md`).
- New-code line coverage is 100% (>= 90% floor); modified `ApplicationGlobals` improved 60.75% -> 73.88% (no regression).
- Toolchain (format/analyze/nullable/test) recorded EXIT_CODE 0; 4194/4194 tests pass.
- No evidence-location violations; no banned-API usage; no secrets.

## Blocking Finding Count

- Blocking (FAIL-level) findings: 1 (Finding 1 — test-file size).
- Non-blocking findings: 1 (Finding 2 — canonical coverage artifact).

## Handoff

Route Finding 1 to atomic planning (`remediation-handoff-atomic-planner`) as a minimal, mechanical test-file split. Finding 2 may be folded into the same remediation pass or handled as a follow-up; it does not block merge on its own.
