# Remediation Finding Status — Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-14-35
- Task: [P5-T12]
- EXIT_CODE: 0

## Finding Status — 12 Findings

`E/` abbreviates
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/`.

| Finding | Severity | Discharging tasks | Evidence artifacts | Verdict |
|---|---|---|---|---|
| **R1** — no green workflow run at head | Blocking | [P0-T13], [P6-T1], [P6-T2] | `E/remediation-baseline/p0-t13-remote-probe.2026-09-20T01-37.md` | **deferred to Phase 6** |
| **R2** — `Sync-PackageReferences.ps1` negative and error paths untested | Blocking | [P1-T1] through [P1-T11], [P5-T3] | `E/regression-testing/fail-before-exception.2026-09-20T01-37.md`; `E/regression-testing/p1-t2-line151...md` through `p1-t9-line345...md`, 8 files; `E/qa-gates/p1-t10-pester-coverage...md`; `E/qa-gates/p1-t11-sync-coverage-reconciliation...md`; `E/qa-gates/p5-t3-pester-coverage.iter1...md` | **discharged** |
| **R3** — push gate discards normalisation and redirect writes | Blocking | [P3-T1], [P3-T2], [P3-T3], [P3-T8] | `E/regression-testing/p3-t1-workflow-fail-before...md`; `E/qa-gates/p3-t2-r3-write-gate...md`; `E/regression-testing/p3-t3-r3-write-set...md`; `E/regression-testing/p3-t8-workflow-pass-after...md` | **discharged** |
| **R4** — committed absolute host paths | Blocking | [P0-T3], [P4-T1], [P4-T2], [P4-T3], [P4-T5], [P6-T3] | `E/remediation-baseline/p0-t3-hostpath-census...md`; `E/qa-gates/p4-t1-substitution-map...md`; `E/qa-gates/p4-t2-sanitisation...md`; `E/qa-gates/p4-t3-residual...md`; `E/qa-gates/p4-t5-md-only...md` | **discharged in the working tree; squash merge required** |
| **R5** — `Invoke-ProjectConsistencyRepair` cannot be called correctly | Major | [P2-T1] through [P2-T4], [P2-T7] | `E/qa-gates/p2-t1-extraction...md`; `E/regression-testing/p2-t2-r5-fail-before...md`; `E/qa-gates/p2-t3-r5-fix...md`; `E/regression-testing/p2-t4-r5-pass-after...md`; `E/qa-gates/p2-t7-pester-coverage...md` | **discharged** |
| **R6** — disclosure step unguarded and appending | Major | [P3-T1], [P3-T4], [P3-T5], [P3-T8] | `E/regression-testing/p3-t1-workflow-fail-before...md`; `E/qa-gates/p3-t4-r6-disclosure-guard...md`; `E/regression-testing/p3-t5-r6-idempotence...md`; `E/regression-testing/p3-t8-workflow-pass-after...md` | **discharged** |
| **R7** — binding-redirect class unreachable from the trigger | Major | [P0-T4], [P3-T1], [P3-T6], [P3-T8] | `E/remediation-baseline/p0-t4-spec-amendment...md`; `E/qa-gates/p3-t6-r7-dead-filter...md`; `E/regression-testing/p3-t8-workflow-pass-after...md` | **discharged as out of scope, decision D2** |
| **R8** — repair commit identity matches no account | Major | [P3-T1], [P3-T7], [P3-T8], [P3-T9] | `E/qa-gates/p3-t7-r8-commit-identity...md`; `E/regression-testing/p3-t8-workflow-pass-after...md`; `E/qa-gates/p3-t9-actionlint...md` | **discharged** |
| **R9a** — two detector false positives in the autoclose list | Minor | [P4-T4], [P6-T3] | `E/qa-gates/p4-t4-autoclose-list...md` | **discharged** |
| **R9b** — unfiltered `Get-AnalyzerAssemblyPath` call site uncommented | Minor | [P2-T5] | `E/qa-gates/p2-t5-r9b-comment...md` | **discharged** |
| **R9c** — non-recursive manifest discovery fails silently | Minor | [P2-T6] | `E/qa-gates/p2-t6-r9c-lister-visibility...md` | **discharged as visibility, decision D4** |
| **R9d** — two production files at the 500-line cap | Minor | [P0-T5], [P1-T14], [P2-T10], [P3-T14], [P5-T10] | `E/remediation-baseline/p0-t5-size-and-text-baseline...md`; `E/qa-gates/p1-t14-size...md`; `E/qa-gates/p2-t10-size...md`; `E/qa-gates/p3-t14-size...md`; `E/qa-gates/p5-t10-file-size-audit...md` | **discharged** |

**12 findings listed. Every finding except R1 carries a discharged verdict with at least one
artifact. R1 carries `deferred to Phase 6`.** Every cited artifact exists on disk.

## Two Discharges Are Narrower Than "Fixed" and Say So

**R7** is discharged **out of scope**, per decision D2. The dead filter clause is removed and
the reachability decision is recorded in three places, but the binding-redirect class remains
unreachable from the `workflow_run` trigger. Making it reachable would be new production
behaviour with new untested paths.

**R9c** is discharged as **visibility**, per decision D4. The verbose record makes a
one-level-deep discovery shortfall observable in the run log. It does **not** prevent a nested
project from being skipped.

**R4** is discharged in the **working tree only**. The pre-sanitisation blobs remain reachable
in this branch's history, so the pull request must be squash-merged. [P6-T3] records that
instruction with its reason.

## Criterion Ledger, Read From `spec.md`

```
Select-String -Path <spec.md> -Pattern '^- \[[ xX]\] \*\*AC\d+ '
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| Total criteria | exactly **26** | **26** | PASS |
| Ticked | exactly **23** | **23** | PASS |
| Unticked | exactly **3** | **3** | PASS |
| AC14 ticked | yes | **yes** | PASS |

