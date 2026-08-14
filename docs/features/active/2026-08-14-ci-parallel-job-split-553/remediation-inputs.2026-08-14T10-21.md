# Remediation Inputs — ci-parallel-job-split (Issue #553)

- **Date:** 2026-08-14 (artifact timestamp 2026-08-14T10-21)
- **Source audits:**
  - `docs/features/active/2026-08-14-ci-parallel-job-split-553/policy-audit.2026-08-14T10-21.md`
  - `docs/features/active/2026-08-14-ci-parallel-job-split-553/code-review.2026-08-14T10-21.md`
  - `docs/features/active/2026-08-14-ci-parallel-job-split-553/feature-audit.2026-08-14T10-21.md`
- **Base:** `main` @ merge base `2073f717bbfac30053f3d6a4e652d99af3ae5c9c`
- **Head:** `feature/ci-parallel-job-split-553` @ `0b016c81a78f3fafc0864de472f4139cc0938002`
- **Blocking finding count: 1**

## Finding B1 — no green workflow run against the branch head

- Severity: Blocking
- Rule: `modified-workflow-needs-green-run` (`.claude/skills/feature-review-workflow/SKILL.md` § Policy Rules; policy audit § 7.3; code review finding F1)
- Trigger: the branch diff modifies `.github/workflows/**` (7 files: `ci.yml` modified; `_actionlint.yml`, `_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`, `README.md` added).
- Evidence gap: no workflow run — PR-context or `workflow_dispatch` — with head SHA `0b016c81a78f3fafc0864de472f4139cc0938002` and conclusion `success` exists or is evidenced anywhere in the feature folder. The branch has not been pushed; plan tasks P3-T1 through P3-T4 are unchecked by design.
- Nature: procedural sequencing, not a code defect. The rule is unconditional and cannot be satisfied before a live run exists. Local actionlint (exit 0, 7 files) cannot substitute; the spec's tailored-setup assumption (msbuild callees without `setup-dotnet`; format callee without `nuget restore`) is explicitly unverified until the first runner execution (spec Residual risk 2, with a documented fallback at ~56s/job).

### Required remediation

Execute the pending phases of the plan of record, `docs/features/active/2026-08-14-ci-parallel-job-split-553/plan.2026-08-14T09-05.md` (P3 onward). Minimum path to clear B1:

1. Commit review artifacts and push the branch (plan P3-T1, P3-T2).
2. Open the PR to `main` via the `pr-author` skill (plan P3-T3 — **orchestrator-confirmation-required**; list only #553 as the closing issue; ignore the spurious `#ISO-8601` / `#SHA-256` tokens in the generated summary).
3. Observe the first split-pipeline run to completion and record `evidence/qa-gates/first-run.<TS>.md` (plan P3-T4). If the tailored-setup assumption fails, execute the fallback (plan P3-T5) and re-run to green.
4. A green `workflow_dispatch` run against the branch head also satisfies the rule if the PR path is blocked.

Note: clearing B1 unlocks the remaining FAIL-pending acceptance criteria, which are not separate blocking findings but must complete before merge: context-name capture (P5-T16), atomic ruleset PUT with pre/payload/post evidence (P6-T1..T4; P6-T3 orchestrator-confirmation-required; spec AC S6/S9), and post-split timing measurement (P4-T6; spec AC S10).

## Non-blocking findings (no remediation plan required; fix opportunistically)

- F2 (Minor): `.github/workflows/README.md` line 82 — reword the "`CI / <gate>` context" phrase to the `<caller job> / <callee job>` context-name form used elsewhere in the document and in spec.md.
- F3 (Minor): add a sibling `baseline.provenance.json` to `evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md` only if that baseline is ever consumed by an automated benchmark regression gate; no action required now (rule-scope assessment in policy audit § 3.3).

## Handoff

Per `remediation-handoff-atomic-planner`, the remediation plan is authored by `atomic-planner`, not by this reviewer. Recommended handoff: delegate to `atomic-planner` with this file as primary context. Because the remediation for B1 is exactly the unexecuted remainder of the existing plan of record (`plan.2026-08-14T09-05.md`, Phases 3–7, already preflighted and partially executed), the planner may ratify that plan as the remediation plan of record rather than authoring a duplicate; two of its tasks (P3-T3, P6-T3) are marked orchestrator-confirmation-required and must not be executed autonomously.

Documented layout note: this file uses the flat `remediation-inputs.<timestamp>.md` form required by the enforced hook `.claude/hooks/validate-feature-review-coverage.ps1`, not the `remediation/<ts>/` folder layout described in the handoff skill (pre-existing skill/hook conflict, recorded in policy audit § 8.4).
