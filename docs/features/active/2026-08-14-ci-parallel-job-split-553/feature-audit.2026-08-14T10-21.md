# Feature Audit — ci-parallel-job-split (Issue #553)

- **Date:** 2026-08-14 (artifact timestamp 2026-08-14T10-21)
- **Work mode:** `full-feature` (persisted marker in `issue.md`)
- **AC sources:** `spec.md` (10 criteria) and `user-story.md` (8 criteria); `issue.md` is an early-draft mirror, not authoritative

## Scope and Baseline

- **Base branch:** `main` (resolved `origin/main`)
- **Merge base:** `2073f717bbfac30053f3d6a4e652d99af3ae5c9c` (independently recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied value)
- **Head:** `feature/ci-parallel-job-split-553` @ `0b016c81a78f3fafc0864de472f4139cc0938002` (3 commits ahead of the merge base)
- **Diff scope:** full branch diff, 37 files — 6 workflow YAML files, the new `.github/workflows/README.md`, the feature folder (issue/spec/user-story/plan/research + evidence tree), 2 archival promoted-potential copies (#554, #555), 6 agent-memory files
- **Evidence sources:** `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` (fresh; recorded head matches `git rev-parse HEAD`), committed feature-folder evidence, and direct file/diff inspection by this review
- **Plan of record:** `plan.2026-08-14T09-05.md` — P0 through P2-T3 checked off; P3-T1 onward unchecked by design (those phases require a pushed branch, a live PR, and a green run)

## Acceptance Criteria Inventory

### spec.md § Acceptance Criteria

| # | Criterion (abbreviated) |
| --- | --- |
| S1 | Four gates run as separate jobs with zero `needs:` edges |
| S2 | Five callee workflows with `workflow_call` + `workflow_dispatch`, own `permissions:`, right-sized `timeout-minutes`, no `concurrency` |
| S3 | `ci.yml` is a pure orchestrator: only `uses:` jobs, no inline `steps:` |
| S4 | No cross-job file sharing; only the preserved `test-results` upload |
| S5 | Gate commands and actionlint step byte-identical, incl. `/t:Rebuild` comment, `$LASTEXITCODE` guards, zero-assembly `throw` |
| S6 | `main` ruleset contexts replaced in one atomic PUT with live-captured strings; pre-PUT JSON, PUT payload, post-PUT GET recorded |
| S7 | README documents per-stage dispatch and branch-protection rename procedures |
| S8 | Green run against branch head (`modified-workflow-needs-green-run`) |
| S9 | Every current gate still enforced; none dropped, weakened, or made non-required |
| S10 | Post-split wall clock measured with the baseline's collection method and compared against 444s |

### user-story.md § Acceptance Criteria

| # | Criterion (abbreviated) | Mirrors |
| --- | --- | --- |
| U1 | Separate jobs, zero `needs:` edges (resolved form) | S1 |
| U2 | Each gate a callable `_<name>.yml` with both triggers | S2 |
| U3 | `ci.yml` orchestrator-only (resolved: actionlint also extracted) | S3 |
| U4 | Cross-job sharing only via explicit artifacts (resolved: none exists; `test-results` upload preserved) | S4 |
| U5 | Ruleset contexts updated via live-captured names, single atomic PUT | S6 |
| U6 | README documents both procedures | S7 |
| U7 | Green run against branch head | S8 |
| U8 | Every gate still enforced; byte-identity, `/t:Rebuild` comment, `throw` guard, separate compiles | S9 + S5 |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
| --- | --- | --- |
| S1 / U1 | PASS | `ci.yml` (32 lines): five jobs, all `uses:`-form; `grep -n "needs:" .github/workflows/*.yml` returns zero matches; direct inspection by this review |
| S2 / U2 | PASS | All five callees declare `on: workflow_call:` + `on: workflow_dispatch:`, `permissions: contents: read`, `timeout-minutes` 10/10/30/30/30; `grep` for `concurrency` in callees returns zero matches |
| S3 / U3 | PASS | `ci.yml` contains no `steps:` key; header, triggers, permissions, and concurrency block byte-identical to merge base (verified against `git show 2073f717:.github/workflows/ci.yml`) |
| S4 / U4 | PASS | Single `upload-artifact` in the new pipeline (`test-results`, `if: always()`, same name/paths/`if-no-files-found: warn`); zero `download-artifact` occurrences |
| S5 | PASS | Independently re-verified by this review: extraction + SHA-256 comparison of 14 step blocks between merge-base `ci.yml` and the callees — 14/14 MATCH, including the full 7-line `/t:Rebuild` rationale comment, both `$LASTEXITCODE` guards, and the zero-assembly `throw`. Corroborates `evidence/qa-gates/byte-identity.2026-08-14T09-54.md` (6/6 SHA-256 rows, 12/12 fragment citations) |
| S6 / U5 | FAIL (pending, scheduled) | Not executed: no ruleset PUT, no `ruleset-pre-put`/`ruleset-put-payload`/`ruleset-post-put` evidence exists. Correctly sequenced by design — the PUT requires context names captured from a live green run (plan P5-T16, P6-T1..T4; P6-T3 is orchestrator-confirmation-required). Procedure fully documented in spec § Required-Status-Check Contract and README |
| S7 / U6 | PASS | README § "Per-stage workflow_dispatch procedure" (commands, UI path, two caveats) and § "Branch-protection rename procedure" (five steps, atomic-PUT payload construction, two-step-edit prohibition, verification, rollback); satisfies the `.claude/skills/orchestrate/SKILL.md` § GitHub Actions Reusable Workflows reference |
| S8 / U7 | FAIL (pending, scheduled) | No green run exists against head `0b016c81`; branch not yet pushed/PR'd (plan P3-T1..T4 unchecked). This is blocking finding B1 in the policy audit and F1 in the code review |
| S9 / U8 | PARTIAL | Workflow-level leg verified PASS: all gate commands byte-identical (S5), analyzer and nullable compiles separate, MSTest callee's plain build carries no analyzer/warning-promotion properties (grep count 0), no gate weakened. "Made non-required" leg depends on the pending ruleset PUT (S6) and the fail-closed strict policy guarantees over-blocking, never under-gating, in the interim. Completes with S6 |
| S10 | FAIL (pending, scheduled) | Post-split measurement requires a live run of the split pipeline; scheduled as plan P4-T6 with the same `gh api .../runs/<id>/jobs` collection method as the 444s baseline (runner-environment parity) |

Additional verification notes:

- The sequential baseline of record (`evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`) is measured from a GitHub-hosted `windows-latest` run with the run URL and collection command recorded; assessed against `.claude/rules/benchmark-baselines.md` in policy audit § 3.3 (PASS with one non-blocking observation).
- Definition of Done and Seeded Test Conditions in spec.md remain unchecked; every unchecked item maps to the pending live-PR phases (P3–P7) and is not independently deliverable from the committed tree.

## Summary

Seven of ten spec criteria (S1–S5, S7; S9 partially) are delivered and verified from the committed tree, including independent re-verification of the byte-identity claim by this review. The three FAIL-pending criteria (S6, S8, S10) and the pending leg of S9 share a single dependency chain: push branch → open PR → green run on the head → capture context names → atomic ruleset PUT → post-split measurement. That chain is exactly what the unexecuted plan Phases 3–7 encode, with the two outward-facing actions (PR creation, ruleset PUT) correctly marked orchestrator-confirmation-required. There is no criterion failing due to a defect in the change set; all open items are sequencing-bound on a live run that cannot exist pre-push. Blocking findings carried into remediation: 1 (green-run rule).

Go/no-go: **not yet ready for merge** (green run and ruleset migration outstanding); **ready to proceed to the live-PR phases** with no code changes required first.

## Acceptance Criteria Check-off

Checked off in this review (evaluated PASS with verified evidence, per `acceptance-criteria-tracking`):

- `spec.md`: S1, S2, S3, S4, S5, S7 (6 items changed `- [ ]` → `- [x]`)
- `user-story.md`: U1, U2, U3, U4, U6 (5 items changed `- [ ]` → `- [x]`)

Left unchecked with documented gaps: spec S6, S8, S9, S10; user-story U5, U7, U8. `issue.md` mirror checkboxes were left untouched (not an authoritative AC source in `full-feature` mode; plan tasks P5-T6..P5-T13 and P7-T2..T3 handle the mirrors).

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-14-ci-parallel-job-split-553/spec.md`, `docs/features/active/2026-08-14-ci-parallel-job-split-553/user-story.md`
- Total AC items: 18 (10 spec + 8 user-story)
- Checked off (delivered): 11
- Remaining (unchecked): 7
- Items remaining: spec S6 (atomic ruleset PUT with recorded evidence), spec S8 (green run against branch head), spec S9 (no gate made non-required — completes with S6), spec S10 (post-split measurement vs 444s baseline), user-story U5 (ruleset update), user-story U7 (green run), user-story U8 (gates still enforced — completes with U5)
