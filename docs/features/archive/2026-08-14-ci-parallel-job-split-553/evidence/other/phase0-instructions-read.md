# Phase 0 — Instructions Read (Issue #553)

- Timestamp: 2026-08-14T09-54 (local) / 2026-08-14T13:54:53Z (UTC)
- Task: [P0-T1]
- Plan of record: `docs/features/active/2026-08-14-ci-parallel-job-split-553/plan.2026-08-14T09-05.md`
- Work Mode: full-feature (from `issue.md` metadata marker)

## Policy Order

Read in the order defined by the plan's Required References section, which is
consistent with `.claude/skills/policy-compliance-order/SKILL.md` (CLAUDE.md →
general code change → general unit test → domain-specific rules), with the
feature's own requirement documents read last:

1. `CLAUDE.md` — all sections, including the C# Code Change Policy and the C# Unit
   Test Policy. Read for scope confirmation only; see the No-C#-Toolchain
   Statement below for why the C# toolchain loop does not apply to this change.
2. `.claude/rules/general-code-change.md` — cross-language code change policy
   (design principles, 500-line file limit, mandatory toolchain loop, I/O
   boundaries).
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy. No unit
   tests are added or modified by this feature; no coverage-bearing language is in
   scope.
4. `.claude/rules/ci-workflows.md` — governs `pwsh` steps in GitHub Actions
   workflows; the deliberately-failing-nested-command pattern and the
   `$LASTEXITCODE` reset requirement. Load-bearing for this feature: every `pwsh`
   step relocated into a callee is reviewed against this rule in P5-T2.
5. `.claude/rules/benchmark-baselines.md` — runner-environment parity for
   performance baselines. Load-bearing: the 444s baseline was captured on a
   GitHub-hosted `windows-latest` runner and the post-split comparison (P4-T6)
   must be drawn against a run of the same runner class.
6. `.claude/rules/tonality.md` — professional tone for all authored content,
   including the new `.github/workflows/README.md` and every evidence artifact.
7. `docs/features/active/2026-08-14-ci-parallel-job-split-553/spec.md` — design of
   record (10 acceptance criteria, 5 Definition-of-Done items, 8 seeded test
   conditions, 8 invariants).
8. `docs/features/active/2026-08-14-ci-parallel-job-split-553/issue.md` — the
   promoted issue and its 8 draft acceptance criteria (AC mirror).
9. `docs/features/active/2026-08-14-ci-parallel-job-split-553/user-story.md` — user
   story and its 8 resolved acceptance criteria (AC mirror). Under Work Mode
   `full-feature`, `spec.md` and `user-story.md` are the authoritative AC sources
   per `.claude/skills/acceptance-criteria-tracking/SKILL.md`; `issue.md` is
   tracked as a third mirror because the plan's check-off tasks name it.
10. `docs/features/active/2026-08-14-ci-parallel-job-split-553/research/2026-08-14T13-30-ci-parallel-job-split-research.md`
    — research artifact. Q8 = required-check migration sequencing (atomic PUT,
    context names captured never assumed); Q9 = `$LASTEXITCODE` hygiene review
    (no step in the current pipeline uses the deliberately-failing pattern).
11. `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`
    — the measured 444s sequential baseline and its step-level breakdown.

## Files Read (all eleven Required References)

| # | Path | Read |
| --- | --- | --- |
| 1 | `CLAUDE.md` | yes |
| 2 | `.claude/rules/general-code-change.md` | yes |
| 3 | `.claude/rules/general-unit-test.md` | yes |
| 4 | `.claude/rules/ci-workflows.md` | yes |
| 5 | `.claude/rules/benchmark-baselines.md` | yes |
| 6 | `.claude/rules/tonality.md` | yes |
| 7 | `docs/features/active/2026-08-14-ci-parallel-job-split-553/spec.md` | yes |
| 8 | `docs/features/active/2026-08-14-ci-parallel-job-split-553/issue.md` | yes |
| 9 | `docs/features/active/2026-08-14-ci-parallel-job-split-553/user-story.md` | yes |
| 10 | `docs/features/active/2026-08-14-ci-parallel-job-split-553/research/2026-08-14T13-30-ci-parallel-job-split-research.md` | yes |
| 11 | `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md` | yes |

Also read as binding execution context: `.claude/skills/atomic-plan-contract/SKILL.md`,
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`,
`.claude/skills/acceptance-criteria-tracking/SKILL.md`, and
`.claude/skills/policy-compliance-order/SKILL.md`.

## No-C#-Toolchain Statement (restated per [P0-T1])

This feature modifies no `*.cs`, `*.csproj`, `*.props`, `*.targets`, or
`packages.config` file in its final diff. Therefore:

- **No C# source is in scope.** The executor must not run `csharpier`, `msbuild`,
  or `vstest.console.exe` as verification of this change. A local C# pass would
  assert nothing about GitHub Actions workflow YAML. Spec Non-Goal 5 states the
  same boundary: "Any C# source, project, or test change" is out of scope.
- **actionlint is the only local harness.** There is no local test harness for
  GitHub Actions workflows. actionlint 1.7.7 (which includes YAML parse
  validation) is the only local verification available and is run three times by
  this plan: P0-T3 (pre-change baseline), P2-T3 (post-change), P5-T1 (final).
- **The authoritative verification is the green run** of the reworked pipeline on
  the branch head after push, per the `modified-workflow-needs-green-run` policy
  rule. Local checks are necessary but not sufficient.
- **No coverage capture applies.** No language with a mandatory coverage policy is
  modified, so no baseline or final-QC coverage tasks exist in this plan. The
  coverage-bearing artifact of the pipeline (the `test-results` upload) is
  preserved unchanged and its continued production is verified in P4-T5 against a
  live run.
- The Phase 4 seeded fault-isolation probes temporarily commit C# edits and then
  revert them. The net branch diff over C# and project files is zero, verified by
  P5-T3. Those probe commits are exercised by CI itself, not by a local toolchain
  pass.

## Acceptance ([P0-T1])

- Artifact exists with `Timestamp:`, `Policy Order:`, the explicit list of all
  eleven Required References, and the restated No-C#-Toolchain Statement.
- No code file has been modified at the time of writing. Verified by
  `git status --porcelain` recorded in the [P0-T2] artifact
  (`evidence/baseline/git-baseline.2026-08-14T09-54.md`): the only entries are
  documentation, evidence, and `.claude/agent-memory` paths; nothing under
  `.github/` and no `*.cs` file.
