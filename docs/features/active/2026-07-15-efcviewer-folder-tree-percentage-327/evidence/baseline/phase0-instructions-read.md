# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-07-16T00-00

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Files read (in policy-compliance order):
1. CLAUDE.md (project standing instructions; loaded into session context)
2. .claude/rules/general-code-change.md (cross-language code change policy; loaded into session context)
3. .claude/rules/general-unit-test.md (cross-language unit test policy; loaded into session context)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards; read from worktree)

Key constraints acknowledged for this feature:
- C# toolchain order: csharpier -> analyzers msbuild -> nullable msbuild -> vstest with coverage; restart on any failure or file change.
- Tests: MSTest + Moq + FluentAssertions; deterministic; no temp files / COM / network.
- New host-neutral modules under UtilitiesCS/OutlookObjects/Folder/ target >= 90% coverage; repository floor must not regress; no changed line may lose coverage.
- WinForms Form-derived + Designer-generated files and EfcFormController are coverage-exempt via [ExcludeFromCodeCoverage]; verified by build + manual QA.
- Legacy packages.config projects require an explicit <Compile Include> item for every new .cs file.
- Evidence written ONLY under docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/<kind>/.
