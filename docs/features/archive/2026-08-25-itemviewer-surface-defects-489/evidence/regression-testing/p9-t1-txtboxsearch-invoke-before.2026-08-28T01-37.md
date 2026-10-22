# P9-T1 — Pre-change occurrence count of `TxtboxSearch.Invoke`

Timestamp: 2026-08-28T01-37
Command: git grep -F -n "TxtboxSearch.Invoke" -- QuickFiler/Viewers/
EXIT_CODE: 0

## Verbatim output

```
QuickFiler/Viewers/ItemViewer.FolderSearch.cs:79:        public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));
```

MatchCount: 1
MatchFile: QuickFiler/Viewers/ItemViewer.FolderSearch.cs
ObservedLineNumber: 79 (recorded for the record only; no line number is asserted by this task)

## Why the exit code is 0 here

A bare `git grep` exits `1` on a zero-match result and `0` when it finds at least one match. This
task is the **fail-before** record and expects exactly one match, so the native exit code is `0` and
no `ExpectedExitCode:` declaration is required. The paired **after** gate, P9-T8, expects zero
matches and therefore wraps the same grep in `(... | Measure-Object).Count`.

## Relationship to the printed line number

`plan.2026-08-25T01-04.md` records that this occurrence stood at `:79` at `BASELINE_SHA`
(`cecd78130a489fcfdc2ddac7970f344256f4a75a`) and that P8-T5's rename inside the same file at `:20`
is token-for-token line-neutral (`Set` to `Add`, both members 14 characters), so the occurrence still
reports at `:79` when this task runs. The observation above confirms that prediction, but the
acceptance condition for this task is the match **count** and the match **file**, not the line
number.

## Fail-before role

This is the fail-before record for the AC31 zero-match assertion. The literal is present exactly once
before P9-T2 runs, so the P9-T8 zero-match assertion is falsifiable: had P9-T2 not changed the file,
P9-T8 would record `1` and fail.

Output Summary: `git grep -F -n "TxtboxSearch.Invoke" -- QuickFiler/Viewers/` returned **exactly 1
match**, in `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, observed at line `79`, with
`EXIT_CODE: 0`. Acceptance met: one match, and it is in `ItemViewer.FolderSearch.cs`. This is the
fail-before baseline against which P9-T8 asserts zero.
