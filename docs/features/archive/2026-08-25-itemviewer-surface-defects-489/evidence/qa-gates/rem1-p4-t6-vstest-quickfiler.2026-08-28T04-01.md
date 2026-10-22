# P4-T6 — Scoped test gate with coverage, `QuickFiler.Test` (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T04-01
Task: [P4-T6]
LoopIteration: 1
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=rem1-p4-t6.trx" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates
EXIT_CODE: 0

FinalPassed: 1122
FinalFailed: 0
FinalSkipped: 0

`$vstest` is the single path returned by
`vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`;
the runner reports `VSTest version 18.9.0 (x64)`. This is the same command shape, including
`/EnableCodeCoverage` and `/InIsolation`, that produced the P0-T5 baseline figures, so the comparison
below is like-for-like.

## Result

TRX `Counters`, verbatim:

```
total="1122" executed="1122" passed="1122" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
disconnected="0" warning="0" completed="0" inProgress="0" pending="0"
```

Console summary: `Test Run Successful.`, `Total tests: 1122`, `Passed: 1122`, total time 12.44
seconds. `EXIT_CODE: 0`.

An outcome tally over the whole TRX returns `Passed` 1122 times and nothing else, apart from one
`outcome="Completed"` on the run-level element, which is the run status rather than a test outcome.
There is no `Failed`, `Skipped`, `NotExecuted`, `Inconclusive` or `Aborted` outcome anywhere in the
document.

## (c) The count is exactly baseline plus one

| | Value |
|---|---:|
| `BaselinePassed:` (P0-T5, from `p11-t7-vstest-quickfiler.2026-08-28T02-22.md`) | 1121 |
| Tests added by this remediation | 1 |
| Expected | 1122 |
| `FinalPassed:` observed | **1122** |

Exact. This remediation adds one test method and changes no other test in the assembly, so the total
must move by exactly one. It did. A larger figure would mean tests entered the assembly from somewhere
this cycle did not account for; a smaller one would mean a test was lost or filtered out. Neither
happened.

`FinalFailed: 0` matches the baseline's 0, and `FinalSkipped: 0` matches the baseline's 0.

## (d) Both named tests pass

| Test | Owner | Outcome |
|---|---|---|
| `UnwireIntentEvents_DetachesPicturesChanged` | this remediation | **Passed** |
| `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | merged sibling 484, unmodified and unrenamed | **Passed** |

Both were read directly from the TRX by test name, in the full unfiltered run — not from the narrower
P2-T3 filtered run — so the sibling's balance test is confirmed green in the context of the whole
assembly.

## TRX hygiene and coverage attachment cleanup

`FEATURE/evidence/qa-gates/rem1-p4-t6.trx`, sanitised in place, case-insensitively:

| Token | Replacement (XML entity form) | Occurrences |
|---|---|---:|
| worktree root | `&lt;repo-root&gt;` | 2244 |
| machine name | `&lt;host&gt;` | 1128 |
| account name | `&lt;user&gt;` | 4 |

Zero residual host tokens after sanitisation, and zero raw-angle-bracket placeholders, so the document
remains parseable. The machine-name count is high because vstest writes a `computerName` attribute on
every one of the 1122 result elements; the account name appeared in the run-user and coverage-
attachment attributes. Re-parsed with a strict XML reader after sanitisation: **parse succeeded**, and
the document contains **1122** `UnitTestResult` elements, matching the reported total of 1122 exactly.

`/EnableCodeCoverage` wrote two directories into the results directory, both carrying host identity in
their names — a GUID-named attachment directory containing a `<user>_<HOST>_<date>.coverage` file, and
a `<user>_<HOST>_<timestamp>` directory. **Both were deleted immediately after the run**, and a
directory listing of the results directory now returns zero subdirectories. Git does not track
directories, so `git status` would never have warned about either.

## Acceptance

| P4-T6 condition | Result |
|---|---|
| (a) `FinalFailed: 0` | **Yes** — `failed="0"` |
| (b) `FinalSkipped: 0` | **Yes** — no skipped or not-executed outcome anywhere |
| (c) `FinalPassed: 1122` exactly | **Yes** — 1122, the baseline 1121 plus the one added test |
| (d) TRX records Passed for both named tests | **Yes** — both `Passed` |

Output Summary: The scoped `QuickFiler.Test` gate **passes**. `EXIT_CODE: 0`,
`Test Run Successful.`, with `FinalPassed: 1122`, `FinalFailed: 0`, `FinalSkipped: 0` —
**exactly** the P0-T5 baseline of 1121 plus the single test this remediation adds, from TRX counters
`total="1122" executed="1122" passed="1122" failed="0"` and an outcome tally of 1122 `Passed` and
nothing else. Both `UnwireIntentEvents_DetachesPicturesChanged` and the unmodified sibling
`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` are recorded `Passed` in the full
unfiltered run. The TRX is sanitised with entity-form placeholders, has zero residual host tokens,
strict-parses, and contains 1122 `UnitTestResult` elements matching the reported total; both
host-named coverage attachment directories were deleted from the results directory.
