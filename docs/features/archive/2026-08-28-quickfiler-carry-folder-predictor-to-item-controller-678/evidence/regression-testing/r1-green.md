# R1 — Green run, with the three pins the fix must not break

- Timestamp: 2026-09-02T01-20
- Issue: #678
- Task: [P1-T5]

Command (Derivation D7):

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary|FullyQualifiedName~RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue|FullyQualifiedName~ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler|FullyQualifiedName~ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull)" /Logger:trx "/ResultsDirectory:TestResults\p1-t5"
```

EXIT_CODE: 0

## Clause 1 — the pre-run build exits 0

`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` → exit **0**.

## Clause 2 — exactly 4 tests discovered and executed, each named individually

```
A total of 1 test files matched the specified pattern.
Total tests: 4
```

Each of the four filter names appears individually in the run's executed-test list:

```
  Passed ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler [174 ms]
  Passed ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull [< 1 ms]
  Passed RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary [335 ms]
  Passed RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue [24 ms]
```

## Clause 3 — all 4 pass

```
Test Run Successful.
Total tests: 4
```

No `Failed:` line appears and the header is `Test Run Successful.`, so the failed count is 0.

The R1 regression test that failed at P1-T2 with
`Expected loaded[0].MailItem to refer to Mock<MailItem:2>.Object ... but found
Mock<MailItem:1>.Object` now passes unmodified. Only production code changed between the two
runs; the test body is byte-identical to the one P1-T2 executed.

## Clause 4 — the three pre-existing tests pass with their bodies unmodified

Command:

```
git status --porcelain -- QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs
```

Output: **empty** (no output at all).

At this point in the plan that is conclusive proof the two files are untouched by this cycle,
because P1-T14 is the first commit this cycle makes and has not yet run, so any modification
would still be uncommitted and would appear in porcelain status.

A base-ref-anchored diff cannot serve here: the previous cycle rewrote both
`QfcHomeControllerRunAsyncHighConfidenceTests.cs` and `QfcQueuePurePathsTests.cs` relative to
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, so an anchored diff is non-empty regardless of
what this cycle does.

## What the three pins establish

- `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` builds its carrier from
  a `Mock<MailItem>` with no `EntryID` setup, so `EntryID` is null. Its passing confirms that
  reference identity is tried before `EntryID` in `QfcPreScoredItem.ResolveCarrier`: an
  `EntryID`-first matcher would strand that item's handler and break the assertion at
  `:228-240` of that file.
- `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull` carries five negative cases. Its
  passing confirms all five still return null after the delegation rewrite: two exit at the
  `preScored is null || preScored.Count == 0` guard, one at the null-mail-item guard, and the
  remaining two pass distinct mock instances so the added reference check does not fire, with
  the null-`EntryID` probe skipped by the retained `!string.IsNullOrEmpty(entryId)` clause
  rather than matched against the carrier's own null.
- `ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler` confirms the
  positive `EntryID` case still matches through the delegated body.

## Output Summary

Pre-run build exit 0. Scoped run discovered and executed exactly 4 tests, named all four
individually, and all 4 passed; run exit code 0. `git status --porcelain` over the two pinned
test files produced no output, so neither file was modified by this cycle.
