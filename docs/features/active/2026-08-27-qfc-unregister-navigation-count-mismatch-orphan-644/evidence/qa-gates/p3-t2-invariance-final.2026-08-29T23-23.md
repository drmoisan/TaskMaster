# [P3-T2] — Invariance Re-Check and Read-Only AC Status

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P3-T2]
Working directory: `<repo-root>` (the repository root of this worktree)
EXIT_CODE: 0 (all seven commands)

Redaction note: no absolute host path, account name, or machine name appears in this artifact.

This task re-runs the five `[P0-T4]` commands and adds the two `spec.md` checkbox commands that
measure the acceptance-criteria invariant stated in the plan's hard scope limits.

## Acceptance clauses

All four clauses hold.

### Clause 1 — coverage artifact byte-for-byte unmodified

Command: `(Get-FileHash -Algorithm SHA256 -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\evidence\qa-gates\p4-t6-coverage-final.2026-08-29T08-15.md).Hash`
EXIT_CODE: 0

`[P0-T4]` value: `912333D90A7918D04A1307B617C0D4D42A2EB5C066E0E3553DE0B850537AB7A2`
Value now:       `912333D90A7918D04A1307B617C0D4D42A2EB5C066E0E3553DE0B850537AB7A2`
Identical: yes. Result: PASS

The prior cycle's coverage artifact was not touched by any task in this cycle.

### Clause 2 — approved plan of record unchanged

Command: `@(Select-String -Path ...\plan.2026-08-29T07-42.md -Pattern '^- \[x\] \[P\d+-T\d+\]').Count`
EXIT_CODE: 0
`[P0-T4]`: `58`   Now: `58`   Required: `58`

Command: `@(Select-String -Path ...\plan.2026-08-29T07-42.md -Pattern '^- \[ \] \[P\d+-T\d+\]').Count`
EXIT_CODE: 0
`[P0-T4]`: `0`   Now: `0`   Required: `0`

Result: PASS. `plan.2026-08-29T07-42.md` still shows 58 checked and 0 unchecked `[P#-T#]` tasks.
The plan this cycle executed is the separate file `remediation-plan.2026-08-29T23-23.md`.

### Clause 3 — production file still carries no change

Command: `git diff a2c69aead286ad0ec6c7087f1bd8c46d39d0d472 --name-only -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0   Output: empty

Command: `git status --porcelain -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0   Output: empty

Both still empty, identical to `[P0-T4]`. Result: PASS

### Clause 4 — no acceptance criterion changed state

Command: `@(Select-String -Path ...\spec.md -Pattern '^- \[x\]').Count`
EXIT_CODE: 0
Cycle entry: `21`   Now: `21`   Required: `21`

Command: `@(Select-String -Path ...\spec.md -Pattern '^- \[ \]').Count`
EXIT_CODE: 0
Cycle entry: `5`   Now: `5`   Required: `5`

Supplementary: `@(Select-String -Path ...\spec.md -Pattern '\[[ xX]\]').Count` = `26`. The two
figures above sum to 26, confirming `spec.md` contains no indented checkbox and no
upper-case-`X` checkbox that the first two patterns would miss.

Result: PASS. No acceptance criterion was checked, unchecked, added, removed, or reworded in
this cycle.

## AC Status Summary — READ-ONLY

Reported read-only. `spec.md` was not modified by this task or by any task in this cycle.

Work mode for this issue is `full-bug`, so the acceptance-criteria source is `spec.md` only.

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md
- Total AC items: 18 (AC-0 through AC-17)
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: AC-16 (no coverage regression on changed lines)
```

Whole-file checkbox accounting, for reconciliation against the clause-4 figures: `spec.md`
carries 26 checkbox lines in total, 21 checked and 5 unchecked. Eighteen of those 26 are the
AC items above. The other eight are bug-report template fields outside the
acceptance-criteria set: the four severity radio-group boxes (`Blocker`, `High`, `Medium`,
`Low`, of which only `Medium` is checked), the `Attached minimal logs or screenshot` field
(unchecked), and three checked test-plan bullets. The five unchecked boxes across the whole
file are therefore `Blocker`, `High`, `Low`, `Attached minimal logs or screenshot`, and AC-16.

### AC-16 disposition — unchanged

AC-16 stands exactly as already adjudicated: **PARTIAL, left unchecked, referred and reported.**

This cycle did not re-open it. No coverage comparison was run, the repository-wide Cobertura
post-processor `scripts/vscode/Invoke-MSTestWithCoverage.ps1` was not re-run, and `spec.md` was
not edited anywhere. That disposition is correct for this cycle on its own merits: this cycle's
entire change is XML documentation comment text and the contents of one string literal in a
test file, which cannot change instrumented production line counts, and re-running an instrument
whose measured noise exceeds the disputed delta would produce a third number without resolving
anything.

No other acceptance criterion changed state. All 17 previously checked items remain checked.

## Output Summary

All seven commands run, all EXIT_CODE 0. All four clauses PASS: the prior cycle's coverage
artifact hash is unchanged at `912333D9...537AB7A2`; the approved plan of record still shows 58
checked and 0 unchecked tasks; both production-file observations are still empty; and `spec.md`
still reports exactly 21 checked and 5 unchecked checkbox lines, identical to the cycle-entry
figures. The AC Status Summary is reported read-only: 18 AC items, 17 checked, AC-16 the single
unchecked item, its PARTIAL / referred-and-reported disposition unchanged.
