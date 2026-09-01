# Post-Change Test-File Line Count (P1-T15)

Timestamp: 2026-09-01T12-47

Command: `(Get-Content 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs').Count`
EXIT_CODE: 0
Output: `479`

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| Post-change line count of the test file | `<= 500` | `479` | Yes |

ACCEPTANCE: MET. Headroom remaining: 21 lines.

## Context

`.claude/rules/general-code-change.md` (File Size Limit) and `CLAUDE.md` General Code Change
Policy section 4 both cap production code, test code, and reusable script files at 500 lines.
The plan flagged this as a live risk in Hard Scope Boundary 5: the file was already at 454
lines before this change, leaving only 46 lines of headroom, so the cap could not be assumed
to hold and needed an explicit post-change check rather than an estimate.

| Point | Line count | Headroom to the 500 cap |
|---|---|---|
| Before this change (`origin/main`) | 454 | 46 |
| After this change | 479 | 21 |

The 25-line increase matches exactly the 25 insertions reported by
`git diff --numstat origin/main` in P1-T13, so no line was added to the file outside the new
test method.

## Companion Check — Production File

The same 500-line cap applies to the production file this item also changes. It is well
inside the limit and is recorded here for completeness:

Command: `(Get-Content 'QuickFiler\Controllers\QfcHomeController.Metrics.cs').Count`
EXIT_CODE: 0
Output: `231` (was 227 before the four-line guard; 269 lines of headroom)

## Forward Note

At 479 lines the test file has 21 lines of headroom. A future addition of another test of
comparable size (the one added here cost 25 lines including its documentation comment) would
breach the cap and require the file to be split first. That is a note for whoever next adds
to this file; it is out of scope for issue #646 and no split is performed here.
