# P2-T3 — GREEN run for RC-1, with the merged sibling's balance test

Timestamp: 2026-08-28T03-51
Task: [P2-T3]
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=rem1-p2-t3.trx" "/TestCaseFilter:FullyQualifiedName~UnwireIntentEvents" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing
EXIT_CODE: 0
ExpectedExitCode: 0

The filter `FullyQualifiedName~UnwireIntentEvents` selects every test whose fully qualified name
contains that substring. In this assembly there are exactly two: the test this remediation adds and
the merged sibling's balance test. Both ran.

This is the **pass-after** half of the fail-before / pass-after pair; P1-T3 is the fail-before half,
recorded against the identical test under the identical runner with only the production code
differing by one line.

## Result

TRX `Counters`, verbatim:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
disconnected="0" warning="0" completed="0" inProgress="0" pending="0"
```

Console summary: `Total tests: 2`, `Passed: 2`, `Test Run Successful.`

| Test | Owner | Outcome | Duration |
|---|---|---|---|
| `UnwireIntentEvents_DetachesPicturesChanged` | this remediation, in `QfcItemController.EventWiringTests.Part2.cs` | **Passed** | < 1 ms |
| `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | merged sibling 484, in `QfcItemController.EventWiringTests.cs:377` | **Passed** | 240 ms |

`failed="0"`.

## The RED-to-GREEN transition

| | P1-T3 (RED) | P2-T3 (GREEN) |
|---|---|---|
| Production `UnwireIntentEvents()` detachments | 16 | 17 |
| `UnwireIntentEvents_DetachesPicturesChanged` | **Failed** — `Moq.MockException`, removal 0 times where once expected | **Passed** |
| `EXIT_CODE` | 1 | 0 |

The only difference between the two runs is the single production line P2-T1 added. The test moved
from failing to passing because of that line and nothing else.

## Why the sibling test stays green unmodified

`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` was read in full at
`QfcItemController.EventWiringTests.cs:377-418` before this cycle began. It asserts sixteen
**individual** detachments, each through a local helper
`void Off(Action<IItemViewer> detach) => viewer.VerifyRemove(detach, Times.Once());`, and it pins
**no total**: the file contains **zero** occurrences of `VerifyNoOtherCalls` and the test declares no
aggregate count assertion. A seventeenth detachment therefore cannot break it — each of its sixteen
`Times.Once()` expectations is still satisfied exactly once, and nothing in the test observes the
existence of a seventeenth. That reasoning is now confirmed empirically: the test passed in this run
against the 17-detachment production code, unmodified.

The test is **not renamed and not edited**. Its name "AllSixteen" is now slightly stale for an
assertion set that remains true — it does detach all sixteen of those events — and that staleness is
deliberate: the name is a merged sibling's stable test node ID, and renaming it would churn a
sibling's identifier for no behavioural gain. The staleness is recorded in prose in the P3-T1 handoff
addendum and the P3-T2 spec amendment instead.

`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` (499/500 lines) is untouched by
this remediation; P4-T8 re-verifies its absence from the remediation diff.

## TRX hygiene

`FEATURE/evidence/regression-testing/rem1-p2-t3.trx`, sanitised in place, case-insensitively:

| Token | Replacement (XML entity form) | Occurrences |
|---|---|---:|
| worktree root | `&lt;repo-root&gt;` | 4 |
| machine name | `&lt;host&gt;` | 5 |
| account name | `&lt;user&gt;` | 3 |

Zero residual host tokens after sanitisation; zero raw-angle-bracket placeholders, so the document
stays parseable. Re-parsed with a strict XML reader: **parse succeeded**, **2** `UnitTestResult`
elements, matching the reported total of 2 exactly. The results directory contains zero `Deploy_*`
directories after the run.

## Acceptance

| P2-T3 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| 0 failed | **Yes** — `failed="0"` |
| TRX records Passed for `UnwireIntentEvents_DetachesPicturesChanged` | **Yes** |
| TRX records Passed for `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | **Yes** |
| TRX sanitised and strict-parsed | **Yes** — 0 residual host tokens, strict parse OK, 2 `UnitTestResult` matching the reported total |

Output Summary: GREEN confirmed. Both tests matching `FullyQualifiedName~UnwireIntentEvents` passed —
`total="2" executed="2" passed="2" failed="0"`, `EXIT_CODE: 0`, `Test Run Successful.` The new
`UnwireIntentEvents_DetachesPicturesChanged` moved from Failed in P1-T3 to **Passed** here, with the
single P2-T1 production line as the only intervening change, completing the fail-before / pass-after
pair. The merged sibling's `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` also **Passed**,
unmodified and unrenamed, confirming empirically what its source already showed: it pins sixteen
individual `Times.Once()` detachments and no total, with no `VerifyNoOtherCalls` anywhere in its file,
so a seventeenth detachment cannot disturb it. The TRX is sanitised, strict-parses, and reports
exactly 2 `UnitTestResult` elements.
