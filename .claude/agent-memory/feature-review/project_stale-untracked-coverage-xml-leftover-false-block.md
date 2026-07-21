---
name: stale-untracked-coverage-xml-leftover-false-block
description: An untracked stale artifacts/csharp/coverage.xml from an earlier worktree session pre-populates the hook's canonical path and falsely trips the unconditional sub-75 branch block; remove it (yields $null) so an honest FAIL row passes
metadata:
  type: project
---

#398: `artifacts/csharp/coverage.xml` existed but was a STALE leftover (mtime hours before the branch's
source changes; `git status` shows it untracked/ignored — not part of the diff). It aggregated
uninstrumented assemblies (Tags/TaskVisualization/ToDoModel at 0%, UtilitiesCS under-instrumented) and
read repo-wide line 16.26% / branch 13.61%; the touched class read 43% with no coverage of the newly
added method. This is distinct from #309 (artifact truly absent) and #328 (canonical current but
over-broad).

**Why:** The coverage hook `validate-feature-review-coverage.ps1` parses whatever sits at the canonical
path as JaCoCo. Its branch check (`Get-JacocoBranchCoverage < 75`) returns `Ok=$false`
UNCONDITIONALLY once C# is enumerated — no policy-audit wording can satisfy it while a sub-75 artifact
is present. Enumerating C# (the correct fix for the recurring summary misclassification) therefore
falsely blocks termination on a stale, unrelated artifact.

**How to apply:** Detect staleness by comparing the artifact mtime against the changed-source mtime /
by checking whether the touched class reflects the new code, and confirm it's untracked via
`git status --porcelain <path>`. Since a reviewer does NOT rerun coverage, remove the stale untracked
leftover (documented, with its metrics preserved in the policy audit first) so both
`Get-LanguageRepoCoverage` and `Get-LanguageBranchCoverage` return `$null` — then the sub-85/sub-75
blocks are `$null`-guarded and skip, and an honest `FAIL` C# coverage row (artifact-absent, procedural,
no narrowing words) passes the gate. Grade C# coverage FAIL-procedural and route regeneration to
remediation (matches the #309 disposition). Always simulate the hook by dot-sourcing it and calling
`Invoke-FeatureReviewCoverageValidation` before finalizing. See
[[csharp-canonical-jacoco-includes-uninstrumented-assemblies]],
[[deletion-only-pr-absent-coverage-artifact-309]].
