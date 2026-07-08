---
name: pr-context-mcp-unavailable-manual-fallback
description: The mcp__drm-copilot__collect_pr_context tool is not in this agent's available toolset in-session; when artifacts/pr_context.summary.txt is missing, hand-author it from git diff data so the validate-feature-review-coverage.ps1 hook's Get-ChangedLanguageSet still works
metadata:
  type: project
---

On issue #269 (2026-07-08), `artifacts/pr_context.summary.txt`/`.appendix.txt` did not exist and no MCP `collect_pr_context` tool was present in the feature-review agent's tool list (only Read/Grep/Glob/Bash/Write/Edit). Rather than skipping the "regenerate if missing" instruction, hand-authored both files directly from `git diff HEAD --numstat` (or `git diff <merge-base>..HEAD --numstat` when the branch has committed history ahead of its base), matching the bullet format the hook's `Get-ChangedLanguageSet` regex expects: `^\s*-\s+(\S+)\s+\(\+\d+/-\d+\)\s*$`.

**Why:** `.claude/hooks/validate-feature-review-coverage.ps1` reads `artifacts/pr_context.summary.txt` to build the changed-language set; if that file is absent, `$changedLanguages.Count -eq 0` and the hook skips coverage-row enforcement entirely (silently passes). Leaving the file missing would make the review's own coverage checks unverifiable by the hook, defeating the purpose. A hand-authored summary in the expected bullet format restores enforcement.

**Caveat — paths containing spaces break the bullet regex for that specific line.** The regex's `(\S+)` capture group cannot span a space, so a bullet like `- QuickFiler/Helper Classes/QfcThemeHelper.cs (+1/-1)` fails to match at all (no partial capture), and that file contributes nothing to language detection. This did not matter for #269 because other changed `.cs` files without spaces in their paths (`Theme.Rendering.cs`, `Theme.MailLabelThemingTests.cs`) still registered `CSharp`. If every changed file of a given language has a space in its path, that language would go undetected by the hook — check for this before relying on a hand-authored summary as a substitute for the real collector.

**How to apply:** When `artifacts/pr_context.summary.txt` is missing at review start and no PR-context MCP tool is available, hand-author both files from verified `git diff --numstat` data, cite the exact base/head SHAs and merge-base resolution used, and record in the policy-audit's Scope section that this was a self-generated substitute (not the real collector output). Relates to [[csharp-repowide-coverage-below-80]] for the downstream coverage-artifact-lookup step this unblocks.
