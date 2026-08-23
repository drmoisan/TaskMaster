# Remediation Review-Handoff Index — Cycle 1 (R1)

Timestamp: 2026-08-23T19-35

Feature root: `docs/features/active/winformspumphost-suite-determinism-511` (abbreviated `FEATURE`
below; every path in this index is written relative to that root).

Branch: `bug/winformspumphost-suite-determinism-511-exec`
Merge base with `origin/main` (`$MergeBase`, recorded by P0-T6):
`f85a36faebaaec29fe5233c9d9f69d223d80e4c5`

No pull request is created and no CI run is monitored by this plan; both are handled outside it, with
the pull request targeting `main` per orchestrator Decision 1.

---

## Markdown evidence artifacts produced by this plan

### Phase 0 — policy reads and remediation baseline

| Path | Task |
| --- | --- |
| `evidence/remediation-baseline/phase0-instructions-read.md` | P0-T5 |
| `evidence/remediation-baseline/git-identity.2026-08-23T20-57.md` | P0-T6 |
| `evidence/remediation-baseline/touched-files-state.2026-08-23T20-57.md` | P0-T7 |
| `evidence/remediation-baseline/toolchain-precheck.2026-08-23T20-57.md` | P0-T8 |
| `evidence/remediation-baseline/evidence-gitignore.2026-08-23T20-57.md` | P0-T9 |

### Phase 1 — comment corrections

| Path | Task |
| --- | --- |
| `evidence/regression-testing/file-size-after-comment-fix.2026-08-23T20-57.md` | P1-T3 |
| `evidence/regression-testing/scope-lock-after-comment-fix.2026-08-23T20-57.md` | P1-T4 |

### Phase 2 — spec and plan reconciliation

| Path | Task |
| --- | --- |
| `evidence/regression-testing/p4-t2-narrowing-rationale.2026-08-23T20-57.md` | P2-T4 |
| `evidence/other/discharged-issue-tasks.2026-08-23T20-57.md` | P2-T6 |

### Phase 3 — final QC loop

| Path | Task |
| --- | --- |
| `evidence/qa-gates/remediation-tool-restore.2026-08-23T20-57.md` | P3-T1 |
| `evidence/qa-gates/remediation-csharpier-format.2026-08-23T20-57.md` | P3-T2 |
| `evidence/qa-gates/remediation-csharpier-check.2026-08-23T20-57.md` | P3-T3 |
| `evidence/qa-gates/remediation-analyzer-gate.2026-08-23T20-57.md` | P3-T4 |
| `evidence/qa-gates/remediation-nullable-gate.2026-08-23T20-57.md` | P3-T5 |
| `evidence/qa-gates/remediation-suite-run.2026-08-23T20-57.md` | P3-T6 |
| `evidence/qa-gates/remediation-coverage.2026-08-23T20-57.md` | P3-T7 |
| `evidence/qa-gates/remediation-coverage-delta.2026-08-23T20-57.md` | P3-T8 |
| `evidence/qa-gates/remediation-coverage-artifact.2026-08-23T20-57.md` | P3-T9 |
| `evidence/qa-gates/remediation-file-size-audit.2026-08-23T20-57.md` | P3-T10 |
| `evidence/qa-gates/remediation-clean-pass.2026-08-23T20-57.md` | P3-T11 |

### Phase 4 — acceptance criteria, handoff, and evidence hygiene

| Path | Task |
| --- | --- |
| `evidence/other/ac-status-summary.2026-08-23T20-57.md` | P4-T7 |
| `evidence/other/remediation-review-handoff.2026-08-23T20-57.md` (this index) | P4-T8 |
| `evidence/other/raw-artifact-deletion.2026-08-23T20-57.md` | P4-T10 — written **after** this index |

This index lists Markdown evidence only. The raw P3-T6 TRX is named separately below.

Verification of the resolves-to-existing condition was performed twice, because the last row above is
written by a later task than the one that writes this index. At P4-T8 time, all 22 listed Markdown
paths other than the P4-T10 row resolved to existing files. After P4-T10 wrote its artifact, the
check was re-run over all 23 listed Markdown paths and all 23 resolved. Both results are recorded in
the executor's task log.

### Raw (non-Markdown) artifact, not subject to the resolves-to-existing condition

`evidence/qa-gates/r1-p3-t6/` — one TRX file plus one binary `.coverage` file produced by the P3-T6
suite run. **deleted by P4-T10.** The directory is excluded by the `r1-p*-t*/` line that P0-T9
appended to `evidence/.gitignore`, so it was never stageable; the distilled Markdown record
`evidence/qa-gates/remediation-suite-run.2026-08-23T20-57.md` is the evidence of record.

---

## The seven remediation exit criteria and the task that discharged each

Source: `remediation-inputs.2026-08-23T20-57.md`, Part 5 (criteria 1 through 6) plus the Part 6
addendum (criterion 7).

