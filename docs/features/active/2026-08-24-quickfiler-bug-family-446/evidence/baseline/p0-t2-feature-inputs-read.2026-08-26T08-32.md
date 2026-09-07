# [P0-T2] Feature Inputs Read and Stale Pointer Correction

Timestamp: 2026-08-26T08-32

Task: [P0-T2]
Feature: docs/features/active/quickfiler-bug-family-446
Work Mode: full-bug

## Files Read

1. `docs/features/active/quickfiler-bug-family-446/spec.md` (976 lines)
2. `docs/features/active/quickfiler-bug-family-446/issue.md` (88 lines)
3. `docs/features/active/quickfiler-bug-family-446/research/2026-08-24T09-50-quickfiler-queue-datamodel-defects-research.md` (999 lines)

## Acceptance-Criteria Identifiers (28)

The acceptance-criteria source for work mode `full-bug` is `spec.md` only. The 28 criterion
identifiers, in the order they appear at `spec.md:875-911`, are:

AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17,
AC18, AC19, AC20, AC21, AC22, AC23, AC24, AC25, AC26, AC27, AC28.

Count: 28. All 28 are unchecked (`- [ ]`) at the time of this read.

Grouping in `spec.md`:

- Failing-first regression tests: AC1 through AC5 (`spec.md:875-879`)
- Behavioural correctness: AC6 through AC16 (`spec.md:883-893`)
- Scope containment: AC17 through AC22 (`spec.md:897-902`)
- Test-quality and toolchain: AC23 through AC28 (`spec.md:906-911`)

## Owned Production Paths (8)

Listed in the spec's Scope and Non-Goals section, "In scope — files this feature owns"
(`spec.md:213-229`), and matching the "Files This Feature Owns" list in `issue.md:59-68`:

1. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` (177 lines at base)
2. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (177 lines at base)
3. `QuickFiler/Controllers/QfcDatamodel.cs` (496 lines at base)
4. `QuickFiler/Controllers/QfcFormController.Actions.cs` (302 lines at base)
5. `QuickFiler/Controllers/QfcHomeController.Iteration.cs` (86 lines at base)
6. `QuickFiler/Interfaces/IQfcDatamodel.cs` (59 lines at base)
7. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (235 lines at base; expected untouched under Scope 427-A)
8. `QuickFiler/Helper Classes/EmailMoveMonitor.cs` (262 lines at base; expected untouched — the #426 defect is in the caller)

Count: 8. Of these, this plan writes six (items 1 through 6); items 7 and 8 are owned but expected
to remain unmodified.

## Document Pointer Correction

`spec.md:970-971` named the non-existent folder `docs/features/active/quickfiler-queue-datamodel-defects-446/`.
Both lines were corrected to the real folder `docs/features/active/quickfiler-bug-family-446/`.
Only the two document-pointer lines in the Links section were edited; no acceptance-criteria text
was modified.

Command: `git grep -c "quickfiler-queue-datamodel-defects-446" -- "docs/features/active/quickfiler-bug-family-446/spec.md"`
EXIT_CODE: 1
Output: (no output — no match)

## Output Summary

All three feature inputs read. 28 acceptance-criteria identifiers enumerated (AC1 through AC28).
8 owned production paths enumerated. The two stale document pointers at `spec.md:970-971` were
corrected and `git grep -c` now reports no match for the stale folder name in `spec.md`
(exit code 1, which is `git grep`'s no-match exit).
