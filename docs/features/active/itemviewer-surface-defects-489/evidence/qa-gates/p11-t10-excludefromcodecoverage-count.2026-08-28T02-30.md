# P11-T10 — Repository-wide recount of the coverage-exclusion attribute

Timestamp: 2026-08-28T02-30
Command: git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs" | Measure-Object | Select-Object -ExpandProperty Count
EXIT_CODE: 0

FinalExcludeAttributeCount: 261

Loop iteration: **1**.

## The command is P0-T16's, verbatim

The pattern is character-for-character the one P0-T16 ran, including the
`(System\.Diagnostics\.CodeAnalysis\.)?` alternation. It was not simplified, and no fixed-string
`-F` recount of the unqualified spelling was substituted.

That alternation is load-bearing, and this run measures why:

```
fully-qualified spelling  [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]  =  12
unqualified spelling      [ExcludeFromCodeCoverage]                                  = 249
alternation (the gate)                                                               = 261
```

12 + 249 = 261. A fixed-string recount of the unqualified spelling alone would report 249 and would
be blind to the twelve fully-qualified applications. Five of those twelve are in
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — confirmed by re-running the same
alternation pattern scoped to that one path, which returns **5**. That is the file P2-T5 adds a
member to, so a `-F` recount could not move whatever this feature wrote there and AC56 would have
been unfalsifiable.

## Acceptance

**`FinalExcludeAttributeCount:` is not greater than `BaselineExcludeAttributeCount:`.**

```
Baseline (P0-T16, evidence/baseline/phase0-excludefromcodecoverage-count.2026-08-27T23-31.md) = 261
Final                                                                                          = 261
261 is not greater than 261.   SATISFIED
```

The count is not merely within bounds but **unchanged**. This feature added no coverage-exclusion
attribute and removed none. In particular the one new production member it introduces,
`CbxPictures_CheckedChanged` in `QfcItemController.EventHandlers.cs`, carries **no** exclusion
attribute — which is consistent with P11-T9 measuring it at `NewMemberLineRate: 1.0`, a figure that
could not exist if the member were excluded from measurement.

## Exit-code accounting

The pipeline wraps `git grep` in `(… | Measure-Object …)`. Under `$ErrorActionPreference = 'Stop'`
the pipeline completed with the automatic success variable `True` and `$Error.Count` at `0`, and
`$LASTEXITCODE` was `0`. `EXIT_CODE: 0` is recorded on the basis of the success variable and the zero
error count.

A caveat is recorded explicitly rather than left implicit: the `Measure-Object`-`Count` wrapper does
**not** reliably force `$LASTEXITCODE` to `0`. On a **zero-match** result `git grep` exits `1` and
that value survives into `$LASTEXITCODE` even though `Count` is `0`, the success variable is `True`
and the error count is `0`. The `$LASTEXITCODE` of `0` observed here is therefore a consequence of
this search matching 261 lines, not of the wrapper neutralising the exit code. The success judgement
above does not depend on it.

Output Summary: The exclusion-attribute gate **passes**. `FinalExcludeAttributeCount: 261` is **not
greater than** — in fact equal to — `BaselineExcludeAttributeCount: 261`, so this feature added no
`[ExcludeFromCodeCoverage]` attribute anywhere in the repository. The command was P0-T16's verbatim,
alternation included; the alternation is demonstrably necessary, since the fully-qualified spelling
accounts for 12 of the 261 occurrences and 5 of those 12 sit in the very file this feature adds a
member to. `EXIT_CODE: 0` is recorded from the success variable and a zero `$Error.Count`;
`$LASTEXITCODE` was `0` here because the search matched, not because the `Measure-Object` wrapper
neutralises it — on a zero-match result it would have been `1`.
