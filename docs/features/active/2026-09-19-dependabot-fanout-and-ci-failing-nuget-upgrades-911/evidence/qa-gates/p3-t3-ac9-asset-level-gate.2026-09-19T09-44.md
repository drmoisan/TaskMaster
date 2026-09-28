# P3-T3 — AC9: the compatibility gate is asset-level

Timestamp: 2026-09-19T23-52

Command — CMD-PESTER-ALL restricted to the compatibility suite, with `<OUTPATH>` set to
`coverage/p3-t3-compat-coverage.xml`:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/PackageCompatibility.Tests.ps1"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p3-t3-compat-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

Per gate rule 4, the explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` placed after
the count-emitting statement is what makes the exit code meaningful: `New-PesterConfiguration`
defaults `Run.Exit` to `$false`, so a bare Pester run exits 0 whatever the tests do.

## Verbatim result line

```
PESTER Passed=8 Failed=0 Skipped=0 Total=8
```

## `Detailed` output, all eight cases named

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
```

The two `AC9-` prefixed cases the acceptance names are both present by name and both passing:

| AC9 clause | `It` name in the `Detailed` output | Result |
|---|---|---|
| the rejection carrying a reason string | `AC9- returns a rejection carrying a non-empty reason when only unconsumable frameworks are offered` | `[+]` passing |
| the acceptance naming the selected asset folder | `AC9- returns an acceptance naming the selected asset folder when a consumable asset is present` | `[+]` passing |

## Why this is asset-level rather than attribute-level

Both `AC9-` cases drive `Test-PackageAssetCompatibility` with an in-memory array standing in for
the folder names a candidate package ships under its library directory. Neither case supplies a
declared `targetFramework` attribute, and the module reads none: it has no manifest parser, no
project-file parser and no filesystem access at all. The rejection case offers
`netstandard2.1`, `net6.0` and `netcoreapp3.1` — three frameworks `net481` cannot load — and the
acceptance case offers the same first two plus `net472`. The only difference between the two
inputs is the presence of one consumable **asset folder**, and that difference alone flips the
decision, which is what makes the gate asset-level as a matter of observed behaviour rather than
of description.

## Coverage of the module under test, from this run's document

Read from `coverage/p3-t3-compat-coverage.xml`, which lies under `coverage/` and is ignored by
`.gitignore:144`. No `.xml` is written under the evidence tree.

| `sourcefile` | Covered | Missed | LINE percent |
|---|---|---|---|
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 100.00 |

Recorded as context, not asserted at this task; the module's coverage floor is asserted at P4-T3.

A mechanical note for the later tasks that read these documents: with the two-member
`CodeCoverage.Path` the `sourcefile` `name` attribute carries a package-directory prefix —
`dependencies/PackageCompatibility.psm1`, `vscode/Sync-PackageReferences.ps1` — whereas the
single-member Phase 0 baseline emitted bare leaf names. Selection by leaf name is therefore the
form that works against both documents. The document enumerates 16 `sourcefile` entries.

This figure is recorded in this `.md` artifact and stands in for a permitted evidence form that
does not exist for the PowerShell route, per gate rule 12 — but note that this task is not one of
the six the standing-in obligation falls on, since it records no aggregate JaCoCo LINE figure; the
per-file value above is contextual.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | 0 | PASS |
| `Total` | 8 | 8 | PASS |
| The `AC9-` rejection case present by name in `Detailed` | present and passing | present, `[+]` | PASS |
| The `AC9-` acceptance case present by name in `Detailed` | present and passing | present, `[+]` | PASS |

The `Failed=0` is not an unguarded absence: `Total=8` is the positive companion, so a run that
discovered no test would report `Total=0` and fail the exact-8 clause rather than passing on an
empty set.

## Acceptance criterion checked off

**AC9 — The compatibility gate is asset-level** is checked off in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.

Output Summary: CMD-PESTER-ALL restricted to
`tests/scripts/dependencies/PackageCompatibility.Tests.ps1` returned EXIT_CODE 0 with
`PESTER Passed=8 Failed=0 Skipped=0 Total=8`. All eight cases are named in the `Detailed` output
and all pass, including both `AC9-` prefixed cases: the gate returns a rejection carrying a
non-empty reason naming the package when offered only `netstandard2.1`, `net6.0` and
`netcoreapp3.1`, and returns an acceptance naming `net472` when that one consumable asset folder is
added to the same set. The decision is taken from asset folder names alone; the module reads no
declared framework attribute and touches no filesystem. `PackageCompatibility.psm1` reports 33
covered of 33 lines, 100.00 percent. AC9 is checked off in `spec.md`.
