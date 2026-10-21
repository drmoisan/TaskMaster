# Remediation Inputs: Build Triage Classifier (#979)

Timestamp: 2026-10-06T23-37
Review-Verdict: REMEDIATION_REQUIRED
Base merge commit: `c76e830c18976221b5730f84b8d88aebbfc4f04b`
Reviewed head: `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`
Current clean target base: `origin/main` at `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`

## Blocking Findings

### CR-979-3 / PA-979-3: Inherited harness commit contaminates the feature history

Remediability: autonomous

Commit `35e7482798dd0b7003afb8f7a75263c807f8da37` predates the issue #979 feature request and contains others' work. It adds or changes 34 `.agents` and `.codex` files, including eight policy documents under `.agents/skills/`. The full feature-vs-base review must not ship those changes as part of issue #979, and the remediation must not directly edit or revert those policy files.

The three issue #979 commits are:

1. `ca8b98d6a69cfbb38439571c2105cda3994ea8f0` — product feature and initial tests.
2. `3a355e14a57109f5470fcf3b7d747351bade5804` — disabled-engine remediation.
3. `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7` — focused test extraction.

Safely isolate those commits onto current `origin/main`. Before any history operation, create and verify an explicit backup ref at the reviewed head. Preserve the current uncommitted audit/remediation artifacts through a named Git stash or an equivalently reversible Git mechanism, record its object identity, replay only the issue #979 commits, restore the artifacts, and retain the backup ref through final PR completion. Use range-diff or equivalent patch-identity evidence to prove the issue commits were preserved.

### CR-979-4 / PA-979-4: Committed review artifacts fail diff hygiene

Remediability: autonomous

`git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7` exits 2 with 19 trailing-whitespace diagnostics:

| File | Diagnostics |
|---|---:|
| `2026-10-06T23-00-audit/code-review.2026-10-06T23-00.md` | 6 |
| `2026-10-06T23-00-audit/feature-audit.2026-10-06T23-00.md` | 5 |
| `2026-10-06T23-00-audit/policy-audit.2026-10-06T23-00.md` | 5 |
| `2026-10-06T23-01-remediation/remediation-inputs.2026-10-06T23-01.md` | 3 |

Remove only the reported trailing spaces. Preserve all wording, findings, evidence, and Markdown structure.

## Required Changes

1. Verify the branch is `feature/build-triage-classifier-979` at reviewed head `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7` before history isolation.
2. Create an explicit backup branch or ref at that exact head and verify it resolves to the reviewed SHA. Do not delete the backup during this remediation.
3. Preserve all uncommitted audit and remediation artifacts using a named, reversible Git mechanism whose object/ref identity is recorded.
4. Rebase or replay only commits `ca8b98d6a`, `3a355e14a`, and `f09f2ae2d` onto clean `origin/main` at `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`.
5. Restore the uncommitted review artifacts and prove that the feature branch has no `.agents/**` or `.codex/**` diff relative to the new base.
6. Remove the 19 reported trailing spaces from the four Markdown artifacts.
7. Record the old head, backup ref, stash identity if used, new head, new merge base, range-diff result, feature commit list, and final diff-check result in canonical evidence.

## Acceptance and Verification

- The backup ref resolves to `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7` and remains present.
- The feature branch merge base with `origin/main` is `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`.
- Exactly the three issue #979 patches are replayed in their original order, with any new SHAs recorded.
- `git diff --name-only origin/main..HEAD -- .agents .codex` returns no paths.
- `git range-diff` or equivalent evidence shows the three issue patches retain their intended content.
- The four historical Markdown artifacts contain no reported trailing whitespace.
- `git diff --check origin/main..HEAD` and the corresponding merge-base-to-working-tree check pass after the documentation correction.
- All uncommitted audit/remediation artifacts are restored after history isolation.
- No C# production or test content is edited by this remediation. Do not rerun the C# baseline or full C# suites unless a rebase conflict changes C# content or patch-identity verification fails. Existing final C# evidence remains authoritative for an unchanged patch set.
- Coverage is not a remediation target. The user's one-time exception covers all issue #979 coverage requirements and does not alter standing policy.

## Do Not Do

- Do not directly edit, revert, or weaken any `.agents/skills/**` policy document.
- Do not delete, overwrite, or lose the inherited `35e748279` work; preserve the original reviewed state under the verified backup ref.
- Do not force-push or publish the rewritten branch during remediation execution.
- Do not change C# production behavior, tests, project files, or acceptance criteria.
- Do not rerun expensive C# baseline or full-suite gates solely for the Markdown whitespace correction and verified patch-preserving history isolation.
- Do not broaden or persist the coverage exception in repository policy.
- Do not remove prior audit findings or rewrite their meaning while correcting whitespace.

## Context Package

- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Policy audit: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-34-audit/policy-audit.2026-10-06T23-34.md`
- Code review: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-34-audit/code-review.2026-10-06T23-34.md`
- Feature audit: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-34-audit/feature-audit.2026-10-06T23-34.md`
- Authoritative acceptance criteria: `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`
- Original feature plan: `docs/features/active/2026-10-06-build-triage-classifier-979/plan.2026-10-06T19-29.md`
- Prior remediation plan: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-01-remediation/remediation-plan.2026-10-06T23-01.md`
- Final C# QA evidence: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t1-format-retry.2026-10-06T23-28.md` through `p3-t7-file-size-and-diff-hygiene.2026-10-06T23-30.md`
- Coverage authorization record: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/other/coverage-exception.2026-10-06T21-37.md`, supplemented by the active-session instruction covering all issue #979 coverage requirements.

## Planner Target

Write the executor-ready atomic plan to:

`docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-37-remediation/remediation-plan.2026-10-06T23-37.md`
