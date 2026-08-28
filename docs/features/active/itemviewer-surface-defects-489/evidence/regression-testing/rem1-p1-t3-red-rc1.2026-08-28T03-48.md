# P1-T3 [expect-fail] — RED run for RC-1

Timestamp: 2026-08-28T03-48
Task: [P1-T3]
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=rem1-p1-t3.trx" "/TestCaseFilter:FullyQualifiedName~UnwireIntentEvents_DetachesPicturesChanged" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing
EXIT_CODE: 1
ExpectedExitCode: 1

`$vstest` is the single path returned by
`vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`;
the runner reports `VSTest version 18.9.0 (x64)`. `/InIsolation` is used so the Moq-based fixture runs
in a separate test host, matching the runner configuration every other test gate on this branch used.

This is the **fail-before** half of the fail-before / pass-after pair the Bugfix Workflow requires.
Production code is unchanged at this point: `UnwireIntentEvents()` still performs 16 detachments.

## (a) Exactly one test ran and it failed

TRX `Counters` element, verbatim:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
disconnected="0" warning="0" completed="0" inProgress="0" pending="0"
```

Console summary: `Total tests: 1`, `Failed: 1`, `Test Run Failed.` One test ran, one failed, zero
passed.

## (b) EXIT_CODE

Observed `1`. `vstest.console.exe` exits non-zero when an executed test fails, so a non-zero exit is
the expected outcome for this task and only for this task. `ExpectedExitCode: 1` is declared above.
This is the single `[expect-fail]` task in the plan.

## (c) The failure is the missing detachment, not an arrange or compile error

Required single-line tokens, both present in the recorded failure:

| Required token | Present |
|---|---|
| `Moq.MockException` | **Yes** |
| `PicturesChanged -=` | **Yes** |

Failure message as reported:

```
Test method QuickFiler.Controllers.Tests.QfcItemController_EventWiringTests.UnwireIntentEvents_DetachesPicturesChanged threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: v => v.PicturesChanged -= It.IsAny<EventHandler>()
```

`but was 0 times` against an expectation of once: the removal happened **zero** times where one was
expected. That is the missing detachment, stated by the mocking framework itself.

Three further facts rule out an arrange failure or a compile failure:

1. **The test compiled and executed.** P1-T2 built the solution with `0 Error(s)`, and the TRX records
   `executed="1"` with a real 279 ms duration and a stack trace terminating inside
   `Moq.Mock.VerifyRemove`. A compile error would have failed the build; an arrange error would have
   thrown before the assertion, from a different frame.
2. **The arrange worked.** Moq's `Performed invocations` list shows **17** `add_*` calls on the mock,
   ending with `IItemViewer.add_PicturesChanged(EventHandler)`. `WireIntentEvents()` ran to completion
   and subscribed all seventeen events, so the reflection-based field injection and the harness
   controller are both healthy.
3. **The unwire also ran, and detached exactly sixteen.** The same list shows 16 `remove_*` calls —
   `ConversationModeChanged`, `FlagTaskClicked`, `PopOutClicked`, `DeleteItemClicked`, `ReplyClicked`,
   `ReplyAllClicked`, `ForwardClicked`, `BodyDoubleClick`, `SearchTextChanged`, `FolderKeyDown`,
   `FolderSelectionChanged`, `WebViewInitializationCompleted`, `ConversationItemSelectionChanged`,
   `SearchKeyDown`, `EmailCopyChanged`, `AttachmentsChanged` — and **no** `remove_PicturesChanged`.

The mock's own invocation ledger therefore reproduces the RC-1 imbalance directly: 17 adds, 16
removes, and the one missing remove is exactly `PicturesChanged`. The test fails for the intended
reason and for no other.

## (d) TRX hygiene

`FEATURE/evidence/regression-testing/rem1-p1-t3.trx`.

Sanitised in place, case-insensitively, before being recorded:

| Token | Replacement (XML entity form) | Occurrences |
|---|---|---:|
| worktree root | `&lt;repo-root&gt;` | 3 |
| machine name | `&lt;host&gt;` | 4 |
| account name | `&lt;user&gt;` | 3 |

Placeholders are written in XML entity form, never as raw angle brackets, which would make the
document unparseable. A search for raw `<repo-root>`, `<host>` or `<user>` in the file returns **0**.
A post-sanitisation search for the worktree root, the main checkout root, the machine name and the
account name (long and 8.3 forms, case-insensitive) returns **0** residual occurrences. `computerName`
now reads `&lt;host&gt;`, which is the attribute vstest writes with the real machine name.

Re-parsed after sanitisation with a strict XML reader: **parse succeeded**, and the document contains
**1** `UnitTestResult` element, matching the reported total of 1 exactly.

The filtered run created a deploy directory named after the account
(`Deploy_<user> <timestamp>_<pid>`, containing an `In/<HOST>` and an `Out` subdirectory). It was
deleted from the results directory immediately after the run. Git does not track directories, so
`git status` would never have warned about it; its absence was confirmed by an explicit directory
listing that now returns zero `Deploy_*` entries.

## Acceptance

| P1-T3 condition | Result |
|---|---|
| (a) exactly 1 test ran, 1 failed, 0 passed | **Yes** — `total="1" executed="1" passed="0" failed="1"` |
| (b) `EXIT_CODE: 1` | **Yes** — observed `1`, declared as `ExpectedExitCode: 1` |
| (c) failure message carries `Moq.MockException` and `PicturesChanged -=`, removal 0 times where once expected | **Yes** — all three, corroborated by a 17-add / 16-remove invocation ledger with no `remove_PicturesChanged` |
| (d) TRX sanitised and strict-parses | **Yes** — 0 residual host tokens, entity-form placeholders, strict parse OK, 1 `UnitTestResult` matching the reported total |

Output Summary: RED confirmed. `UnwireIntentEvents_DetachesPicturesChanged` ran alone under the
`FullyQualifiedName~UnwireIntentEvents_DetachesPicturesChanged` filter and **failed**:
`total="1" executed="1" passed="0" failed="1"`, `EXIT_CODE: 1` with `ExpectedExitCode: 1` declared.
The failure is `Moq.MockException: Expected invocation on the mock once, but was 0 times:
v => v.PicturesChanged -= It.IsAny<EventHandler>()` — the removal never happened. Moq's performed-
invocation ledger corroborates the cause precisely: **17** `add_*` calls including
`add_PicturesChanged`, **16** `remove_*` calls, and no `remove_PicturesChanged`, which is RC-1's
17-versus-16 imbalance observed at runtime rather than inferred from a grep. The TRX is sanitised with
entity-form placeholders, strict-parses, and reports exactly 1 `UnitTestResult`; the account-named
deploy directory the filtered run created was deleted.
