# Feature Audit: Efc full-path destination resolution regression (#609)

**Audit Date:** 2026-08-25
**Feature Folder:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609`
**Base Branch:** `origin/main`
**Head Branch:** `bug/efc-full-path-destination-resolution-regression-609` at `67db82a928e6b0c023ed16bf42ca48e526f07a0e`
**Work Mode:** `full-bug`
**Audit Type:** Final post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `origin/main` (`507a40a549d573b221da0fb59c3e18af5ce8d473`)
- **Head branch/commit:** `bug/efc-full-path-destination-resolution-regression-609` (`67db82a928e6b0c023ed16bf42ca48e526f07a0e`)
- **Merge base:** `b5c751519c6cf0eaeb2326d9e80b2439aeee7265`
- **Evidence sources:**
  - Primary: refreshed `artifacts/pr_context.summary.txt`.
  - Secondary baseline diff: refreshed `artifacts/pr_context.appendix.txt` and `git diff --check` at the stated range.
  - Feature evidence: `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/evidence/`.
  - Additional evidence: reviewer-focused VSTest and CSharpier check on the reviewed head.
- **Feature folder used:** active Issue #609 folder identified by canonical PR context and its matching `issue.md`/`spec.md`.
- **Requirements source:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md`.
- **Work mode resolution note:** `issue.md` records `Work Mode: full-bug`; therefore `spec.md` is the authoritative AC source.
- **Scope note:** PR context was refreshed because its prior head did not match `67db82a...`; no source code was modified by this review.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md` — only authoritative source for `full-bug`.

### Acceptance criteria

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
| 1 | Full hierarchy router lookup | PASS | `BreadcrumbBridgeRouterIssue439Tests.cs:258-305` sets an exact mailbox-root hierarchy expectation. | Canonical filtered compatibility evidence. | Lookup is deliberately full-path only at the hierarchy-provider boundary. |
| 2 | Direct selection remains relative | PASS | Same direct-row test verifies the filing value is `Clients\North`. | Canonical filtered compatibility evidence. | Full-path result does not cross into filing flow. |
| 3 | Ancestor and child activation remain relative | PASS | `BreadcrumbBridgeRouterIssue439Tests.cs:307-427` contains explicit ancestor and immediate-child Issue609 tests. | Canonical filtered compatibility evidence. | Both navigation paths retain archive-relative targets. |
| 4 | `@` mailbox root receives exactly one prefix | PASS | `EmailFilerConfig_Tests.cs:255-276` uses an `@` mailbox root and relative stem. | Canonical filtered compatibility evidence. | Expected Outlook and filesystem path mapping is asserted. |
| 5 | Existing behavior remains covered | PASS | Existing banner, trash, root-boundary, and relative cases remain in compatibility suite; full coverage suite passed. | `Invoke-MSTestWithCoverage.ps1` evidence, exit 0. | 6,479/6,479 passed. |
| 6 | No prohibited `@` parsing or `Store.FilePath` change | PASS | Exact diff and `issue-609-case-variant-scope-conformance.2026-08-25T14-29.md`. | `git diff --check` and changed-file inspection. | Production diff is limited to FolderPredictor projection. |
| 7 | Narrow, correct startup projection | PASS | `FolderPredictor.cs:845-858` uses `OrdinalIgnoreCase` and root-plus-separator; tests at `FolderPredictorTests.cs:192-249` assert all projection invariants. | Fail-before artifact expected exit 1; focused post-fix 2/2 pass; reviewer-focused test 2/2 pass. | The direct case-variant regression resolves the prior review finding. |
| 8 | Final C# QA and canonical evidence | PASS | Final CSharpier, analyzer, nullable, test/coverage, coverage-comparison, and diff-check artifacts are under the feature `evidence/` folders. | Recorded head commands exited 0; reviewer CSharpier and diff checks exited 0. | Repository coverage 84.7709%; changed method coverage 100%. |

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**

- **PASS:** 8 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:** None.

**Recommended follow-up verification steps:**

1. Continue through the normal PR review and CI workflow.
2. Confirm hosted CI separately when GitHub CLI or PR metadata is available; no local review finding blocks readiness.

## Acceptance Criteria Check-off

All eight authoritative checkbox criteria in `spec.md` were already checked. This review evaluated each item as PASS and made no change to the requirement source, preserving its text and state.

### AC Status Summary

- Source: `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 8 | 8 | 0 | Checkbox-backed, authoritative `full-bug` requirements source; review did not modify it. |
