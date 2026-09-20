# P9-T2 — PowerShell QA step 2, PoshQC analyze (iteration 1) — FAILED

Timestamp: 2026-09-20T09-44

Command: CMD-POSHQC-ANALYZE — MCP tool `mcp__drm-copilot__run_poshqc_analyze`, `workspace_root`
passed as `<execution-worktree-root>`.

Exact `scan_folders` argument value passed:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

EXIT_CODE: 1

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 18 issue(s).
```

## Integer total finding count

**18.** The acceptance requires exactly 13. This iteration fails.

## The five findings outside the baseline set

All five lie in `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, one of the
fifteen files this change owns, for which the acceptance requires a count of exactly 0. The file was
created at P7-T2 and no analyzer step ran between its creation and this one, which is why the
findings surface here rather than at a Phase 7 gate.

| File path | Rule name | Severity | Line |
|---|---|---|---|
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | PSUseShouldProcessForStateChangingFunctions | Warning | 86 |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | PSReviewUnusedParameter | Warning | 93 |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | PSReviewUnusedParameter | Warning | 94 |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | PSReviewUnusedParameter | Warning | 95 |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | PSUseShouldProcessForStateChangingFunctions | Warning | 114 |

The remaining thirteen are the P0-T17 baseline rows 1 to 13, unchanged.

## Correction applied, and why it changes no assertion

Two causes, both in test-helper plumbing rather than in any assertion:

1. PSUseShouldProcessForStateChangingFunctions fired on the two fixture builders
   `New-RepairFixture` and `New-StandardFixture`. The New verb is in the analyzer state-changing set,
   and neither function changes any state: each returns an in-memory hashtable. Declaring
   `SupportsShouldProcess` would assert a capability the functions do not have, so they were renamed
   to `Get-RepairFixture` and `Get-StandardFixture`. Both are defined and called only inside this
   file, so the rename reaches nothing else; all nine references were updated.

2. PSReviewUnusedParameter fired on the `$Asset`, `$Identity` and `$Listing` parameters of the
   fixture builder. Each is referenced only inside a nested scriptblock closed over by
   `GetNewClosure()`, which the analyzer data-flow does not follow. Each is now read into a local at
   body level and the closures capture the local. That is behaviour-preserving: `GetNewClosure()`
   captures the same values either way.

No test assertion, expectation or fixture datum was altered. The loop restarts from P9-T1, whose
iteration 2 artifact records the re-run format step, and iteration 2 of this task records the result.
