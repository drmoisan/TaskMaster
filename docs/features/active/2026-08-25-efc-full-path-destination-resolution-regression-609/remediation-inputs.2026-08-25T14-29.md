# Issue #609 review remediation inputs

**Timestamp:** 2026-08-25T14-29
**Primary requirements source:** this file
**Review finding source:** `policy-audit.2026-08-25T14-29.md`, `code-review.2026-08-25T14-29.md`, and `feature-audit.2026-08-25T14-29.md`.

## Required Fixes

1. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`: change only `ProjectSuggestionPath` so an in-root Outlook hierarchy suggestion matches `ArchiveRootPath` without case sensitivity while still requiring exactly one following `\\` separator. The output must remove exactly that prefix and separator. Already-relative values, root-only values, paths that merely share a textual prefix, and out-of-root paths must remain byte-for-byte unchanged.
   - Verification: a deterministic `Issue609_` test must fail before the change for an in-root path whose root casing differs from `ArchiveRootPath`, then pass after it.
2. `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`: extend the existing direct startup-projection test or add one focused deterministic `Issue609_` test using the existing mocked `IApplicationGlobals`/`IOlObjects` seam. Assert both `FolderArray` and `FolderRowArray` display `Clients\\North` and the associated non-null `FolderScore.FolderPath` is the same archive-relative value for a case-variant in-root full path.
   - Verification: preserve assertions that exact-case in-root values project, already-relative and out-of-root full values do not change, and the suggestion separator remains present.
3. Reconcile Acceptance Criterion 7 in `spec.md` only after the remediation verification is complete, following `acceptance-criteria-tracking`.
   - Verification: post-remediation feature audit marks all eight criteria PASS.
4. Repeat the C# QA loop and record new canonical evidence in `evidence/regression-testing/` and `evidence/qa-gates/`: CSharpier format/check, analyzer rebuild, nullable rebuild, coverage-enabled MSTest, and coverage comparison.
   - Verification: no new failures; repository coverage remains at least 80%; changed/new branch coverage meets the repository threshold.

## Do Not Do

- Do not modify `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Controllers/EfcFormController.cs`, persistence, Outlook COM interactions, filesystem APIs, generic source-map normalization, `Store.FilePath`, or mailbox `@` parsing.
- Do not broaden path normalization beyond the startup suggestion projection.
- Do not weaken policy, acceptance criteria, or test assertions.
- Do not silently skip fail-before/pass-after evidence or any final QA command.
