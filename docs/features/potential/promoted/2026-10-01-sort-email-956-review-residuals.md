# sort-email-956-review-residuals (Issue #966)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/sort-email-956-review-residuals/ (Issue #966)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #966
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/966
- Last Updated: 2026-10-02
## Summary

PR #965 (#956) split `SortEmail.cs` and added the `YesNoToAllPromptSession` seam. Its spec and review left these residuals in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail*.cs`. The logic defects L1 to L4 are tracked separately in #959.
- **F1:** apply the prompt session to the overwrite and alternate-name prompts, which still call the dialog directly.
- **F2:** delete the unused `SaveAttachmentsOld` and `IsPicture`.
- **F3:** remove `[ExcludeFromCodeCoverage]` from members that already have tests.
- **CR-1:** the spec's boundary wording does not match where `new DirectoryInfo` now runs.
- **Try-save path:** replace the `Debug.WriteLine` calls with the project logger, and remove the outer rethrow, which has no effect.
- **Partial files:** remove the unneeded `using` directives copied into each one.

## Environment

- OS/version: n/a
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: review of PR #965
- Data source or fixture: n/a

## Steps to Reproduce

1. Read the `SortEmail` partial files on `main` at `59cbab04f` or later.
2. Compare them with the items above.

## Expected Behavior

- All `SortEmail` prompts go through the injectable prompt session.
- There is no dead code.
- Coverage exclusions exist only where they are unavoidable.
- Logging goes through the project logger.
- No unused `using` directives remain.

## Actual Behavior

As listed in the summary.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: PR #965 body, Follow-ups; #956 spec, Rollout and Follow-up.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

#956 was scoped to the split and one seam. Coordinate this with #959, because both touch the same partial files. Doing #959 first lets its regression tests use the new seams.

## Proposed Fix / Validation Ideas

- [ ] Route the remaining prompts through `YesNoToAllPromptSession`, with tests.
- [ ] Delete the dead members, confirming there are no callers.
- [ ] Remove the exclusions where tests exist, and confirm that changed-line coverage does not drop.
- [ ] Clean up the logging and the `using` directives.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
