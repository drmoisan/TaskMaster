# Phase 0 — Instructions read (Issue #824, task P0-T1)

Timestamp: 2026-09-09T14-57

Policy Order: `policy-compliance-order` — CLAUDE.md first, then the cross-language code-change
policy, then the cross-language unit-test policy, then the language- and domain-specific rules for
the files in scope (C#), then the supporting rule files, then the three feature inputs.

## Files read, in the order read

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `.claude/rules/plan-acceptance-gates.md`
- `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/spec.md`
- `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/issue.md`
- `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/research/ilglobals-static-publication-2026-09-08T23-45.md`

## Binding constraints extracted for this run

- Work Mode is `full-bug`; `spec.md` is the sole acceptance-criteria source (AC1..AC12). No
  `user-story.md` exists and none is created.
- C# toolchain order: `dotnet tool run csharpier format .`, the analyzer msbuild, the nullable
  msbuild, then the coverage-enabled test run. Any failure or formatter rewrite restarts the loop.
- `/t:Rebuild` is required on both msbuild gates. `/p:Nullable=enable` is prohibited.
- Tests use MSTest, Moq where mocking is required, and FluentAssertions.
- No `Thread.Sleep`, no `Task.Delay`, no retry, no timing tolerance, no temporary file, and no
  additional thread in any delivered test.
- No policy document, coverage threshold, exclusion list, or analyzer severity is modified.
- No file may exceed 500 lines.

## Coverage-threshold conflict, recorded rather than resolved (plan D10)

CLAUDE.md requires repository-wide line coverage `>= 80%` and `>= 90%` for new modules, classes and
methods. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` require `>= 85%`
line and `>= 75%` branch. The repository coverage runner enforces an 80 percent document-level
floor. This plan gates on AC12's per-class non-regression criterion and reports the document-level
line-rate against both the 80 and the 85 figure. No threshold is lowered, weakened, or deleted.
