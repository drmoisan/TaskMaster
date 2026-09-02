# Baseline — Cobertura Coverage Headline (P0-T11)

Timestamp: 2026-09-01T12-18

Working directory: repository root (worktree for branch
`bug/qfc-metrics-flush-writes-empty-session-file-646`)
HEAD: `8a2054cd6c857195712c7db6cee0a34b631f3ca7`

Referenced artifact: `evidence/baseline/baseline-coverage.cobertura.xml`

## Discovery of the .coverage Input

Command:
`Get-ChildItem -Path TestResults -Filter *.coverage -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1`
EXIT_CODE: 0
Selected input `LastWriteTime`: `2026-09-01T12:14:55.1096677-04:00`, which matches the
P0-T10 run. The file resides under a GUID-named subdirectory of `TestResults/` and its name
is machine- and account-derived, so it is identified here by timestamp rather than by
literal name.

## Conversion

Command:
`dotnet-coverage merge -f cobertura -o docs\features\active\2026-08-27-qfc-metrics-flush-writes-empty-session-file-646\evidence\baseline\baseline-coverage.cobertura.xml <the-located-coverage-file>`
EXIT_CODE: 0
Tool version reported: `dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.11]`
Output: `Merged into file ...\evidence\baseline\baseline-coverage.cobertura.xml.`

## Baseline Coverage Headline (verbatim root `<coverage>` attribute values)

| Attribute | Value |
|---|---|
| `line-rate` | `0.3404862683334974` |
| `branch-rate` | `1` |
| `lines-covered` | `48426` |
| `lines-valid` | `142226` |

`line-rate` is a numeric string, not a placeholder, satisfying the task acceptance
condition. As a percentage the baseline repository-wide figure is **34.05%**.

Two qualifications on that figure, both recorded rather than resolved:

1. The denominator is every assembly the `QuickFiler.Test` run loaded, including vendored
   and third-party code (for example `Mono.Reflection`, whose source paths are not in this
   repository at all). It is therefore not the "first-party testable denominator" that
   `CLAUDE.md` UT2 defines its >= 80% floor against, and it is not directly comparable to
   that floor.
2. `branch-rate="1"` is not a meaningful 100% branch result. The root element carries no
   `branches-covered` or `branches-valid` attributes at all (both read empty), so this
   converter emitted no branch data for this run and the value cannot be interpreted.

Per the plan's Coverage Policy Note, the repository-wide percentage is treated as a
recorded, non-blocking figure. The blocking gate is P2-T7: no regression in this same
`line-rate`, measured the same way, plus `hits > 0` on each of the four guard lines added
by P1-T5.

## Baseline Detail for the File Under Change

`QuickFiler/Controllers/QfcHomeController.Metrics.cs` appears as three `<class>` elements
(the type plus two compiler-generated nested types):

| `<class name=...>` | `line-rate` |
|---|---|
| `QuickFiler.Controllers.QfcHomeController` | `0.6986301369863014` |
| `QuickFiler.Controllers.QfcHomeController.<>c` | `1` |
| `QuickFiler.Controllers.QfcHomeController.<WriteMetricsAsync>d__103` | `0.8775510204081632` |

Taking the union of `<line>` entries across all three elements and deduplicating by line
number (keeping the maximum `hits` per line, since the async state machine reports some
lines under more than one element), the baseline for this file is **94 of 122 distinct
lines covered = 77.05%**.

Baseline hit counts at the two edit anchors and at the region the plan declares off-limits:

| Line | Role | `hits` |
|---|---|---|
| 174 | Anchor A — `var lines = strOutput.Where(...).ToArray();` | 1 |
| 179 | Anchor B — `bool metricsWritten = await MetricsFileWriter(` | 1 |
| 185 | `if (!metricsWritten)` — the #647 failure branch condition | 1 |
| 186-191 | body of the #647 failure branch | 0 |
| 192 | method close | 1 |

This independently corroborates the plan's cited anchor line numbers against the current
tree: both anchors exist and are executed by the existing suite.

## Sanitisation Micro-Action Recorded

`dotnet-coverage` writes each `<class filename="...">` attribute as an absolute path. The
generated file contained 3253 occurrences of the absolute worktree prefix, which may not
appear in a committed artifact. A literal string replacement removed that prefix, rendering
every path repository-relative (for example
`QuickFiler\Controllers\QfcHomeController.Metrics.cs`).

| Check | Result |
|---|---|
| Occurrences of the absolute worktree prefix replaced | 3253 |
| Residual occurrences of the account name | 0 |
| Residual occurrences of the machine name | 0 |
| Residual occurrences of `C:\Users` | 0 |
| XML still well-formed after replacement | Yes (reparsed as `[xml]`) |
| Root `line-rate` after replacement | `0.3404862683334974` (unchanged) |
| File size | 26,064,187 bytes |

No angle-bracket placeholder was substituted into any XML attribute; the prefix was removed
rather than replaced with a token, so no attribute value was made ill-formed.

## Output Summary

Baseline repository-wide `line-rate` is `0.3404862683334974` (34.05%) over 48,426 of
142,226 lines; `branch-rate` is `1` but carries no branch counts and is not interpretable.
`QuickFiler/Controllers/QfcHomeController.Metrics.cs` is at 94/122 distinct lines (77.05%)
with both P1-T1 edit anchors executed. The artifact exists, parses, and reports a numeric
`line-rate`.
