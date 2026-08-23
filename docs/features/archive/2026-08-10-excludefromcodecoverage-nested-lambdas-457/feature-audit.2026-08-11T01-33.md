# Feature Audit — bug/excludefromcodecoverage-nested-lambdas-457

- Feature: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`
- Issue: #457
- Branch: `bug/excludefromcodecoverage-nested-lambdas-457` at `0105e71c` vs merge base `1c221399` on `epic/build-ci-coverage-gate-fidelity-integration`
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source per `.claude/skills/acceptance-criteria-tracking/SKILL.md`. `user-story.md` exists per the epic deliverables list and carries zero checkbox items (verified); it is not an AC source and its lack of AC content is expected, not a gap.
- Reviewer: feature-review agent
- Timestamp: 2026-08-11T01-33

## Baseline Comparison (measured, verified from committed evidence)

Repository-wide, post-#441 arithmetic, before -> after (from `evidence/qa-gates/coverage-delta.2026-08-11T01-58.md`, cross-checked against the verbatim document-element extract in `evidence/qa-gates/coverage-final-extract.2026-08-11T01-56.md`):

| Measure | Before | After | Delta |
|---|---|---|---|
| lines-covered | 53663 | 53375 | -288 |
| lines-valid | 62873 | 62401 | -472 |
| line-rate | 0.853514 | 0.855355 | +0.001841 |
| branches-covered | 12609 | 12541 | -68 |
| branches-valid | 15956 | 15872 | -84 |
| branch-rate | 0.790236 | 0.790134 | -0.000102 |

Reviewer arithmetic checks: 53375/62401 = 0.855355 and 12541/15872 = 0.790134, matching the recorded rates; 232/234 = 0.991453 matches the recorded per-file rate for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` (0.906977 -> 0.991453, 258 -> 234 lines). `TaskVisualization/FlagTasks.cs` is absent from the post-change report, which is the correct semantic for a wholly attributed type. These figures match the review directive's table exactly.

## Acceptance Criteria Evaluation (16 items, `spec.md` § Acceptance Criteria)

