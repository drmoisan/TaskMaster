# [P0-T1] Policy instructions read — remediation for issue #782

Timestamp: 2026-09-06T01-26

Policy Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`

The six files were read in that order, which is the order the remediation plan's [P0-T1] states and
which matches the reading order in `.claude/skills/policy-compliance-order/SKILL.md` with the
C#-specific rule file inserted at position four and the tier and tonality rule files following it.

## Line counts

Each count is the number of lines on disk, measured with `Get-Content -LiteralPath <path>` and
cross-checked against the file's newline count and its trailing-newline state. All six files end with
a trailing newline, so the two measures agree for every file.

- `CLAUDE.md` — 447 lines
- `.claude/rules/general-code-change.md` — 80 lines
- `.claude/rules/general-unit-test.md` — 105 lines
- `.claude/rules/csharp.md` — 96 lines
- `.claude/rules/quality-tiers.md` — 51 lines
- `.claude/rules/tonality.md` — 80 lines

## Access mode

These six files were opened read-only. No task in this remediation plan writes under `.claude/`, and
the plan's scope boundary prohibits any change there. `[P0-T12]` records the `.claude/` before state
and `[P5-T1]` gates the after state.

## Points carried into execution

- The C# toolchain order is format, then lint, then type-check, then test, and the loop restarts at
  step one whenever a step fails or rewrites a file (`CLAUDE.md` § "C# Toolchain",
  `.claude/rules/csharp.md` line 19).
- `/t:Rebuild` is required for both msbuild gates locally; `/t:Build` can skip `CoreCompile` through
  incrementality and exit 0 without running analyzers (`.claude/rules/csharp.md` lines 15-16).
- `/p:Nullable=enable` is not passed to the nullable gate; nullable participation in this repository
  is per-file opt-in through `#nullable enable` (`.claude/rules/csharp.md` line 16, `CLAUDE.md`
  § C#1.3).
- CSharpier is invoked through `dotnet tool run` so the manifest-pinned version is used
  (`.claude/rules/csharp.md` line 14).
- No production or test file may exceed 500 lines
  (`.claude/rules/general-code-change.md` § "File Size Limit").
- The tone requirements in `.claude/rules/tonality.md` bind every prose replacement this plan writes
  into `spec.md` and into the evidence artifacts.
