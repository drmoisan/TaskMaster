# Size and Text Baseline — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-28-54
- Task: [P0-T5]
- Finding: R9d
- EXIT_CODE: 0

## Line Counts of Every File This Cycle Edits

```
([System.IO.File]::ReadAllLines((Resolve-Path <path>).ProviderPath)).Count
```

| # | Path | Lines | Cap | Under cap |
|---|---|---|---|---|
| 1 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **498** | 500 | yes, by 2 |
| 2 | `scripts/dependencies/ConsistencyVerifier.psm1` | **493** | 500 | yes, by 7 |
| 3 | `scripts/dependencies/ProjectConsistency.psm1` | **331** | 500 | yes |
| 4 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | **185** | 500 | yes |
| 5 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | **335** | 500 | yes |
| 6 | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | **382** | 500 | yes |
| 7 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | **275** | 500 | yes |
| 8 | `.github/workflows/dependabot-repair.yml` | **123** | 500 | yes |

Exactly 8 counts, every one an integer at most 500. **No file starts over the cap.** The two files
R9d declares at capacity are rows 1 and 2, with 2 and 7 lines of headroom respectively, which is
why decision D1 extracts a function out of row 1 rather than appending to it.

Every count agrees with the plan's `Facts Measured for This Cycle` table. All eight are invalidated
by this cycle's own edits, so no later task asserts one of them as a predicted value; each size
clause reads a ceiling and this measured value.

## The Six Workflow Lines Phase 3 Rewrites, Verbatim

Identified by their text rather than their number, because Phase 3's own edits move every number
below them.

**1. The `repair-count=` output line** (currently `.github/workflows/dependabot-repair.yml:89`):

```
          "repair-count=$($result.RepairCount)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
```

**2. The `beyond-known-weak=` assignment line** (currently `:86`):

```
          $beyondKnownWeak = @($kind | Where-Object { $_ -ne 'Analyzer' -and $_ -ne 'BindingRedirect' }).Count
```

**3. The commit step's `if:` line** (currently `:95`):

```
        if: steps.repair.outputs.repair-count != '0'
```

**4. The first `git config` line** (currently `:100`):

```
          git config user.name 'dependabot-repair[bot]'
```

**5. The second `git config` line** (currently `:101`):

```
          git config user.email 'dependabot-repair[bot]@users.noreply.github.com'
```

**6. The disclosure step's `name:` line** (currently `:106`):

```
      - name: Disclose the repairs on the pull request
```

Six fragments recorded verbatim, each non-empty.

## Adjacent State the Phase 3 Tasks Depend On

Recorded because the acceptance conditions of [P3-T2], [P3-T4], [P3-T6] and [P3-T7] read against it.

- The repair step publishes four outputs today: `repair-count`, `beyond-known-weak`, `skip-count`
  and `report-path`. There is no `written-count`. [P3-T2] adds one.
- The disclosure step at `:106` carries **no** `if:` line. [P3-T4] adds one.
- The disclosure body composition at `:119` is `($existing + "\`n\`n" + $report)`, an unconditional
  append with no delimiters. [P3-T4] replaces it.
- `BindingRedirect` appears exactly once in the file today, at `:86`, inside the filter clause
  decision D2 removes. [P3-T6] leaves exactly one occurrence, relocated into the reachability
  comment.
- The literal `dependabot-repair[bot]@users.noreply.github.com` appears exactly once, at `:101`.
  [P3-T7] leaves zero.

## Output Summary

Eight line counts recorded, all integers at most 500; the two capacity files sit at 498 and 493.
Six workflow lines quoted verbatim by text rather than by number. The adjacent workflow state the
Phase 3 acceptance conditions read against is recorded alongside.
