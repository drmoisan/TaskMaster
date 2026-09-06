# Baseline — #584 Specification Re-derivation (P0-T9, SD11 item 1, AC12)

Timestamp: 2026-09-05T19-36

Command:

```text
Read docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md lines 1-15
Search the same file for lines matching the pattern ^- \[[ x]\] AC
```

EXIT_CODE: 0

Output Summary:

## Status line, verbatim

The `- **Status:**` value spans lines 7 to 11 of the file. Its five lines are quoted verbatim:

```text
- **Status:** Draft (amended in plan revision round 15: write set and AC4 extended to a sixth file;
  amended in plan revision round 16: AC5 returned to unchecked pending the sixth file's token-filter
  artifact; amended in plan revision round 17 (preflight round 17 non-blocking findings N1-N4
  applied), of which finding N4 is the only one touching this file: AC5's Evidence line now states the
  diff's added-line figure as the artifact records it)
```

The value begins with the token `Draft`, as the acceptance condition requires.

## Version line, verbatim

```text
- **Version:** 0.5
```

Version is `0.5`.

## Acceptance-criteria lines

The search for `^- \[[ x]\] AC` returned exactly seven lines. Each is quoted verbatim with its line
number:

| Line | Text |
|---|---|
| 261 | ``- [x] AC1: `UiThread.Dispatcher` throws a named `InvalidOperationException` (not a bare`` |
| 271 | ``- [x] AC2: The `null!` null-forgiving suppression on `UiThread`'s `_dispatcher` backing field is`` |
| 279 | `- [x] AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left unmodified unless the` |
| 288 | ``- [x] AC4: No regression in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,`` |
| 316 | `- [x] AC5: No retry, sleep, or timing tolerance is introduced anywhere in the diff.` |
| 339 | `- [x] AC6: Full C# toolchain (csharpier -> analyzer msbuild -> nullable msbuild -> vstest with` |
| 352 | `- [x] AC7: Repository-wide line coverage does not regress relative to the recorded baseline, and` |

Several criteria wrap onto continuation lines in the source; the text above is the matched line
itself, which is the unit the search operates on.

## Result

All seven acceptance criteria carry `[x]`. Version is `0.5`. The Status value begins with the token
`Draft`. The observed state is the all-seven-checked state, so the P5-T1 task amends only the Status
line and edits no checkbox. Any subsequent task asserting the #584 acceptance-criteria state cites
this artifact.
