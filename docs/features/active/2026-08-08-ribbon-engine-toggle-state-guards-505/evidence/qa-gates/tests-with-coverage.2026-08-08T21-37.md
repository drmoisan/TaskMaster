# P5-T6 — Full Test Suite with Coverage

Timestamp: 2026-08-08T21-37

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -Configuration Debug -SearchRoot . -CoverageOutput coverage\coverage-final-505.cobertura.xml
```

run from `<REPO>`.

EXIT_CODE: **0**

## Output Summary

### Discovery

- **Discovered test assemblies: 9** — the expected count per plan rule 8. A count of 0 would be a
  filter/tooling bug, never an empty suite.

### Test results

| Metric | Value |
|---|---|
| Total | **6435** |
| Passed | **6435** |
| **Failed** | **0** |
| **Skipped** | **0** |

`Test Run Successful.`

The total rose from the P0-T9 baseline's 6399 by exactly **36**, which reconciles precisely to this
delivery's additions: 5 (`RibbonViewerEngineCallbackShapeTests`) + 7 (`EngineToggleCatalogTests`,
including 2 data rows) + 18 (`EngineToggleStateCoordinatorTests`, including 3 data rows) + 6 (the
new `EngineCommandCatalogTests` data rows) = 36. No pre-existing test was removed, renamed away, or
silently dropped.

### Reconciliation against the P0-T10 pre-existing set

The P0-T10 recorded set is:

```
UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict   (issue #508)
```

This run produced **zero** failures, so the failure set is the empty set, which is trivially a
subset of the recorded set. The #508 flake did not fire. Zero tests were skipped.

### Coverage (root `<coverage>` attributes, measured verbatim)

| Attribute | Value |
|---|---|
| `line-rate` | **0.859190** |
| `branch-rate` | **0.793602** |
| `lines-covered` | **95993** |
| `lines-valid` | **111725** |
| `branches-covered` | 22278 |
| `branches-valid` | 28072 |
| `<package>` node count | 9 |

Read directly from the emitted post-processed Cobertura document
(`coverage\coverage-final-505.cobertura.xml`, 10,482,949 bytes). The raw dump stays under the
gitignored `coverage\` directory and is never committed (rule 9); the numeric headline values above
are the committed record.

### Earlier flaky invocations (disclosed)

Two earlier invocations of this command failed with 2 and then 7 failures respectively, all in
`QuickFiler.Controllers.Tests.QfcItemController_InitializationTests` /
`QfcItemController_CreationTests` — the `WinFormsPumpHost` message-pump test family, failing with
`Invoke or BeginInvoke cannot be called on a control until the window handle has been created` and
with 60-second `[Timeout]` expiries. The diagnosis, including the proof that `QuickFiler` has no
reference to `TaskMaster` and that the same tests pass 4/4 in isolation once machine load is
reduced, is recorded in
`<FEATURE>\evidence\other\phase5-attempt1-aborted.2026-08-08T21-30.md`. No test was weakened, no
assertion relaxed, no retry or sleep added, and no `QuickFiler` source was modified.

Binary outcome: **PASS** — zero failed, zero skipped.
