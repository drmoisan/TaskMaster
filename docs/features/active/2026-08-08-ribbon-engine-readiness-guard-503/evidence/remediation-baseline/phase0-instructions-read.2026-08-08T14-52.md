# Phase 0 — Policy Instructions Read (Remediation Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T1]
Command: Read tool invocations against each absolute path listed below (read-only inspection; no command executed)
EXIT_CODE: 0

Policy Order: The order defined by `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\policy-compliance-order\SKILL.md` — standing instructions first (`CLAUDE.md`), then the cross-language code-change policy, then the cross-language unit-test policy, then the language- and domain-specific rules for the files in scope (C#), followed by the architecture-boundary, quality-tier, and tonality rules.

## Files read, in order

1. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\CLAUDE.md` (441 lines)
2. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\general-code-change.md` (80 lines)
3. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\general-unit-test.md` (105 lines)
4. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\csharp.md` (96 lines)
5. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\architecture-boundaries.md` (46 lines)
6. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\quality-tiers.md` (51 lines)
7. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\tonality.md` (80 lines)

All seven paths were confirmed present on disk with the line counts shown.

## Output Summary

Seven policy files read in the required order. Constraints that bind this remediation cycle:

- **CLAUDE.md / `.claude/rules/csharp.md`** — C# toolchain order is format (CSharpier) -> lint (MSBuild analyzers) -> type-check (MSBuild nullable) -> test (vstest with coverage). Restart from step 1 on any failure or file change. `dotnet format` is prohibited. MSTest + Moq + FluentAssertions are the mandated test stack.
- **`.claude/rules/csharp.md` Prohibited Behaviors** — weakening assertions to make tests pass is prohibited. This cycle strengthens the AC5 assertion (F1); it does not weaken any assertion.
- **`.claude/rules/general-code-change.md`** — 500-line file cap for production, test, and reusable script files; Markdown documentation is exempt. `RibbonExplorer.xml` is over the cap at the merge-base (accepted as AC25); F2 reduces it toward that accepted figure.
- **`.claude/rules/general-unit-test.md`** — line coverage >= 85 percent, branch coverage >= 75 percent; no regression on changed lines; temporary files in tests are prohibited. This cycle writes no temporary file inside a test.
- **`.claude/rules/architecture-boundaries.md`** — no new runtime code is added by this cycle; the ribbon callback surface is pre-existing legacy VSTO code that this cycle does not extend.
- **`.claude/rules/quality-tiers.md`** — uniform coverage thresholds across T1-T4.
- **`.claude/rules/tonality.md`** — professional, factual, measured tone in all artifacts produced by this cycle.
