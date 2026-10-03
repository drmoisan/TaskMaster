# Fail-Before Exception Dossier (P5-T12, PD-10)

Timestamp: 2026-10-03T12-33
Command: CMD-GREP-FACTS (STAGE final), run as pwsh -NoProfile -Command with Set-Location to the item worktree (read-only regex counts over fixed file sets)
EXIT_CODE: 0
Output Summary:
- CS-FILES: 1708
- DEAD-MEMBERS-CS: 0
- TODOMODEL-CSPROJ-MATCHES: 2
- TODOMODEL-CSPROJ-FILES: ToDoModel.Test\ToDoModel.Test.csproj
- NEW-IDENTIFIERS: 18
- SORTEMAIL-FILES: SortEmail.AttachmentSaving.cs,SortEmail.cs,SortEmail.MailItemSort.cs,SortEmail.TrySaveAttachment.cs,SortEmail.UndoAndMoveLog.cs
- SORTEMAIL-FILE-COUNT: 5
- SHOWDIALOG-CALLS-PARTIALS: 0
- ENUM-FIELDS-A: 0
- DEAD-TOKENS-PARTIALS: 0
- BANNED-USINGS-PARTIALS: 0
- USING-SYSTEM-PARTIAL-FILES: 5
- DEBUG-WRITELINE-T: 0
- EFCC-PARTIALS: 18
- TESTS6-PRESENT: 6
- BANNED-TEST-APIS: 0
- NON-APPROVED-FRAMEWORKS: 0
- LEGACY-FILE-EXISTS: False
- TODOMODEL-FILE-EXISTS: False

WhyFailingRunImpossible: The eight entries below are refactors with no observable behavioural defect: using-directive pruning, logger replacement and removal of a rethrow-only catch, deletion of dead or uncompiled code, coverage-exclusion attribute changes, a documentation correction, a seam extraction and a structural-test replacement. No test can be red against a change that preserves behaviour, so a failing run cannot be produced; each entry is instead verified by pin tests that stay green across the step, by the compiler, or by a mutation control.

## Entries

### Entry 1
- Entry: using directives
- Step: D10; P2-T7, P3-T4, P4-T4, P5-T1, P5-T2
- Verification: CMD-USINGS `USINGS-EXACT-FILES: 5` and the P5-T3 production rebuild with warnings as errors (MSBUILD_EXIT_CODE 0, no CS0246, CS0103, CS0104 or CS1061 lines)
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t3-usings-build.2026-10-03T12-20.md

### Entry 2
- Entry: logger replacement and outer-catch removal
- Step: D2; P3-T4
- Verification: pins T7 and T10 of NAMES-T and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`, green in the pass-after run
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-try-save-retry.md (section `## Pass-after (P3-T6)`)

### Entry 3
- Entry: F2 deletions
- Step: D7; P4-T4, P4-T5
- Verification: `DEAD-MEMBERS-CS: 0`, `LEGACY-FILE-EXISTS: False` and the P4-T7 build
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T10-17.md (ITERATION: 2) and docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T11-25.md (ITERATION: 2)

### Entry 4
- Entry: F3 exclusion changes
- Step: D8; P2-T7, P4-T4
- Verification: `EFCC-PARTIALS: 18` and the coverage comparison of P6-T8 (FEATURE/evidence/qa-gates/coverage-comparison.md, written later in Phase 6)
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p2-t7-undo-final-census.2026-10-03T08-48.md and docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t4-attachmentsaving-extract-census.2026-10-03T09-00.md

### Entry 5
- Entry: ToDoModel deletion
- Step: D12; P5-T10
- Verification: `TODOMODEL-FILE-EXISTS: False`, `TODOMODEL-CSPROJ-MATCHES: 2` naming only ToDoModel.Test.csproj, and the Phase 6 rebuilds
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t10-todomodel-deletion.2026-10-03T12-30.md

### Entry 6
- Entry: CR-1
- Step: D9; P5-T11
- Verification: the six `S956-` literal counts (S956-OLD-99 0, S956-NEW-99 1, S956-149 1, S956-NEW-149 1, S956-OLD-155 0, S956-NEW-155 1)
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p5-t11-cr1-spec956.2026-10-03T12-32.md

### Entry 7
- Entry: F1 seam extraction
- Step: D5, PD-14; P4-T4 and the P4-T7 rewrite to the `TrySaveAttachmentDelegate` seam type
- Verification: the compile-red record, the P4-T7 ITERATION: 2 green build and the P4-T8 and P4-T12 green runs
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/compile-red-attachment-saving-seams.md, docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T11-25.md, docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t8-savecase-and-tst1-runs.2026-10-03T11-27.md and docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t12-sortemail-family-run.2026-10-03T12-13.md

### Entry 8
- Entry: structural-test replacement
- Step: D3 phase F1; P4-T2
- Verification: the mutation control (one element removed from `AllPromptSessions` turned `Cleanup_Files_ResetsEveryPromptSession` red with `but found 3`; restoring the fix returned it to green)
- Evidence: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t14-control-applied.2026-10-03T12-15.md and docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/p4-t15-control-restored.2026-10-03T12-17.md

## Acceptance (P5-T12, all four required)

1. Every CMD-GREP-FACTS line matches its final expectation (DEAD-MEMBERS-CS 0, SORTEMAIL-FILE-COUNT 5, SHOWDIALOG-CALLS-PARTIALS 0, ENUM-FIELDS-A 0, DEAD-TOKENS-PARTIALS 0, BANNED-USINGS-PARTIALS 0, USING-SYSTEM-PARTIAL-FILES 5, DEBUG-WRITELINE-T 0, EFCC-PARTIALS 18, TESTS6-PRESENT 6, BANNED-TEST-APIS 0, NON-APPROVED-FRAMEWORKS 0, LEGACY-FILE-EXISTS False, TODOMODEL-FILE-EXISTS False, TODOMODEL-CSPROJ-MATCHES 2 with TODOMODEL-CSPROJ-FILES ToDoModel.Test\ToDoModel.Test.csproj): met.
2. The dossier carries all eight entries: met.
3. Every Evidence: path exists (checked with the Glob tool before this artifact was written): met.
4. The file name matches fail-before-exception.*.md: met.
