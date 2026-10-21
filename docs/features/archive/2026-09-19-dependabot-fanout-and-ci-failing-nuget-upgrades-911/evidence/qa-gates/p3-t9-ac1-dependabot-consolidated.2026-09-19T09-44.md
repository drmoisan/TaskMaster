# P3-T9 — AC1: Dependabot configuration is consolidated

Timestamp: 2026-09-20T00-37

Command — CMD-PESTER-ALL restricted to the AC1 suite with the `*AC1-*` full-name filter and
`<OUTPATH>` set to `coverage/p3-t9-ac1-coverage.xml`:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/DependabotConfig.Tests.ps1"); $c.Filter.FullName = "*AC1-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p3-t9-ac1-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Verbatim result line

```
PESTER Passed=5 Failed=0 Skipped=0 Total=5
```

## `Detailed` output, each of the five AC1 assertions named individually

```
Describing Dependabot configuration consolidation
 Context Grouping and pull-request volume
   [+] AC1- declares exactly one entry under groups
   [+] AC1- declares applies-to version-updates and a catch-all pattern on that entry
   [+] AC1- limits open pull requests to one
 Context Ignore entries
   [+] AC1- carries one unqualified Deedle ignore entry
   [+] AC1- retains the merge-base semver-major pair set element by element
```

Five named cases, five passing, mapping one-to-one onto the five clauses AC1 states.

## Why the filter yields exactly 5

Per gate rule 11, `$c.Filter.FullName` matches against `Describe > Context > It` joined, so a
criterion token in an outer block name would admit every `It` beneath it and break the exact
`Total`. The three outer names in this file — `Dependabot configuration consolidation`,
`Grouping and pull-request volume` and `Ignore entries` — carry no `AC`-digit token, measured at
P3-T8 as `OUTER_AC_DIGIT=0`, so the filter selects on the `It` names alone.

The trailing hyphen in `*AC1-*` is what keeps the file's future `AC4-` cases out of this
population, and would likewise keep `AC10-`, `AC11-` and `AC12-` out: `*AC1*` without the hyphen
would match all of them. At this point in the run the file carries `AC1-` cases only, so the
filtered `Total` of 5 equals the file's unfiltered `Total` of 5; P3-T10 adds the `AC4-` cases and
the two figures separate.

## What each case fails on

| Case | Fails when |
|---|---|
| exactly one entry under `groups` | a second group is added, or the sole group is removed |
| `applies-to: version-updates` and the catch-all pattern | the group is narrowed to a pattern list, or the applies-to key is dropped |
| `open-pull-requests-limit` equals 1 | the limit is raised back towards the merge-base value of 10 |
| one unqualified `Deedle` entry | the entry is dropped, duplicated, or re-qualified with `versions` or `update-types` |
| the semver-major pair set | any of the eight entries is dropped, renamed, reordered, or re-qualified, or a ninth is added |

A reintroduced inert partition key is caught indirectly by the first case when it appears as a
group key, and directly by the P3-T7 measurement recorded in that task's artifact.

## Coverage document

Written to `coverage/p3-t9-ac1-coverage.xml`, under `coverage/`, which `.gitignore:144` ignores.
This task records no aggregate JaCoCo LINE figure and is therefore not one of the six tasks the
gate rule 12 standing-in obligation falls on. No `.xml` is written under the evidence tree.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | 0 | PASS |
| `Total` | 5 | 5 | PASS |
| Each of the five AC1 assertions named individually in `Detailed` | five names | five names, all `[+]` | PASS |

`Failed=0` is guarded by the exact `Total=5`: a filter that selected nothing reports `Total=0` and
fails.

## Acceptance criterion checked off

**AC1 — Dependabot configuration is consolidated** is checked off in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.

Output Summary: the `*AC1-*` filtered run over
`tests/scripts/dependencies/DependabotConfig.Tests.ps1` returned EXIT_CODE 0 with
`PESTER Passed=5 Failed=0 Skipped=0 Total=5`. All five AC1 assertions are named individually in
the `Detailed` output and all pass: exactly one group entry; that entry declaring
`applies-to: version-updates` and the catch-all pattern; `open-pull-requests-limit` equal to 1;
one unqualified `Deedle` ignore entry; and the eight-member semver-major pair set matching the
merge-base census element by element. AC1 is checked off in `spec.md`.
