# [P0-T4] — Invariance Baselines

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P0-T4]
Working directory: repository root of the worktree
EXIT_CODE: 0 (all five commands)

These are the baselines `[P3-T2]` re-checks after this cycle's edits.

## Command 1 — SHA-256 of the prior cycle's coverage artifact

Command: `(Get-FileHash -Algorithm SHA256 -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\evidence\qa-gates\p4-t6-coverage-final.2026-08-29T08-15.md).Hash`
EXIT_CODE: 0

Hash, recorded verbatim:

```
912333D90A7918D04A1307B617C0D4D42A2EB5C066E0E3553DE0B850537AB7A2
```

This artifact must remain byte-for-byte unmodified through this cycle. No task in this plan
edits it.

## Command 2 — approved plan, checked task count

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\plan.2026-08-29T07-42.md -Pattern '^- \[x\] \[P\d+-T\d+\]').Count`
EXIT_CODE: 0
Measured: `58`   Expected: `58`   Match: yes

## Command 3 — approved plan, unchecked task count

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\plan.2026-08-29T07-42.md -Pattern '^- \[ \] \[P\d+-T\d+\]').Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

`plan.2026-08-29T07-42.md` is the approved plan of record and is not modified by this cycle.
The plan this cycle executes is the separate file `remediation-plan.2026-08-29T23-23.md`.

## Command 4 — production file, anchored name-only diff

Command: `git diff a2c69aead286ad0ec6c7087f1bd8c46d39d0d472 --name-only -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0
Measured: empty output   Expected: empty output   Match: yes

## Command 5 — production file, porcelain status companion

Command: `git status --porcelain -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0
Measured: empty output   Expected: empty output   Match: yes

Commands 4 and 5 together confirm the production file carries no uncommitted change at cycle
entry. The anchored diff observes tracked modification against the cycle-entry head; the
porcelain companion additionally observes any untracked or staged state the diff alone cannot
report.

## Supplementary observation (not one of the five mandated commands)

Recorded here as a convenience cross-check of the acceptance-criteria invariant that `[P3-T2]`
formally re-measures against `spec.md`:

- `@(Select-String -Path ...\spec.md -Pattern '^- \[x\]').Count` = `21`
- `@(Select-String -Path ...\spec.md -Pattern '^- \[ \]').Count` = `5`
- `@(Select-String -Path ...\spec.md -Pattern '\[[ xX]\]').Count` = `26`

The first two sum to the third, confirming `spec.md` contains no indented checkbox and no
upper-case-`X` checkbox that the first two patterns would miss. These match the cycle-entry
figures the plan's hard scope limits state. No acceptance criterion is checked, unchecked,
added, removed, or reworded by any task in this cycle.

## Output Summary

Five commands run, all EXIT_CODE 0. All five measured values equal their expected values:
coverage artifact SHA-256 captured as `912333D9...537AB7A2`; approved plan shows `58` checked
and `0` unchecked `[P#-T#]` tasks; both production-file observations empty. Supplementary
`spec.md` entry state confirmed at 21 checked and 5 unchecked, 26 checkbox lines total.
