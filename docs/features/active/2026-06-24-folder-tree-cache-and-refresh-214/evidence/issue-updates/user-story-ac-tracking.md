# User Story Acceptance Criteria Tracking

Issue: 214

## Checked Criteria

- AC1: Supported by canonical issue #214 feature folder and plan/evidence paths.
- AC2: Supported by shared folder tree service implementation, app-globals ownership, and caller migration tests.
- AC3: Supported by `caller-migration-search.md`.
- AC4: Supported by removal of `Task.Run(() => new FolderTree...)` in issue #214 caller paths.
- AC5: Supported by dispatcher-yield and deadline-clock seams with builder tests.
- AC6: Supported by `banned-api-search.md`.
- AC7: Supported by cancellation/deadline snapshot builder tests.
- AC8: Supported by notification sink and service invalidation tests.
- AC9: Supported by store-aware key/snapshot tests.
- AC10: Supported by notification sink and service disposal tests.
- AC11: Supported by compatibility-view and filter-controller disposal tests.
- AC12: Supported by service concurrency/state tests.
- AC13: Supported by filter-controller and subject-map caller-local selection tests.
- AC14: Supported by fake hierarchy, fake clock, fake dispatcher-yield, fake notification, fake resolver, and no-live-Outlook-COM search evidence.
- AC15: Supported by `issue-214-startup-scope-exclusion-final.md`.
- AC16: Supported by final C# QA evidence showing CSharpier, analyzer build, nullable/TWAE build, MSTest coverage, comparable coverage thresholds, and file-size compliance passed.

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
