# Phase 0 — Policy Instructions Read (issue #742)

Timestamp: 2026-09-14T01-55

Policy Order: the five policy files below were read in this exact order, per [P0-T1] of
`docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/plan.2026-09-12T16-09.md`
and per the repository's `policy-compliance-order` skill.

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/tonality.md`

## Files Read (explicit list)

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/tonality.md`

## Output Summary

All five policy files were read in full from the item worktree
`C:/Users/DanMoisan/repos/TaskMaster-wt/bugs-2026-09-11-item-742`.

Constraints carried into execution:

- C# toolchain order is format (`dotnet tool run csharpier format .` then `check .`), analyzers
  (`msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), nullable
  (`msbuild ... /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`), then tests. Restart
  from step 1 whenever a step fails or rewrites a tracked file.
- `/t:Rebuild` is required for the analyzer and nullable gates; a warm `/t:Build` skips
  `CoreCompile` and cannot fail.
- Tests use MSTest, Moq, and FluentAssertions; no temporary files; no `Thread.Sleep` or
  `Task.Delay`; deterministic time via `TimeProvider` / `FakeTimeProvider`.
- `CLAUDE.md` carries a `## Committed Test Evidence Format` section stating that raw coverage
  collector documents and raw test-platform (`.trx`) documents must not be added to git in any
  form, including under a feature folder's evidence tree. This matches this plan's
  evidence-artifact discipline: raw output is written under the gitignored `coverage\` directory,
  transcribed into Markdown, and deleted.
- Tonality policy: professional, factual, neutral; no humor, hyperbole, or decorative metaphor in
  any artifact written by this execution.
