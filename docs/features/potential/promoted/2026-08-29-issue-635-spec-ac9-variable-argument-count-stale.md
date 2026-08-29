# issue-635-spec-ac9-variable-argument-count-stale (Issue #692)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/issue-635-spec-ac9-variable-argument-count-stale/ (Issue #692)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #692
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/692
- Last Updated: 2026-08-29
## Summary

`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` AC-9 states there are six variable-argument reflection call sites targeting the removed members' own type, but the actual derivation performed while implementing issue #635 found eight such sites. The approved acceptance-criterion text is numerically wrong.

## Environment

- OS/version: Windows, git repository `TaskMaster`
- Command/flags used: n/a - documentation/spec accuracy issue
- Data source or fixture: `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` line 352, and the item-635 orchestrator delegation report from `/parallel-run bugs-635-440`

## Steps to Reproduce

1. Read AC-9 at `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md:352-354`: "Each of the six variable-argument reflection call sites that target the removed members' own type is named individually by file and line...".
2. Compare against the item-635 implementation's own derivation, which enumerated eight variable-argument sites (including `GetField(` calls taking a `string name` against `typeof(QfcCollectionController)`), not six.
3. Note the merged fix (PR #688) discharged AC-9 using the eight-element superset rather than the stated six-element set, since no six-element subset of the eight is identifiable.

## Expected Behavior

The approved spec's AC-9 should state the correct count (eight) so the acceptance criterion matches what was actually required and verified.

## Actual Behavior

AC-9 still reads "six" in the merged spec on `main`, while the actual evidence and the merged fix cover eight sites. The criterion text is stale relative to the verified requirement.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: parallel-run bugs-635-440 final report: "Item 635's spec.md AC-9 names six variable-argument reflection call sites; derivation yields eight. Discharged by superset, but the approved figure is stale — amending approved criterion text is yours."

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

The original AC-16 search that fed the #635 spec's variable-argument-site count did not cover the full `GetField(` reflection family (172 hits across the QuickFiler test tree, eight of them variable-argument sites against `QfcCollectionController`), so the spec was written against an undercount before the fuller derivation was performed during implementation.

## Proposed Fix / Validation Ideas

- [ ] Edit AC-9 in the merged spec to state eight variable-argument reflection call sites, matching the evidence already on `main`
- [ ] Cross-check no other AC in the same spec references the stale six-site count
- [ ] No code change required; this is a documentation-accuracy correction only

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
