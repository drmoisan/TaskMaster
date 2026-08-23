# [P3-T9] Threshold assessment

Timestamp: 2026-08-11T02-00
Command: `git status --porcelain -uall -- CLAUDE.md .claude/rules`; comparison of the `[P3-T7]` figures
against the thresholds documented in `.claude/rules/general-unit-test.md`,
`.claude/rules/quality-tiers.md` and `CLAUDE.md` § UT2
EXIT_CODE: 0

**This plan changed no threshold.** Threshold reconciliation is owned by issue #494 (epic wave 2) and
runs after this feature. Adjusting a threshold here would be a scope violation regardless of whether
a corrected figure fails one.

## Comparisons

Post-change figures are from `[P3-T7]`
(`<FEATURE>/evidence/qa-gates/coverage-collection.2026-08-11T01-56.md`).

| # | Threshold | Source document | Measured | Verdict |
|---|---|---|---|---|
| 1 | Line coverage >= 85% | `.claude/rules/general-unit-test.md` § Coverage Requirements; `.claude/rules/quality-tiers.md` § Uniform across all tiers | **85.5355%** (`line-rate` 0.855355) | **PASS** |
| 2 | Branch coverage >= 75% | `.claude/rules/general-unit-test.md`; `.claude/rules/quality-tiers.md` | **79.0134%** (`branch-rate` 0.790134) | **PASS** |
| 3 | Repository-wide line coverage >= 80% | `CLAUDE.md` § UT2 (line 297) | **85.5355%** | **PASS** |
| 4 | New module line coverage >= 85% | `.claude/rules/powershell.md` § Testing Standards; `.claude/rules/quality-tiers.md` | **100%** (`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, `[P3-T4]`) | **PASS** |
| 5 | New module branch coverage >= 75% | `.claude/rules/powershell.md`; `.claude/rules/quality-tiers.md`; `spec.md` § Coverage impact | **not measurable** — Pester 5.6.1 emits no branch counter | **UNMEASURABLE — handed to #494** |
| 6 | No coverage regression on changed lines | `.claude/rules/general-unit-test.md`; `.claude/rules/powershell.md` | new module at 100%; no C# line changed | **PASS** |

Every measurable comparison passes. **No corrected figure falls below any documented threshold.**

## Margin observation (recorded, not acted on)

The repository line rate carries a thin margin above the 85% floor, and this feature moved it
**upward**:

| | line-rate | margin above 85% |
|---|---|---|
| baseline (`[P0-T11]`) | 0.853514 | +0.3514 points |
| post-change (`[P3-T7]`) | 0.855355 | +0.5355 points |

The delegating orchestrator recorded an expectation of 85.0317% with a 0.03-point margin; the figure
measured in this worktree is 85.3514% at baseline. The difference is a property of the worktree and
build state measured here and is recorded as an observation, not reconciled — reconciliation is #494's
work.

## Documented conflict between threshold sources (recorded, NOT resolved)

`CLAUDE.md` § UT2 line 297 states:

> Repository-wide line coverage must remain `>= 80%`.

`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` both state:

> Line coverage must remain >= 85% across all tiers (T1-T4).

These two documented figures conflict. The conflict is recorded here as an observation and is **not
resolved by this feature**. It is precisely the reconciliation that issue #494 owns. The measured
85.5355% satisfies both figures, so the conflict is not load-bearing for this feature's outcome.

`CLAUDE.md` § UT2 additionally scopes its 80% floor to a "testable denominator" after COM/VSTO/WinForms
exemptions, whereas the `.claude/rules/*` 85% floor is stated without that qualifier. That is a second
dimension of the same divergence and is likewise recorded, not resolved.

## Handoff note to issue #494

No figure fails a threshold, so no failure handoff is required. One **measurement-gap** handoff is
recorded:

> **To issue #494 (`coverage-threshold-policy-reconciliation-494`, epic wave 2), from issue #457.**
>
> 1. **Branch-coverage floor is unmeasurable for PowerShell modules.**
>    - Figure: not emitted.
>    - Threshold: branch coverage >= 75%.
>    - Source documents: `.claude/rules/powershell.md` § Testing Standards line 64;
>      `.claude/rules/quality-tiers.md` § Uniform across all tiers; `spec.md` § Coverage impact.
>    - Measured fact: Pester 5.6.1's JaCoCo output emits no branch counter. It reports a
>      command/line coverage percent only. The header line Pester prints, `Covered 93.43% / 75%`,
>      compares the LINE figure against a configured target and is not a branch measurement. The
>      repository therefore documents a PowerShell branch-coverage floor that its configured tooling
>      cannot evaluate. #494 should decide whether to change the tooling, scope the floor to
>      languages that can report it, or remove it.
>
> 2. **Threshold-source conflict, for the record.** `CLAUDE.md` § UT2 says >= 80% repository-wide
>    line coverage; `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` say
>    >= 85%. Both are satisfied by the corrected figure of 85.5355%, so this is not blocking today,
>    but the divergence remains for #494 to reconcile.
>
> 3. **Corrected baseline for #494's use.** Post-#457 repository figures, measured against the
>    post-#441 arithmetic: `lines-covered` 53375, `lines-valid` 62401, `line-rate` 0.855355,
>    `branches-covered` 12541, `branches-valid` 15872, `branch-rate` 0.790134. Recorded in
>    `<FEATURE>/evidence/qa-gates/coverage-collection.2026-08-11T01-56.md`. This is the figure #494
>    was blocked on and should decide against.

## Governance-file check

`git status --porcelain -uall -- CLAUDE.md .claude/rules` (verbatim, recorded including the empty
result):

```
```

The command returned **no output**. Neither `CLAUDE.md` nor anything under `.claude/rules/` is
modified by this feature. Those edits are owned by sibling features #512 and #494.

## Output Summary

Six threshold comparisons: five PASS, one unmeasurable. Repository line coverage 85.5355% against
floors of 85% and 80%; branch coverage 79.0134% against 75%; the new module at 100% against 85%. No
corrected figure fails any threshold, so no failure handoff is required. A measurement-gap handoff for
the unmeasurable PowerShell branch-coverage floor, the documented 80%-versus-85% conflict, and the
corrected baseline figures are recorded for issue #494. No threshold was changed and no governance
file was touched.
