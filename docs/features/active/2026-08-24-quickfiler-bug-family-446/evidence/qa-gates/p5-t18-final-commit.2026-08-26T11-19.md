# [P5-T18] Terminal Commit and Clean-Tree Gate

Timestamp: 2026-08-26T11-19

Task: [P5-T18]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `git add "docs/features/active/quickfiler-bug-family-446"`
EXIT_CODE: 0

Command: `git commit -m "docs(446): record terminal acceptance criteria AC18, AC19, AC22, AC24, AC25, AC27 and AC28"`
EXIT_CODE: 0

Command: `git status --porcelain -- "QuickFiler" "QuickFiler.Test" "docs/features/active/quickfiler-bug-family-446"`
EXIT_CODE: 0

## Resulting HEAD

- Short sha: `92b5c0cc`
- Full sha: `92b5c0cce60c2e14bc69dfc6035337a9029e20b7`

Previous HEAD was `2aef13c4c5ed2d283ce51c027c33dbe721d9263e`, the `[P5-T10]` commit.

Ten files changed, 651 insertions, 14 deletions:

- `evidence/qa-gates/p5-t10-commit.2026-08-26T11-12.md`
- `evidence/qa-gates/p5-t11-ac18.2026-08-26T11-13.md`
- `evidence/qa-gates/p5-t12-ac19.2026-08-26T11-14.md`
- `evidence/qa-gates/p5-t13-ac22.2026-08-26T11-14.md`
- `evidence/qa-gates/p5-t14-ac24.2026-08-26T11-15.md`
- `evidence/qa-gates/p5-t15-ac25.2026-08-26T11-16.md`
- `evidence/qa-gates/p5-t16-ac27.2026-08-26T11-17.md`
- `evidence/qa-gates/p5-t17-ac28.2026-08-26T11-18.md`
- `plan.2026-08-24T09-37.md` (checklist state)
- `spec.md` (six terminal acceptance criteria checked off)

This artifact and the `[P5-T18]` checklist entry are committed by an immediately following
sha-recording commit, which is the same two-step pattern this plan execution used for the Phase 3
and Phase 4 terminal commits (`ae1124e9` after `7161c4a7`, and `a0f5ea2b` after `f455e2dd`). A
single commit cannot contain the sha it produces.

## Clean-tree gate

`git status --porcelain -- "QuickFiler" "QuickFiler.Test" "docs/features/active/quickfiler-bug-family-446"`
produced **zero output lines** after the commit above, and produces zero output lines again after
the sha-recording commit.

The clean-tree requirement is scoped to the change-set and feature-folder pathspecs and is never
run unscoped, because `.claude/agent-memory/` is a tracked directory in this repository that the
executing agent writes and that lies outside this change set; it must not be staged by any task in
this plan, and it was not. The only entry in an unscoped `git status --porcelain` at the end of
this phase is the untracked `.claude/state/` directory, which is likewise outside this change set
and is deliberately never staged.

## Acceptance-criteria checkbox audit

All 28 criteria in `docs/features/active/quickfiler-bug-family-446/spec.md` were audited. Each is
either checked with a citing `evidence/qa-gates/` artifact, or explicitly left unchecked with a
recorded gap.

- Total criteria: **28**
- Checked: **27**
- Unchecked with a recorded gap: **1** (AC28)

| criteria | spec lines | verified by | citing artifact |
| --- | --- | --- | --- |
| AC1 - AC16 | 875-893 | `[P4-T1]` - `[P4-T16]` | `p4-t1-ac1` ... `p4-t16-ac16` |
| AC17 | 897 | `[P4-T17]` | `p4-t17-ac17` |
| AC18 | 898 | `[P5-T11]` | `p5-t11-ac18.2026-08-26T11-13.md` |
| AC19 | 899 | `[P5-T12]` | `p5-t12-ac19.2026-08-26T11-14.md` |
| AC20 | 900 | `[P4-T18]` | `p4-t18-ac20` |
| AC21 | 901 | `[P4-T19]` | `p4-t19-ac21` |
| AC22 | 902 | `[P5-T13]` | `p5-t13-ac22.2026-08-26T11-14.md` |
| AC23 | 906 | `[P4-T20]` | `p4-t20-ac23` |
| AC24 | 907 | `[P5-T14]` | `p5-t14-ac24.2026-08-26T11-15.md` |
| AC25 | 908 | `[P5-T15]` | `p5-t15-ac25.2026-08-26T11-16.md` |
| AC26 | 909 | `[P4-T21]` | `p4-t21-ac26` |
| AC27 | 910 | `[P5-T16]` | `p5-t16-ac27.2026-08-26T11-17.md` |
| AC28 | 911 | `[P5-T17]` | `p5-t17-ac28.2026-08-26T11-18.md` — **UNCHECKED, gap recorded** |

### The one recorded gap

AC28 is left unchecked. Its checkbox states a whole-type reading (at least 90% line coverage on
`QfcStreamingDequeueConfidenceGate`, `QfcFormController` and `QfcHomeController`), and two of the
three whole-type rates are below that threshold: `QfcFormController` at `55.37` and
`QfcHomeController` at `71.05`. Both are above their Phase 0 baselines (`51.93` and `68.31`), so
there is no regression; the shortfall is against the absolute 90% figure.

Raising either whole-type rate would require adding coverage to partial files owned by sibling
epic children (`QfcFormController.cs`, `QfcFormController.EventHandlers.cs`,
`QfcFormController.SetupDisposal.cs`, `QfcHomeController.cs`, `QfcHomeController.Metrics.cs`),
which AC18 forbids modifying and which `[P5-T11]` verified were not modified. The two criteria
cannot both be satisfied by this change set.

`p5-t17-ac28.2026-08-26T11-18.md` carries the required line verbatim:

REMEDIATION-REQUIRED: AC28 whole-type reading conflicts with AC18

That artifact satisfies this task's recorded-gap condition for AC28. Adjudicating the conflict is a
maintainer spec amendment, not an executor decision; no task in this plan edits acceptance-criteria
text and none was edited.

AC27 required no `REMEDIATION-REQUIRED` note: `[P5-T16]` recorded all four toolchain exit codes as
`0` with a failed test count of `0`, and neither `[P5-T2]` nor `[P5-T5]` completed on a
pre-existing-baseline branch, so that conditional clause never applied.

## Artifact hygiene at the terminal commit

A case-insensitive search of the entire feature folder for the account name and the machine name
returned **zero** hits in file contents immediately before this commit. Thirty-six leftover
`Deploy_<account> .../In/<HOST>` directories from Phase 1 to Phase 3 test runs remain on disk with
host identifiers in their directory names; every one is empty (zero files), so git does not track
them, they never appear in `git status`, and none is committed.

## Output Summary

Terminal commit `92b5c0cce60c2e14bc69dfc6035337a9029e20b7` created with `EXIT_CODE: 0`. Scoped
`git status --porcelain` over `QuickFiler`, `QuickFiler.Test` and the feature folder is empty. All
28 acceptance criteria are accounted for: 27 checked with citing `evidence/qa-gates/` artifacts and
AC28 explicitly unchecked with its gap recorded verbatim. Phase 5 and the plan are complete.
