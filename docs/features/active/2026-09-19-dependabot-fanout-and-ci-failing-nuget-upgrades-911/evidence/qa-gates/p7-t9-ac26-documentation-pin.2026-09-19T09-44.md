# P7-T9 — AC26: documentation matches the delivered behaviour

Timestamp: 2026-09-20T02-12

Command:

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/DependabotConfig.Tests.ps1"); $c.Filter.FullName = "*AC26-*"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p7-t9-ac26-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
   [+] AC26- records a NuGet pin in the workflow README equal to every workflow literal
PESTER Passed=1 Failed=0 Skipped=0 Total=11 NotRun=10
PESTER_EXIT=0
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| `EXIT_CODE` | 0 | PASS |
| `Failed` | 0 | PASS |
| `Total` (executed population), at least 1 | 1 | PASS |
| The test records the README literal | `7.9.0` | PASS |
| The test records the set of workflow literals it compared | enumerated below | PASS |
| That workflow set has exactly 3 members | **4** | **DISCREPANCY — see below** |

`Total` is the executed population, `Passed + Failed + Skipped` = 1, per the `CMD-PESTER-ALL`
retarget for a task whose command sets `$c.Filter.FullName`. `TotalCount` is **11** and
`NotRunCount` is **10**, recorded as context.

## The two sides of the comparison, as measured

README three-part literals, read from backtick-quoted spans:

```
README-THREE-PART-LITERALS: 18.10.0,5.6.1,7.9.0
```

`18.10.0` is the `dotnet-coverage` pin and `5.6.1` the Pester pin, both pre-existing; `7.9.0` is the
NuGet CLI pin P7-T8 recorded. The test intersects the README's literals with the literals the
workflow steps declare, so the comparison is against the NuGet pin alone and yields a single
value.

Workflow `setup-nuget` steps and the literal each declares:

```
STEP-COUNT: 4
  _build-analyzers.yml  line 31 => 7.9.0
  _build-nullable.yml   line 31 => 7.9.0
  _mstest-coverage.yml  line 47 => 7.9.0
  dependabot-repair.yml line 65 => 7.9.0
DISTINCT-LITERAL: 7.9.0
```

The test asserts that the declared set collapses to exactly one distinct value, that the README
records that value, and then that each of the four steps declares it. The criterion fails when the
pin is bumped in one place only: the distinct-value assertion goes to 2, and the per-step equality
fails on the step that moved. The comparison runs over a non-empty set in both directions, so
neither an empty README nor a broken step enumerator can satisfy it vacuously.

## DISCREPANCY: the plan expects a 3-member workflow set and the tree carries 4

P7-T9 states "with the workflow set having exactly 3 members". Measured, the set has **4**: the
three CI gates the P0-T21 census recorded, plus `.github/workflows/dependabot-repair.yml`, which
**P7-T6 of this same plan requires** — "setting up MSBuild and NuGet pinned to `7.9.0`". The
numeral therefore predates its own plan's fourth workflow rather than describing a defect in the
tree: no reading of the phrase yields 3 after P7-T6 lands. The set of distinct literal values is 1,
the set of declaring files is 4, and the set of declaring steps is 4.

Two possible corrections, for the coordinator rather than for this executor:

1. Restate the clause as "at least 3 members" or as "exactly 4 members, the three CI gates plus the
   repair workflow", leaving the test as authored here, which compares every declaring step.
2. Restrict the AC26 comparison to the three CI gates, which would make the numeral true and would
   stop testing the repair workflow's own pin.

This artifact records the first as implemented, because a comparison that skipped the repair
workflow would leave the newest pin untested, and because the assertion the criterion names —
"the literal recorded in the README equals the literal declared in the workflow files" — is about
the workflow files rather than about a subset of them.

This task checks off **AC26** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`. The
criterion itself is discharged: the README literal equals every workflow literal, over a non-empty
set, with a failing condition that a one-place bump reaches.
