# [P8-T2] Final Status

- **Issue:** #438
- **Task:** [P8-T2]
- **Timestamp:** 2026-08-08T11-41
- **Branch:** `bug/quickfiler-search-keystroke-focus-steal-438`
- **Baseline HEAD:** `904b4c38dba0f9f41707c3c0f077e123c78de59c`
- **Delivery commit:** `3298f0f16466751b3ccb20fbf48c6c4be889ad54` (73 files changed)

## Command

`pwsh -NoProfile -Command "git add -A ; git commit -F <message> ; git status --porcelain"`

- **EXIT_CODE:** 0

## (1) Plan checklist state

All **58** tasks in `plan.2026-08-08T09-57.md` are `[x]`:

| Phase | Tasks | State |
|---|---|---|
| Phase 0 — Baseline & Policy Capture | P0-T1…P0-T7 | all `[x]` |
| Phase 1 — Fail-Before Regression Capture | P1-T1…P1-T3 | all `[x]` |
| Phase 2 — Session & Router Additive Transitions | P2-T1…P2-T5 | all `[x]` |
| Phase 3 — takeFocus Intent Through the Open Pipeline | P3-T1…P3-T8 | all `[x]` |
| Phase 4 — Viewer Presentation Composite | P4-T1…P4-T5 | all `[x]` |
| Phase 5 — Controller Fix & Regression Pass | P5-T1…P5-T6 | all `[x]` |
| Phase 6 — Final QA Loop | P6-T1…P6-T7 | all `[x]` |
| Phase 7 — Acceptance Criteria Check-Off | P7-T1…P7-T15 | all `[x]` |
| Phase 8 — Documentation & Handoff | P8-T1, P8-T2 | all `[x]` |

Zero unchecked `- [ ] [P#-T#]` entries remain.

## (2) Evidence paths resolve

All 25 artifacts exist under `<FEATURE>/evidence/` (canonical scheme; no non-canonical path was used or supplied):

**`evidence/baseline/` (8)** — `toolchain-bootstrap`, `phase0-instructions-read`, `git-baseline`, `format-baseline`, `analyzer-baseline`, `nullable-baseline`, `test-coverage-baseline`, `coverage-baseline.cobertura.xml`

**`evidence/regression-testing/` (4)** — `fail-before`, `fail-before-exception`, `fail-before-controller`, `pass-after`

**`evidence/other/` (6)** — `p2-gate`, `p3-gate`, `p4-gate`, `scope-guard`, `ac-reconciliation`, `final-status` (this file)

**`evidence/qa-gates/` (8)** — `final-format`, `file-size-audit`, `final-analyze`, `final-nullable`, `test-coverage-final`, `coverage-final.cobertura.xml`, `coverage-delta`, `wiring-audit`

Every artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

## (3) Working tree committed and clean

```
$ git status --porcelain
(no output)
```

`HEAD` = `3298f0f16466751b3ccb20fbf48c6c4be889ad54` on `bug/quickfiler-search-keystroke-focus-steal-438`. **Clean porcelain.** No branch switch occurred at any point.

## (4) Final toolchain pass — commands and exit codes

Run in the mandated order. The loop restarted twice (once for the P6-T2 size violation, once for the P6-T6 coverage threshold); the figures below are the **final uninterrupted pass**.

| # | Stage | Command | EXIT_CODE | Result |
|---|---|---|---|---|
| 1 | Format | `& ./.dotnet-sdk/dotnet.exe tool run csharpier format .` | **0** | `Formatted 1501 files in 1484ms.` |
| 1b | Format gate | `& ./.dotnet-sdk/dotnet.exe tool run csharpier check .` | **0** | `Checked 1501 files in 4625ms.` — zero violations |
| 2 | Size audit | `git diff --name-only 904b4c38 -- '*.cs'` + `git ls-files --others --exclude-standard -- '*.cs'`, line counts | **0** | 30 files, all <= 500 lines (max 499) |
| 3 | Analyzers | `& msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | **0** | 0 errors; 6 pre-existing warnings |
| 4 | Nullable | `& msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | **0** | 0 errors |
| 5 | Tests + coverage | `& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml'` | **0** | 6348 / 6348 passed, 0 failed, 40.8 s |

Every invocation ran through `pwsh -NoProfile -Command "... ; exit $LASTEXITCODE"` and its exit code was checked explicitly, per the plan's Environment Warning 1.

### Coverage summary

| Metric | Baseline | Post-change | Delta |
|---|---:|---:|---:|
| Repository `line-rate` | 0.858261 | **0.858665** | +0.000404 |
| Repository `branch-rate` | 0.792082 | **0.792502** | +0.000420 |
| QuickFiler `line-rate` | 0.8081586615283392 | **0.8091631603553062** | + |
| UtilitiesCS `line-rate` | 0.895326282732185 | **0.8957251943617782** | + |
| Minimum new/changed member line coverage | n/a | **95.24%** (`BeginOpenCore`) | gate >= 90% |

## (5) Acceptance criteria

AC-1 through AC-14 are all `[x]` in `spec.md`, each backed by an evidence artifact enumerated in `ac-reconciliation.2026-08-08T11-41.md`. HV-1 remains `[ ]` by design (non-gating human verification, discharged post-fix per the runbook).

## (6) Outstanding items for the caller

Neither is a defect in this change; both are recorded rather than repaired because they fall outside the plan's scope lock.

1. **Pre-existing `CS2002`** — `UtilitiesCS.Test.csproj` declares `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` twice (lines 304 and 356). Proven present at baseline HEAD `904b4c38`. Recommend promotion as its own issue.
2. **Load-induced test flakiness** — `QfcItemController_InitializationTests.*ThroughThePumpHost*` (real WinForms message pump) and `WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` (process-global `UiThread.Dispatcher`) fail nondeterministically under CPU saturation. Both pass in isolation and in the final run. The pump-host family also creates a visible window during a test run. Recommend promotion as its own issue.

## Result

- **Output Summary:** All 58 plan tasks are checked off, all 25 evidence artifacts resolve on disk under the canonical `<FEATURE>/evidence/` scheme, and the working tree is committed clean at `3298f0f16466751b3ccb20fbf48c6c4be889ad54` on `bug/quickfiler-search-keystroke-focus-steal-438`. The final toolchain pass ran format -> analyzers -> nullable -> tests, every stage returning EXIT_CODE 0, ending with 6348 of 6348 tests passing and repository coverage improved on both line and branch rate. Accept criteria met.
