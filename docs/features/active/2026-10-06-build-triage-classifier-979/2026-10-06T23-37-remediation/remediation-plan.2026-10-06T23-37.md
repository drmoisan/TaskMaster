# Cycle 3 Remediation Plan: Build Triage Classifier (#979)

Canonical issue number: 979

## Objective

Remove two non-coverage blockers from the issue #979 branch without changing the
delivered C# feature or inherited harness work. Preserve the reviewed head and
all uncommitted audit/remediation artifacts under verified Git refs, replay only
the three issue commits onto `origin/main` at
`5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`, prove patch identity, and remove
only the 19 reported trailing spaces from four feature-owned Markdown files.

## Authoritative inputs and invariants

- Feature folder: `docs/features/active/2026-10-06-build-triage-classifier-979`.
- Requirements: `issue.md`, `spec.md`, and `user-story.md` in the feature folder.
  `issue.md` declares `Work Mode: full-feature` and supplies the five
  issue-level cross-checks required by the remediation handoff. Under
  `acceptance-criteria-tracking`, `spec.md` and `user-story.md` are the
  authoritative full-feature acceptance-criteria sources, with six checked
  criteria in each and 12 total.
- Remediation source:
  `2026-10-06T23-37-remediation/remediation-inputs.2026-10-06T23-37.md`.
- Reviewed feature head:
  `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`.
- Inherited predecessor that must remain preserved and excluded from the feature
  PR: `35e7482798dd0b7003afb8f7a75263c807f8da37`.
- Clean target base:
  `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`.
- Issue commits, in required order:
  `ca8b98d6a69cfbb38439571c2105cda3994ea8f0`,
  `3a355e14a57109f5470fcf3b7d747351bade5804`, and
  `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`.
- Backup branch name:
  `backup/issue-979-pre-isolation-f09f2ae2`.
- Uncommitted-artifact snapshot ref:
  `refs/backup/issue-979/uncommitted-artifacts-f09f2ae2`.
