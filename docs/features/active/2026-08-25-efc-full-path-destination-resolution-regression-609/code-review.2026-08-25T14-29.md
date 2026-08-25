# Code Review: Efc full-path destination resolution regression (#609)

**Review Date:** 2026-08-25
**Reviewer:** feature-reviewer-c3
**Feature Folder:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609`
**Base Branch:** `origin/main` (`b5c751519c6cf0eaeb2326d9e80b2439aeee7265`)
**Head Branch:** `bug/efc-full-path-destination-resolution-regression-609` (`a8f6c276f4ddf8138f2bc2888536148ef17d4fa2`)
**Review Type:** Post-remediation feature-branch review

## Executive Summary

The branch adds deterministic tests for full hierarchy lookup/archive-relative filing and a narrow `FolderPredictor` startup projection. It preserves router, configuration, data-model, controller, persistence, and filesystem production boundaries. Committed evidence shows the full C# QA suite passed after a restart.

The projection is incomplete: it compares `ArchiveRootPath` with a persisted Outlook hierarchy value using ordinal case-sensitive semantics, while the feature requirements describe the Outlook `FolderPath` contract as case-insensitive. This leaves an in-root suggestion with different casing unprojected and can pass a full destination into the filing flow.

**PR readiness recommendation:** **Needs Revision** — correct the comparison semantics and add a focused case-variant regression before normal PR flow.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 852-858 | `ProjectSuggestionPath` uses `StartsWith(archivePrefix, StringComparison.Ordinal)`. | Use an ordinal case-insensitive root comparison and retain the exact root-plus-separator boundary; add a corresponding test. | Outlook folder identities are case-insensitive per `spec.md`; casing differences in persisted suggestions can otherwise preserve a full in-root value. | Diff inspection; `spec.md` Root Cause Analysis; `FolderPredictorTests.cs` lacks a case-variant input. |

No Blocker findings. The committed QA evidence passes, but the Major correctness finding requires remediation.

## Implementation Audit

### C# implementation audit

- The implementation localizes projection to `FolderPredictor` and keeps `FolderArray` and `FolderRowArray` text and score keys aligned.
- The null-globals guard preserves existing test behavior without changing external contracts.
- No public API is added. Final analyzer and nullable evidence reports zero new diagnostics.
- No exception or logging path changes; the remaining issue is value classification.

## Test Quality Audit

The tests are deterministic and use strict Moq seams. They verify exact full lookup, direct/ancestor/immediate-child archive-relative selection, single `@` mailbox prefixing, suggestion separators, aligned scores, exact-case in-root projection, already-relative preservation, and out-of-root preservation. The missing scenario is a case-variant in-root path.

### Reviewed test and QA artifacts

- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` — direct startup projection regression; missing case-insensitive case.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — exact full lookup and relative selection compatibility.
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` — `@` mailbox single-prefix regression.
- `evidence/regression-testing/issue-609-folder-predictor-fail-before.2026-08-25T14-18.md` — deterministic expected failure.
- `evidence/regression-testing/issue-609-folder-predictor-post-fix.2026-08-25T14-18.md` — targeted post-fix pass.
- `evidence/qa-gates/csharp-tests-coverage-final.2026-08-25T14-18.md` — 6,479/6,479 full coverage suite pass.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection found no credential/config additions. |
| No unsafe subprocess or command construction | PASS | No production process code changed. |
| Input validation at boundaries | PARTIAL | Prefix boundary requires case-insensitive Outlook identity semantics. |
| Error handling remains explicit | PASS | No broad catches or suppressed errors introduced. |
| Configuration / path handling is safe | PARTIAL | Exact root-plus-separator is preserved, but case-sensitive comparison is incomplete. |

## Research Log

No external research was required. The repository specification, refreshed PR context, exact diff, and feature evidence are the reviewed sources.

## Verdict

The branch is not ready for merge. Remediation must change only the startup projection comparison and add deterministic case-variant coverage; no router, configuration, persistence, Outlook COM, or filesystem scope expansion is required.
