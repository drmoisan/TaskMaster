# Remediation Inputs — Issue #752

- Timestamp: 2026-09-03T12-23
- Source artifacts:
  - `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md`
  - `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md`
  - `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md`
- Blocking findings requiring remediation: **1**
- Non-blocking findings recorded for disposition, not for this loop: 6

## Remediation-required finding

### R-1 (from POL-2) — Absolute host path committed in a file inside the branch diff

**Severity:** Blocking.

**Location:** `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`, line 5.

**What is wrong:** The line is a `- Worktree:` bullet whose value is an absolute filesystem path
rooted at the operator's user-profile directory. It carries the account name and the full worktree
directory layout.

**Rule violated:** `.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 8-13:

> No file committed to this repository may contain an absolute host path or a host identifier.
> Absolute paths leak the operator's account name, machine name, and directory layout, and they are
> not reproducible on any other machine. Prohibited: `C:\Users\<account>\...` ...

The same file's verification rule at lines 85-87 scopes the sweep to the files the branch changed,
and this file is one of them. `plan.2026-09-03T07-23.md` line 22 restates the rule as
non-negotiable for this item.

**How it was found:**
`git -C <repo-root> diff 87233f867ad60c0a5c0d19b09cc121ae536d7ba1 HEAD` piped through a
case-insensitive search of added lines for the account name, `C:\Users`, `/Users/`, and the
worktree-parent directory name. Exactly one added line matches, and it is the bullet above. No other
added line in the 39-file diff carries an identifier of this class; the three committed JaCoCo XMLs
were checked separately and use leaf file names and a `vscode` package name only.

**Why it was not caught during execution:** The `[P4-T11]` residual sweep is scoped to
`evidence/` only, by design, and this file is under `research/`. The executor detected the leak
anyway and disclosed it by class in `evidence/other/followups.2026-09-03T07-23.md` entry 3, then
declined to repair it because the file is not in the plan's Write Set. That reasoning is correct
under the executor's scope lock. It does not discharge the branch-level obligation, because the
audit scope is the full branch diff and merging publishes the identifier to `origin/main`.

**Required remediation:**

1. Replace the absolute path on line 5 with the `<repo-root>` placeholder, or remove the bullet
   entirely — the branch name it also carries is recoverable from git and the path adds no
   reviewable information. Keep the rest of the line unchanged.
2. Do not write a sanitisation record that quotes the removed value; describe the substituted token
   by class only (`.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 88-92).
3. Re-run the branch-scoped verification after the edit:
   `git -C <repo-root> diff <merge-base> HEAD` filtered case-insensitively for the account name,
   `C:\Users`, `/Users/`, and the worktree-parent directory name. Expect zero added-line matches.
4. Merge this branch with a **squash merge**. A sanitising commit removes the value from the tip
   but leaves the pre-sanitisation blob reachable through commit `5375bcc9`; squashing is what keeps
   the identifier out of `main`'s history.

**Acceptance for this remediation:** the branch-scoped added-line sweep returns zero matches, and
the PR is configured for squash merge.

**Explicitly out of bounds for this remediation:** do not touch
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, the new test file, or any spec/plan acceptance
checkbox. All six acceptance criteria are verified PASS and must remain checked.

## Findings recorded but NOT requiring remediation in this loop

| ID | Source | Summary | Disposition |
|---|---|---|---|
| POL-3 | policy-audit | Nine committed evidence artifacts carry a `Timestamp:` that postdates the commit containing them; two of the three committed JaCoCo XMLs carry an embedded writer clock nine to ten minutes earlier than the artifact label | Non-blocking. No measured value is affected; every substantive claim was independently re-derived. Recommend a process change so evidence tasks emit the capture time from the running process. |
| POL-4 | policy-audit | `artifacts/pr_context.summary.txt` is stale in both the review worktree (#735) and the session working directory (#707); both are tracked files belonging to other items | Non-blocking deviation. Scope was derived from `git merge-base` plus `git diff --numstat`. Regenerating over another item's tracked file was declined deliberately. |
| POL-5 | policy-audit | The policy-audit template MCP asset and the artifact validator are not reachable from this agent's tool profile | Non-blocking deviation. Artifact hand-authored preserving all thirteen canonical major headings. |
| POL-6 | policy-audit | Plan task `[P3-T6]` mandates stop-and-report for the measured combination; the executor recorded the divergence and continued, leaving the checkbox unchecked | Non-blocking. The divergence is a coverage gain, is fully explained, and the explanation was independently reproduced from the committed XMLs. The plan's branch enumeration was incomplete, not the execution. |
| CR-2 | code-review | Roughly 40 duplicated lines between the new test file and `Invoke-MSTest.RunSettings.Tests.ps1` | Non-blocking. Forced by the 488/500-line cap on the original file. Optional follow-up: extract a shared setup helper under `tests/scripts/vscode/`. |
| CR-5 | code-review | No permanent test pins the `No test assemblies found ... Build first.` throw | Non-blocking, pre-existing. Optional follow-up: add an `It` mocking `Get-ChildItem` to `@()` and asserting `Should -Throw -ExpectedMessage 'No test assemblies found*'`. |

The PowerShell line-coverage row in `policy-audit.2026-09-03T12-23.md` section 5 carries a FAIL
verdict at 78.3313 percent against the 85 percent floor. Its disposition is non-blocking and it is
not a remediation trigger for this loop: the shortfall predates this item, the delta versus baseline
is positive, and the changed line moved from unmeasured to covered. Closing a repository-wide
6.7-point shortfall is a separate body of work that a single-line defect fix cannot carry.
