# Changed-line coverage (P7-T10)

Timestamp: 2026-09-01T11-15
Task: [P7-T10]
Working directory: WORKTREE

Command:

```
git diff origin/main -- QuickFiler/Controllers/FilerQueue.cs QuickFiler/Controllers/QfcFormController.EventHandlers.cs
```

EXIT_CODE: 0
Diff size: 226 lines.

Coverage source: `coverage\post-change.cobertura.xml`, from the clean P7-T6 run.

## Why the two-dot form

The two-dot form is required here and matches P6-T8. It compares the **working tree** against the base,
so it enumerates the same file text that P7-T6 measured. A three-dot form is commit-to-commit, and the
last commit before this phase is the one P2-T7 took, which covers Phases 1 and 2 only — the entire fix
would have been missing from the changed-line set, making the gate vacuous. The working-tree form is
also what survives P7-T2, which may reformat these files after any earlier commit and shift their line
numbers away from the numbers recorded in the Cobertura file.

`git rev-parse origin/main` at this point returns `06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72`, identical
to the value P0-T3 recorded, so `origin/main` did not advance during execution and no re-run against a
substituted merge-base SHA was required. The merge base equals `origin/main`, so this diff is against
the true merge base.

## Changed-line enumeration

Added and modified line numbers were taken from the unified diff by tracking the new-file line counter
across each hunk header and recording every `+` line.

| File | Changed lines | Of those, instrumented by the coverage tool | Uncovered (`hits` = 0) |
|---|---|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | 126 | 58 | **0** |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 12 | 7 | **0** |
| **Total** | **138** | 65 | **0** |

The changed-line count is 138, which is greater than 0, so the gate is not vacuous and the
`REMEDIATION-REQUIRED` branch for a zero changed-line count was not taken.

The gap between changed lines and instrumented lines is expected and is not a coverage gap: a Cobertura
file carries `line` elements only for executable statements, so XML documentation comments, explanatory
comments, blank lines, field declarations without initializers, and brace-only lines carry no `line`
element and cannot have a hit count. A large share of the 126 changed lines in `FilerQueue.cs` is the XML
documentation added for the monitor, the counter, the drain signal, the consumer-running flag,
`ItemProcessor`, and `WhenDrainedAsync`.

## Uncovered changed-line list

Empty. **The uncovered changed-line list contains no line at all.**

An empty list is trivially a subset of the single-member exemption set the acceptance condition defines
— the production default `ItemProcessor` initializer lambda in `QuickFiler/Controllers/FilerQueue.cs`
added by P1-T1 — so the acceptance condition is satisfied. No line lies outside that set.

The quote-and-justify requirement is conditional on the initializer line actually appearing in the
uncovered list, and it does not appear, so no quotation or justification paragraph is required. The plan
anticipated this outcome and wrote the requirement conditionally for exactly this reason: it noted that
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:350` has `InjectFilingCollaborators` hand
the controller an `IFilerHomeController` whose `FilerQueue` getter returns a real `FilerQueue`, which
carries the P1-T1 production default `ItemProcessor`, so any test filing through that helper can enter
the default lambda. That is what happened: the line is covered, with `hits` greater than 0.

Output Summary: 138 production lines changed, of which 65 are instrumented, and **zero** are uncovered.
Coverage did not regress on any line changed by this fix; every changed executable line is executed by
the test suite. This is consistent with the P7-T9 per-file finding that
`QuickFiler/Controllers/FilerQueue.cs` reaches a per-file rate of 1.0000 with an empty uncovered set.

This artifact supplies the changed-line half of the evidence for the AC20 check-off in P8-T24.
