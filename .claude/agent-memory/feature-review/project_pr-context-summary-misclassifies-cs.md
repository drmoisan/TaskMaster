---
name: pr-context-summary-misclassifies-cs
description: The PR-context summary's automated "Changed files overview" can mislabel substantial C# changes as docs and report "Core logic changes: 0 files"; always verify against git diff.
metadata:
  type: project
---

The automated `artifacts/pr_context.summary.txt` "Changed files overview" classifier has, at least once (Issue #171, 2026-06-02), reported `Core logic changes: 0 files` and labeled all C# production/test changes as "Docs/templates/agents/tooling" while the actual branch diff contained 9 C# production files, 7 test files, and 4 `.csproj` files.

**Why:** The feature-review coverage validator (`validate-feature-review-coverage.ps1`) derives changed languages by parsing `- <path> (+N/-N)` lines in the summary. If `.cs` lines are missing/misclassified, the hook detects zero changed languages and trivially passes coverage validation — masking missing coverage for a language that actually changed.

**How to apply:** In every feature review, treat the PR-context summary as untrusted for the changed-files overview. Run `git diff --name-status <base>..<head>` and `--numstat` as the authoritative scope source. If the summary's overview disagrees, correct the `Changed files overview` section of the summary (stale-evidence correction, not a scope narrowing) so both the audit and the hook operate on truthful data. See [[gitignore-tracking-diff-scope]] for the related rule that diff scope, not plan claims, governs the audit.
