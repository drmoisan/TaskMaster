# P4-T6 — QuickFiler.Test run (second pass, pass-after)

Timestamp: 2026-09-03T22-07

Command:
```text
if [ -f docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md ]; then echo "FAIL_BEFORE_PRESERVATION: FOUND IN PLACE, LEFT UNTOUCHED"; else cp docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md; echo "FAIL_BEFORE_PRESERVATION: CREATED FROM THE FIRST-PASS ARTIFACT"; fi
grep -n -F 'Failed: 8' docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t6 /TestCaseFilter:TestCategory!=LiveOutlook
```

The first two commands ran from the worktree root, as the first action of this task and before the
test run. The test command's flag set is identical to P0-T11's; the two differ only in the
`/ResultsDirectory` value.

EXIT_CODE:
- preservation conditional — 0
- `grep -n -F 'Failed: 8' ...` — 0
- `vstest.console.exe ...` — 0

Aggregate EXIT_CODE: 0

## Output Summary

### FAIL_BEFORE_PRESERVATION:

Branch line printed by the conditional, verbatim:

```text
FAIL_BEFORE_PRESERVATION: CREATED FROM THE FIRST-PASS ARTIFACT
```

The destination `evidence/regression-testing/p4-t6-first-pass-failure.md` did not exist, so the
create branch ran and the first-pass artifact was copied to it before this task overwrote
`evidence/qa-gates/p4-t6-quickfiler-tests.md`.

Verification `grep` output, verbatim:

```text
23:     Failed: 8
30:- **Failed: 8** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
105:| Fixed (P2-T1 applied) | `Total tests: 8` | **Failed: 8** |
```

The preserved file carries `Failed: 8` on three lines and the `grep` exited 0, so the fail-before
figure is verified against the file rather than assumed. No BLOCKED condition applies.

### Test counts

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 1312
     Passed: 1312
```

- **Total tests: 1312** (console summary block)
- **Passed: 1312** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p4-t6/` by this task's own `/Logger:trx` switch)
- **Skipped: 0** (derived as `total` minus `executed`)

TRX `<Counters .../>` values used for the derivation:

- `total` = **1312**
- `executed` = **1312**
- `failed` = **0**
- derived `Skipped` = 1312 - 1312 = **0**

The `notExecuted` attribute was NOT used, per constraint 5 of "Shell constraints measured in this
worktree".

TRX SELECTED: most recently modified .trx in TestResults/p4-t6/
Last-modified timestamp of the selected file: `2026-09-03 22:06:59.182210700 -0400`.
That directory held two `.trx` files at the moment this artifact was written (the one written by the
first, failing Phase 4 pass on 2026-09-03 and the one this second pass produced), so the TRX
selection rule stated after constraint 5 applies. The selected file's own name is deliberately not
recorded, and the run's `Results File:` console line is deliberately not quoted.

FAILING_TEST_SET: empty.

### The eight `EmailMoveMonitorTests` methods, console pass lines verbatim

```text
  Passed HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe [143 ms]
  Passed UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem [4 ms]
  Passed UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation [1 ms]
  Passed UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry [< 1 ms]
  Passed AllComAccess_FlowsThroughInjectedMarshalDelegate [< 1 ms]
  Passed UnhookAll_UnsubscribesEveryFolder_AndClearsState [< 1 ms]
  Passed DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe [4 ms]
  Passed UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread [1 ms]
```

These are exactly the eight methods the first pass of this task reported as failing, quoted from the
preserved first-pass copy at `evidence/regression-testing/p4-t6-first-pass-failure.md`. This block is
therefore the direct pass-after counterpart of that recorded fail-before.

## Acceptance

Satisfied on all five clauses:

1. `Total tests` is **1312**, equal to the baseline `Total tests` recorded in P0-T11. No test was
   added to or removed from this assembly, and the filter is unchanged.
2. `Passed` is **1312** and the failing-test set is EMPTY.
3. `Failed` is **0**, read from the `failed` attribute of the single `<Counters .../>` element in the
   TRX this task wrote under `TestResults/p4-t6/`.
4. The `total` (1312) and `executed` (1312) values from which `Skipped` was derived are recorded, and
   the derived `Skipped` is **0**.
5. All eight `EmailMoveMonitorTests` methods are listed by name as passing in the console output, as
   quoted verbatim above.
