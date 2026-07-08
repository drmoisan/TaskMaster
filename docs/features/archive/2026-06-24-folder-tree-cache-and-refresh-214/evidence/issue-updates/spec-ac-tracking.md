# Spec Acceptance Criteria Tracking

Issue: 214

## Checked Criteria

- AC1: Supported by the canonical feature folder, plan record, and issue #214 evidence paths.
- AC2: Supported by folder tree service implementation/tests and caller migration search evidence.
- AC3: Supported by caller migration evidence showing direct `Task.Run(() => new FolderTree...)` removal from in-scope callers.
- AC4: Supported by `IDispatcherYield`, `WpfDispatcherYield`, `IDeadlineClock`, builder implementation, and focused folder snapshot builder yield tests.
- AC5: Supported by `banned-api-search.md`.
- AC6: Supported by folder snapshot builder cancellation/deadline tests and final banned API evidence.
- AC7: Supported by notification sink/service invalidation tests.
- AC8: Supported by snapshot key/store identity tests and multi-store snapshot tests.
- AC9: Supported by notification sink disposal and service disposal tests.
- AC10: Supported by compatibility view disposal tests and filter controller disposal tests.
- AC11: Supported by folder tree service concurrency, state, invalidation, and stale-current tests.
- AC12: Supported by `FilterOlFoldersController_Tests` and `SubjectMapSco_Orchestration_Tests`.
- AC13: Supported by fake hierarchy, fake dispatcher yield, fake notification, resolver, cancellation, and caller migration tests.
- AC14: Supported by `issue-214-startup-scope-exclusion-final.md`.
- AC15: Supported by final C# QA evidence showing CSharpier, analyzer build, nullable/TWAE build, MSTest coverage, comparable coverage thresholds, and file-size compliance passed.

## Evidence Paths

- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/other/caller-migration-search.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/other/banned-api-search.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/other/no-live-outlook-com-tests.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/other/issue-214-startup-scope-exclusion-final.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-csharpier.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-dotnet-analyzers.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-nullable.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-mstest-coverage.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/other/file-size-check.md`
- Focused test evidence from Phase 1 through Phase 7 command outputs in this execution session.

## Unchecked Criteria

- None.