| # | Exit criterion | Discharged by | Evidence |
| --- | --- | --- | --- |
| 1 | Both comment blocks state the measured truth, including the present redundancy of the read | P1-T1, P1-T2 | the two `.cs` files in the diff; `evidence/regression-testing/file-size-after-comment-fix.2026-08-23T20-57.md` |
| 2 | Spec AC 6 revised to the measured inherited state; AC 3 revised to the owned-class scope citing #594; AC 8, AC 13 and AC 14 satisfied and checked off with cited evidence | P2-T1, P2-T2, P4-T1 through P4-T4, P4-T6 | `spec.md` (14 of 14 checked); `evidence/other/ac-status-summary.2026-08-23T20-57.md` |
| 3 | P4-T2's zero condition narrowed to owned classes and recorded as satisfied on existing evidence | P2-T3, P2-T4 | `plan.2026-08-21T18-10.md`; `evidence/regression-testing/p4-t2-narrowing-rationale.2026-08-23T20-57.md` |
| 4 | The Phase 5 (here Phase 3) toolchain completes green in a single final pass with numeric coverage recorded | P3-T1 through P3-T11 | `evidence/qa-gates/remediation-clean-pass.2026-08-23T20-57.md`; `evidence/qa-gates/remediation-coverage.2026-08-23T20-57.md` |
| 5 | The evidence `.gitignore` exists and the raw `.trx` / `.coverage` files are removed | P0-T9, P4-T10 | `evidence/remediation-baseline/evidence-gitignore.2026-08-23T20-57.md`; `evidence/other/raw-artifact-deletion.2026-08-23T20-57.md` |
| 6 | No artifact claims this branch repairs #511 or #571, and no closing keyword for either appears in the branch or the pull-request body | P4-T8 (file scan), P4-T9 and P4-T10 (git-log scan) | the file-scan and git-log-scan results recorded in this index and in the two commit tasks |
| 7 | The spec's `## Scope & Non-Goals` "In scope" bullets no longer assert the falsified premise | **P2-T7 and P2-T8** | the exit-criterion-7 re-check recorded below |

---

## Exit-criterion-7 spec-scope re-check

`Select-String -SimpleMatch` against `spec.md` only, for each of the four falsified-premise literals:

| Literal | Matching lines in `spec.md` | Required |
| --- | --- | --- |
| `Deterministic creation of the` | **0** | 0 |
| `#571 in full` | **0** | 0 |
| `removing the handle race removes` | **0** | 0 |
| `as requiring its own issue against` | **0** | 0 |

All four falsified-premise literals are absent from `spec.md`. The two bullets that remained accurate
were retained and verified present: `Regression tests for the new fixture invariant` matches exactly
1 line and `An empirical pre-fix and post-fix determinism record captured as evidence.` matches
exactly 1 line.

---

## Closing-keyword file scan

Scan: `Select-String` with the case-insensitive regex `(fix|clos|resolv)[a-z]* #(511|571)` against
five files.

| # | Scanned file | Matches | Required |
| --- | --- | --- | --- |
| a | `spec.md` | **0** | 0 |
| b | `remediation-plan.2026-08-23T20-57.md` | **0** | 0 |
| c | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | **0** | 0 |
| d | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | **0** | 0 |
| e | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | **0** | 0 |

The git-log leg of the scan is deliberately not run here: this task runs before the P4-T9 and P4-T10
commits, so `git log --format=%B $MergeBase..HEAD` could not yet see the messages this plan produces
and the leg could not fail. It runs post-commit in P4-T9 and is repeated in P4-T10.

### Requirements-input carve-out record

`remediation-inputs.2026-08-23T20-57.md` carries **three** matches of the scan regex, at its lines
227, 248, and 264, all inside negations that deny the repair claim. That file is **exempt by design**
from the file scan per the plan preamble: it is the input this plan consumes, not an artifact this
plan produces, and it was already committed to the branch before this cycle opened. GitHub parses
closing keywords only in commit messages and pull-request bodies, never in file contents, so its
committed text cannot auto-close either issue. A literal re-audit of "no closing keyword anywhere in
the branch" must apply this carve-out rather than raise the requirements input as a finding.

### One additional observation, recorded rather than repaired

The same class of pre-existing, already-committed file-content match exists at
`plan.2026-08-21T18-10.md` line 26, in the original plan's summary prose. That file is not among the
five the plan directs this task to scan, the match predates this cycle (it is byte-identical in
`HEAD`), and it is file content rather than a commit message or pull-request body, so it cannot
auto-close either issue. It is recorded here for the re-audit's benefit and was deliberately not
edited, because editing it is work this plan does not describe.

---

## Residual conditions recorded rather than repaired

1. **The genuine defect behind the #511 report** — the load-induced 60,000 ms `PumpTimeoutMs` expiry
   cascade under machine load — is tracked as issue **#592** and is out of scope for this branch.
   Findings A, B, and C are accepted: the fixture-hardening statement forces a handle that
   construction already created, and this branch's value is the hardening, the regression tests
   pinning the inherited state, and the mechanism finding.
2. **The three pre-existing `UtilitiesCS.Test` flakes** blocking any suite-wide zero gate are tracked
   as issue **#594**. Every suite gate in this plan is scoped to the `QuickFiler.Test` assembly
   accordingly. In this cycle's P3-T6 run and P3-T7 coverage run the flakes did not fire — both
   recorded 6,459 passed and 0 failed on an otherwise idle machine — so they remain a real risk under
   load that this cycle does not claim repaired.
3. **The repository-wide analyzer version skew** is tracked as issue **#597**; this plan back-fills
   nothing new and edits no project file.
4. **Residual CPU-contention sensitivity** of the pump-hosted suite is a stated trade, unchanged by
   this cycle.
5. **The `InvokeBeginInvoke` production asymmetry** remains a follow-up recorded in the spec's
   `## Rollout & Follow-up`, not addressed here.

---

## Diff scope at handoff

`git diff --name-only $MergeBase`, filtered to `.cs`, `.csproj`, `.props`, `.targets`, and `.config`,
is exactly three paths, all under `QuickFiler.Test/`:

1. `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`
2. `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`
3. `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`

Prohibited counts, each exactly 0: paths beginning `QuickFiler/`, paths ending `.csproj`, and paths
beginning `.claude/` other than `.claude/agent-memory/`. Recorded in
`evidence/regression-testing/scope-lock-after-comment-fix.2026-08-23T20-57.md` and re-verified by
P4-T9 after the commit.
