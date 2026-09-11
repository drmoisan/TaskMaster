---
name: 752-review-residuals
description: "#752 PowerShell one-line discovery-predicate fix: 6/6 AC PASS, 1 blocking (host-path leak in a pre-plan research commit inside the branch diff); scope-lock reasoning does not discharge the branch-level hygiene obligation"
metadata:
  type: project
---

Issue #752 (`bug/coverage-assembly-discovery-excludes-own-worktree-root-752`, PowerShell, `full-bug`).
One-line production fix at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` plus one new 99-line
Pester file. Cycle-1 verdict: 6/6 AC PASS, **1 blocking**, 2 non-blocking FAIL, 4 observations.

**The blocking finding is the transferable lesson.** The executor's `followups` artifact disclosed an
absolute host path at `research/research-findings.2026-09-03T00-00.md:5` and declined to repair it
because "the file is not in this plan's Write Set." That reasoning is correct *for the executor's
scope lock* and wrong *for the reviewer*: the audit scope is the full merge-base-to-HEAD diff, the
file was added by the branch's own pre-plan preparation commit, and merging publishes the account
name to `main`. `_shared_no_absolute_host_paths.md` lines 85-87 even scope its own verification sweep
to `git diff --name-only <base>..HEAD`, which is exactly the set the file is in.

**How to apply:** when an executor discloses a leak and declines repair on Write-Set grounds, do not
inherit the scope lock. Re-run the branch-scoped sweep yourself
(`git diff <merge-base> HEAD | grep -i -E "^\+.*(<account>|C:\\\\Users|/Users/)"`) and rank any hit
blocking. Remedy is a one-line `<repo-root>` substitution **plus squash-merge**, because the
pre-sanitisation blob stays reachable through the original commit.

Other residuals worth carrying:

- The plan's `[P3-T6]` mandated stop-and-report for the combination that actually occurred; the
  executor recorded the divergence, left the checkbox unchecked, and continued. Non-blocking — the
  plan's branch enumeration was incomplete, not the execution. See
  [[pester-line-coverage-node-appears-only-with-an-analyzable-command]] for the mechanism.
- Nine evidence artifacts carry `Timestamp:` values postdating the commit containing them; the JaCoCo
  `<report name=...>` writer clock falsified two more. See
  [[evidence-timestamps-are-synthetic-cross-check-commit-dates]] item 6.
- PowerShell line coverage over `scripts/vscode` is 78.33% (baseline 78.30%) — FAIL against the 85%
  floor, non-blocking, pre-existing, positive delta.
- `artifacts/pr_context.summary.txt` was stale in BOTH the review worktree (#735) and the session cwd
  (#707), and neither lists a source file, so an automated changed-language reader sees zero languages.
  Derive from git; see [[pr-context-artifacts-are-tracked-not-gitignored]].
- Confirms [[poshqc-bundled-coverage-artifact-reads-zero]]: `artifacts/pester/powershell-coverage.xml`
  in the review worktree read `missed="6403" covered="0"`.
- Anchor detail worth reusing: switching a Windows path regex from an absolute path to a
  `GetRelativePath` result **requires** re-anchoring, because the relative path has no leading
  separator. `'\\\.claude\\'` must become `'(^|\\)\.claude\\'` or the previously-excluded nested case
  silently starts passing. The planner caught this pre-execution; a literal substitution would have
  broken the preserved regression test while appearing to implement the AC.
