## [P0-T1] Phase 0 Instructions Read

- Timestamp: 2026-08-08T20-45
- Command: `pwsh -NoProfile -Command "Test-Path CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/csharp.md, docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/remediation-inputs.2026-08-08T13-25.md, docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: All six `Test-Path` checks returned `True`. All policy and requirement files read and confirmed present.

### Policy Order

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `<FEATURE>/remediation-inputs.2026-08-08T13-25.md` (R1 section and do-not-do list)
6. `<FEATURE>/spec.md` § Acceptance Criteria

### Files Read

- `CLAUDE.md` — repo-standing instructions (already loaded into session context; content confirmed present on disk via `Test-Path`).
- `.claude/rules/general-code-change.md` — already loaded into session context; content confirmed present on disk.
- `.claude/rules/general-unit-test.md` — already loaded into session context; content confirmed present on disk.
- `.claude/rules/csharp.md` — read in full this task: CSharpier / .NET analyzer / nullable / MSTest+Moq+FluentAssertions toolchain, DI seam ordering, analyzer stack (Meziantou, SonarAnalyzer, Roslynator, AsyncFixer, BannedApiAnalyzers), prohibited behaviors (no weakening assertions, no reporting success without running toolchain).
- `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/remediation-inputs.2026-08-08T13-25.md` — read in full: single blocking finding R1 (branch coverage gap in `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`, 50% branch/100% line), R2 non-blocking disposition item (out of scope), do-not-do list (no production edit, no `[ExcludeFromCodeCoverage]`, no weakening existing tests, no EfcViewer/suggestions/gesture edits, 500-line ceiling, no policy/spec/#400-folder edits).
- `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md` § Acceptance Criteria — confirmed AC-1 through AC-14 are all currently `[x]`; HV-1 is a documented non-merge-gate human-verification item (unchecked by design, not a gating AC).

### Confirmation

AC-1 … AC-14 are all `[x]` in `spec.md` as of this read. No spec edit is planned or permitted by this remediation cycle.
