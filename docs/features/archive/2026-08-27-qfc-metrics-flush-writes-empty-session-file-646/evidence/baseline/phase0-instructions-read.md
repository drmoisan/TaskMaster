# Phase 0 — Policy Instructions Read (Issue #646)

Timestamp: 2026-09-01T12-09

Policy Order: The `policy-compliance-order` skill sequence was followed exactly, in this
order:

1. `CLAUDE.md` (repository root) — standing instructions, always loaded
2. `.claude/rules/general-code-change.md` — cross-language code change policy
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy
4. `.claude/rules/csharp.md` — C#-specific rules, applicable because both in-scope source
   files are `*.cs`

## Files Read

| # | Repository-relative path | Plan task | Read in full |
|---|---|---|---|
| 1 | `CLAUDE.md` | P0-T1 | Yes (448 lines) |
| 2 | `.claude/rules/general-code-change.md` | P0-T2 | Yes (81 lines) |
| 3 | `.claude/rules/general-unit-test.md` | P0-T3 | Yes (106 lines) |
| 4 | `.claude/rules/csharp.md` | P0-T4 | Yes (95 lines) |

## Constraints Extracted and Carried Into Execution

- C# toolchain order is format, lint, type-check, test; restart from step 1 on any failure
  or file rewrite (`CLAUDE.md` CUT3; `.claude/rules/csharp.md` Toolchain).
- Both `msbuild` gates use `/t:Rebuild`, not `/t:Build`; a warm `/t:Build` skips
  `CoreCompile` and returns exit 0 without running analyzers or nullable-flow diagnostics
  (`CLAUDE.md` C#1.2 and C#1.3).
- Do not pass `/p:Nullable=enable`; nullable enforcement in this repository is per-file
  opt-in via `#nullable enable` (`CLAUDE.md` C#1.3).
- CSharpier is invoked through `dotnet tool run` so the manifest-pinned version is used.
- Tests use MSTest, Moq, and FluentAssertions (`CLAUDE.md` CUT1/CUT2).
- No file may exceed 500 lines (`.claude/rules/general-code-change.md`, File Size Limit).
  Tracked for this item by plan task P1-T15 against the test file.
- Temporary files in tests are prohibited (`.claude/rules/general-unit-test.md`, External
  Dependencies). The new regression test uses only in-memory delegate capture.
- Bugfix workflow applies (this item is a defect): failing regression test first, then the
  minimal targeted fix, then full local verification (`CLAUDE.md`, Bugfix Workflow). The
  plan implements this as P1-T2/P1-T4 (fail-before), P1-T5 (fix), P1-T9 (pass-after).

## Coverage Threshold Conflict (Recorded, Not Resolved Here)

`CLAUDE.md` UT2 states a repository-wide line-coverage floor of >= 80% with >= 90% for new
modules/classes/methods. `.claude/rules/general-unit-test.md` states a uniform >= 85% line
and >= 75% branch floor. The two documents disagree on the repository-wide figure. Per the
plan's "Coverage Policy Note", this execution records the repository-wide percentage as a
non-blocking figure and treats changed-line no-regression plus new-code coverage of the
four added guard lines as the blocking gate (plan task P2-T7). This execution does not
resolve the documentary conflict.
