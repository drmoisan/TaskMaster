# Code Review — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T19-30
- Reviewer: feature-review (remediation re-audit, cycle 1, R4)
- Range: `bd43572498474be89d80e1f9620dffb132ade377..8a1b7b98b7d12dac69fd1bee5d5f109d4095c3c6`
- Files reviewed: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`,
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`

## Executive Summary

Remediation cycle 1 added exactly one new MSTest test
(`PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke`) to close the
marginal class-level branch-coverage gap flagged in `code-review.2026-07-20T18-00.md` /
`policy-audit.2026-07-20T18-00.md`, and trimmed 26 purely-structural `// Act`/`// Assert`
comment-header lines from 12 pre-existing tests to keep the file under the 500-line limit while
adding the new test. No production code changed in this cycle (the production file's diff is
byte-identical to cycle 1's original fix). No blocking findings. Both cycle-1 low-severity findings
(CR-1 duplication, CR-2 file-size headroom) are addressed or unchanged in status below; the cycle-1
informational note (CR-3, the unused static helper's empty-array gap) is unaffected and still
applies.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (carried forward, unresolved) | QuickFiler/Controllers/QfcItemController.FolderHandling.cs | Lines 202-204 and 230-231 | The fallback clamp `<count> == 1 ? 0 : 1` remains duplicated verbatim across `AssignFolderComboBox()` and `PopulateAndSelectFolder(...)`; this remediation cycle did not touch production code, so the finding from `code-review.2026-07-20T18-00.md` (CR-1) is unchanged. | Unchanged: extract a small private static helper (e.g. `ResolveFallbackIndex(int suggestionCount)`) and call it from both sites, at a future opportunity — not required for this cycle's coverage-only scope. | General Code Change Policy §Reusability. Non-blocking; carried forward for tracking. | `git diff bd435724..8a1b7b98 -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (identical to cycle 1's diff). |
| Info (resolved) | QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | Whole file | Cycle 1's CR-2 (file at exactly 500 lines, zero headroom) is resolved: the file is now 498 lines. Room was made by removing 26 bare `// Act`/`// Assert` comment-header lines from 12 pre-existing tests (verified via diff: comment-line deletions only, no assertion/name/behavior change) rather than by splitting the file or weakening a test. | No action required. If further tests are added to this file, consider the two-file split contemplated in CR-2 before the limit is reached again, since headroom is now only 2 lines. | General Code Change Policy §Module & File Structure (500-line limit); this is a disclosed, verifiable technique that preserves test documentation (AAA structure remains visible via blank-line separation and descriptive test names) while reclaiming line budget. | `wc -l QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` -> 498; `git diff 8f34f8ef..8a1b7b98` shows only comment deletions plus the one new test. |
| Info (unaffected, carried forward) | QuickFiler/Controllers/QfcItemController.FolderHandling.cs | Line 231 (`PopulateAndSelectFolder`) | Cycle 1's CR-3 (the static helper's ternary does not special-case an empty `folderArray`, and would select an invalid index 1 if ever called with zero items) is unaffected by this cycle — no production code changed. | Unchanged from cycle 1: no action required for this bug fix's scope; extend the guard only if `PopulateAndSelectFolder` ever gains a production caller that does not pre-guard against an empty array. | Documented for future-maintainer awareness only; still out of Scope-Lock. | Unchanged from cycle 1: `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` test `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection`. |

## Positive Observations

- The new test follows the established `InvokeRequired == true` / `viewer.Verify(v =>
  v.Invoke(It.IsAny<Delegate>()), Times.Once())` idiom already used identically in five sibling test
  files in this project (`QfcItemController.ViewerSetupTests.cs`,
  `QfcItemController.ConversationTests.cs`, `QfcItemController.FocusAndThemeTests.cs`,
  `QfcItemController.SeamCoreTests.cs`, `QfcItemController.NavigationTests.cs`), per the branch-gap
  analysis evidence — consistent, low-risk reuse of an existing project convention rather than a
  novel technique.
- The remediation is minimal and precisely targeted: zero production-code changes, one new test,
  closing exactly the floor gap identified in the prior audit with no unrelated scope expansion.
- The comment-trimming technique used to preserve file-size headroom is disclosed and independently
  verifiable (a straightforward `git diff` confirms only comment-line deletions), rather than being
  silently absorbed into the diff.
- Test independence is preserved: the new test constructs its own `Mock<IItemViewer>`,
  `Mock<IApplicationGlobals>`, and `FolderController`, with no shared mutable state with any other
  test in the file.

## Scope Confirmation

Findings above are scoped to the full feature-vs-base diff
(`bd43572498474be89d80e1f9620dffb132ade377..8a1b7b98b7d12dac69fd1bee5d5f109d4095c3c6`), covering both
the original fix commit (`8f34f8ef`) and the remediation commit (`8a1b7b98`), not any plan/task/phase
subset. The only two non-Markdown files in this diff are the production and test file reviewed above
and in the original cycle's code review; no other source file (any language) was changed.
