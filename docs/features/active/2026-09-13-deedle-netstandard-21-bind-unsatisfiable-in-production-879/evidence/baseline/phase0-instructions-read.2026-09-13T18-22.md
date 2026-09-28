# Phase 0 — Policy Instructions Read

Timestamp: 2026-09-13T23-03

Policy Order: CLAUDE.md first, then the cross-language code-change policy, then the
cross-language unit-test policy, then the module rigor tier system, then the C#-specific
rule file, then the tonality policy, then the atomic-plan acceptance-gate rules.

Files read, in this exact order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/csharp.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

All seven files were read in full from the item worktree
`bugs-2026-09-11-item-879` at the commit recorded below.

Repository HEAD at read time: `1546119bdbbf6f9365a286998b0528d8629625a6`
Branch: `bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879`

## Threshold Authority Note

`CLAUDE.md` states the repository-wide C# line-coverage floor as `>= 80%` on the testable
denominator, new modules at `>= 90%`, and no regression on changed lines.
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%`
line and `>= 75%` branch. `CLAUDE.md` is first in the policy compliance order recorded in
its own `## Policy Compliance Order` section, so the `80 / 90 / no-regression` figures are
the thresholds this plan is executed against.

## Requirements Sources

Read in full at 2026-09-13T23-03:

1. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/issue.md`
2. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md`
3. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/research/2026-09-13T19-05-deedle-netstandard-bind-research.md`

AC source: spec.md section "## Acceptance Criteria", 19 criteria

## Observed Correction to the Defect Narrative

The plan's `## Defect Statement` asserts that no `*.config` file in the repository contains
the string `netstandard`. That sweep is literally refuted by three NuGet package manifests:
`QuickFiler/packages.config` line 31, `TaskMaster/packages.config` line 27 and
`UtilitiesCS/packages.config` line 103 each carry a `NETStandard.Library` package
identifier. Those are NuGet package ids, not assembly-binding entries. No `app.config` and
no `*.dll.config` matches, so the substantive premise of the defect statement — that no
binding redirect covers the `netstandard` reference in any host — is unaffected, and no
acceptance condition in this plan depends on the refuted sweep. Recorded here as an
observed correction rather than treated as a blocking premise failure.
