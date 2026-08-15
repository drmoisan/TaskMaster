# [P4-T22] Acceptance criteria status summary

Timestamp: 2026-08-11T02-10

Work Mode: `full-bug`. Per `acceptance-criteria-tracking`, `spec.md` is the **sole** acceptance-criteria
source under this mode. `user-story.md` carries no `## Acceptance Criteria` section and is not an AC
source. `issue.md` carries a 7-item `## Acceptance Criteria` section, but under `full-bug` that section
is not the AC source either and was not modified.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/spec.md`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Check-off integrity

`git diff --stat` on `spec.md` reports **16 insertions, 16 deletions** — exactly one changed line per
criterion. Only `- [ ]` became `- [x]`. No criterion text was altered, no criterion was added, and no
other section of `spec.md` was modified. `[P4-T5]` took the `YES` branch, which authorizes no spec
edit, so the § Risks & Mitigations residual text is also unchanged.

## Per-criterion check-off record with cited evidence

| AC | Criterion (abbreviated) | Task | Cited evidence |
|---|---|---|---|
| 1 | Lambda inside an `[ExcludeFromCodeCoverage]` member does not appear in the denominator | `[P4-T6]` | `evidence/regression-testing/case-01-exclude.2026-08-11T00-44.md`; `evidence/regression-testing/case-06-pre-merge-ordering.2026-08-11T01-06.md`; `evidence/qa-gates/coverage-collection.2026-08-11T01-56.md` (P3-T7 measured figures); `evidence/regression-testing/pass-after-run.2026-08-11T01-30.md` (cases 1 and 6 `Passed`) |
| 2 | Lambda inside a non-attributed member still appears in the denominator | `[P4-T7]` | `case-02-keep.2026-08-11T00-46.md`; `case-03-async-guard.2026-08-11T00-46.md`; `pass-after-run.2026-08-11T01-30.md` (cases 2 and 3 `Passed`) |
| 3 | Fix surface recorded with justification against every candidate | `[P4-T8]` | `spec.md` § Proposed Fix candidate table (Candidates 1a, 1b, 1c, 1c-source, 2, 3) |
| 4 | Deterministic Pester tests, no temp files, no on-disk fixtures, no `.cs` sources | `[P4-T9]` | `evidence/regression-testing/fixture-purity-audit.2026-08-11T01-10.md`; `pass-after-run.2026-08-11T01-30.md` (cases 1-3 `Passed`, discharging the "both required directions" clause) |
| 5 | Baseline re-captured against post-#441 arithmetic, recorded numerically | `[P4-T10]` | `evidence/baseline/dependency-441-verification.2026-08-11T00-02.md`; `evidence/baseline/coverage-collection.2026-08-11T00-30.md`; `evidence/qa-gates/coverage-collection.2026-08-11T01-56.md` |
| 6 | No threshold changed; failing figures handed to #494 | `[P4-T11]` | `evidence/qa-gates/threshold-assessment.2026-08-11T02-00.md` |
| 7 | Full PowerShell toolchain pass in order with recorded exit codes | `[P4-T12]` | `evidence/qa-gates/poshqc-format.iter2.2026-08-11T01-42.md`; `poshqc-analyze.iter2.2026-08-11T01-44.md`; `poshqc-test.iter2.2026-08-11T01-46.md`; `toolchain-loop.2026-08-11T01-48.md` |
| 8 | Filter invoked after normalization, before the merge; proven end-to-end by case 6 | `[P4-T13]` | `[P2-T9]` call site as measured in `evidence/other/production-surface-audit.2026-08-11T01-26.md`; `case-06-pre-merge-ordering.2026-08-11T01-06.md`; `pass-after-run.2026-08-11T01-30.md` (case 6 `Passed`) |
| 9 | Presence set admits `d__` names; covered lambda in a non-exempt async member retained | `[P4-T14]` | `case-03-async-guard.2026-08-11T00-46.md`; `pass-after-run.2026-08-11T01-30.md` (case 3 `Passed`) |
| 10 | All ten regression cases implemented as individually named passing tests | `[P4-T15]` | `pass-after-run.2026-08-11T01-30.md` (all ten named, all `Passed`) |
| 11 | Filter is a pure XML-to-XML transform and is idempotent | `[P4-T16]` | `case-09-unit-purity.2026-08-11T01-02.md`; `case-10-idempotence.2026-08-11T01-02.md`; `evidence/other/filter-purity-audit.2026-08-11T01-26.md`; `pass-after-run.2026-08-11T01-30.md` (cases 9 and 10 `Passed`) |
| 12 | Unrecognized name shape causes retention, never removal | `[P4-T17]` | `case-04-mixed-closure.2026-08-11T00-50.md` (orchestrator-level, the `.ctor` retention assertion); `case-09-unit-purity.2026-08-11T01-02.md` (unit-level); `pass-after-run.2026-08-11T01-30.md` (cases 4 and 9 `Passed`) |
| 13 | Production changes limited to the new file and exactly two edits; both under 500 lines | `[P4-T18]` | `evidence/other/production-file-size.2026-08-11T01-24.md`; `evidence/other/production-surface-audit.2026-08-11T01-26.md`; `evidence/qa-gates/production-surface-final.2026-08-11T02-02.md` |
| 14 | Corrected per-file figure measured, with the `<>c__DisplayClass42_0` numerator note | `[P4-T19]` | `evidence/qa-gates/coverage-delta.2026-08-11T01-58.md` |
| 15 | Three residuals recorded and handed off as follow-up references | `[P4-T20]` | `evidence/other/documented-residuals.2026-08-11T02-04.md`; `docs/features/potential/2026-08-11-exempt-async-member-lambdas-remain-counted.md`; `docs/features/potential/2026-08-11-local-functions-in-exempt-members-remain-counted.md`; `docs/features/potential/2026-08-11-overload-name-collision-under-exclusion.md` |
| 16 | Async-`d__` probe executed, result recorded, residual corrected if contradicted | `[P4-T21]` | `evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md`; `evidence/other/probe-reconciliation.2026-08-11T02-06.md` |

Notes on citation discipline, applied as the plan directs:

- For AC 1, 2, 4, 8, 9, 11, 12 the Phase 1 `case-NN` artifacts are written by `[expect-fail]` tasks and
  record the **pre-implementation failing** state, so they cannot on their own discharge a delivery
  criterion. The `[P3-T1]` `pass-after-run` artifact is the only record of the passing state and is
  cited alongside them in every case.
- For AC 9 and AC 12, an acceptance criterion stated in a plan task is not evidence. `[P2-T4]`'s
  presence-set rule and `[P2-T5]`'s fail-safe clause are therefore **not** cited; the discharging
  evidence is the regression cases and the passing run.
- For AC 13, the "exactly two edits" clause is checked off against the
  `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` added-line/removed-line
  measurement (2 added, 0 removed), recorded by `[P2-T11]` and re-measured post-format by `[P3-T10]`,
  not against `git status --porcelain`, which cannot count edits.

## Plan outcome

All 16 acceptance criteria are checked off against evidence that exists on disk with complete field
sets. No item remains unchecked, so the plan outcome is **not INCOMPLETE** on acceptance-criteria
grounds.
