# Feature Audit: Issue 614

**Audit Timestamp:** 2026-08-27T03-52
**Feature:** EFC store-root selection leaks full Outlook path into filing boundary
**Exact Head:** `eaf29fb1b1341a0217e5feb4759cd22fd1deb8d6`
**Base Branch:** `main`
**Merge Base:** `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`

## Scope and Baseline

This audit covers the complete feature-vs-main diff and remediation cycles 1–3. The primary scope sources are `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, and this feature folder's `spec.md`. The PR context is anchored to the exact review head and the stated merge base.

The source marks this as a `full-bug` feature. The branch contains the archive-stem contract and path guards, producer/consumer integration changes, regression tests, and cycle-3 hosted-test remediation. Independent exact-head QA passed all required C# checks and all 6,587 tests.

The recorded user scope decision in `artifacts/orchestration/orchestrator-state.json` at `human_interaction.requirements[0]` preserves exactly two documentation/evidence accepted risks. It does not waive code behavior, tests, coverage, CI, review, or strict validator failures.

## Acceptance Criteria Inventory

- Acceptance-criteria source: `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`
- Total criteria: 26
- Checked in source: 25
- Unchecked in source: 1 (`AC24`)
- Source SHA-256: `E64AB58252595DF0B2BC86AA58E44F0B82955BB6525B356D734B3BD7E6A79AC5`

## Acceptance Criteria Evaluation

| AC | Status | Verification |
|---|---|---|
| AC1 — Archive-stem contract type | PASS | `ArchiveStemContract` exists with focused normalization and validation behavior. |
| AC2 — Decision D1 | PASS | Archive stem handling is explicit and covered by unit tests. |
| AC3 — Decision D2 | PASS | Selection validation prevents invalid store-root state from entering filing. |
| AC4 — Decision D3 | PASS | Full hierarchy and archive-relative path responsibilities are separated. |
| AC5 — Decision D4 | PASS | Root-path guarding is implemented and regression tested. |
| AC6 — Decision D5a | PASS | EFC data-model integration uses the explicit contract. |
| AC7 — Decision D5b | PASS | Form-controller integration preserves the intended boundary. |
| AC8 — Decision D5c | PASS | File-system folder-path integration preserves hierarchy behavior without leaking it into filing. |
| AC9 — Decision D5d | PASS | Outlook-object producer integration is covered by regression tests. |
| AC10 — Decision D5e | PASS | Email-filer configuration receives archive-relative state. |
| AC11 — Decision D5f | PASS | Folder conversion honors the boundary contract. |
| AC12 — Decision D5g and dead parameter removal | PASS | Reviewed integration removes the obsolete path flow and its dead parameter use. |
| AC13 — Decision D6 | PASS | Failure behavior is explicit and tested for invalid selections. |
| AC14 — Decision D7 | PASS | Case-variant path handling is covered. |
| AC15 — Decision D8 | PASS | Sensitive full paths are redacted from relevant diagnostics. |
| AC16 — Decision D9 | PASS | Hierarchy-only display/navigation behavior is preserved. |
| AC17 — Primary regression coverage | PASS | Tests reproduce and protect the reported full-path filing-boundary defect. |
| AC18 — Producer companion coverage | PASS | Producer-level tests verify the archive-relative output boundary. |
| AC19 — Issue 609 and 439 non-regression | PASS | Explicit interaction tests verify prior archive-root and breadcrumb behavior. |
| AC20 — Issue 499 interaction | PASS | Regression coverage verifies the related initialization/path interaction. |
| AC21 — Redaction | PASS | Tests verify that sensitive hierarchy paths are not exposed in diagnostics. |
| AC22 — Unit-test policy | PASS | Changed tests are deterministic, isolated, and use repository MSTest conventions. |
| AC23 — Coverage | PASS | Exact-head line coverage is 84.8796%; changed production methods are 100% line covered. |
| AC24 — Full four-step toolchain | PARTIAL — ACCEPTED RISK | The authoritative formatter, analyzer, nullable, and canonical coverage runner all passed. The literal direct `vstest.console.exe ... /EnableCodeCoverage` command and `<FEATURE>/evidence/qa/` path wording are not fully met. AC24 remains unchecked and `spec.md` remains unchanged under the recorded user decision. |
| AC25 — Scope and file-size limits | PASS | Changed implementation is Issue 614 scoped; no newly introduced file exceeds the repository limit. |
| AC26 — Manual validation | PASS | Feature evidence records the required manual validation, and automated regression coverage independently supports the boundary behavior. |

## Summary

| Result | Count |
|---|---:|
| PASS | 25 |
| PARTIAL — accepted risk | 1 |
| UNVERIFIED | 0 |
| FAIL | 0 |
| Blocking findings | 0 |

**Overall Feature Readiness:** PASS

The implementation satisfies all code-behavior, test, coverage, and regression criteria reviewed at the exact head. AC24 remains partially met because the literal evidence command/path convention is not satisfied; it is intentionally left unchecked. The two related documentation/evidence defects remain exactly as approved:

1. `evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` preserves the mixed-expectation normalized `FAIL` (SHA-256 `CCA698B1CFB2EDFF6B768C45749F7C08038033AFD705DD1CA863E945AD7F6D5D`).
2. `change-description.2026-08-26.md` preserves the incorrect `#637` reference where `#638` is intended (SHA-256 `679C6A759BCE3D5388986CCB77552925782DF60ADC39F2DAD939BC06C8D05943`).

These accepted risks are nonblocking only within the recorded scope decision. They do not extend to any later code, test, coverage, CI, review, or validator failure.

## Acceptance Criteria Check-off

No source checkbox was changed during review. `spec.md` remains at 25 checked criteria and one unchecked criterion, AC24. This preserves the coordinating adjudication and the exact approved risk disclosure.

## Recommendation

The feature is ready to proceed from feature review with zero blocking findings. No remediation-inputs or new remediation-plan artifact is required for review cycle 3. The coordinating orchestration should continue through its remaining CI and strict artifact-validation completion gates while retaining the accepted-risk and AC24 disclosures.

**FEATURE AUDIT VERDICT: PASS**
**BLOCKING FINDING COUNT: 0**
