# remaining-stale-binding-redirect-pairs (Issue #973)

- Date captured: 2026-10-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/remaining-stale-binding-redirect-pairs/ (Issue #973)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #973
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/973
- Last Updated: 2026-10-02
## Summary

Preparation for #953 found that, besides the 11 Fizzler redirects #953 fixes, 15 other assembly and version pairs in the repository's `app.config` files redirect to a `newVersion` that no deployed assembly has. #953 adds a Pester test that records these 15 pairs as known exceptions and fails on any new mismatch. This issue covers correcting the 15 pairs and emptying that list.

## Environment

- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (.NET Framework 4.8 `app.config` binding redirects)
- Command/flags used: the #953 redirect-consistency Pester test
- Data source or fixture: `*/app.config` against `packages/` assembly versions

## Steps to Reproduce

1. After #953 merges, read the known-mismatch list in the #953 Pester test. Plan task P2-T16 records the 15 pairs.
2. Compare each pair's `newVersion` with the deployed assembly version.

## Expected Behavior

Every `bindingRedirect` `newVersion` names an assembly version that is actually deployed, so the known-mismatch list is empty.

## Actual Behavior

15 pairs name versions that are not deployed. They are latent today, but each becomes a load failure as soon as a dependency requests that assembly, which is how #418 happened.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #953 plan P2-T16 and its evidence.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Package updates advanced deployed versions without a matching sweep of the redirects. This is the same cause as #953 and #418. Sequence it after #953 merges.

## Proposed Fix / Validation Ideas

- [ ] For each pair, correct the redirect or remove it when nothing references the assembly. Remove the pair from the known list in the same change, so the test proves each fix.
- [ ] Re-test the #418 designer path for `PictureBoxSVG` after the sweep.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
