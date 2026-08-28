# P3-T1 — Handoff-record addendum acceptance check

Timestamp: 2026-08-28T03-53
Task: [P3-T1]
Command: git grep -F -c "ObligationDischargedInBranch: true" -- docs/features/active/itemviewer-surface-defects-489/evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md
EXIT_CODE: 0

## Result

```
docs/features/active/itemviewer-surface-defects-489/evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md:1
```

Reported count: **1**, exactly as the acceptance condition requires. The literal appears on one line
only, at `:124`, as the machine-checkable field.

This grep found a match, so it exits `0` and no `ExpectedExitCode:` declaration is needed. The
zero-match residual described in the plan's convention 6 does not apply to a matching grep; the
verdict here is nonetheless taken from the **reported count of 1** rather than from the exit code, in
line with that convention's general rule.

**A first attempt reported 2, and that is recorded rather than quietly corrected.** The addendum
originally repeated the literal inside its own Output Summary paragraph, so `git grep -c`, which
counts *matching lines*, returned 2 and the gate failed. The Output Summary was rewritten to refer to
the field in prose instead of restating it verbatim; the field itself was not moved or altered. The
gate then returned 1. This is exactly the failure mode a count-based gate is meant to catch, and it
demonstrates the gate can fail.

## What the addendum records

Appended to `FEATURE/evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md` as a new
final section headed `## Addendum — 2026-08-28: the obligation was discharged in this branch`.

| Required content | Present |
|---|---|
| Its own `Timestamp:` | **Yes** — `2026-08-28T03-52` |
| `ObligationDischargedInBranch: true` | **Yes** — once, at `:124` |
| Feature review (RC-1) found the leak shipped on this branch | **Yes** — cites `code-review.2026-08-28T03-13.md` and `remediation-inputs.2026-08-28T03-13.md` |
| 484 was already merged (`Upstream484Landed: true`), so the obligation was never transferable | **Yes** |
| Discharged in-branch by the detachment line plus `UnwireIntentEvents_DetachesPicturesChanged` | **Yes**, with pointers to the RED and GREEN evidence artifacts |
| No follow-up issue against 484 is required for this detachment | **Yes**, stated explicitly and tied back to the paragraph it supersedes |
| The 484-owned test name is deliberately left unrenamed, with the reason | **Yes** — stable merged-sibling node ID, still-true assertion set, churn without behavioural gain |

## The original sections are unaltered

`git diff --stat` for the file reports **82 insertions and 0 deletions**. A line-by-line comparison of
the committed version against the first 115 lines of the amended file reports them **identical**. The
addendum is a pure append; nothing above it was rewritten.

## Encoding

The file was pure LF with no BOM before the edit and is pure LF with no BOM after it: `\r` count 0,
BOM absent. No in-place stream editor was used — the append was performed byte-wise so the file's line
endings could not be silently rewritten.

## Acceptance

| P3-T1 condition | Result |
|---|---|
| `git grep -F -c "ObligationDischargedInBranch: true"` over the handoff record reports exactly 1 | **Yes** — reported 1 |

Output Summary: The dated addendum is appended in place to the 16-to-17 handoff record — 82
insertions, 0 deletions, the original 115 lines byte-identical, pure LF and no BOM preserved. It
carries its own `Timestamp: 2026-08-28T03-52` and the machine-checkable field
`ObligationDischargedInBranch: true`, and it records that RC-1 found the leak shipped on this branch,
that `Upstream484Landed: true` meant the obligation was never transferable to 484's in-flight work,
that it is discharged in-branch by the one detachment line plus the regression test, that no follow-up
issue against 484 is required for this detachment, and that the 484-owned test
`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` is deliberately left unrenamed. The
acceptance grep reports exactly **1** matching line, `EXIT_CODE: 0`. A first attempt reported 2
because the Output Summary restated the literal; that is recorded here rather than hidden, and it
shows the gate is capable of failing.
