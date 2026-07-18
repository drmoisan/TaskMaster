---
name: pr-context-stale-after-remediation-commit
description: pr_context.summary.txt/appendix.txt captured during cycle 1 do not auto-update when a remediation commit lands; a re-audit must detect and refresh them before trusting Head ref/coverage-artifact claims
metadata:
  type: project
---

Issue #354, remediation_pass 1 re-audit (R4): `artifacts/pr_context.summary.txt` and
`artifacts/pr_context.appendix.txt` were last regenerated during the cycle-1 review and still
recorded `Head ref (resolved): ... @ 96ec70a4...` (the pre-remediation commit) even though the
branch's actual `HEAD` had since advanced to the remediation commit `6c12cfc8` (which added the
pytest suite and refactored the script). The summary's own coverage-artifact claim ("no
`artifacts/python/lcov.info` exists") was also stale by the time of the re-audit.

**Why this matters:** `validate-feature-review-coverage.ps1`'s `Get-ChangedLanguageSet` reads
`artifacts/pr_context.summary.txt`, not `git diff` directly. A stale summary either misses newly
changed languages/files from a later commit, or (worse) still asserts a coverage gap that a
remediation commit already closed, which could mislead a reviewer who trusts the artifact's prose
instead of independently re-running `git rev-parse HEAD` / `git diff --numstat` against the
resolved merge-base.

**How to apply:** On every re-audit cycle (R2+), before writing any review artifact: (1) run
`git rev-parse HEAD` and compare against the summary's recorded `Head ref`; if they differ, the
summary is stale and must be regenerated to cover the full range merge-base..HEAD (not just the
delta since the last cycle); (2) preserve prior-cycle evidence as historical record (do not
delete it) but add a clearly labeled "cycle N" section for the new commit(s); (3) re-verify the
per-language changed-file bullet lines still match the hook's strict `- <path> (+N/-M)` format
exactly (no trailing prose) — see [[project_coverage-hook-label-substring-false-positive]] and
[[project_pr-context-mcp-unavailable-manual-fallback]] for the regex-compliance mechanics.
