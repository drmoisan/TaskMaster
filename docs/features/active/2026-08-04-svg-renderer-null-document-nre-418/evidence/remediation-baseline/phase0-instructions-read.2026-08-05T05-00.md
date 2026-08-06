# Phase 0 Instructions Read — Remediation Cycle 2

- Task: `[P0-T2]`
- Timestamp: 2026-08-04T23-23
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Command: (documentary read task — no shell command; files read in full with the Read tool)
- EXIT_CODE: 0

## Policy Order

The order mandated by `.claude/skills/policy-compliance-order/SKILL.md` and by `CLAUDE.md`
§ `## Policy Compliance Order`, applied as read:

1. `CLAUDE.md` (standing instructions; all sections)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

## Files read, in full, in that exact order

| # | Path | Lines read |
|---|---|---|
| 1 | `CLAUDE.md` | 1-442 (entire file) |
| 2 | `.claude/rules/general-code-change.md` | 1-81 (entire file) |
| 3 | `.claude/rules/general-unit-test.md` | 1-106 (entire file) |
| 4 | `.claude/rules/csharp.md` | 1-97 (entire file) |

## Constraints extracted that bear on this cycle

- **C# toolchain order (`CLAUDE.md` § `## C# Toolchain`, `.claude/rules/csharp.md` § Toolchain):**
  format → lint → type-check → test. Restart from step 1 if any step fails or changes files.
  Phase 2 of this plan implements exactly this order.
- **Formatting authority (`CLAUDE.md` C#1.1):** csharpier output wins over hand formatting; do not use
  `dotnet format`. This governs `[P1-T2]`'s single-line `packages.config` entry: if `[P2-T1]` reflows it,
  the reflowed form is correct.
- **UT1 Independence (`.claude/rules/general-unit-test.md` § Core Principles item 1):** "Tests must be
  able to run in any order without impacting each other." This is the policy statement the blocking
  finding of this cycle violates and that `[P1-T5]`/`[P1-T6]`/`[P2-T9]` demonstrate restored.
- **Determinism (same § item 4)** and **IDE/CLI parity (`.claude/rules/csharp.md` § Deterministic Test
  Rules: "Tests must produce identical results in the IDE test runner and in CLI runs")** are the two
  further statements the order-dependence violated.
- **Coverage Exclusion Policy (`.claude/rules/general-unit-test.md`):** no production file may be
  excluded from coverage measurement; an `exclude` matching a production source path is Blocking. This
  forecloses any `[ExcludeFromCodeCoverage]` or `coverage.config` response to G-1 or G-9.
- **Coverage floors:** `.claude/rules/general-unit-test.md` states line >= 85% and branch >= 75%;
  `CLAUDE.md` UT2 and `.claude/rules/csharp.md` state repository-wide line >= 80% and >= 90% for new
  modules. `[P2-T8]` records verdicts against the 85% line / 75% branch floors the plan names.
- **Prohibited behaviors (`.claude/rules/csharp.md` § Prohibited Behaviors):** weakening assertions or
  relaxing test expectations to make tests pass; reporting success without running the required
  toolchain. Both are restated as halt conditions in `[P1-T5]`.
- **File size limit (500 lines)** applies to production, test, and reusable script files; Markdown
  documentation is exempt, which is why this cycle's evidence artifacts are unconstrained by it. This
  cycle modifies no `.cs` file, so the limit is not engaged by any edit.

## Output Summary

All four policy files were read in full, in the mandated order, before any Phase 1 task. No conflicting
instruction was found between them and this plan; the one apparent divergence — the repository-wide line
floor stated as `>= 80%` in `CLAUDE.md`/`.claude/rules/csharp.md` and `>= 85%` in
`.claude/rules/general-unit-test.md` — is resolved conservatively by adopting the stricter `>= 85%`
figure that the plan's `[P2-T8]` names, so no halt-and-notify condition arises.
