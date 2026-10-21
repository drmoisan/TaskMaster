# QA Gate — Cobertura Coverage Headline, Final (P2-T6)

Timestamp: 2026-09-01T12-59

Referenced artifact: `evidence/qa-gates/final-coverage.cobertura.xml`

## Discovery of the .coverage Input

Command:
`Get-ChildItem -Path TestResults -Filter *.coverage -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1`
EXIT_CODE: 0
Selected input `LastWriteTime`: `2026-09-01T12:27:38.3748288-04:00`, which matches the P2-T5
run and is distinct from the P0-T10 baseline input (`12:14:55`), confirming the correct,
newer artifact was selected rather than the baseline being re-read.

## Conversion

Command:
`dotnet-coverage merge -f cobertura -o docs\features\active\2026-08-27-qfc-metrics-flush-writes-empty-session-file-646\evidence\qa-gates\final-coverage.cobertura.xml <the-located-coverage-file>`
EXIT_CODE: 0
Tool version reported: `dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.11]` — the same
version used for the baseline conversion, so baseline and final are directly comparable.
Output: `Merged into file ...\evidence\qa-gates\final-coverage.cobertura.xml.`

## Final Coverage Headline (verbatim root `<coverage>` attribute values)

| Attribute | Value |
|---|---|
| `line-rate` | `0.3405230596175478` |
| `branch-rate` | `1` |
| `lines-covered` | `48436` |
| `lines-valid` | `142240` |

`line-rate` is a numeric string, not a placeholder, satisfying the task acceptance
condition. As a percentage the final repository-wide figure is **34.05%**.

The same two qualifications recorded at baseline still apply and are unchanged: the
denominator includes vendored and third-party assemblies loaded by the test run and is not
the first-party testable denominator `CLAUDE.md` UT2 defines its floor against; and
`branch-rate="1"` carries no `branches-covered` or `branches-valid` attributes at all, so no
branch data was emitted and the value is not interpretable.

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| The `.cobertura.xml` artifact exists | yes | yes, 26,067,082 bytes, reparsed successfully as `[xml]` | Yes |
| Its root `line-rate` is a numeric string, not a placeholder | yes | `0.3405230596175478` | Yes |

ACCEPTANCE: MET.

## Sanitisation Micro-Action Recorded

As with the baseline artifact, `dotnet-coverage` wrote absolute paths into every `<class
filename="...">` attribute. The absolute worktree prefix was removed by literal string
replacement, rendering all paths repository-relative.

| Check | Result |
|---|---|
| Occurrences of the absolute worktree prefix replaced | 3255 |
| Residual occurrences of the account name | 0 |
| Residual occurrences of the machine name | 0 |
| Residual occurrences of `C:\Users` | 0 |
| XML still well-formed after replacement | Yes (reparsed as `[xml]`) |
| Root `line-rate` after replacement | `0.3405230596175478` (unchanged) |

No angle-bracket placeholder was substituted into any XML attribute; the prefix was removed
rather than replaced with a token.

The occurrence count rose from 3253 at baseline to 3255 here. Both added occurrences are
`<class>` elements for the same two compiler-generated state-machine types that the guard's
early `return;` introduced into `QfcHomeController.Metrics.cs`'s coverage output; they are a
consequence of the change, not of the sanitisation.

## Output Summary

Final repository-wide `line-rate` is `0.3405230596175478` (34.05%) over 48,436 of 142,240
lines, produced by the same tool version and the same method as the baseline. The artifact
exists, parses, reports a numeric `line-rate`, and contains no absolute host path. The
baseline-to-final delta and the per-line hit counts for the four new guard lines are
evaluated in P2-T7.
