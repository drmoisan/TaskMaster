# P1-T6 — PackageGraph suite run

Timestamp: 2026-09-19T12-46

Command:

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/PackageGraph.Tests.ps1"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p1-t6-packagegraph-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

CMD-PESTER-ALL with `$c.Run.Path` restricted to the single file the task names, per the task text.
The explicit `exit` is what gives the run a process exit code at all, per gate rule 4.

EXIT_CODE: 0

## Result line, verbatim

```
PESTER Passed=32 Failed=0 Skipped=0 Total=32
```

## Coverage figures read from `coverage/p1-t6-packagegraph-coverage.xml`

The `sourcefile` element named `PackageGraph.psm1`, its `counter` child with `type="LINE"`:

| Measure | Value |
|---|---|
| Covered lines | **164** |
| Missed lines | **0** |
| LINE percentage | **100.00** |
| Instructions | 217 covered, 0 missed |

Aggregate across the instrumented population, recorded as an observation only: LINE 164 covered,
871 missed, **15.85 percent**. That figure is low by construction and is not an acceptance clause
here: the run instruments both `scripts/dependencies` and `scripts/vscode` while executing only the
PackageGraph suite, so every `scripts/vscode` file reports 0 covered. The aggregate gate over the
full suite is P2-T3.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| `Failed=0` | 0 | PASS |
| `Total` greater than or equal to 8 | **32** | PASS |
| The `sourcefile` entry named `PackageGraph.psm1` reports LINE at least 90 | **100.00** | PASS |
| `Total=0` would be a failure, because it would mean discovery found no test | Total is 32, and the `Detailed` output names all 32 cases individually | PASS |

## Standing-in statement required by gate rule 12

This artifact records a JaCoCo LINE figure, so it is one of the six tasks that carry the
standing-in obligation — P0-T18, **P1-T6**, P2-T3, P4-T3, P6-T3 and P9-T3.

The figures recorded above stand in for a permitted committed-evidence form that **does not exist
for the PowerShell route**. The `## Committed Test Evidence Format` section of the authoritative
`CLAUDE.md` defines its three permitted forms against the C# route and its post-processed Cobertura
document: a package-level JaCoCo projection of that document, the one-line first-party coverage
summary, and a trx-derived test-result summary. A Pester run emits JaCoCo directly with no
Cobertura stage, and `ConvertTo-JacocoPackageProjection` accepts Cobertura only, so none of the
three can be produced here. The figures in this `.md` artifact are therefore a fourth form the
section does not define. The gap is stated rather than closed, because closing it would mean either
committing the prohibited collector document or building a Cobertura stage this change has no
reason to build.

The collector document itself is at `coverage/p1-t6-packagegraph-coverage.xml`, which
`.gitignore:144` ignores. No `.xml` is written under the evidence tree and none enters a commit
pathspec.

Output Summary: the PackageGraph suite runs green — `Passed=32 Failed=0 Skipped=0 Total=32`, exit
0 — and `PackageGraph.psm1` reports 164 covered lines of 164, **100.00 percent LINE**, against the
task's floor of 90. `Total=32` is well clear of the minimum of 8 and rules out the empty-discovery
failure the task names.
