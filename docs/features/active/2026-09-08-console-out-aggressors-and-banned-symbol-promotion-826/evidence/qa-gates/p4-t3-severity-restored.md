# RS0030 severity at `suggestion` in the tree, unconditionally (issue #826, [P4-T3])

Timestamp: 2026-09-09T19-23

CHANNEL: SARIF

The value above is copied from the [P4-T1] artifact. This gate runs unconditionally, whichever channel
was certified, and is what makes D7's channel-3 branch safe: raising a severity to measure it is
permitted, leaving it raised is not.

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
@(Select-String -LiteralPath ".editorconfig" -CaseSensitive -SimpleMatch "dotnet_diagnostic.RS0030.severity = suggestion").Count
@(Select-String -LiteralPath ".editorconfig" -CaseSensitive -SimpleMatch "dotnet_diagnostic.RS0030.severity = warning").Count
git diff $Base -- .editorconfig
```

with the changed lines then partitioned in-process: lines beginning with a single `+` or `-`, excluding
the `+++` and `---` file headers, counted for the `-SimpleMatch` token `dotnet_diagnostic.`.

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `dotnet_diagnostic.RS0030.severity = suggestion` count | 1 | 1 |
| `dotnet_diagnostic.RS0030.severity = warning` count | 0 | 0 |
| anchored-diff changed lines containing `dotnet_diagnostic.` | 0 | 0 |

Total changed lines in the anchored diff of `.editorconfig` against base
`dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`: 11, all of them comment lines in the `BannedApiAnalyzers`
block, none of them a severity assignment.

## Channel-3 was never entered

[P4-T1] certified `CHANNEL: SARIF`, the first channel in D7's order, because all three of its
RS0030-scoped control counts were at least 1. Channel 2 (detailed-verbosity file logger) and channel 3
(temporary severity raise) were therefore never attempted, and no severity value was mutated at any
point during this feature's execution. This gate confirms the end state independently of that history,
which is the point of running it unconditionally.

Output Summary: `.editorconfig` carries exactly one `dotnet_diagnostic.RS0030.severity = suggestion` line
and zero `= warning` lines, and the anchored diff contains zero changed lines mentioning
`dotnet_diagnostic.`. AC11's deliberate no-change decision holds against the whole of this feature's
work, not only against the item-3 edit.
