# 2026-05-07-efc-form-populate-folder-null-ref (Plan)

- **Issue:** #145
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-07T13-39
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** minor-audit

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

**Requirements source:** `docs/features/active/2026-05-07-efc-form-populate-folder-null-ref-145/issue.md` (AC1–AC4)

---

## Phase 0 — Baseline Capture

- [x] [P0-T1] Record current branch: `bug/efc-form-populate-folder-null-ref-145`; confirm working tree is clean.
- [x] [P0-T2] Run the existing test suite and record baseline pass/fail summary. Evidence: `artifacts/orchestration/145-phase0-baseline.txt` — 3990 total, 3988 passed, 2 skipped, 0 failed.
- [x] [P0-T3] Confirm `EfcFormControllerTests.cs` does not yet exist in `QuickFiler.Test/Controllers/`. Evidence: `Test-Path` returned `False`.

---

## Phase 1 — Implementation (small path, constrained)

- [x] [P1-T1] In `QuickFiler/Controllers/EfcFormController.cs`, method `PopulateFolderCombobox`, add null guard immediately after `await _dataModel.InitFolderHandlerAsync(folderList)`:
  ```csharp
  if (_formViewer is null) return; // Guard: Cleanup() may have run during the await above
  ```
  Satisfies AC2.
- [x] [P1-T2] Create `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` with a regression test class. The test must document the race-condition bug and verify the maximum unit-testable behavior (see constraint note). Satisfies AC1 and AC3.

  **Unit-test constraint note:** `EfcDataModel.InitFolderHandlerAsync` unconditionally delegates to `Task.Run` with real Outlook COM objects; a COM-free unit test cannot exercise the full async race path. The regression test will use the established `EfcHomeControllerTests` reflection pattern: create the controller via its private no-arg constructor, pre-set `_dataModel` and `_formViewer` fields via reflection to null, and confirm the method throws on `_dataModel` (first dereference) rather than on `_formViewer`—documenting the ordering that makes the guard correct.

---

## Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format .` — no changes to our files. ✅
- [x] [P2-T2] Run `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — 0 errors, 0 warnings. ✅
- [x] [P2-T3] Run `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — 0 errors, 0 warnings. ✅
- [x] [P2-T4] Full test suite — 3991 total, 3989 passed, 2 skipped, 0 failed. New test `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel` passes. Evidence: `artifacts/orchestration/145-phase2-test.txt` ✅
- [x] [P2-T5] AC1–AC4 satisfied. Updated AC checkboxes in `issue.md`. ✅