| # | Criterion (abridged) | Verdict | Evidence relied on |
|---|---|---|---|
| 1 | Exempt-member lambda leaves the coverage denominator | PASS | Regression cases 1, 6, 8 (reviewer re-ran: pass); measured 24-line denominator reduction for `BreadcrumbPopupUiOperations.cs` and full disappearance of `FlagTasks.cs` (`coverage-delta.2026-08-11T01-58.md`). |
| 2 | Non-exempt-member lambda still counted | PASS | Regression cases 2 (plain member), 3 (async member), 4 (mixed closure); reviewer verified the assertions are scoped to the closure class's own rollup and are non-vacuous. |
| 3 | Fix surface recorded in `spec.md` with justification against every candidate alternative | PASS | `spec.md` § Proposed Fix carries the five-row candidate table (1a, 1b, 1c, 1c-source, 2, 3) with disqualification reasons and live counter-examples. |
| 4 | Deterministic Pester regression tests, both directions, no temporary files / on-disk fixtures / committed `.cs` sources; inline here-strings only | PASS | Reviewer read both test files in full and grep-verified zero file-system primitives; all fixtures are inline here-strings; 31/31 pass deterministically on re-run. |
| 5 | Repository baseline re-captured against post-#441 arithmetic, recorded numerically under `evidence/baseline/` and `evidence/qa-gates/` | PASS | `evidence/baseline/coverage-baseline-extract.2026-08-11T00-30.md` + `coverage-collection.2026-08-11T00-30.md`; `evidence/qa-gates/coverage-final-extract.2026-08-11T01-56.md` + `coverage-collection.2026-08-11T01-56.md`; `dependency-441-verification.2026-08-11T00-02.md` confirms #441 present at merge base. |
| 6 | No coverage threshold changed; failing-figure handoff to #494 | PASS | Full-diff scan: no threshold change anywhere; `evidence/qa-gates/threshold-assessment.2026-08-11T02-00.md` records the #494 handoff (measurement gap + corrected baseline). No measurable figure fails a documented floor. |
| 7 | Full PowerShell toolchain pass in order with recorded exit codes | PASS | `evidence/qa-gates/toolchain-loop.2026-08-11T01-48.md` (iteration 1 fail at analyze, iteration 2 clean pass, exit codes per step); reviewer independently reproduced the analyze result (1 pre-existing diagnostic) and the test result (31/31). |
| 8 | Filter invoked inside `ConvertTo-KoverageCoberturaXml` after path normalization and before the merge; case 6 proves pre-merge ordering end-to-end | PASS | Call site Helpers.ps1:427 verified; reviewer probe proved a post-merge placement is a silent no-op and that case 6's assertions fail under it (details in `policy-audit.2026-08-11T01-33.md` § 3). |
| 9 | Presence set admits `Type.<Member>d__<N>`; case 3 proves a covered lambda inside a non-exempt async member is retained | PASS | ClosureFilter.ps1:202 (end-anchored regex); regression case 3 passes and models the verified live counter-example (`<>c__DisplayClass33_1` / `CreateAndInstallSurfaceAsync`). |
| 10 | All ten regression cases implemented as individually named, passing Pester tests across the two named files | PASS | Cases 1-5, 7-10 in ClosureFilter.Tests.ps1; case 6 in Helpers.Tests.ps1; each names its case in a scenario comment; all pass on reviewer re-run. |
| 11 | Filter is a pure XML-to-XML transform; idempotent | PASS | Full module read: no file, process, clock, randomness, or network access; case 10 proves idempotence non-vacuously and asserts silence on all output streams. |
| 12 | Unrecognized name shape causes retention, never removal | PASS | Fail-safe path ClosureFilter.ps1:296-300; case 4 (`.ctor` retained in a class the filter did modify) and case 9 (`MoveNext`/`.ctor` derive `$null`) discharge it. |
| 13 | Production changes limited to the new file plus exactly two edits in Helpers.ps1; both files under 500 lines; no C#, `coverage.config`, `*.runsettings`, `CLAUDE.md`, or `.claude/rules/**` modified | PASS | `git diff --numstat`: Helpers.ps1 exactly +2/-0 (dot-source at line 2, call at line 427); `wc -l`: 389 and 457; no prohibited file in the 57-file diff. |
| 14 | Corrected `BreadcrumbPopupUiOperations.cs` figure measured (not derived), with the `42_0` covered-lines note | PASS | `coverage-delta.2026-08-11T01-58.md` § "Every figure above is MEASURED": measured 0.991453 (232/234) explicitly contrasted with the wrong derived 0.991525 (234/236); the -2 covered delta demonstrates numerator-and-denominator removal. |
| 15 | Three residuals recorded and handed off as follow-up references, not absorbed | PASS | `evidence/other/documented-residuals.2026-08-11T02-04.md` records (a), (b), (c) with rationale; three potential entries exist on disk with the promotion-tooling headings verbatim and `## Provenance` sections naming #457. Note: the entries are potential entries, not yet GitHub issues — the recorded promotion path is `potential_to_issue` at epic close by the epic orchestrator (the executor's MCP tool set did not include the entry/promotion tools; the plan's documented fallback branch was taken). The epic orchestrator owes that promotion before epic close; this is a tracked follow-through, not a gap in this branch. |
| 16 | Async `d__` probe executed and recorded; spec corrected if it showed no `d__` class | PASS | `evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md`: `Probe Answer: YES` against a verified-raw corpus (soundness guard: attribute present 35 days before corpus capture), so the residual stands as written and no spec correction was required. Two measurement errors during probing were caught, corrected, and recorded rather than silently absorbed. |

No AC was found whose evidence fails to support its existing check-off; no premature check-off finding.

## Check-Off Verification

All 16 items in `spec.md` were already `[x]` at review start. Per the acceptance-criteria-tracking skill, the reviewer verified each check-off against evidence (table above) and confirms all 16 stand. The branch diff over `spec.md` and the plan is checkbox-only (verified by diff filtering); no criterion text was altered. The unchecked boxes in `issue.md` are not AC-source items under `full-bug` mode and impose no action.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/spec.md`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Outstanding Follow-Through (non-blocking)

1. AC 15 promotion: run `potential_to_issue` for the three residual entries at epic close (recorded owner: epic orchestrator).
2. Code-review CR-1/CR-2/CR-3 (see `code-review.2026-08-11T01-33.md`) are follow-up candidates, not remediation items.

## Verdict

All 16 acceptance criteria PASS. Blocking findings: 0. The feature delivers what issue #457 specified, on the measured post-#441 baseline, within all scope prohibitions.
