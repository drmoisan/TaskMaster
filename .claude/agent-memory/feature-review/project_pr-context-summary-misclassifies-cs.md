---
name: pr-context-summary-misclassifies-cs
description: The PR-context summary's automated "Changed files overview" can mislabel substantial C# changes as docs and report "Core logic changes: 0 files"; always verify against git diff.
metadata:
  type: project
---

The automated `artifacts/pr_context.summary.txt` "Changed files overview" classifier has recurred at least twice: Issue #171 (2026-06-02) reported `Core logic changes: 0 files` while the diff had 9 C# production + 7 test + 4 `.csproj` files; Issue #181 (2026-06-08) reported `Core logic changes: 0 files` / "Docs/templates/agents/tooling: 26 files" while the diff had 31 C# build-config files (15 `.csproj`, 15 `packages.config`), a new `BannedSymbols.txt`, and a +567-line `.editorconfig`. The misclassification is especially likely for C# build-config-only changes (csproj/packages.config/editorconfig); record it under `## Rejected Scope Narrowing` in the policy audit and proceed with the full diff scope regardless.

**Why:** The feature-review coverage validator (`validate-feature-review-coverage.ps1`) derives changed languages by parsing `- <path> (+N/-N)` lines in the summary. If `.cs` lines are missing/misclassified, the hook detects zero changed languages and trivially passes coverage validation — masking missing coverage for a language that actually changed.

**How to apply:** In every feature review, treat the PR-context summary as untrusted for the changed-files overview. Run `git diff --name-status <base>..<head>` and `--numstat` as the authoritative scope source. If the summary's overview disagrees, correct the `Changed files overview` section of the summary (stale-evidence correction, not a scope narrowing) so both the audit and the hook operate on truthful data. See [[gitignore-tracking-diff-scope]] for the related rule that diff scope, not plan claims, governs the audit.
