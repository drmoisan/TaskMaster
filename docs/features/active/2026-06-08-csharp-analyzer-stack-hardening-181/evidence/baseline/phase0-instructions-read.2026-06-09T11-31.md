# Phase 0 — Policy Instructions Read

Timestamp: 2026-06-09T11-31

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards)

Files Read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md

Key constraints carried into this cycle:
- Deterministic Test Rules (.claude/rules/csharp.md): no wall-clock waits; use seam-based mocking for clocks/timers.
- Prohibited Behaviors: no weakening assertions; no adding sleeps/retries/timing hacks to mask flakiness; no reporting success without running the required toolchain.
- Toolchain order (exact): csharpier -> analyzer build -> nullable build (TreatWarningsAsErrors) -> vstest with coverage. Restart from csharpier on any change/failure.
- Coverage: repo-wide line coverage >= 80%; new/changed methods target >= 90%; no regression on changed lines.
- MSTest + Moq + FluentAssertions only; no temp files; no external dependencies in tests.
- No new NuGet packages; reuse ITimerWrapper/IGenericTimer; single new test helper ManualFireTimerWrapper.cs.
- OUT OF SCOPE (never touch/revert/stage): UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs and UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs.
