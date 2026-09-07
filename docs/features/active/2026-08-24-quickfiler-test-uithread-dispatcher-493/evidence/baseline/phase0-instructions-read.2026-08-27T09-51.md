# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-08-27T09-51
Task: [P0-T1]
Command: (documentary read task — no shell command; files read with `cat` / `sed -n`)
EXIT_CODE: 0
Output Summary: All 14 required documents were read in the mandated order. Policy order is the order
given by `.claude/skills/policy-compliance-order/SKILL.md`, followed by the three plan-governance
skills, followed by the three feature requirement documents.

Policy Order: `CLAUDE.md` then `.claude/rules/general-code-change.md` then
`.claude/rules/general-unit-test.md` then the language/domain rules for the files in scope
(`.claude/rules/csharp.md`, `.claude/rules/architecture-boundaries.md`,
`.claude/rules/quality-tiers.md`, `.claude/rules/plan-acceptance-gates.md`,
`.claude/rules/tonality.md`) then the plan-governance skills then the feature requirement documents.

## Files read, in order

| # | Repo-relative path | Category |
| --- | --- | --- |
| 1 | `CLAUDE.md` | standing instructions (policy order position 1) |
| 2 | `.claude/rules/general-code-change.md` | cross-language code-change policy |
| 3 | `.claude/rules/general-unit-test.md` | cross-language unit-test policy |
| 4 | `.claude/rules/csharp.md` | C#-specific toolchain and standards |
| 5 | `.claude/rules/architecture-boundaries.md` | architecture-boundary rules |
| 6 | `.claude/rules/quality-tiers.md` | module rigor tiers T1-T4 |
| 7 | `.claude/rules/plan-acceptance-gates.md` | acceptance-gate rules G1-G6 |
| 8 | `.claude/rules/tonality.md` | tone policy |
| 9 | `.claude/skills/atomic-plan-contract/SKILL.md` | atomic plan format and QA loop rules |
| 10 | `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` | evidence paths and timestamp format |
| 11 | `.claude/skills/acceptance-criteria-tracking/SKILL.md` | AC check-off protocol |
| 12 | `docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` | requirements / sole AC source (full-bug) |
| 13 | `docs/features/active/quickfiler-test-uithread-dispatcher-493/issue.md` | constraints |
| 14 | `docs/features/active/quickfiler-test-uithread-dispatcher-493/research/2026-08-24T11-05-uithread-dispatcher-restore-scope-research.md` | design source |

`.claude/skills/policy-compliance-order/SKILL.md` was also read; it is the document that supplies
the ordering above rather than an ordered item within it.

## Binding constraints extracted

- Work Mode is `full-bug`; `spec.md` is the sole acceptance-criteria source (AC-1 through AC-10).
  `user-story.md` is an inert placeholder and is neither read for criteria nor edited.
- C# toolchain order: `dotnet tool run csharpier format/check .`, then the analyzer msbuild step
  (`/t:Rebuild`, `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), then the type-check
  msbuild step (`/t:Rebuild`, `/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`), then
  `vstest.console.exe` with `/EnableCodeCoverage`. Restart from step 1 on any failure or file
  rewrite.
- MSTest + Moq + FluentAssertions only. No `Thread.Sleep`, no `Task.Delay`, no wall-clock waits,
  no temporary files in tests.
- 500-line ceiling on every production, test, and reusable script file.
- Evidence artifacts resolve to `<FEATURE>/evidence/<kind>/`; no `artifacts/` path is valid.
- net481 target: `init` accessors, `record`, and `record struct` must not be used.
- Architecture-boundary rules concern production runtime code and the No-COM architecture; this
  change touches only `QuickFiler.Test` and introduces no VSTO, Outlook Interop, or `[ComVisible]`
  surface, so no boundary assertion is engaged.
- Plan acceptance gates G1-G6: this plan's asserted search tokens are short, single-line, and
  quoted verbatim in the plan prose, and no coverage argument is asserted, so no gate applies to
  the executor's own actions beyond honouring the plan text as written.
