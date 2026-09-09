# utilitiescs-test-hygiene-residuals (Issue #817)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-test-hygiene-residuals/ (Issue #817)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #817
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/817
- Last Updated: 2026-09-08
- Work Mode: full-bug

## Summary

Standing residuals issue for low-severity hygiene findings in `UtilitiesCS.Test` that reviews surface
but that do not each warrant their own issue. Opened with one entry: the pre-existing 1067-line
`FolderPredictorTests.cs`, which is more than twice the repository's 500-line file cap.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: file line counts taken during issue 809's `[P4-T5]` gate
- Data source or fixture: not applicable

## Steps to Reproduce

1. Count the lines in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`.
2. Compare against the 500-line cap in `.claude/rules/general-code-change.md`, "File Size Limit".

## Expected Behavior

No test file exceeds 500 lines. The cap has no test-code exemption; the listed exemptions are
throwaway agent scripts, raw text fixtures for language-processing test data, and Markdown.

## Actual Behavior

`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is 1067 lines.

This is pre-existing and was not introduced by issue 809; that item's line-count gate simply recorded
it. Splitting the file along its existing behavioural groupings is the obvious remedy, but it touches
a file several in-flight items depend on, so it should be sequenced rather than done opportunistically.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: issue 809 `evidence/qa-gates/p4-t5-line-counts.md`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: a maintainability cost, with no behavioural defect. Recorded so it is not rediscovered by every
subsequent review of this assembly.

## Suspected Cause / Notes

This entry is intended as the standing residuals record for the `UtilitiesCS.Test` subsystem. Append
further low-severity findings here as checklist items rather than opening a new issue for each, per
the 2026-09-07 ruling that residuals be batched by blast radius and standalone issues reserved for
Medium severity or higher.

Sequencing note: issue 811 (`utilitiescs-test-determinism-780-803-594-811`) is scheduled on run
`bugs-2026-09-06` and touches this assembly, including `FolderPredictorTests.cs` by way of the shared
`UtilitiesCS.Test.csproj`. Do not begin a file split until 811 has merged.

Resolution note: the 5-file split landed with all 8 `spec.md` acceptance-criteria boxes checked and
the full `UtilitiesCS.Test` pass count unchanged (baseline Total tests: 4904, Passed: 4904; post-split
Total tests: 4904, Passed: 4904).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: none new; a split must preserve the existing test set exactly, verified by
      comparing test counts and names before and after.
- [ ] Integration scenario to retest: a full `UtilitiesCS.Test` run with unchanged pass count.
- [ ] Manual verification notes: confirm every resulting file is under 500 lines and that
      `UtilitiesCS.Test.csproj` lists each new file.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
