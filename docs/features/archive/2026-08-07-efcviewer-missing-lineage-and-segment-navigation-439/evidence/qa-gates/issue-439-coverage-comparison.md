Timestamp: 2026-08-24T19:53:00-04:00
Command: read-only PowerShell Cobertura comparison over `evidence/baseline/issue-439-baseline.cobertura.xml`, `evidence/qa-gates/issue-439-final.cobertura.xml`, and `git diff --unified=0 c83468e2a15560233e20735b0d9a049823fc7613` for the eight plan target files.
EXIT_CODE: `1`
Output Summary: numeric baseline/final repository, per-file, and changed-line coverage comparison.
Baseline Commit: `c83468e2a15560233e20735b0d9a049823fc7613`
Repository baseline/final/delta: `85.58%` / `70.14%` / `-15.43%`
QuickFiler/Controllers/EfcFormController.cs: baseline `COVERAGE_UNAVAILABLE` due to pre-existing `[ExcludeFromCodeCoverage]`; final `9.99%`.
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs baseline/final/delta: `97.87%` / `95.74%` / `-2.13%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs baseline/final/delta: `98.02%` / `97.42%` / `-0.60%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs baseline/final/delta: `100%` / `100%` / `0%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs baseline/final/delta: `100%` / `100%` / `0%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs baseline/final/delta: `95.65%` / `96.12%` / `0.46%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs baseline/final/delta: `96.90%` / `96.99%` / `0.09%`
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: `COVERAGE_UNAVAILABLE — structural no-sequence-point exception`, excluded from per-file numeric arithmetic.
Changed/new instrumentable lines covered/total/percentage: `190` / `321` / `59.19%`
REMEDIATION_REQUIRED: repository coverage is below `80%`; BreadcrumbBridgeRouter and BreadcrumbRow regressed against baseline; changed/new coverage is below `90%`. P4-T6 is not accepted and P4 QA is not clean.

---
## P4-T6 authoritative normalized comparison restart

Timestamp: 2026-08-24T20:37:03-04:00
Command: read-only PowerShell Cobertura comparison using `evidence/qa-gates/issue-439-baseline.normalized.cobertura.xml`, `evidence/qa-gates/issue-439-final.normalized.cobertura.xml`, and `git diff --unified=0 c83468e2a15560233e20735b0d9a049823fc7613 -- '*.cs'`.
EXIT_CODE: `0`
Baseline Commit: `c83468e2a15560233e20735b0d9a049823fc7613`
Normalized Input SHA256: baseline `1286E4AE37B4839C12C59817B37C1FE1283161F789FEF7C2490710A2B5B5A025`; final `0FB03C3B7AD0019004D623F288A9BA6A59992A2E7549FE83DA79636DF172AD5B`.
Output Summary: both inputs satisfy the normalized P3-T6 invariants; all required numeric values are sourced only from those inputs.

Repository baseline/final/delta: `85.5756%` (`53495/62512`) / `84.7835%` (`53757/63405`) / `-0.7921` percentage points. Final repository coverage satisfies `>=80%`.
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs baseline/final/delta: `97.87234%` (`276/282`) / `97.87234%` (`368/376`) / `0` percentage points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs baseline/final/delta: `98.019802%` (`99/101`) / `98.709677%` (`153/155`) / `+0.689875` percentage points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs baseline/final/delta: `100%` (`114/114`) / `100%` (`116/116`) / `0` percentage points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs baseline/final/delta: `100%` (`23/23`) / `100%` (`30/30`) / `0` percentage points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs baseline/final/delta: `95.652174%` (`88/92`) / `96.116505%` (`99/103`) / `+0.464331` percentage points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs baseline/final/delta: `96.899225%` (`125/129`) / `96.992481%` (`129/133`) / `+0.093256` percentage points.
QuickFiler/Controllers/EfcFormController.cs: baseline `COVERAGE_UNAVAILABLE` solely because the baseline retained `[ExcludeFromCodeCoverage]`; final numeric coverage `11.234397%` (`81/721`).
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: `COVERAGE_UNAVAILABLE — sole structural no-sequence-point exception`, excluded from numeric arithmetic.
Changed/new remaining instrumentable production lines: `200/203 = 98.522167%`. The denominator excludes deleted lines and changed lines without normalized sequence points. This passes `>=90%`; P3's historical `190/203 = 93.596059%` was already above the gate and did not drive test additions. The final increase is attributable to the required P3-T7 binding-boundary coverage.

Result: PASS. No sixth-file regression, unavailable numeric value, non-normalized input, or threshold failure occurred.
