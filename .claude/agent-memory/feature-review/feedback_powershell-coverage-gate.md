---
name: powershell-coverage-mandatory-when-ps1-in-diff
description: The validate-feature-review-coverage hook requires an explicit PASS/FAIL PowerShell coverage verdict whenever .ps1 files appear in the PR summary changed-files list.
metadata:
  type: feedback
---

The SubagentStop hook `.claude/hooks/validate-feature-review-coverage.ps1` blocks feature-review termination unless the policy-audit carries an explicit PASS or FAIL coverage-scoped verdict for every language with changed files in `artifacts/pr_context.summary.txt`. It maps `.ps1`/`.psm1` to PowerShell, reads `artifacts/pester/powershell-coverage.xml` (Jacoco) for repo-wide coverage, treats `N/A`/`UNVERIFIED`/"informational only"/"out of scope" on a coverage row as failures, and requires a FAIL verdict when repo-wide coverage is below 80%.

**Why:** The repository scope invariant forbids narrowing audit scope; the hook enforces it mechanically so a reviewer cannot mark a changed language's coverage as N/A.

**How to apply:** When `.ps1` files are in the branch diff, the policy-audit MUST contain a row that mentions PowerShell/pester, mentions coverage, and carries PASS or FAIL — never N/A. If `artifacts/pester/powershell-coverage.xml` shows <80% (or is stale/missing covered lines), the row must say FAIL. See [[gitignore-tracking-expands-diff-scope]].
