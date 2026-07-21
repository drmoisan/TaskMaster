# Code Review — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T23-28
- Reviewer: feature-review
- Base: main @ cd6362f0 | Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 4412d2da
- Scope: full branch diff (2 C# production files, 6 C# test files, 1 test .csproj, memory + docs)
- Cycle: remediation cycle 1 re-audit (R4)

## Executive Summary

The fix is a targeted, minimal bug remediation consistent with the repository's bugfix workflow and
design principles. The root cause — a transient empty/partially-populated window in
`FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` created by an up-front `_model.Clear()` before the
first `await` — is eliminated by building the upgraded rows into a local `List<BreadcrumbStateRow>` and
publishing them through a single atomic `BreadcrumbStateModel.ReplaceRows` reference swap. The swap
reconciles the selected index against the new count before publishing the new backing list, so a reader
never observes a new list paired with a stale out-of-range index. The change is well-documented with
why-oriented comments, fails fast on null input, and preserves the pre-existing readback contract.

The R4 remediation was test-only (R1 file splits) plus regeneration of the coverage artifact (R2); it
did not alter production behavior. No blocking or high-severity code-quality findings were identified.
The observations below are informational.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | `_rows` field (line ~186) | The backing list field was changed from `readonly` to mutable to permit the reference swap in `ReplaceRows`. | Accept. The mutation is confined to `ReplaceRows`/construction and the field remains private; no external mutability is exposed. | Necessary for an atomic single-reference publish; alternative in-place clear+refill would reintroduce the transient window. | Production diff, `ReplaceRows` body |
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | `ReplaceRows` (lines ~213-238) | Selection is reconciled before the list is published (`_selectedIndex` reset to -1 when >= new count); subfolder selection is reset unconditionally, matching `Clear()` semantics. | Accept. | The ordering guarantees no reader sees the new list with a stale index; subfolder reset is consistent with a full row replacement. | Production diff |
| Info | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | `SetSuggestionsAsync` (lines ~50-90) | Scored rows whose ancestor chain cannot be resolved now fall back to a plain row carrying the score's folder path (previously `AddPlainRow(path)`), preserving the exact-path selection contract. | Accept. | Behavior is preserved through the refactor from incremental `_model.Add*` calls to local-list construction; documented with a why-comment. | Production diff; AC-4 regression tests pass |
| Info | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | local list capacity | `new List<BreadcrumbStateRow>(rows.Count)` pre-sizes the local buffer. | Accept. | Minor, appropriate allocation hygiene; no correctness impact. | Production diff |
| Info | UtilitiesCS.Test/OutlookObjects/Folder/*Tests.cs | R1 split files | The two over-limit test files were split into scenario-grouped files (Sequence/InFlight), each < 500 lines, using MSTest + FluentAssertions and `TaskCompletionSource`-gated fakes (no sleeps, no temp files). | Accept. | Complies with the file-size limit and deterministic-test rules; test intent is preserved. | `git diff --numstat`; head line counts |

## Design and Policy Alignment

- Simplicity: the fix is the smallest change that closes the race — a local build plus one atomic swap.
  No opportunistic refactoring of unrelated code.
- Separation of concerns: the atomicity invariant lives in the model (`ReplaceRows`), the row-building
  logic in the router; the split is appropriate.
- Error handling: `ReplaceRows` fails fast with `ArgumentNullException` on null input; the router
  retains its existing null guard.
- Naming and docs: `ReplaceRows` is descriptive; XML doc explains the why (the #398 empty-window race).
- Determinism: regression tests use `TaskCompletionSource` gating rather than timing sleeps, satisfying
  the general and C# unit-test deterministic requirements.
- Concurrency note: the issue's secondary concern (cross-thread mutation of the `List`-backed model
  without synchronization) is materially mitigated for the reported path by collapsing the multi-step
  rebuild into a single reference assignment; a full memory-model synchronization hardening remains
  possible follow-up but is out of scope for this minimal bug fix and is not a regression.

## Verdict

PASS. No blocking or high-severity findings. The change is well-scoped, documented, and covered by
deterministic regression tests.
