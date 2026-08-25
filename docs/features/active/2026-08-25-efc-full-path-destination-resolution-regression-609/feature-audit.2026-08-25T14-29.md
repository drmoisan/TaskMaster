# Feature Audit: Efc full-path destination resolution regression (#609)

**Audit Date:** 2026-08-25
**Feature Folder:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609`
**Base Branch:** `origin/main`
**Head Branch:** `bug/efc-full-path-destination-resolution-regression-609` at `a8f6c276f4ddf8138f2bc2888536148ef17d4fa2`
**Work Mode:** `full-bug`
**Audit Type:** Post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `origin/main` (`b5c751519c6cf0eaeb2326d9e80b2439aeee7265`)
- **Head branch/commit:** `bug/efc-full-path-destination-resolution-regression-609` (`a8f6c276f4ddf8138f2bc2888536148ef17d4fa2`)
- **Merge base:** `b5c751519c6cf0eaeb2326d9e80b2439aeee7265`
- **Evidence sources:** primary `artifacts/pr_context.summary.txt`; secondary exact diff `artifacts/pr_context.appendix.txt`; feature evidence under this feature's `evidence/` folders.
- **Requirements source:** `spec.md`, resolved from `issue.md` work mode `full-bug`.

## Acceptance Criteria Inventory

**Authoritative AC source files:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md`.

1. `BreadcrumbBridgeRouterIssue439Tests.cs` verifies that `ResolveLeafKeyAsync` receives exactly `\\mailbox@example.com\Archive\Clients\North` for a `Clients\North` row under the specified archive root.
2. Direct row selection for that row returns `Clients\North` to the Efc filing flow and never returns a full Outlook hierarchy path.
3. Typed ancestor-segment activation and immediate-child activation for that row return archive-relative filing targets only.
4. `EmailFilerConfig_Tests.cs` verifies that an `@` mailbox root plus a relative `DestinationOlStem` produces exactly one archive-root prefix and the expected save-path mapping.
5. Existing banner, trash, root-boundary, and relative search/suggestion behavior remains covered and unchanged.
6. No implementation parses `@` as a mailbox delimiter or substitutes `Store.FilePath` for Outlook `FolderPath` in this flow.
7. If a direct `FolderPredictor.FolderArray` fail-before test proves an in-root persisted full Outlook value reaches startup presentation verbatim, any production correction is limited to an archive-root-aware `FolderPredictor` projection that removes only the matching root plus one separator and projects the aligned score key; `BreadcrumbBridgeRouter`, `EmailFilerConfig`, `EfcDataModel`, `EfcHomeController`, generic source-map normalization, `@` parsing, `Store.FilePath`, Outlook COM calls, persistence, and filesystem behavior remain unchanged.
8. The final C# formatting, analyzer, nullable-analysis, and coverage-enabled MSTest pass completes without new failures, with evidence written only under this feature's `evidence/<kind>/` folders.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| 1 | Full hierarchy router lookup | PASS | Strict direct-row mock expects exact hierarchy path. | Filtered suite evidence. | Exact `@` mailbox path asserted. |
| 2 | Direct selection stays relative | PASS | Direct-row test asserts `Clients\North`, not hierarchy path. | Filtered suite evidence. | Filing target preserved. |
| 3 | Ancestor and child activation stay relative | PASS | Added ancestor and immediate-child tests assert relative results. | Filtered suite evidence. | Both navigation paths covered. |
| 4 | `@` root gets one prefix | PASS | `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`. | Filtered suite evidence. | Outlook and filesystem outputs asserted. |
| 5 | Existing behavior remains covered | PASS | Existing Issue439 tests plus 27-test compatibility suite; full suite 6,479/6,479. | Final coverage evidence. | No unrelated production source changed. |
| 6 | No `@` parsing or `Store.FilePath` substitution | PASS | Exact diff changes only FolderPredictor production code. | `git diff --name-only origin/main...HEAD`. | No prohibited scope found. |
| 7 | Narrow, correct startup projection | PARTIAL | Scope boundaries hold, but `ProjectSuggestionPath` uses a case-sensitive comparison. | Diff inspection; `FolderPredictor.cs:852-858`. | Outlook `FolderPath` is case-insensitive in the specification; no case-variant regression exists. |
| 8 | Final C# QA and canonical evidence | PASS | Canonical QA artifacts; full coverage suite passed. | Recorded commands plus reviewer CSharpier check exit 0. | 6,479/6,479; 84.7853% repository coverage. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

- **PASS:** 7 criteria
- **PARTIAL:** 1 criterion
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gap preventing PASS:** Use a case-insensitive Outlook hierarchy root comparison in `ProjectSuggestionPath` and test a casing variant while preserving the root-plus-separator boundary.

## Acceptance Criteria Check-off

The seven passing criteria were already checked in `spec.md`. Criterion 7 remains source-checked from earlier execution but is review-PARTIAL. This review does not modify requirement sources; remediation must reconcile the checkbox after verification.

### AC Status Summary

- Source: `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md`
- Total AC items: 8
- Checked off (source state): 8
- Remaining (source state): 0
- Items requiring remediation: Criterion 7.

| Source File | Total AC | Checked | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 8 | 8 | 0 | Criterion 7 requires remediation despite the existing source checkmark. |
