# Remediation Inputs — Issue #267 (ci-quality-gates-speedup)

- Entry timestamp: 2026-07-08T01-41
- Source audit artifacts:
  - `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/policy-audit.2026-07-08T01-41.md` (§ 5.3, § 7)
  - `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/feature-audit.2026-07-08T01-41.md` (AC6 evaluation)
  - `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/code-review.2026-07-08T01-41.md` (no blocking code findings; two non-blocking informational items)

## Blocking Finding

**AC6 / `modified-workflow-needs-green-run` — unmet.** The branch diff modifies `.github/workflows/ci.yml`, which matches `.github/workflows/**`, triggering the `modified-workflow-needs-green-run` policy rule (`.claude/skills/feature-review-workflow/SKILL.md`, backed by `.claude/rules/ci-workflows.md`). No evidence of a green GitHub Actions run — nor a green `workflow_dispatch` run — against branch head `7ffc96cc67e85983d6034632d4fd1fd466deda5c` exists anywhere in the repository, working tree, or accessible artifacts. `gh` (GitHub CLI) is unavailable in this environment, so live CI status could not be queried directly during review.

This finding is **not** a defect in the workflow content: AC1–AC5 all evaluate PASS, `actionlint` was independently re-run and returned zero findings, and the two full-solution `msbuild` passes are supported by detailed, internally consistent diagnostic-parity evidence. The remediation required here is procedural/evidentiary, not a code change.

## Enumerated Fix List

1. **Produce a green CI run against the branch head.**
   - Expected behavior: open (or update) the pull request for `refactor/ci-quality-gates-speedup-267` against `main` so the `actionlint` and `quality-gates` GitHub Actions jobs run on GitHub-hosted runners against head SHA `7ffc96cc67e85983d6034632d4fd1fd466deda5c` (or a later commit on the same branch, re-evaluating the rule against the new head), or trigger a `workflow_dispatch` run against that head if opening the PR is not yet desired.
   - Verification command: after the run completes, capture its conclusion and head SHA (e.g. via `gh run list --branch refactor/ci-quality-gates-speedup-267` / `gh run view <run-id>` once `gh` is available and authenticated, or the equivalent GitHub UI/API evidence) and confirm `conclusion == success` and the run's head SHA matches the branch head at PR time.
   - File(s): none (no source file changes expected); this step produces evidence only.

2. **Record the green-run evidence in the feature folder.**
   - Expected behavior: write a new evidence artifact (e.g. `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/ci-green-run.<timestamp>.md`) containing the run URL, head SHA, workflow name(s), and conclusion for both the `actionlint` job and the `quality-gates` job.
   - Verification: the artifact's recorded head SHA must match the branch head at the time the run completed; if the branch has moved since 7ffc96cc, re-run and record the new head instead of reusing stale evidence.
   - File(s): new evidence file only.

3. **Check off AC6 in `issue.md` once the green-run evidence exists.**
   - Expected behavior: change `- [ ] AC6` to `- [x] AC6` in `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`, citing the new evidence artifact from step 2.
   - Verification: re-run feature-review (or a targeted re-audit) to confirm AC6 now evaluates PASS with the new evidence present.
   - File(s): `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md` (checkbox only; no criterion text change).

## Do Not

- Do not weaken, remove, or bypass the `actionlint` job or the `quality-gates` job's two `msbuild` enforcement passes to force a green run.
- Do not check off AC6 based on a local `msbuild`/`actionlint` re-run alone — AC6 specifically requires a GitHub Actions run (or `workflow_dispatch` run) against the branch head, not a local-machine substitute.
- Do not consolidate the two retained `msbuild` passes as part of this remediation — that was already investigated and explicitly rejected by the Scope Decision (2026-07-07) in `issue.md`; reopening it is out of scope for this remediation cycle.
- Do not fold the separately tracked CI-nullable-check gap (`docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`) into this remediation; it is an intentionally separate follow-up item.
- Do not silently skip this gate by marking AC6 "N/A" or "out of scope" — the rule explicitly applies because `.github/workflows/ci.yml` is in the diff.

## Handoff Note

Per `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`, a required-CI-check remediation of this kind is ordinarily routed through `atomic-planner` for a formal remediation plan and `atomic-executor` for preflight/execution. Given that no source-code change is required here — only opening/updating the PR, waiting for the hosted runner, and recording evidence — the orchestrating agent should evaluate whether a full atomic-plan cycle is proportionate or whether the three steps above can be executed directly and then re-audited. Either path must end with a fresh `feature-review` pass confirming AC6 evaluates PASS before this feature is considered merge-ready.
