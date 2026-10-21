# Phase 0 — Policy Documents Read (Issue #680)

Timestamp: 2026-08-28T14-55

Policy Order: per `.claude/skills/policy-compliance-order/SKILL.md` — CLAUDE.md first, then the
cross-language code-change policy, then the cross-language unit-test policy, then the
language/domain-specific rules for the files in scope (C#), then the supporting repository rules.

Files read (in order):

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`

Count: 6 of 6 files read.

## Key constraints carried into execution

- C# toolchain order is format (`dotnet tool run csharpier format .`) -> analyzers
  (`msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) ->
  nullable (`msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true`) -> tests with coverage.
  Restart from step 1 whenever a step fails or rewrites a file.
- `/t:Rebuild` is mandatory locally; `/t:Build` can skip `CoreCompile` and run no analyzers.
- Do not pass `/p:Nullable=enable`; nullable is per-file opt-in via `#nullable enable`.
- MSTest + Moq + FluentAssertions for all C# tests; Arrange-Act-Assert structure.
- 500-line ceiling for every production, test, and reusable script file.
- No temporary files in tests; no external dependencies in unit tests.
- Repository-wide line coverage >= 80% on the testable denominator (CLAUDE.md UT2);
  new/changed members >= 90%; no regression on changed lines.
- Professional, neutral tone in all artifacts; no humor, hyperbole, or decorative metaphor.
