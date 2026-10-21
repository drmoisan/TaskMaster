# Baseline coverage denominator classification (P0-T11)

Timestamp: 2026-09-01T10-34
Task: [P0-T11]
Working directory: WORKTREE
Source file read: `coverage\baseline.cobertura.xml`, produced by P0-T10.

Command: `pwsh -NoProfile -File <scratchpad>/covdenom.ps1 -Xml coverage/baseline.cobertura.xml`
EXIT_CODE: 0

## Root `coverage` element attributes

| Attribute | Value |
|---|---|
| `line-rate` | 0.853172 |
| `lines-covered` | 54882 |
| `lines-valid` | 64327 |
| `branch-rate` | 0.793172 |
| `branches-covered` | 13081 |
| `branches-valid` | 16492 |

`sources` element present: **yes** (one `source` child).

## Sorted list of every `package` element `name` attribute value

1. `QuickFiler`
2. `SVGControl`
3. `Tags`
4. `TaskMaster`
5. `TaskTree`
6. `TaskVisualization`
7. `ToDoModel`
8. `UtilitiesCS`
9. `VBFunctions`

Package count: 9.

## Classification

Every one of the nine package names is a first-party project assembly name in this solution. No vendored
third-party assembly appears in the list — specifically, neither `log4net` nor `Mono.Reflection` is
present, and no other non-project name is present either. The `sources` element that
`ConvertTo-KoverageCoberturaXml` injects is present. Both observations agree with the P0-T10 signal that
the wrapper reached `Set-Content` at script line 343.

DENOMINATOR: FILTERED

## Baseline line coverage percentage

`line-rate` 0.853172 multiplied by 100 = **85.32** percent (two decimal places).

Branch coverage, recorded for completeness: 0.793172 multiplied by 100 = 79.32 percent.

Output Summary: The baseline Cobertura file is the post-processed, first-party-filtered artifact.
Baseline repository-wide line coverage is 85.32 percent over a denominator of 64327 valid lines with
54882 covered. The `REMEDIATION-REQUIRED` branch was not taken: the run was green and the filtered line
rate is above the 80 percent threshold that `Assert-CoberturaLineCoverageThreshold` enforces, so a
filtered baseline exists and the P7-T9 comparison and AC20 are reachable. This figure is comparable only
to a post-change figure that P7-T7 also classifies as `FILTERED`.