- All command evidence must be written below this feature folder under
  `evidence/remediation-baseline/`, `evidence/regression-testing/`,
  `evidence/qa-gates/`, or `evidence/other/`. Each command artifact must record
  `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The executor
  must obtain each filename timestamp from the host with
  `Get-Date -Format yyyy-MM-ddTHH-mm` when the command runs.
- Do not edit, revert, stage, or commit any `.agents/**` or `.codex/**` path.
- Do not change C# production files, tests, project files, acceptance criteria,
  or prior review findings. Do not force-push or otherwise publish rewritten
  history during remediation execution.
- If the replay conflicts, abort it and verify recovery to the reviewed head.
  Do not resolve a conflict by changing C# or inherited harness content.
- If range-diff does not show three exact patch matches, stop with remediation
  required. Do not use a C# test run as a substitute for patch identity.
- Prior final C# evidence remains authoritative only when replay is conflict-free,
  range-diff reports three exact matches, and the working remediation diff has
  no `.cs` or `.csproj` paths. Under those conditions this documentation/history
  remediation does not rerun the C# toolchain. The user's one-time issue #979
  exception covers all coverage requirements only; no other gate is waived.

## Execution and recovery rules

- Before the first mutation, stop overlapping mutation in the shared worktree.
- Every precondition failure is fail-closed. Record the failed command and state
  in the task's canonical evidence artifact, preserve both backup refs, and stop.
- Create refs only when absent. If either planned ref already exists, continue
  only when it resolves to the exact expected object; otherwise stop without
  moving or deleting it.
- Keep the branch backup and uncommitted-artifact snapshot ref through final PR
  completion. Applying the artifact snapshot must not delete its ref.
- The only permitted tracked-content edit in Phases 0 through 4 is removal of
  trailing spaces from the four paths named in P3-T1.

### Phase 0 — Policy, Requirements, and Remediation Baseline

- [x] [P0-T1] Read `AGENTS.md` in the required order (standing instructions,
  cross-language code-change policy, cross-language unit-test policy, then C#
  policy), `.agents/skills/csharp/SKILL.md`,
  `.agents/skills/policy-compliance-order/SKILL.md`,
  `.agents/skills/atomic-plan-contract/SKILL.md`,
  `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`,
  `.agents/skills/acceptance-criteria-tracking/SKILL.md`,
  `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`,
  `spec.md`, `user-story.md`,
  `2026-10-06T23-37-remediation/remediation-inputs.2026-10-06T23-37.md`,
  all three files in `2026-10-06T23-34-audit/`, and the prior plan
  `2026-10-06T23-01-remediation/remediation-plan.2026-10-06T23-01.md`. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t1-instructions-read.<timestamp>.md`.
  Acceptance: the artifact records the policy order and complete file list, and
  identifies PA-979-3/CR-979-3, PA-979-4/CR-979-4, the coverage-only exception,
  the no-policy-edit constraint, and the no-force-push constraint.

- [x] [P0-T2] Capture the pre-mutation repository state with `git branch
  --show-current`, `git rev-parse HEAD`, `git rev-parse origin/main`, `git
  merge-base origin/main HEAD`, `git status --short --untracked-files=all`, and
  `git rev-list --reverse
  35e7482798dd0b7003afb8f7a75263c807f8da37..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`.
  Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t2-history-and-worktree-baseline.<timestamp>.md`.
  Acceptance: the branch is `feature/build-triage-classifier-979`, HEAD is the
  reviewed SHA, `origin/main` is the clean target SHA, the commit list is exactly
  the three issue commits in required order, and every uncommitted path is
  inventoried before preservation.

- [x] [P0-T3] [expect-fail] Run `git diff --check
  c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`
  and write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t3-whitespace-baseline.<timestamp>.md`
  with `ExpectedExitCode: 2`. Acceptance: the artifact records exit code 2 and
  exactly 19 diagnostics distributed 6/5/5/3 across the four paths named in
  P3-T1; any different count or path set stops execution before mutation.

- [x] [P0-T4] Run `git diff --name-only
  c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7
  -- .agents .codex` and `git diff --name-only
  35e7482798dd0b7003afb8f7a75263c807f8da37..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7
  -- .agents .codex`. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t4-inherited-scope-baseline.<timestamp>.md`.
  Acceptance: the first command records the inherited harness/policy paths and
  the second returns no paths, proving the three issue commits did not modify
  `.agents/**` or `.codex/**`.

- [x] [P0-T5] Reconcile all 12 authoritative checked criteria in
  `docs/features/active/2026-10-06-build-triage-classifier-979/spec.md` and
  `docs/features/active/2026-10-06-build-triage-classifier-979/user-story.md`,
  and separately reconcile the five checked issue-level cross-checks in
  `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`, with
  `evidence/qa-gates/p3-t2-format-check-retry.2026-10-06T23-28.md` through
  `evidence/qa-gates/p3-t7-file-size-and-diff-hygiene.2026-10-06T23-30.md` and
  `evidence/other/coverage-exception.2026-10-06T21-37.md`. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t5-prior-qa-and-ac-reuse.<timestamp>.md`.
  Acceptance: the artifact records 5,017 passing UtilitiesCS tests, 478 passing
  TaskMaster standard-QC tests, clean formatter/analyzer/nullable results,
  per-source counts of `spec.md` 6/6, `user-story.md` 6/6, and `issue.md`
  cross-checks 5/5, plus the issue-only coverage exception; it also states that
  these results may be reused only if P2-T3 and P4-T2 prove unchanged patches
  and no C# working-tree edits.

### Phase 1 — Preserve Reviewed State and Uncommitted Artifacts

- [x] [P1-T1] Re-run the exact branch, HEAD, target-base, tracked-diff, staged-diff,
  and untracked-path checks from P0-T2 immediately before mutation, and write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p1-t1-pre-mutation-guard.<timestamp>.md`.
  Acceptance: branch, HEAD, and `origin/main` still match the three fixed SHAs;
  `git diff --quiet` and `git diff --cached --quiet` both succeed; and all
  untracked paths are contained within
  `docs/features/active/2026-10-06-build-triage-classifier-979/`.

- [x] [P1-T2] Create local branch
  `backup/issue-979-pre-isolation-f09f2ae2` at
  `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`, without moving an existing ref,
  then verify it with `git show-ref --verify
  refs/heads/backup/issue-979-pre-isolation-f09f2ae2` and `git rev-parse`.
  Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p1-t2-reviewed-head-backup.<timestamp>.md`.
  Acceptance: the backup branch resolves exactly to the reviewed SHA and remains
  present; a pre-existing different object is a blocking conflict.

- [x] [P1-T3] Snapshot all uncommitted paths inventoried by P1-T1 with a named
  `git stash push --include-untracked --message
  "issue-979-cycle3-artifacts-f09f2ae2" --
  docs/features/active/2026-10-06-build-triage-classifier-979`, record the stash
  commit and its untracked parent, create
  `refs/backup/issue-979/uncommitted-artifacts-f09f2ae2` at that stash commit,
  then apply the exact stash object without dropping either ref. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p1-t3-uncommitted-artifact-snapshot.<timestamp>.md`.
  Acceptance: `git ls-tree -r --name-only <stash-object>^3` contains every
  inventoried untracked audit, remediation, plan, and Cycle 3 evidence path;
  the dedicated backup ref resolves to the recorded stash object; the same
  untracked paths are restored in the worktree; and `refs/stash` plus the
  dedicated ref remain available.

- [x] [P1-T4] Verify both preservation refs and hash the restored files below
  `2026-10-06T23-34-audit/` and `2026-10-06T23-37-remediation/`; write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p1-t4-preservation-verification.<timestamp>.md`.
  Acceptance: the branch backup still resolves to the reviewed head, the
  artifact ref still resolves to the recorded stash object, and the restored
  file inventory and SHA-256 values match the pre-snapshot inventory.

### Phase 2 — Replay Only the Issue #979 Commits

- [x] [P2-T1] From `feature/build-triage-classifier-979` at the reviewed head,
  run `git rebase --onto
  5ddf7f03d6b92b2981cd0d5d74f10a0733e80964
  35e7482798dd0b7003afb8f7a75263c807f8da37`. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t1-history-replay.<timestamp>.md`.
  Acceptance: the rebase completes without conflict and without editing any
  file manually. If any conflict occurs, run `git rebase --abort`, verify the
  feature branch again resolves to the reviewed head and both preservation refs
  remain valid, record the blocked outcome, and stop.

- [x] [P2-T2] Record `git merge-base origin/main HEAD`, `git rev-list --reverse
  5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..HEAD`, and `git log --format="%H
  %s" --reverse
  5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..HEAD` in
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t2-replayed-commit-inventory.<timestamp>.md`.
  Acceptance: the merge base is the fixed target base, exactly three new SHAs
  appear in the original order, and their subjects correspond to the feature,
  disabled-engine fix, and focused-test extraction commits. Record the third
  new SHA as the replayed-feature head for all later range-diff checks.

- [x] [P2-T3] Run `git range-diff --no-color
  35e7482798dd0b7003afb8f7a75263c807f8da37..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7
  5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..<replayed-feature-head>` and write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t3-range-diff-identity.<timestamp>.md`.
  Acceptance: the output contains exactly three `=` mappings in the required
  order and contains no `!`, `<`, or `>` mapping. Any other result is blocking;
  retain the backup refs and stop without correcting or committing C# content.

- [x] [P2-T4] Run `git diff --name-only origin/main..HEAD -- .agents .codex`
  and `git status --short --untracked-files=all -- .agents .codex`, then write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t4-harness-scope-elimination.<timestamp>.md`.
  Acceptance: both commands return no paths, the current branch is still
  `feature/build-triage-classifier-979`, and the reviewed-head backup continues
  to preserve inherited commit `35e7482798dd0b7003afb8f7a75263c807f8da37`.

### Phase 3 — Correct Feature-Owned Markdown Whitespace

- [x] [P3-T1] Use `apply_patch` to remove only the six, five, five, and three
  reported trailing-space sequences from, respectively,
  `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-00-audit/code-review.2026-10-06T23-00.md`,
  `feature-audit.2026-10-06T23-00.md`,
  `policy-audit.2026-10-06T23-00.md`, and
  `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-01-remediation/remediation-inputs.2026-10-06T23-01.md`.
  Acceptance: exactly those four tracked files change, all wording and line
  ordering remain unchanged, and no new file is edited by this task.

- [x] [P3-T2] Compare the P3-T1 working tree with the recorded replayed-feature
  head using `git diff --exit-code --ignore-space-at-eol
  <replayed-feature-head> --` followed by the four P3-T1 paths, and write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p3-t2-whitespace-only-content-proof.<timestamp>.md`.
  Acceptance: the command exits 0, proving the four changes differ only in
  end-of-line whitespace; a nonzero result stops execution before staging.

- [x] [P3-T3] Run `git diff --check origin/main` and an explicit trailing-space
  scan over the four P3-T1 files; write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p3-t3-working-tree-diff-hygiene.<timestamp>.md`.
  Acceptance: both checks exit 0 and report zero trailing-whitespace findings.

- [x] [P3-T4] Run `git diff --name-only HEAD -- "*.cs" "*.csproj"`, `git status
  --short --untracked-files=all -- "*.cs" "*.csproj"`, and `git diff
  --exit-code HEAD --
  docs/features/active/2026-10-06-build-triage-classifier-979/issue.md
  docs/features/active/2026-10-06-build-triage-classifier-979/spec.md
  docs/features/active/2026-10-06-build-triage-classifier-979/user-story.md`.
  Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p3-t4-no-code-or-requirements-edit.<timestamp>.md`.
  Acceptance: all three commands return no changes, so prior C# QA remains reusable
  and all acceptance-criteria text and checkboxes remain unchanged.

### Phase 4 — Final Focused QA and Acceptance Reconciliation

- [x] [P4-T1] Re-run the preservation-ref checks from P1-T4 after all working
  edits and write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p4-t1-preservation-refs.<timestamp>.md`.
  Acceptance: the reviewed-head backup and uncommitted-artifact snapshot ref
  still resolve to their recorded objects, and neither ref has been deleted or
  moved.

- [x] [P4-T2] Re-run the exact P2-T2 commit inventory, P2-T3 range-diff, P2-T4
  `.agents/.codex` scope check, and P3-T4 C# working-diff check; write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p4-t2-final-patch-and-scope-verification.<timestamp>.md`.
  Acceptance: the merge base remains the target SHA, the three replayed patches
  remain exact matches, `.agents/**` and `.codex/**` remain absent from the
  feature diff, and no working `.cs` or `.csproj` path is modified. Any failure
  is remediation-required and prevents reuse of prior C# evidence.

- [x] [P4-T3] Run `git diff --check origin/main` and record the full changed-path
  inventory relative to `origin/main` plus the untracked Cycle 3 artifacts in
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p4-t3-final-diff-hygiene-and-scope.<timestamp>.md`.
  Acceptance: diff-check exits 0; the four historical Markdown files contain no
  trailing whitespace; and every working change outside the three replayed
  patches is a feature-owned audit, remediation, plan, or evidence artifact.

- [x] [P4-T4] Map all 12 authoritative full-feature acceptance criteria in
  `docs/features/active/2026-10-06-build-triage-classifier-979/spec.md` and
  `docs/features/active/2026-10-06-build-triage-classifier-979/user-story.md`,
  and separately map the five checked remediation-input cross-checks in
  `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`, to the
  unchanged implementation proof in P2-T3, prior final C# evidence in P0-T5,
  and the final scope proof in P4-T2. Write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/other/p4-t4-cycle3-acceptance-summary.<timestamp>.md`.
  Acceptance: the artifact reports `spec.md` 6/6, `user-story.md` 6/6, 12
  authoritative criteria total, 12 checked, and zero remaining; reports the
  five `issue.md` items separately as 5/5 remediation-input cross-checks;
  preserves every checkbox text and state; records that no checkbox edit was
  needed; and distinguishes the issue-only coverage exception from all passing
  non-coverage gates.

### Phase 5 — Commit and Fresh Re-Review Gates

- [x] [P5-T1] Stop overlapping mutation, verify `git status --short
  --untracked-files=all` contains only the permitted feature-owned paths from
  P3-T1 and Cycle 3 audit/remediation/evidence paths, then run the remediation
  workflow's required `git add -A`. Acceptance: staging is nonempty and `git
  diff --cached --name-only` contains no `.agents/**`, `.codex/**`, `*.cs`, or
  `*.csproj` path; any unrelated path blocks the commit without unstaging or
  overwriting another contributor's work.

- [x] [P5-T2] Run `git diff --cached --check` and compare the staged four-file
  whitespace patch with P3-T2; write
  `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p5-t2-staged-diff-hygiene.<timestamp>.md`,
  add that artifact to the same staged set, and rerun `git diff --cached
  --check`. Acceptance: both staged checks exit 0, the second staged inventory
  remains within the P5-T1 allowlist, and all preservation refs still resolve.

- [x] [P5-T3] Invoke the `drm-copilot` MCP `collect_commit_context` tool after
  P5-T2 and persist its returned on-disk path in
  `artifacts/orchestration/orchestrator-state.json` as `commit-context-path`.
  Acceptance: the MCP response is successful, its context describes the exact
  staged remediation set, and the returned file exists; do not reconstruct
  commit context locally if the MCP operation fails.

- [x] [P5-T4] Delegate the staged context from P5-T3 to the routed C4
  `commit-steward` and require its exact fenced-text result contract. Acceptance:
  the steward returns one conventional commit message derived only from the
  collected staged context; a missing routing receipt, context path, or exact
  result blocks commit creation.

- [ ] [P5-T5] Commit the staged remediation once with the exact P5-T4 message,
  then record the new commit SHA, its replayed-feature parent, complete path
  list, `git diff-tree --check` result, and both preservation-ref objects in
  `artifacts/orchestration/orchestrator-state.json`.
  Acceptance: the commit contains only permitted feature-owned Markdown and
  evidence paths, its parent is the third replayed issue commit, diff-tree
  hygiene passes, both backup refs remain valid, the worktree has no remaining
  feature-owned change, and no push or force-push occurs in this task.

- [ ] [P5-T6] Invoke the `drm-copilot` MCP `collect_pr_context` tool with base
  `main` and target `feature/build-triage-classifier-979`; require fresh
  `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`
  for the P5-T5 head. Acceptance: both context files identify the clean
  `origin/main` merge base and current committed head, contain no inherited
  `.agents/**` or `.codex/**` changes, and the MCP response satisfies the
  repository automation contract.

- [ ] [P5-T7] Delegate a fresh full-feature re-review to the routed C4
  `feature-reviewer`, supplying P5-T6 context, this plan, all Cycle 3 evidence,
  the three `2026-10-06T23-34-audit/` inputs, and the coverage authorization at
  `evidence/other/coverage-exception.2026-10-06T21-37.md`. Acceptance: the
  reviewer returns the exact result fields, writes validator-clean policy,
  code, and feature audit artifacts below
  `docs/features/active/2026-10-06-build-triage-classifier-979/`, confirms
  PA-979-3/CR-979-3 and PA-979-4/CR-979-4 are resolved, reports all five
  supplied issue-level cross-checks remain supported, uses `spec.md` and
  `user-story.md` as the authoritative full-feature AC sources, reports all 12
  authoritative criteria passing, and returns `REVIEW_STATUS: PASS`. If it
  returns another exact status, follow the orchestrator's remediation/halt
  contract; do not claim PR readiness.

## Acceptance criteria for this remediation

- [ ] The reviewed state remains recoverable from
  `backup/issue-979-pre-isolation-f09f2ae2` at
  `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`.
- [ ] Every initially uncommitted audit, remediation, plan, and Cycle 3 evidence
  file is recoverable from
  `refs/backup/issue-979/uncommitted-artifacts-f09f2ae2` and is restored in the
  worktree before documentation correction.
- [ ] The feature branch merge base with `origin/main` is
  `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`.
- [ ] Exactly the three issue #979 patches are replayed in their original order,
  and range-diff reports three exact `=` matches.
- [ ] `git diff --name-only origin/main..HEAD -- .agents .codex` returns no
  path; the inherited `35e748279` work remains preserved under the backup ref.
- [ ] The four historical Markdown files have only the 19 reported trailing
  spaces removed, with all wording and findings retained.
- [ ] Working, staged, and committed diff-hygiene checks pass with zero
  trailing-whitespace diagnostics.
- [ ] No C# production, test, or project content is edited during remediation;
  prior final C# evidence remains applicable through exact patch identity.
- [ ] All 12 authoritative `spec.md` and `user-story.md` acceptance criteria and
  all five issue-level remediation cross-checks remain checked and supported;
  coverage remains waived only for issue #979 under the user's one-time
  exception.
- [ ] Canonical commit context and refreshed PR context are collected through
  `drm-copilot`, the remediation is committed without any push, and the fresh
  routed feature review returns `REVIEW_STATUS: PASS` before PR readiness is
  reported.
