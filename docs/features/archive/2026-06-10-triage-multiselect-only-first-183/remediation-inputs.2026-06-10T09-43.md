# Remediation Inputs: triage-multiselect-only-first (Issue #183)

**Generated:** 2026-06-10T09-43
**Feature Folder:** `docs/features/active/2026-06-10-triage-multiselect-only-first-183`
**Base Branch:** `main` (merge-base `c8feca8c`)
**Head Branch:** `bug/triage-multiselect-only-first-183` (implementation commit `a530932f`)
**Work Mode:** `minor-audit`

## Source Audit Artifacts

- `docs/features/active/2026-06-10-triage-multiselect-only-first-183/policy-audit.2026-06-10T09-43.md`
- `docs/features/active/2026-06-10-triage-multiselect-only-first-183/code-review.2026-06-10T09-43.md`
- `docs/features/active/2026-06-10-triage-multiselect-only-first-183/feature-audit.2026-06-10T09-43.md`

## Remediation-Required Findings

### R1 (Blocking): Test file exceeds 500-line file-size limit

- **Severity:** Major / FAIL (policy-conformance)
- **File:** `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`
- **Detail:** The file is 553 lines, exceeding the repository General Code Change Policy 500-line file-size limit. It was 469 lines at baseline `c8feca8c`; the issue #183 regression-test addition crossed the limit. Test code is not an excepted file type (the only exceptions are throwaway scripts, raw text fixtures, and Markdown).
- **Policy reference:** CLAUDE.md General Code Change Policy "File Size Limit"; C# Unit Test Policy structure rules. Policy-audit Sections 2.3, 4 (C#), 8.
- **Required remediation:** Bring the file under 500 lines by splitting the fixture (for example, extract a `Triage_OlLogicTests.TrainSelection.cs` partial class or a separate test fixture file under the same folder), without weakening or removing any existing test. Alternatively, record an explicit approved file-size exception with justification.
- **Constraints for the fix:**
  - Do not modify production behavior; this is a test-organization change only.
  - Preserve all 22 existing test methods and their assertions; no weakening.
  - After the split, re-run the full C# toolchain in order (CSharpier, analyzer build, nullable/TWAE build, MSTest with coverage) and confirm a single clean pass for first-party code.
  - Re-verify that each resulting file is under 500 lines.

## Non-Blocking / Informational (no remediation required)

- The single failing test `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` is pre-existing, unrelated, and identical at baseline and post-change. It does not block this change and is out of scope for issue #183.
- Null/empty `ConversationID` training-bucket behavior in `TrainSelectionAsync` is a documented, intentional design decision (train such items exactly once). No remediation required.

## Acceptance Criteria Status

All five acceptance criteria (AC1–AC5) are PASS with committed evidence (see `feature-audit.2026-06-10T09-43.md`). The remediation item R1 is a policy-conformance breach independent of the acceptance criteria; resolving it does not change any AC verdict.

## Handoff

Route R1 to the atomic planner / executor for a test-file split (or to the author for an explicit file-size exception). No functional change to `Triage_OlLogic.cs` is required.
