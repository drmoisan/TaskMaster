# Phase 0 — Full-Suite Test and Coverage Baseline (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T11]
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug -CoverageOutput coverage\remediation-baseline.cobertura.xml` run from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55`
EXIT_CODE: 0

## Output Summary

### Test counts

```text
Test Run Successful.
Total tests: 6338
     Passed: 6338
 Total time: 40.7684 Seconds
```

| Metric | Value |
|---|---|
| Total | 6338 |
| Passed | 6338 |
| Failed | **0** |
| Skipped | **0** |

### Failure set

**Empty.** No test failed in this baseline run. In particular `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` — the pre-existing order-dependent flake tracked as issue **#508** — **passed** in this run. That is consistent with its recorded character: it is order-dependent, not deterministically failing. P0-T13 records the resulting pass rule.

### Coverage headline (root `<coverage>` attributes, verbatim)

```xml
<coverage line-rate="0.858462" branch-rate="0.792559" complexity="24678" version="1.9"
          timestamp="1786215545" lines-covered="95467" lines-valid="111207"
          branches-covered="22133" branches-valid="27926">
```

| Attribute | Value |
|---|---|
| `line-rate` | **0.858462** (85.8462%) |
| `branch-rate` | **0.792559** (79.2559%) |
| `lines-covered` | **95467** |
| `lines-valid` | **111207** |
| `branches-covered` | 22133 |
| `branches-valid` | 27926 |

### Artifact location

`coverage\remediation-baseline.cobertura.xml` — 187,490 lines, 10 MB. This path is gitignored (`.gitignore:144` ignores `coverage/*`) and is **never committed**. P0-T12 projects it into a compact package-level JaCoCo summary, and only that summary is written under `<FEATURE>\evidence\`.

Binary outcome satisfied: zero skipped tests, and the failure set is recorded above as empty.
