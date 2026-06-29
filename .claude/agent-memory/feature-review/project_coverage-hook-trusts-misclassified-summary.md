---
name: coverage-hook-trusts-misclassified-summary
description: validate-feature-review-coverage hook detects changed languages from the pr_context.summary "Changed files overview" bullets, so a C#-as-docs misclassification makes it silently skip C# coverage enforcement
metadata:
  type: project
---

The `validate-feature-review-coverage.ps1` SubagentStop hook derives the set of changed languages by parsing `artifacts/pr_context.summary.txt` for bullet lines matching `^\s*-\s+(\S+)\s+\(\+\d+/-\d+\)\s*$` (the "Changed files overview" section), then mapping file extensions. It does NOT read `git diff`.

**Why:** Observed on issue #222. The summary listed only `.md` files under "Docs/templates/agents/tooling" and reported "Core logic changes: 0 files" — even though the branch modified 5 C# production files and 2 C# test files. Because no `.cs` bullet appeared in the overview, `Get-ChangedLanguageSet` returned empty and the hook returned Ok without enforcing any C# coverage verdict. This is the same summary misclassification noted in [[pr-context-summary-misclassifies-cs]].

**How to apply:** Never rely on the coverage hook to catch a missing C# coverage verdict. Enumerate changed languages yourself from `git diff --stat <merge-base>..<head>` per the scope invariant, and produce explicit PASS/FAIL coverage rows for every language with changed files regardless of what the hook would gate. The hook passing is not evidence that C# coverage was verified. Relatedly, the canonical `artifacts/csharp/coverage.xml` is frequently absent for these features ([[feedback_csharp_coverage_artifact_gate]]); its absence is a fail-closed FAIL on the repo-wide floor even when new-code coverage is strong.
