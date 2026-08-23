# Acceptance Criteria Reconciliation

Timestamp: 2026-08-08T17-11

Task: [P2-T25]

PostedAs: unknown (local mirror only — no GitHub issue update was performed by this executor; the
orchestrator handles issue posting and all commits)

AC source (sole, per `minor-audit` work mode):
`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/issue.md`,
`## Acceptance Criteria` section.

## All nine boxes are `[x]`, each citing an artifact that exists on disk

| AC | State | Cited evidence artifact | Exists | Verdict |
|---|---|---|---|---|
| AC1 | `[x]` | `evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md` | yes | PASS |
| AC2 | `[x]` | `evidence/qa-gates/prohibited-fix-audit.2026-08-08T17-07.md` | yes | PASS |
| AC3 | `[x]` | `evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md` | yes | PASS |
| AC4 | `[x]` | `evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md` | yes | PASS |
| AC5 | `[x]` | `evidence/qa-gates/prohibited-fix-audit.2026-08-08T17-07.md` | yes | PASS |
| AC6 | `[x]` | `evidence/regression-testing/fail-before.2026-08-08T16-26.md` | yes | PASS |
| AC7 | `[x]` | `evidence/qa-gates/repeat-run-1.2026-08-08T16-58.md`, `repeat-run-2.2026-08-08T17-00.md`, `repeat-run-3.2026-08-08T17-02.md` | yes (all 3) | PASS |
| AC8 | `[x]` | `evidence/qa-gates/toolchain-clean-pass.2026-08-08T16-56.md` | yes | PASS |
| AC9 | `[x]` | `evidence/qa-gates/coverage-delta.2026-08-08T17-04.md` | yes | PASS |

Verified against the live `issue.md` (lines 124-146): 9 of 9 items are `- [x]`; zero remain `- [ ]`.
Only the checkbox characters were changed; no criterion text was modified, and no AC item was added
or removed.

## Substantiation summary

- **AC1** — Three consecutive runs under class-level parallelization produced identical counts
  (4667/4667/0) with all four tests green; per-test durations varied across runs, proving scheduling
  genuinely differed while outcomes did not. The test now arranges both `??` operands explicitly via
  the seam, so it cannot depend on the pooled thread, on execution order, or on whether
  `UiThread.Initialize()` ran.
- **AC2** — Zero hits on all seven prohibited patterns across the 270-line scoped diff, and the
  assertion is still exactly `ThrowAsync<InvalidOperationException>()` (1 occurrence, line 134). The
  production guard and its message text are byte-identical to pre-change.
- **AC3** — All three branches pinned by dedicated tests; mechanically confirmed by 100% (2/2)
  condition coverage on line 60 (the `??` resolution) and line 62 (the null guard).
- **AC4** — Public surface gained only the explicit parameterless constructor (reproducing the
  implicit one's signature); seam constructor is `internal`; defaults reproduce the pre-change
  expressions exactly; both out-of-scope call sites unchanged and compiling.
- **AC5** — Same audit as AC2; none of the five approaches in the issue's `## Prohibited Fixes` list
  was used.
- **AC6** — A genuine failing run was produced: EXIT_CODE 1, `Failed: 1`, "Expected a
  `<System.InvalidOperationException>` to be thrown, but no exception was thrown", after a verified
  rebuild (DLL mtime 16:18:36 -> 16:24:18) ruling out a stale-assembly false pass. No exception
  dossier was needed.
- **AC7** — Three consecutive full parallel runs recorded as separate artifacts, identical counts,
  all four tests green in every run (12/12 observations).
- **AC8** — Pass 4 attested as a single clean pass: format (0), check (0), analyzers (0), nullable
  (0), tests+coverage (0, 6295/6295). Per-step artifacts exist for every step. Earlier failing
  passes are disclosed in the same artifact.
- **AC9** — Repository-wide line-rate 0.858162 -> 0.858328 (delta **+0.000166**, non-negative);
  changed-class coverage went from unmeasured (attribute-excluded at baseline) to 96.43% deduped /
  97.37% tool-reported line and 100% branch, so it cannot have decreased.

## Qualifications recorded honestly (not gate weakenings)

These are disclosed in the cited artifacts and are restated here so the reduced audit sees them at
the reconciliation point:

1. **AC8 required four loop passes.** Passes 1 and 2 failed on two pre-existing, out-of-scope
   `QuickFiler.Test` failures; pass 3 was abandoned after a stale-build false-pass condition was
   detected and corrected; pass 4 is clean. A controlled attribution experiment
   (`evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`) proved those
   failures reproduce at merge-base with the change fully reverted (6293/6291/2, matching the "Run 1"
   figures at `issue.md:53`). No test was ignored, filtered, or retried to reach the clean pass.
2. **The nullable toolchain step is an incremental no-op** in this repository, at the gate exactly
   as at the baseline, so the comparison is like-for-like but the step enumerates nothing itself.
   The effective nullable check on the changed code is the analyzer build, which recompiled both
   projects and reported zero CS86xx. A forced rebuild reveals 195 pre-existing repository-wide
   nullable errors, none in `WpfDispatcherYield.cs`; that debt is out of scope.
3. **AC4's "is justified in the PR body"** clause is satisfiable only when the PR is authored. The
   technical substance of the justification is fully recorded in
   `evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md` and in the plan's
   `## Design Decision — Seam Shape` section, which the PR body should draw from. This executor does
   not author PRs or commit.

Output Summary: PASS. All nine acceptance criteria in the `## Acceptance Criteria` section of
`issue.md` are `[x]`, each citing an evidence artifact confirmed to exist on disk under
`<FEATURE>/evidence/<kind>/`. Only checkbox characters were altered; no criterion text changed. Three
qualifications are disclosed: AC8 required four toolchain passes (failures proven pre-existing and
out of scope by a merge-base attribution experiment), the nullable step is an incremental no-op at
both baseline and gate, and AC4's PR-body clause awaits PR authoring by the orchestrator.
