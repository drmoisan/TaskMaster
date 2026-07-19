---
name: coverage-hook-skips-when-no-pr-context-summary
description: TaskMaster validate-feature-review-coverage.ps1 skips ALL per-language coverage-row checks when artifacts/pr_context.summary.txt is absent
metadata:
  type: project
---

TaskMaster's `validate-feature-review-coverage.ps1` (SubagentStop hook) computes the changed-language set solely from `artifacts/pr_context.summary.txt` via `Get-ChangedLanguageSet`. If that file is absent, `$changedLanguages.Count -eq 0` and the function returns `Ok=true` BEFORE running any per-language coverage-row PASS/FAIL / narrowing-phrase check (line ~426-428). Only the three artifact-path-advertisement + canonical-location + matching-folder/timestamp checks still run.

**Why:** #367 (epic child, base = epic integration branch) had no pr_context artifacts in-session, so the C#-coverage-row enforcement never fired even though 19 `.cs` files changed.

**How to apply:** When no `artifacts/pr_context.summary.txt` exists, the hook will not block on a missing/narrowed C# coverage verdict — but the feature-review policy still requires explicit per-language coverage verdicts, so write a clean `PASS`/`FAIL` coverage row anyway. Also note: `Get-LanguageRepoCoverage` for C# reads `artifacts/csharp/coverage.xml`; when that file is intentionally NOT emitted (annotation-only epic children where the fixed 85% floor would false-FAIL against a pre-existing repo-wide %), the C# repo pct resolves `$null` and no forced-FAIL occurs. Writing audit artifacts to `docs/features/active/<f>/evidence/qa-gates/` is NOT a forbidden path (enforce-evidence-locations only blocks `artifacts/*` prefixes), and the coverage hook's path regex accepts the nested `evidence/qa-gates/` folder since its Folder group is `.+`. See [[csharp-coverage-artifact-is-cobertura]], [[pr-context-mcp-unavailable-manual-fallback]].
