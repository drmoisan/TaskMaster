# P3-T6 — AC7: the incompatible framework is excluded, not ranked (#902)

Timestamp: 2026-09-20T00-18

Command — CMD-PESTER-ALL restricted to the two AC7 suites, with `<OUTPATH>` set to
`coverage/p3-t6-ac7-coverage.xml`:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/PackageCompatibility.Tests.ps1","tests/scripts/vscode/Sync-PackageReferences.Tests.ps1"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p3-t6-ac7-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

Per gate rule 4, the explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` placed after
the count-emitting statement is what makes the exit code meaningful.

## Verbatim result line

```
PESTER Passed=14 Failed=0 Skipped=0 Total=14
```

## `Detailed` output, all fourteen cases

```
Describing PackageCompatibility asset selection and gate decisions
 Context Selector over the asset folders a package ships
   [+] returns net481 when net481 is present
   [+] returns net48 when net481 is absent
   [+] returns netstandard2.0 when offered netstandard2.1 and netstandard2.0 together
   [+] returns no selection when offered only netstandard2.1
   [+] returns no selection when offered only a .NET-Core-era framework
   [+] returns no selection for an empty set
 Context Gate decision records over the same asset evidence
   [+] AC9- returns a rejection carrying a non-empty reason when only unconsumable frameworks are offered
   [+] AC9- returns an acceptance naming the selected asset folder when a consumable asset is present
Describing Sync-PackageReferences framework selection parity with the shared module
 Context Selection cases resolved through the script wrapper
   [+] AC7- resolves net481 through the shared module when net481 is present
   [+] AC7- resolves net48 through the shared module when net481 is absent
   [+] AC7- resolves netstandard2.0 when the package ships netstandard2.1 and netstandard2.0
   [+] AC7- resolves no selection for the three unconsumable asset sets
 Context Absence of any ordering local to the script
   [+] AC7- declares no ordering of its own, so an asset set the deleted array would have resolved returns no selection
 Context End-to-end repair driven through the injected seam
   [+] AC7- repairs a stale hint path to the asset folder the shared module selects
```

## The case the acceptance names

The acceptance requires the `Detailed` output to name the passing case whose `It` name contains
**"returns no selection when offered only netstandard2.1"**. It is present and passing:

```
   [+] returns no selection when offered only netstandard2.1
```

That case is what makes a merely-demoted framework fail. A demotion leaves the framework in the
ordered collection, so when it is the only candidate a ranking still returns it; only an outright
exclusion returns nothing. The case is therefore the discriminator between the two designs, and it
is the one AC7's "Fails if" clause names.

## Both halves of AC7, and where each is asserted

| AC7 clause | Asserted in | Cases |
|---|---|---|
| the selector returns `net481`, `net48`, `netstandard2.0`, and no selection for the three unconsumable sets | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | the 6 selector cases above |
| `scripts/vscode/Sync-PackageReferences.ps1` resolves the same selection through the shared module | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | the 4 parity cases, each comparing the script's answer with the module's over the same offered set |
| and declares no framework ordering of its own | the same suite | the fifth `AC7-` case, over an asset set where the deleted `$tfmPreference` array would have returned `netstandard2.1` and the correct answer is no selection, with a positive `net472` control alongside |

## Coverage of the two files under test, from this run's document

Read from `coverage/p3-t6-ac7-coverage.xml`, which lies under `coverage/` and is ignored by
`.gitignore:144`.

| `sourcefile` | Covered | Missed | LINE percent |
|---|---|---|---|
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 100.00 |
| `vscode/Sync-PackageReferences.ps1` | 95 | 32 | 74.80 |

Recorded as context, not asserted at this task. Both files' coverage clauses are asserted at
P4-T3, where the module's floor is 90 and the script's requirement is strictly greater than its
P0-T18 baseline of 0 covered of 84 lines. The 95 covered here already satisfies that direction,
and the figure will be re-measured by P4-T3's own full-suite run.

This task records no aggregate JaCoCo LINE figure and is therefore not one of the six tasks the
gate rule 12 standing-in obligation falls on.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | 0 | PASS |
| `Total` | at least 13 | 14 | PASS |
| The `Detailed` output names the passing "returns no selection when offered only netstandard2.1" case | present and passing | present, `[+]` | PASS |

The `Failed=0` is guarded by `Total=14`: a run that discovered no test would report `Total=0` and
fail the at-least-13 clause rather than passing on an empty set.

## Acceptance criterion checked off

**AC7 — The incompatible framework is excluded, not ranked (#902)** is checked off in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.

Output Summary: CMD-PESTER-ALL over
`tests/scripts/dependencies/PackageCompatibility.Tests.ps1` and
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` returned EXIT_CODE 0 with
`PESTER Passed=14 Failed=0 Skipped=0 Total=14`, against a required minimum of 13. All fourteen
cases are named in the `Detailed` output and all pass, including the discriminating case
`returns no selection when offered only netstandard2.1`, which a merely-demoted framework would
fail. The four script-side parity cases assert the rewritten
`scripts/vscode/Sync-PackageReferences.ps1` returns exactly what the shared module returns for
each AC7 selection case, and the fifth shows the script carries no ordering of its own. Coverage
from this run: `PackageCompatibility.psm1` 33 of 33 lines at 100.00 percent,
`Sync-PackageReferences.ps1` 95 of 127 at 74.80 percent, both recorded as context. AC7 is checked
off in `spec.md`.
