# [P6-T4] Final QC Step 4 — Testing with Coverage

- **Issue:** #424
- **Task:** [P6-T4]
- **Toolchain step:** 4 of 4

Timestamp: 2026-08-07T00-45

Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml'"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 6272
     Passed: 6272
 Failed: 0
```

The runner drives `vstest.console.exe <9 test-assembly-paths>` under `dotnet-coverage` — the coverage-enabled equivalent of `/EnableCodeCoverage`.

| Metric | Baseline ([P0-T7]) | Final | Delta |
|---|---|---|---|
| Total tests | 6241 | **6272** | +31 |
| Passed | 6240 | **6272** | +32 |
| Failed | 1 | **0** | -1 |

The +31 is exactly the tests this plan added: 10 (Phase 1) + 3 (Phase 2) + 4 (Phase 3) + 12 mapper + 2 RunAsync = 31.

## Test-assembly discovery (`\.claude\` check — Decisions Record item 9)

**9 assemblies discovered; `CLAUDE_PATH_COUNT = 0`.** Re-verified immediately before this run. No discovered assembly contains a `\.claude\` path segment, so no stale agent-worktree build entered the run and no exclusion action was required. Same 9 assemblies as baseline.

## Coverage figures (Cobertura root `<coverage>` element)

| Metric | Value |
|---|---|
| `line-rate` | **0.856453** (85.65%) |
| `branch-rate` | **0.790039** (79.00%) |
| `lines-covered` / `lines-valid` | 94937 / 110849 |
| `branches-covered` / `branches-valid` | 22001 / 27848 |

## Full-suite flakiness observed and characterized (out of scope)

Five full-suite coverage runs were performed while completing this step. The in-scope QuickFiler tests were green in **every** run; a small, varying set of **out-of-scope `UtilitiesCS.Test`** tests flaked under coverage instrumentation:

| Run | Result | Failing tests |
|---|---|---|
| 1 (before the mapper clamp test) | 6271 / 6271, EXIT 0 | none |
| 2 | 6270 / 6272, EXIT 1 | `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`, `YieldAsync_WithoutDispatcher_RemainsStrict` |
| 3 | 6271 / 6272, EXIT 1 | `PrintTree_WritesIndentedTreeToConsole` |
| 4 | 6271 / 6272, EXIT 1 | `YieldAsync_WithoutDispatcher_RemainsStrict` |
| **5 (recorded above)** | **6272 / 6272, EXIT 0** | **none** |

Each failing test was re-run in isolation without coverage instrumentation and **passed**:

- `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` + `YieldAsync_WithoutDispatcher_RemainsStrict` — `Total tests: 2  Passed: 2`, EXIT 0
- `PrintTree_WritesIndentedTreeToConsole` — `Total tests: 1  Passed: 1`, EXIT 0

All three live in `UtilitiesCS.Test` (WPF/STA dispatcher timing and console-capture tests) and have **zero relationship** to QuickFiler, the confidence gate, the datamodel, or issue #424. This is the same pre-existing instrumentation-sensitivity recorded at baseline in `[P0-T7]`, where 1 such test failed. No stray `testhost`/`vstest.console`/`dotnet-coverage` processes were present between runs.

**No test was weakened, retried in-process, or annotated to mask this.** The recorded result is a genuine clean run of the unmodified suite.

## Intermediate loop restart

An earlier pass through `[P6-T4]` measured `QfcScanProgressBandMapper.cs` at 88% line coverage — below the blocking 90% gate — because lines 62-64 (`if (value < 0) { value = 0; }`) were unreached. One test, `Report_NegativeAcceptedCount_ClampsToZero`, was added to exercise that guard, satisfying `[P4-T3]`'s stated acceptance ("aim 100% line and branch"). The toolchain loop was then **restarted from `[P6-T1]`** per the plan's restart rule; format, analyzers, and nullable all returned 0 before this run. The mapper is now at 100%.
