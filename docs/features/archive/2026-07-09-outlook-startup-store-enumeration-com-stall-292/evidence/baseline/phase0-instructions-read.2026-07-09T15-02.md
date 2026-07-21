# Phase 0 — Policy Read Evidence (issue #292)

- Timestamp: 2026-07-09T15-02
- Task: [P0-T1]

## Policy Order

Policies were read in the mandatory order defined by `policy-compliance-order`:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language-specific rule for C# files in scope: `.claude/rules/csharp.md`
5. `.claude/rules/architecture-boundaries.md` (No-COM architecture boundaries)
6. `.claude/rules/quality-tiers.md` (module rigor tiers T1-T4)

## Files Read

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/architecture-boundaries.md`
- `.claude/rules/quality-tiers.md`

## Key Constraints Extracted

- C# toolchain order: csharpier format -> analyzer msbuild -> nullable msbuild -> vstest with coverage; restart from step 1 on any change/failure.
- MSTest + Moq + FluentAssertions; no live Outlook; no temp files; no sleeps/retries/timing hacks.
- File-size cap 500 lines for production/test/reusable script files.
- Repository-wide line coverage >= 80%; new/changed code >= 90%; no regression on changed lines.
- Do not weaken assertions or exclude production files from coverage.
- Legacy packages.config projects require explicit `<Compile Include>` wiring (no glob).
