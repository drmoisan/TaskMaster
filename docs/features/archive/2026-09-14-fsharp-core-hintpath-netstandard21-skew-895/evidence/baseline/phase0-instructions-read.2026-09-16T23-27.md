# Phase 0 — Policy Read Evidence (Issue #895)

Timestamp: 2026-09-17T01-11
Task: [P0-T1]

## Policy Order:

The seven policy files were read in this exact order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/csharp.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

## Requirements Sources:

The three feature documents were then read in full:

1. `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/issue.md`
2. `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md`
3. `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/research/2026-09-16T23-30-fsharp-core-hintpath-research.md`

AC source: spec.md section "## Acceptance Criteria", 5 criteria

The five criteria are at `spec.md` lines 422 (AC1), 430 (AC2), 444 (AC3), 450 (AC4) and 457 (AC5).
`issue.md` records `- Work Mode: full-bug`, so `spec.md` is the sole acceptance-criteria source and
`user-story.md` is correctly absent from the feature folder.

## Threshold Authority Note:

`CLAUDE.md` is first in the compliance order and is therefore the authority on C# coverage floors. It
fixes them at:

- repository-wide line coverage `>= 80%` measured on the testable denominator (production-only
  first-party code after the ratified COM/VSTO/WinForms exemption);
- `>= 90%` for any new module, class or method;
- no regression on changed lines.

`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state an `85%` line and
`75%` branch pair together with a T1-T4 tier system keyed to `quality-tiers.yml` and
`docs/ci.research.md`. Neither of those two files exists in this repository, and the research
artifact section 6 records the same absence (with the earlier finding from issue #494 that the
85/75/T1-T4 cluster is reference-repository leakage). The compliance order resolves the conflict in
favour of `CLAUDE.md`, so the operative floors for this run are 80 / 90 / no-regression.

Application to this change: no production `.cs` file is modified, so the changed-line obligation is
discharged by `CHANGED-PRODUCTION-LINES: 0`; the two new files are test code, which the coverage
runner excludes from instrumentation through its `.*\.Test\.dll$` module exclusion, so the
new-module figure is `N/A`.

## Observed Corrections:

Reproduced from the plan's `## Observed Corrections to the Requirements Documents` section.

1. **Already applied upstream; recorded here as confirmed rather than outstanding.** An earlier
   revision of the plan recorded that `spec.md` denied the existence of any `879` folder under
   `docs/features`. `spec.md` has since been corrected: its `### Dependencies or blocked work:`
   bullet at lines 257-267 now states that issue #879's active feature folder
   `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/`
   still exists on disk with a fully checked plan and has not yet been archived, and that the
   comment-only edit targets `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`,
   ordinary shared repository test code outside #879's own feature folder. Re-derived on the
   assigned worktree during this task: the folder exists, its `plan.2026-09-13T18-22.md` carries
   zero unchecked task lines, and its `evidence/other/preflight-clearance.2026-09-14T01-55.md`
   records the completed run. The ownership conclusion is unchanged: #879 is delivered and the
   comment-only edit belongs to this issue; nothing in this plan writes into #879's canonical
   feature-folder state. The superseded claim is not restated as current `spec.md` text.
2. AC5 (`spec.md` line 463) says a grep of the unfixed file for `unsatisfiable` returns the stale
   sentence at lines 366-367. Re-derived: the token occurs at line 281 (the
   `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which stays true after the fix and
   is retained) and at line 366 (the stale sentence). The pre-fix count is therefore 2 and the
   post-fix count is 1; `[P0-T9]` records the baseline count and `[P3-T2]` gates the transition.

## Output Summary:

All seven policy files and all three requirements documents were read from the execution worktree.
No policy conflict remains unresolved: the single conflict found (coverage floors) is resolved by
the compliance order in favour of `CLAUDE.md`, as recorded in the Threshold Authority Note above.

EXIT_CODE: 0
