# Phase 0 — Instructions Read Receipt

Timestamp: 2026-07-19T00-00

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md

Files read (in required order):
1. C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9450e42e00ec9d58/CLAUDE.md (standing instructions; C# toolchain section — csharpier → msbuild analyzers/codestyle → msbuild nullable (TreatWarningsAsErrors) → vstest coverage)
2. C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9450e42e00ec9d58/.claude/rules/general-code-change.md (cross-language code change policy; 500-line file limit, fail-fast error handling, toolchain loop)
3. C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9450e42e00ec9d58/.claude/rules/general-unit-test.md (cross-language unit test policy; determinism, no temp files, coverage-no-regression on changed lines)
4. C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9450e42e00ec9d58/.claude/rules/csharp.md (C#-specific toolchain and standards; MSTest + Moq + FluentAssertions, nullable annotations, analyzer stack)

Feature-specific scope invariants confirmed (from plan Scope Invariants and issue.md Architecture):
- Per-file `#nullable enable` pragma opt-in ONLY; no `<Nullable>` element added to UtilitiesCS/UtilitiesCS.csproj (AC2).
- Verification via per-file pragma gate (`msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable`).
- net481 / C# 12: nullable post-condition attributes ([NotNullWhen], [MaybeNullWhen], etc.) are NOT available and MUST NOT be used/polyfilled.
- Annotation and null-safety ONLY; no behavior changes, no refactors (AC3, AC5).
- ArrayExtensions.cs is annotation-only (not split); DfDeedle.EmailRecord stays a plain private struct.
