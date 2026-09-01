# Post-change coverage denominator classification (P7-T7)

Timestamp: 2026-09-01T11-11
Task: [P7-T7]
Working directory: WORKTREE
Source file read: `coverage\post-change.cobertura.xml`, produced by the clean P7-T6 run.

Command: `pwsh -NoProfile -File <scratchpad>/covdenom.ps1 -Xml coverage/post-change.cobertura.xml`
EXIT_CODE: 0

This records the same field set that P0-T11 recorded for the baseline file, so the two are comparable
field by field.

## Root `coverage` element attributes

| Attribute | Value |
|---|---|
| `line-rate` | 0.85391 |
| `lines-covered` | 54973 |
| `lines-valid` | 64378 |
| `branch-rate` | 0.794014 |
| `branches-covered` | 13106 |
| `branches-valid` | 16506 |

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

Package count: 9. The list is identical to the baseline's, in both membership and order.

## Classification

All nine package names are first-party project assembly names. No vendored third-party assembly appears
— neither `log4net` nor `Mono.Reflection`, and no other non-project name. The `sources` element that
`ConvertTo-KoverageCoberturaXml` injects is present.

DENOMINATOR: FILTERED

## Post-change line coverage percentage

`line-rate` 0.85391 multiplied by 100 = **85.39** percent (two decimal places).

Branch coverage, recorded for completeness: 0.794014 multiplied by 100 = 79.40 percent.

Output Summary: The post-change Cobertura file is the post-processed, first-party-filtered artifact, and
carries the same nine-package denominator as the baseline. Post-change repository-wide line coverage is
85.39 percent over 64378 valid lines with 54973 covered.

Both this file and the baseline classify as `FILTERED`, so the P7-T9 comparison is between two figures
taken from green runs over the same filtered denominator and is a real comparison rather than a phantom
one. The `UNFILTERED` branch, under which P7-T6 would have had to be resolved first, was not taken.
