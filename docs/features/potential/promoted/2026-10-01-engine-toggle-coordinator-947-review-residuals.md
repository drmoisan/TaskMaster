# engine-toggle-coordinator-947-review-residuals (Issue #964)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-coordinator-947-review-residuals/ (Issue #964)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #964
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/964
- Last Updated: 2026-10-01
## Summary

The #947 review (PR #963) left three residuals in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`:
1. On the refusal path of `HandleToggleClickAsync`, the "engines unavailable" notification call is still unguarded. A throwing notification sink can escape into the Office ribbon callback, so the method's "never throws" comment overstates that path. This is the same root cause as #947.
2. The `GetPrimeTask` doc comment opens with "The prime task", but the method returns the registration marker.
3. The file is 476 of 500 lines, so it needs splitting before its next change.

## Environment

- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: review of PR #963
- Data source or fixture: n/a

## Steps to Reproduce

1. Construct the coordinator with engines unavailable and a notification delegate that throws.
2. Invoke the toggle click.

## Expected Behavior

- The click handler never throws into the ribbon callback on any path.
- Doc comments match behavior.
- The file has room under the 500-line limit.

## Actual Behavior

- The notification exception escapes on the refusal path.
- The `GetPrimeTask` doc is inaccurate.
- The file is 476 lines.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: PR #963 body, Follow-ups 1 to 3; #947 review artifacts.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

#947 guarded the two `_logError` call sites only. The duplicated guard could become one helper that is applied to both log sinks and notification sinks.

## Proposed Fix / Validation Ideas

- [ ] Write a regression test first: a throwing notification on the refusal path must not escape. Then add a shared "invoke sink safely" helper and use it at all three sites.
- [ ] Correct the `GetPrimeTask` doc comment.
- [ ] Split the file into partials before or with the change, keeping each partial under 500 lines.
- [ ] Sequence this with #948, which edits the same file.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
