# Code Review — Issue #166 (worktrees-missing-claude-dir)

- Generated: 2026-05-27T11-47
- Reviewer: feature-review agent
- Base branch (resolved): development
- Merge-base SHA: b7bd81626a512c70c264a8badad5fa5691ca1c16
- Head SHA: ca531c67b1a3605562894a5fe49d7cd38b382819

> Template note: the MCP `code-review-template` asset is not available in this branch.
> This artifact is constructed to the required shape (Executive Summary, Findings Table with
> the mandated header). Provenance is recorded as UNVERIFIED template source.

## Executive Summary

The hand-authored production change is a single, correct, minimal `.gitignore` edit: the bare
`.claude` entry is removed and replaced with two targeted ignores
(`.claude/settings.local.json` and `.claude/agent-memory/`) plus two explanatory comments
citing Issues #166 and #149. Live verification confirms the intended behavior: the `.claude/`
tooling subtrees (agents, hooks, rules, skills, settings.json) are no longer ignored, while the
two Issue #149 paths remain ignored. The functional change is sound and well-commented.

The review-blocking concern is not the `.gitignore` line itself but the consequence of the
change combined with the orchestrator `git add -A` step: 70 files become newly tracked,
including 17 PowerShell hook scripts under `.claude/hooks/`. Those PowerShell production files
enter the branch-vs-base diff without any coverage verification against this branch. The only
PowerShell coverage artifact is stale (dated 2026-05-06) and reports 0% line coverage. The
feature's QA summary marked coverage "N/A" on the incorrect basis that "no source files
changed," which is contradicted by the executor's own dry-run evidence listing the 17 `.ps1`
files as newly staged.

The PowerShell hooks themselves are not modified by this branch (they are added as-is from the
previously-untracked working tree), so this review does not re-audit their internal code
quality line-by-line. The blocking finding is the absence of valid coverage for the
now-tracked PowerShell production files, which is a mandatory gate for any language with
changed files in the diff.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | (branch-wide) | 17 `.ps1` files under `.claude/hooks/` | PowerShell production files are added to the tracked tree by this change, but there is no valid PowerShell coverage for them. The only coverage artifact is stale and reports 0.00% repo-wide line coverage. | Generate current Pester coverage against this branch via `mcp__drm-copilot__run_poshqc_test`; bring repo-wide PowerShell coverage to >= 80% and each newly-tracked hook to >= 90%, or formally scope the hooks' test obligations and re-run the coverage gate. | Coverage is mandatory for every language with changed files in the branch diff; PowerShell is below the 80% floor. | `artifacts/pester/powershell-coverage.xml` report total `missed="284" covered="0"`; header dated `Pester (05/06/2026 21:49:23)`. |
| Blocker | docs/.../evidence/qa/166-toolchain-summary.txt | lines 4, 34-35 | QA toolchain summary asserts "No source files changed" and "Coverage — N/A," which is incorrect for the branch diff: 17 `.ps1` source files are added. | Correct the toolchain determination to reflect the PowerShell files in the diff and run the PowerShell format/analyze/test/coverage chain. | A coverage-N/A claim for a language with changed files is a rejected scope narrowing under the repo scope invariant and the in-repo coverage hook. | `evidence/qa/166-git-add-dryrun.txt` lines 22-38 list 17 `.ps1` files as staged; `166-toolchain-summary.txt` line 4. |
| Major | (branch-wide) | 17 `.ps1` hooks | PowerShell format (PoshQC `Invoke-Formatter`) and analyze (PSScriptAnalyzer) results for these files on this branch are not available in the review environment. | Run `mcp__drm-copilot__run_poshqc_format` and `mcp__drm-copilot__run_poshqc_analyze` against the changed hooks and record results. | The PowerShell toolchain (format -> analyze -> test) applies to any branch with `.ps1` changes. | No format/analyze evidence under `evidence/`; only `git check-ignore` outputs are recorded. |
| Info | .gitignore | final block (lines 351-354) | The `.gitignore` edit is correct, minimal, and well-commented; behavior verified live. No change requested. | None. | Confirms the functional fix meets the design-simplicity and comment-why policies. | `git diff <merge-base>..<head> -- .gitignore`; live `git check-ignore` runs. |
| Info | .gitignore | end of file | The replacement block ends with no trailing newline (`\ No newline at end of file`), matching the prior file state. | Optional: add a trailing newline for POSIX-friendliness; not required and not blocking. | Pre-existing condition, not introduced as a regression. | `.gitignore` diff shows `\ No newline at end of file` on both sides. |

## Typed-language Review Notes

No Python or TypeScript files changed in the branch diff; typed-Python review is not
applicable. No C# files changed; C# review is not applicable.

## Notes on Reviewed-vs-Authored Scope

The 17 PowerShell hooks and the `.md`/`.json` files are added to git as-is from the
previously-untracked working tree; they were not edited by this branch's commits. The internal
quality of those scripts is therefore out of scope for line-by-line authored-change review, but
their coverage obligation is in scope because they are changed (added) files in the branch
diff. This distinction is recorded so the blocking finding is understood as a coverage-gate
failure, not an allegation of defective hook code.
