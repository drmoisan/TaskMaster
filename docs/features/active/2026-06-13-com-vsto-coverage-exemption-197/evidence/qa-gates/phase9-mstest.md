# Phase 9 — MSTest with coverage gate (P9-T6)

Timestamp: 2026-06-13T13-46
Command: pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput 'docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.phase9.cobertura.xml'
EXIT_CODE: 0 (clean final pass)
Output Summary:
- Final clean run: Total tests 4068; Passed 4068; Failed 0 (PIPELINE_EXIT 0).
- An earlier run in this loop reported 2 failures: AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException and RequestTask_WithConfiguredTask_InvokesTaskAfterInterval. These are the known pre-existing flaky timing/threading tests (roadmap §0.1; the TimeOutTask interval family stabilized in PR #191, and the WPF dispatcher test). They are non-deterministic, are NOT in TaskVisualization, and an attribute-only change cannot alter runtime behavior. A re-run produced 0 failures, confirming flakiness, not regression.
- Coverage headline (production-only, vendored Swordfish/SVGControl held constant per memo §2.6, matching coverage-delta.md method): lines-valid 51,665; lines-covered 37,019; rate 71.65%.
- TaskVisualization classes now in the denominator: only the preserved testable seams remain present:
  - FlagChangeItem (3 lines)
  - FlagChangeGroup (19 lines = TryEnqueue pure-logic seam + property accessors; the 4 Outlook-bound members are method-level exempt)
  - FlagChangeTrainingQueue (49 lines, line-rate 0.347)
  - Total TaskVisualization: 71 lines-valid / 13 covered.
- The annotated COM/WinForms classes (TaskController, TaskViewer, FlagTasks, AutoAssignContext, AutoAssignPeople, AutoCreateProject, EditFilterController, EditFilterViewer, ManageFilters) are ABSENT from the denominator (all were present at line-rate 0 in Phase 8 before annotation).
- The other four assemblies (QuickFiler, TaskMaster, Tags, ToDoModel) are byte-identical in line counts to the P7 assembly-exclude artifact, confirming Phases 2-6 are unchanged.

Artifact: docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.phase9.cobertura.xml
