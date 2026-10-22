# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-26T11-30
Task IDs: [P0-T1], [P0-T2], [P0-T3], [P0-T4]
Work Mode: full-bug
Issue: #614

Policy Order: the four files below were read in full, in this exact order, per
`.claude/skills/policy-compliance-order/SKILL.md` (CLAUDE.md → general code change →
general unit test → language-specific rules for the files in scope, which are C#).

## Files read (in order)

1. `CLAUDE.md` (repo root) — 447 lines — read in full. [P0-T1]
2. `.claude/rules/general-code-change.md` — 80 lines — read in full. [P0-T2]
3. `.claude/rules/general-unit-test.md` — 105 lines — read in full. [P0-T3]
4. `.claude/rules/csharp.md` — 96 lines — read in full. [P0-T4]

## Supporting skills read

- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`
- `.claude/rules/plan-acceptance-gates.md`

## Binding constraints extracted for this change

- C# toolchain order: `dotnet tool run csharpier format .` → analyzer msbuild `/t:Rebuild` →
  nullable msbuild `/t:Rebuild` → coverage-enabled vstest. Restart from step 1 on any failure or
  formatter rewrite.
- `/p:Nullable=enable` must NOT be added (diverges from `.github/workflows/ci.yml`).
- `/t:Build` must NOT be substituted for `/t:Rebuild` on either MSBuild gate.
- MSTest + Moq + FluentAssertions; AAA; no temp files; no `Thread.Sleep` / `Task.Delay` /
  `DateTime.Now` / `Random.Shared` in tests.
- 500-line file limit (see the plan's recorded AC25 net-non-growth interpretation for the two
  pre-existing over-limit files).
- Evidence resolves only under `<FEATURE>/evidence/<kind>/`; never under `artifacts/`.

Output Summary: All four required policy files were read in full in the mandated order, plus the
five supporting skill/rule documents that govern this plan's evidence, AC check-off, and
acceptance-gate authoring. No conflicting instruction was found between them and the approved plan.
