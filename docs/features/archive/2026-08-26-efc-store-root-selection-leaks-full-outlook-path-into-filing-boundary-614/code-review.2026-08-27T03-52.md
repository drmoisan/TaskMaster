# Code Review: Issue 614

**Review Timestamp:** 2026-08-27T03-52
**Exact Head:** `eaf29fb1b1341a0217e5feb4759cd22fd1deb8d6`
**Base Branch:** `main`
**Merge Base:** `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
**Review Scope:** Complete feature-vs-main diff and remediation cycles 1–3

## Executive Summary

The complete Issue 614 branch diff was reviewed without scope narrowing. The implementation establishes an explicit separation between Outlook hierarchy paths used for display/navigation and archive-relative paths used at the filing boundary. The guards and contracts are focused, invalid states fail explicitly, and the cycle-3 `ApplicationGlobals` environment-reader seam preserves existing production construction while making hosted tests deterministic.

No blocker or major code finding was identified. Independent exact-head C# QA passed: formatting, analyzer rebuild, nullable rebuild, 6,587 / 6,587 tests, repository line coverage 84.8796%, branch coverage 78.8657%, and 100% line coverage for changed production methods. The two approved documentation/evidence defects are disclosed as informational accepted risks and do not change the code-review verdict.

**Verdict:** PASS
**Blocking findings:** 0

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info — accepted risk | `evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` | Normalized result | The preserved evidence reports `FAIL` because it combines different expectation sources, despite authoritative exact-head QA passing. | Do not modify this artifact solely for this finding. Continue to disclose the approved limitation. | The user explicitly approved skipping this documentation/evidence defect, and the checkpoint limits that approval to this artifact. | `human_interaction.requirements[0]`; SHA-256 `CCA698B1CFB2EDFF6B768C45749F7C08038033AFD705DD1CA863E945AD7F6D5D`. |
| Info — accepted risk | `change-description.2026-08-26.md` | Issue cross-reference | The change description uses `#637` where `#638` is intended. | Do not modify this artifact solely for this finding. Disclose it until the accepted risk is retired. | The defect is documentation-only and is explicitly covered by the recorded user scope decision. | `human_interaction.requirements[0]`; SHA-256 `679C6A759BCE3D5388986CCB77552925782DF60ADC39F2DAD939BC06C8D05943`. |
| Info | `coverage/coverage.cobertura.xml` | Exact-head independent run | Exact-head aggregate coverage differs slightly from the cycle-3 executor report: 84.8796% line and 78.8657% branch versus 84.8938% and 78.8780%. | Retain the independently measured values in review evidence and continue enforcing repository and changed-method thresholds. | The variation changes covered-line counts but does not reduce coverage below the original or cycle-3 baseline and does not affect changed-method 100% coverage. | Independent canonical runner: 53,986 / 63,603 lines; 12,751 / 16,168 branches; 6,587 / 6,587 tests. |

## Implementation Review

### Filing-boundary contract

`ArchiveStemContract`, `EfcSelectionGuard`, and `ArchiveRootPathGuard` encode separate responsibilities: normalize the archive stem, validate the selected root, and prevent a hierarchy-derived full path from crossing into archive-relative filing. This structure directly addresses the reported boundary leak and keeps policy decisions out of UI glue.

### Producer and consumer integration

The changes to `EfcDataModel`, `EfcFormController`, `AppFileSystemFolderPaths`, `AppOlObjects`, `EmailFilerConfig`, `FolderConverter`, and the breadcrumb routing path were reviewed together. The resulting flow retains the hierarchy path for display and navigation while producing an archive-relative value for filing. Regression tests cover the producer boundary and interactions with Issues 609, 439, and 499.

### Cycle-3 environment seam

The new `ApplicationGlobals` constructor accepts a `Func<string, string>` environment reader and passes it to `AppFileSystemFolderPaths`. Existing constructors pass the production default, so runtime behavior remains unchanged. Test constructors inject a fixed `OneDriveCommercial` value, eliminating dependence on the hosted test machine's environment. The full constructor call-site census was reviewed, including the eight adapted test files.

### Error handling and redaction

Invalid root states are rejected explicitly. Tests cover path-redaction behavior so diagnostics do not expose sensitive full Outlook paths. No broad exception swallowing or new ad hoc production console output was identified in the reviewed diff.

### Scope and maintainability

The branch contains 11 changed production C# files and 19 changed test C# files. The production changes align with Issue 614 and the recorded remediation findings. No new external dependency or breaking public API was identified. Changed files remain within the repository file-size constraint.

## Test Review

The tests cover positive, negative, boundary, case-variant, producer-integration, prior-issue interaction, redaction, and hosted-environment scenarios. They use MSTest and repository-standard isolation patterns. Cycle 3 specifically converts environment-sensitive initialization to deterministic injected inputs.

Independent execution at exact head produced:

- CSharpier: PASS, 1,530 files checked.
- Analyzer rebuild: PASS, 0 errors; five pre-existing `System.Reactive` package warnings.
- Nullable rebuild: PASS, 0 errors; the same five pre-existing warnings.
- MSTest with coverage: PASS, 6,587 passed and 0 failed across 9 assemblies.
- Line coverage: 84.8796%, above the 84.7797% merge-base baseline.
- Branch coverage: 78.8657%, above the 78.6938% merge-base baseline.
- Changed production methods: 100% line coverage.

## Acceptance and Risk Notes

AC24 remains unchecked and PARTIAL because its literal command/path wording is not fully met. The authoritative repository C# QA runner did pass, but the accepted documentation/evidence scope decision does not authorize editing `spec.md`. The approval does not apply to code behavior, tests, coverage, CI, review, or strict-validator failures.

## Recommendation

The exact review head is ready to proceed from code review. No code remediation plan is required. PR readiness remains contingent on the coordinating orchestration's remaining CI and strict artifact-validation gates. The accepted documentation/evidence risks and unchecked AC24 must remain disclosed.

**REVIEW VERDICT: PASS**
**BLOCKING FINDING COUNT: 0**
