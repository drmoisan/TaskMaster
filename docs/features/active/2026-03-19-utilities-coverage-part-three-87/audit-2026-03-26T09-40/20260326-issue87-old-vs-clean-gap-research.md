<!-- markdownlint-disable-file -->

# Task Research Notes: issue87-old-vs-clean-gap

## Research Executed

### File Analysis

- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-plan.final-clean-issue87.2026-03-26T16-10.md`
  - The final clean-branch plan expected the clean `#87` branch to contain only `UtilitiesCS/**`, `UtilitiesCS.Test/**`, and the issue-`#87` feature folder.
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-branch-split-source-map.md`
  - The intended clean-branch replay set for issue `#87` was the direct cherry-pick bucket plus selected bootstrap restores from mixed commits.
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-focused-diff.md`
  - The clean branch currently reports an in-scope diff only, which means the remaining problem is not out-of-scope contamination but missing in-scope content.
- `artifacts/research/20260326-issue87-unstacking-sequence-research.md`
  - The verified unstacking order was `#97` -> `#96` -> residual excluded work -> clean `#87`.
- `.git/branch_analysis_issue87.txt`
  - Commit-path attribution identifies the original intended issue-`#87` commits and the mixed commits that required bootstrap handling.

### Code Search Results

- `feature/utilities-coverage-part-three-87-clean..feature/utilities-coverage-part-three-87`
  - Live git comparison shows the old mixed branch still contains `105` issue-`#87`-scope paths that are not present on `feature/utilities-coverage-part-three-87-clean`.
- `origin/development...feature/utilities-coverage-part-three-87` vs `origin/development...feature/utilities-coverage-part-three-87-clean`
  - The old branch contains `236` issue-`#87`-allowlist paths, while the clean branch contains `141`.
- `missing substantive files`
  - Only `17` of the `105` missing allowlist paths are production/test files; the other `88` are docs/evidence artifacts that remained on the old workspace branch during remediation execution.
- `per-file attribution for old-only code`
  - The missing code/test files trace back to old-branch-only changes from `4009d1c`, `ee9e4d9`, and especially `5661a47`.

### External Research

- #fetch:https://git-scm.com/docs/git-diff
  - Verified that comparing both branches against the same merge-base (`origin/development`) is the correct way to isolate paths present on the old branch but absent from the clean branch.
- #fetch:https://git-scm.com/docs/git-log
  - Verified that `git log <clean>..<old> -- <path>` is the right primitive for attributing each missing file to the old-branch-only commits that touched it.

### Project Conventions

- Standards referenced: remediation uses `origin/development` as the canonical base; the clean `#87` branch is intended to be issue-`#87`-only; PR scope is verified from git diff and recorded in issue-specific evidence artifacts.
- Instructions followed: `policy-compliance-order`, `evidence-and-timestamp-conventions`, `general-code-change.instructions.md`, and the task-researcher scratch-space restriction.

## Key Discoveries

### Project Structure

The clean branch is not missing unrelated split-out work. The remaining delta is entirely within the issue-`#87` allowlist:

- `105` issue-`#87` paths are still present on `feature/utilities-coverage-part-three-87` but absent from `feature/utilities-coverage-part-three-87-clean`.
- `88` of those are docs/evidence files under `docs/features/active/2026-03-19-utilities-coverage-part-three-87/**`.
- `17` are substantive `UtilitiesCS` / `UtilitiesCS.Test` files.

That means the branch-splitting work itself largely succeeded: the clean branch does not still need `#96`, `#97`, residual, `.codex`, `.github`, `QuickFiler`, or `TaskMaster` scope moved over. What remains is a partial issue-`#87` content gap.

### Implementation Patterns

The old-only substantive gap is concentrated in three old-branch-only commit areas:

- `4009d1c` — missing two test-file updates
- `ee9e4d9` — missing one test-file update
- `5661a47` — missing the bulk of the remaining production/test changes

The verified substantive files still only on the old branch are:

#### Production files still missing from `feature/utilities-coverage-part-three-87-clean`

- `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailTokenizer.cs`
- `UtilitiesCS/Extensions/IEnumerableExtensions.cs`
- `UtilitiesCS/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew.cs`
- `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs`
- `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs`

#### Test files still missing from `feature/utilities-coverage-part-three-87-clean`

