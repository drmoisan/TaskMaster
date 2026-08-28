# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-08-28T15-41
Command: Read (file reads only; no shell command)
EXIT_CODE: 0

Policy Order: `CLAUDE.md` → `.claude/rules/general-code-change.md` → `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md` → `.claude/rules/quality-tiers.md` → `.claude/rules/plan-acceptance-gates.md` → feature requirement documents (`issue.md`, `spec.md`, research artifact).

## Files Read (all nine)

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/plan-acceptance-gates.md`
7. `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/issue.md`
8. `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/spec.md` (v0.3)
9. `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/research/2026-08-28T09-15-quickfiler-outlook-keyboard-suppression-677-research.md`

## Output Summary

All nine files read in the stated order. Governing constraints extracted and carried into execution:

- Bugfix workflow is binding: failing regression test first (Phase 1, compile-red per plan D2), minimal targeted fix (Phases 2-3), full toolchain verification last (Phase 5).
- C# toolchain order is format (CSharpier via `dotnet tool run`) → analyzers (`msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) → nullable (`msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true`, never `/p:Nullable=enable`) → `vstest.console.exe` with coverage. Restart from step 1 on any failure or file-changing auto-fix.
- MSTest + Moq + FluentAssertions are mandatory for C# tests; no temporary files in tests; no external process/network dependencies.
- 500-line ceiling applies to every production and test file touched.
- Coverage: repository-wide line coverage floor per CLAUDE.md (>= 80% against the ratified COM/VSTO/WinForms exemption denominator); new/changed code >= 90%; no regression on changed lines.
- Work Mode is `full-bug` (from `issue.md` metadata), so `spec.md` is the sole acceptance-criteria source and `user-story.md` is intentionally absent.
- Plan-acceptance-gate rules G1-G6 read; this plan's acceptance conditions use single-line non-interpolated literals and named tests.
- Out-of-scope invariant confirmed from `spec.md` Scope & Non-Goals and the research artifact: `QuickFiler/Controllers/KeyboardHandler.cs` is not modified.
