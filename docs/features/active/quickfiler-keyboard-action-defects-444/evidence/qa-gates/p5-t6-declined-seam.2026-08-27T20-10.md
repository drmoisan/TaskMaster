# [P5-T6] Declined-seam gate

Timestamp: 2026-08-27T20-10
Command: `$mb = git merge-base HEAD origin/epic/quickfiler-bug-family-integration` then `git diff "$mb..HEAD" -- QuickFiler/Controllers/QfcItemController.Navigation.cs`, filtered to added lines (`+*` excluding `+++*`) and counted for the substring `Timer`
EXIT_CODE: 0
Output Summary: `added_lines=28`, `added_with_Timer=0`. The count of added lines containing `Timer` is
exactly `0`, so sibling #484's proposed timer-factory seam was not adopted.

The merge base is bound to a variable and interpolated inside a quoted string, the same form
`[P5-T1]` uses: PowerShell parses a subexpression followed by adjacent bare text as two tokens
rather than concatenating them, so the inline `$(git merge-base ...)..HEAD` spelling would not have
produced a single revision-range argument.

## Counts

| Measure | Value |
| --- | --- |
| added lines in the diff for this file | 28 |
| **added lines containing `Timer`** | **0** |
| occurrences of `Timer` in the file at the branch head | 4 |

The four occurrences at the branch head are pre-existing: they belong to the `_emailIsReadTimer`
handling inside `ToggleExpansionOn()` and `ToggleExpansionOff()`, which this feature does not touch.
Their presence is what makes the zero-hit assertion meaningful rather than vacuous — the literal does
occur in the file, so a gate that returned zero on the added lines is discriminating between the
pre-existing code and this feature's additions, not merely failing to find a token that is absent
everywhere.

## What was declined

Sibling #484's downstream note proposed introducing a timer-factory seam at the
`QfcItemController.Navigation.cs` timer construction site so the 4000 ms `System.Threading.Timer`
could be replaced under test. `spec.md` declines that proposal explicitly in its
`### Downstream notes` section: adopting it would widen this defect fix into a testability refactor
of a member outside the three defects' root causes, and it would change a member the spec's upstream
contract lists as UNCHANGED for siblings #464 and #489.

This feature instead avoids the timer in its own tests by arranging `ItemHelper.UnRead` as `false`
explicitly, so `ToggleExpansionOn` never reaches the timer construction and no wall-clock wait is
introduced. That is a test-arrangement choice, not a production seam, and it leaves the production
code path unmodified.

## Acceptance

- The count of added lines containing `Timer` is exactly `0` — met.