The three unticked criteria, read verbatim from the file:

- **AC18** — The repair commit is pushed under the GitHub App identity.
- **AC19** — The required checks re-run and pass on the post-repair head SHA.
- **AC20** — Disclosure is present and conditional.

**AC14 is ticked and carries the amendment [P0-T4] verified**, applied and committed by the
coordinator before execution began.

**No other criterion was reworded by this cycle.** [P5-T11] confirms this from the footprint:
`spec.md` does not appear in the union of the anchored diff and porcelain, so no criterion text
changed after the [P0-T2] anchor. Per **gate rule 19** the executor made no criterion edit.

**No criterion was newly ticked by this cycle.** The three that remain unticked are the three
that were unticked at the review, and they stay unticked because they require a GitHub App
credential and an open Dependabot pull request, neither of which exists.

## The Four Residuals That Remain Unverifiable Until #914

Reproduced here per **gate rule 20**. `.github/workflows/dependabot-repair.yml` has never
executed, and [P0-T13] recorded why: **zero** repository Actions secrets, from an authorised
query, and **zero** open pull requests.

1. **That `actions/create-github-app-token@v3` publishes an `app-slug` output.** Decision D3
   records this as an assumption of record. [P3-T7]'s first guard converts a wrong assumption
   into a named step failure rather than a silent bad identity.
2. **That the resolved bot user id produces a commit whose `author.login` ends `[bot]`** and is
   not `github-actions[bot]`. This is AC18's stated acceptance.
3. **That the push causes the required checks to re-run** on the post-repair head SHA. This is
   AC19.
4. **That the disclosure edit produces exactly one block on a real pull-request body.** [P3-T5]
   exercises the expression against a body this repository constructs, not one GitHub returned.
   This is AC20.

Each maps to an unticked criterion: residual 2 to AC18, residual 3 to AC19, residual 4 to AC20.
Residual 1 underlies residual 2. **All three criteria stay unticked**, and issue **#914** is
where all four are settled.

## One Acceptance Clause of This Cycle Was Not Met

Recorded here so it reaches the reviewer rather than only the task artifact.

[P3-T10] required the anchored numstat on `.github/workflows/dependabot-repair.yml` to show
**at least 6 deletions**. The measured figure is **5**, against 55 additions where at least 12
were required. The delivered disclosure rewrite replaces one line rather than two, because the
`$updated = Join-Path ...` line was preserved verbatim; raising the count would mean deleting a
line that needs no deletion.

The clause's stated purpose — that it "fails if one of the four edits was not in fact applied"
— is satisfied by four independent exact-count measurements and by [P3-T8]'s four red-to-green
pairs. No plan text, acceptance clause or fixture was adjusted to accommodate it.
`E/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md` carries the per-edit accounting.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
- Total AC items: 26
- Checked off (delivered): 23
- Remaining (unchecked): 3
- Items remaining:
  - AC18 - The repair commit is pushed under the GitHub App identity.
  - AC19 - The required checks re-run and pass on the post-repair head SHA.
  - AC20 - Disclosure is present and conditional.
```

## Output Summary

12 findings listed; 11 discharged with evidence, R1 deferred to Phase 6. Two discharges are
narrower than "fixed" and are labelled accordingly, and R4 is discharged in the working tree
only. The criterion ledger reads exactly 26 total, 23 ticked, 3 unticked, with AC14 ticked and
carrying its amendment and no criterion reworded by this cycle. The four #914 residuals are
reproduced and AC18, AC19 and AC20 stay unticked. One acceptance clause, [P3-T10]'s deletion
floor, was not met and is reported rather than accommodated.
