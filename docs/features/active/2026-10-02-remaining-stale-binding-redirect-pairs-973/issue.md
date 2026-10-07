# remaining-stale-binding-redirect-pairs (Issue #973)

- Date captured: 2026-10-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/remaining-stale-binding-redirect-pairs/ (Issue #973)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #973
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/973
- Last Updated: 2026-10-02
- Work Mode: full-bug

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

## Scope Amendment (2026-10-03, maintainer direction)

On 2026-10-03 the maintainer directed that the following related defects be folded into #973 rather than filed as separate issues, under the related-defect remediation directive (defects in the same files, component or root cause are remediated inside the item). The analysis is in `research/2026-10-03T00-41-graph-usings-and-claude-md-bullet-research.md` (cited in spec.md as research 3). spec.md remains the sole acceptance-criteria source; the folded scope is carried there as Proposed Fix Parts F, G and H, acceptance criteria AC19 to AC23 and a Scope Amendment Log.

- (A) Unused `using Microsoft.Graph.*` directives. Research 1 section 5.4 found, and research 3 section 1 confirmed per directive, that six `using Microsoft.Graph.*` directives in five UtilitiesCS files bind no type and no extension method: UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs line 7, UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs line 10, UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs lines 11 and 12, UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs line 18, UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs line 6. Each is removed only after the per-file confirmation that no type resolves through it (research 3 sections 1 and 2); the compile gates are the proof. The Microsoft.Graph package reference itself stays (research 3 section 4).
- (B) CLAUDE.md C#1 item 3, the "Do not add `/p:Nullable=enable`" bullet (CLAUDE.md line 211). Its premise "there is no `Directory.Build.props`" is stale: Directory.Build.props (#730, sets only RxUseUnsupportedPackagesConfig) and Directory.Build.targets (VSTO signing) both exist at the repository root. The bullet is reworded to state that no project and neither root build file sets a `<Nullable>` element; the conclusion (do not add the property) is unchanged (research 3 section 5). Files under .claude/ are push-down owned and are not edited.
- File-size split. CategoryClassifierGroup.cs is 539 lines, above the 500-line file limit (research 3 section 3). Because the file is touched by (A) and file-size splits in touched files are in scope under the related-defect directive, the orchestrator ruled that the file is brought under 500 lines in this item by a behaviour-preserving split into a partial class (one new file beside it plus its Compile Include in UtilitiesCS/UtilitiesCS.csproj, which is already in the write set). The spec names the rule: both files under 500 lines, no member body changed, member set identical, Compile Include added.

The previously recorded follow-up for (A) in spec.md Scope and Rollout is withdrawn by this amendment; nothing is filed separately for (A), (B) or the split.
