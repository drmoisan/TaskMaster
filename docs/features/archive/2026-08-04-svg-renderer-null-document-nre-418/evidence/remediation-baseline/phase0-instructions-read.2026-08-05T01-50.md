# Phase 0 Instructions Read — Remediation Cycle 1

- Task: `[P0-T2]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-22 (UTC)

Policy Order: `CLAUDE.md` → `.claude/rules/general-code-change.md` → `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`

## Files read, in the mandated order

1. `CLAUDE.md` (repo root) — read in full
2. `.claude/rules/general-code-change.md` — read in full
3. `.claude/rules/general-unit-test.md` — read in full
4. `.claude/rules/csharp.md` — read in full

Supporting skills also read for this cycle:

- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

## Constraints extracted that bind this cycle

- C# toolchain order is format → lint → type-check → test; restart from step 1 on any
  failure or file modification (`CLAUDE.md`, `.claude/rules/csharp.md` § Toolchain).
- Formatting is CSharpier only; `dotnet format` is prohibited because it rewrites legacy
  non-SDK `.csproj` files.
- No production, test, or reusable script file may exceed 500 lines
  (`.claude/rules/general-code-change.md` § File Size Limit). This is the constraint that
  makes R-6 run first: `SVGControl/SvgRenderer.cs` is at 497.
- No production source path may be excluded from coverage measurement; any such `exclude`
  or `[ExcludeFromCodeCoverage]` on a production file is a Blocking finding
  (`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy).
- Temporary files in tests are strictly prohibited with zero approved exceptions
  (`.claude/rules/general-unit-test.md` § External Dependencies, UT4).
- Tests use MSTest + Moq + FluentAssertions, `[TestClass]`/`[TestMethod]`,
  Arrange–Act–Assert (`.claude/rules/csharp.md` § Testing Standards).
- Weakening assertions or relaxing test expectations is a prohibited behavior
  (`.claude/rules/csharp.md` § Prohibited Behaviors).
- Analyzer severities for the five-package analyzer stack are held at `suggestion` so the
  `/p:TreatWarningsAsErrors=true` nullable gate is not broken by analyzer diagnostics
  (`.claude/rules/csharp.md` § Severity-first ordering invariant).
- Evidence must be written under `<FEATURE>/evidence/<kind>/`; `artifacts/`-rooted
  evidence paths are forbidden (`evidence-and-timestamp-conventions`).

EXIT_CODE: 0

Output Summary: All four mandated policy files read in the required order, in full. No
conflicting instruction found between them and this cycle's remediation plan; execution
proceeds to `[P0-T3]`.
