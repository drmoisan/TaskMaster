---
name: poshqc-test-drops-coverage-xml-at-repo-root
description: run_poshqc_test and direct Pester coverage runs write an untracked coverage.xml to the repo root that is neither gitignored nor in .csharpierignore, so it inflates the CSharpier file count and can leak into a diff
metadata:
  type: project
---

`mcp__drm-copilot__run_poshqc_test`, and a direct `Invoke-Pester` run with
`$c.CodeCoverage.Enabled = $true`, both write a `coverage.xml` to the **repository root**. As of
2026-08-11 that path is in neither `.gitignore` nor `.csharpierignore`.

**Why it matters:** CSharpier 1.2.6 processes `*.xml`, so the stray file raises the CSharpier
`Checked N files` count by one between two otherwise-identical runs, which looks like formatter
drift when you are comparing a baseline count against a final-QC count. It is also untracked, so a
`git add -A` at commit time will sweep it into the diff.

**How to apply:** in any route that runs a PowerShell test gate before a C# format gate, delete the
root `coverage.xml` between the two and record the deletion in the format-step evidence artifact.
Re-check for it immediately before `git add`. Observed on issue #512 / PR #540; the executor removed
it before each CSharpier gate and attributed the count difference in the evidence.

Distinct from [[jacoco-not-cobertura-for-evidence]] (which is about what coverage format to commit)
and from [[feature-review-coverage-85-floor-trap]] (about `artifacts/csharp/coverage.xml`).