- `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroup_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`
- `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`
- `UtilitiesCS.Test/HelperClasses/ComStreamWrapper_Tests.cs`
- `UtilitiesCS.Test/HelperClasses/DispatchUtility_Tests.cs`
- `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
- `UtilitiesCS.Test/HelperClasses/TimedDiskWriterTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/LockingObservableLinkedList_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/LockingObservableLinkedListNode_Tests.cs`
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs`

#### Issue-`#87` docs/evidence still only on the old branch

These are not out-of-scope contaminants; they are issue-`#87` branch-management and QA artifacts that were written to the original workspace branch during remediation execution and never copied to the clean branch. Representative examples include:

- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/code-review.2026-03-26T09-40.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/policy-audit.2026-03-26T09-40.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/feature-audit.2026-03-26T09-40.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-inputs.2026-03-26T09-40.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-plan.final-clean-issue87.2026-03-26T16-10.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-*`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-*`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-*`

These documentation gaps explain most of the `105` missing paths.

### Complete Examples

```text
Verified substantive old-only issue-87 gap

Commits involved:
- 4009d1c  -> ComStreamWrapper_Tests.cs, StoreWrapperController_Tests.cs
- ee9e4d9  -> DispatchUtility_Tests.cs
- 5661a47  -> the remaining 14 production/test file deltas listed above

Counts:
- Old branch issue-87 allowlist paths vs development:   236
- Clean branch issue-87 allowlist paths vs development: 141
- Old-only issue-87 allowlist paths:                    105
- Of those, docs/evidence:                               88
- Of those, production/test files:                       17
```

### API and Schema Documentation

The comparison that produced the verified gap list was:

- compare old branch to `origin/development` within the issue-`#87` allowlist
- compare clean branch to `origin/development` within the same allowlist
- compute the set difference (`old - clean`)
- attribute each remaining production/test path with `git log feature/utilities-coverage-part-three-87-clean..feature/utilities-coverage-part-three-87 -- <path>`

### Configuration Examples

```text
Issue-87 allowlist used for the comparison

UtilitiesCS/**
UtilitiesCS.Test/**
docs/features/active/2026-03-19-utilities-coverage-part-three-87/**
```

### Technical Requirements

- The clean branch does **not** still need any `#96`, `#97`, residual excluded-work, `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test`, `TaskMaster`, or `UtilitiesSwordfish` paths moved over.
- The remaining branch gap is a partial issue-`#87` gap only.
- If the goal is feature parity between the old and clean issue-`#87` branches, the `17` substantive files above are the highest-priority items to move.
- The `88` docs/evidence files only need to move if the clean branch is expected to carry the full remediation/audit/evidence trail instead of leaving that audit trail on the archived mixed branch.

**Mandatory unachievable objective callout**:
- None identified. The branch-gap identification objective was achievable and was completed with live git evidence.

## Recommended Approach

Treat the remaining delta as two separate buckets:

1. **Must-evaluate for movement**: the `17` substantive `UtilitiesCS` / `UtilitiesCS.Test` files above, because these represent real issue-`#87` behavior and coverage changes that are still present on the old branch but not on the clean branch.
2. **Optional / process-driven movement**: the `88` docs and evidence files, because they are mostly remediation execution artifacts created on the original workspace branch while the clean branch was built in a sibling worktree.

If the immediate question is “what real issue-`#87` implementation content is still not on the clean branch,” the answer is the `17` substantive files listed above.

## Implementation Guidance

- **Objectives**: close the remaining issue-`#87` parity gap between `feature/utilities-coverage-part-three-87` and `feature/utilities-coverage-part-three-87-clean`.
- **Key Tasks**: inspect the diffs for the `17` substantive files; decide whether to replay the missing hunks from `4009d1c`, `ee9e4d9`, and `5661a47`; separately decide whether the clean branch must also carry the old-branch remediation/audit evidence files.
- **Dependencies**: `feature/utilities-coverage-part-three-87`, `feature/utilities-coverage-part-three-87-clean`, `origin/development`, `.git/branch_analysis_issue87.txt`, and the final clean-branch remediation plan.
- **Success Criteria**: either (a) the clean branch includes the intended `17` missing substantive files and any required issue-`#87` docs/evidence, or (b) each missing file is explicitly classified as intentionally omitted.