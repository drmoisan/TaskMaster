# Phase 0 — Policy Read Evidence (Issue #228)

Timestamp: 2026-06-30T22-07

Policy Order:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C# language-specific rules)
5. .claude/rules/ci-workflows.md (CI workflow authoring rules)
6. .claude/rules/tonality.md (tone policy)

Files Read:
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\.claude\rules\ci-workflows.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-30-17-46\.claude\rules\tonality.md

Output Summary: All six policy files read in the required order. Key constraints noted for this work: C# toolchain order csharpier -> analyzer msbuild -> nullable msbuild (TreatWarningsAsErrors) -> vstest with /EnableCodeCoverage, restart on any change/fail; MSTest + Moq + FluentAssertions for tests; banned APIs DateTime.Now/UtcNow/Random.Shared/Thread.Sleep/Task.Delay; preserve TimeProvider.Delay; repo-wide line coverage >=80% (testable denominator), new/changed code >=90%; 500-line file cap; no temp files in tests; professional tone.
