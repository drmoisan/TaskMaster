# Phase 0 — Policy Instructions Read ([P0-T1])

- Issue: #644
- Task: `[P0-T1]`
- Timestamp: 2026-08-29T08-15
- Repository root: `<repo-root>` (branch `bug/qfc-unregister-navigation-count-mismatch-orphan-644`)

## Base-commit substitution (authorized deviation)

The approved plan names base commit `ecdb1c84ba8541ab67042985919cfed4df768c01`. The
parallel-orchestrator parent directed this run to reconcile against the current `origin/main`
tip `fa2ddefacf2c08abe18f3e3250d77da804534637`, which carries the merged fix for issue #638
(PR #700). That fix also touches `QuickFiler.Test/QuickFiler.Test.csproj`, a file this plan
modifies. `origin/main` was merged into the branch before execution began; the merge was clean.

**Substitution, authorized by the orchestrator:** wherever the plan writes
`ecdb1c84ba8541ab67042985919cfed4df768c01` as a git ref operand, this execution uses
`e968a1a8804b7641380d4489c496662824d45767` (the merge commit) instead. This applies to
`[P1-T2]`, `[P4-T8]`, `[P4-T9]`, and `[P5-T20]`. The substitution narrows each diff to this
run's own changes and widens no acceptance clause.

`git rev-parse HEAD` at Phase 0 start returned `e968a1a8804b7641380d4489c496662824d45767`.

## Policy Order:

Read in the `policy-compliance-order` sequence mandated by `[P0-T1]`:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/csharp.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

## Files read (explicit list)

| # | Path | Read |
|---|---|---|
| 1 | `CLAUDE.md` | yes |
| 2 | `.claude/rules/general-code-change.md` | yes |
| 3 | `.claude/rules/general-unit-test.md` | yes |
| 4 | `.claude/rules/quality-tiers.md` | yes |
| 5 | `.claude/rules/csharp.md` | yes |
| 6 | `.claude/rules/tonality.md` | yes |
| 7 | `.claude/rules/plan-acceptance-gates.md` | yes |

Additional documents read before execution, per the delegation directive:

- `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md` (the plan of record, read in full)
- `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md` (sole acceptance-criteria source, work mode `full-bug`)
- `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/issue.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`
- `.claude/skills/policy-compliance-order/SKILL.md`

## Constraints carried forward into execution

- C# toolchain order is format -> lint -> type-check -> test; restart from step 1 on any failure
  or any file rewrite.
- `msbuild` uses `/t:Rebuild`, never `/t:Build`, for the analyzer and type-check gates.
- Do not add `/p:Nullable=enable` to the type-check gate.
- CSharpier is invoked only through `dotnet tool run`.
- MSTest + Moq + FluentAssertions for all C# tests; no temporary files; no live Outlook, COM,
  WinForms handle, or STA apartment in any test added by this fix.
- Work mode is `full-bug`, so `spec.md` is the sole acceptance-criteria source and no
  `user-story.md` is created.
- Evidence is written only under
  `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/<kind>/`.
- No absolute host path, account name, or machine name is written into any artifact.

Command: read-only document review; no shell command executed for this task.
EXIT_CODE: 0
Output Summary: All seven policy files listed under `Policy Order:` were read in that order,
together with the plan, spec, and issue for #644 and the four governing skills. The base-commit
substitution to `e968a1a8804b7641380d4489c496662824d45767` is recorded above and was verified
against `git rev-parse HEAD`.
