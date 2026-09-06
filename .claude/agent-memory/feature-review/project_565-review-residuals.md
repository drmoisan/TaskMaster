---
name: 565-review-residuals
description: "#565 PASS/0 blocking, 6/6 AC; a CONCURRENT reviewer committed artifacts into the same item worktree mid-review with a future-dated timestamp, and its own host-path self-clearing claim was false"
metadata:
  type: project
---

Issue #565 (`Set-Content` before `Assert-CoberturaLineCoverageThreshold` in
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) — second independent review pass, verdict **PASS, 0
blocking, 6/6 AC**. Two-file footprint, pure statement reorder, provable RED-first.

**Why these residuals matter:** three of them are process traps that will recur in parallel mode, not
facts about this fix.

1. **A concurrent agent wrote into the same item worktree while I was reviewing.** `HEAD` moved from
   `101d5ec8` to `e5dcbffd` mid-audit, adding `policy-audit/code-review/feature-audit.2026-09-03T12-15.md`.
   An earlier `git ls-files` and `git status` had shown neither. Re-run `git log --oneline -1` and
   `git status` near the END of the audit, not only at the start — otherwise findings are written
   against a head that no longer exists.
2. **That artifact set is timestamped 35 minutes in the FUTURE** (12-15 against an actual UTC clock of
   11-40). Since consumers pick review artifacts by "latest timestamp wins", a future-dated set
   outranks a later, more accurate one. Do not copy a sibling's timestamp to stay ordered — use the
   real clock and say so explicitly in the report.
3. **The prior audit's self-clearing claim was false.** Its § Absolute Host Path Check asserted
   ``Grep -pattern "C:\\Users|<account>|C:/Users"`` across the feature folder "returns no matches",
   while line 5 of that same file read
   ``- Worktree: `<user-profile>/repos/TaskMaster/.claude/worktrees/agent-...` ``. Re-run any
   hygiene grep yourself; a prior artifact clearing itself is the least trustworthy kind of evidence.
   Extends [[verify-the-callers-factual-correction]] and [[verify-asserted-evidence-mechanism]].
4. **`pr_context.summary.txt` started stale (#735) and was regenerated mid-review** (11:38:15 UTC) to
   correct content. Deriving scope from git first meant the regeneration became independent
   corroboration rather than a rewrite of my conclusions. Keep doing that — see
   [[pr-context-artifacts-are-tracked-not-gitignored]].
5. **Repo-wide first-party PowerShell coverage is far below floor and it is nobody's regression.**
   Measured in-session over `scripts/**/*.ps1`: 534/747 = 71.49% line (77.50% excluding the untracked
   stray `scripts/temp-extract-coverage.ps1`). The entire shortfall is five scripts with no tests:
   `run-actionlint.ps1` 0%, `Invoke-Restore.ps1` 0%, `TestProcessCleanup.ps1` 0%,
   `Install-RepoDotNetSdk.ps1` 9.09%, `Sync-PackageReferences.ps1` 63.10%. That attribution list is
   stable and reusable even though the percentages are not (see
   [[powershell-coverage-nondeterministic-vsbuild-tests]]).
6. **Residual advisory:** `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is at 496/500
   lines. The next `It` added there breaches the file-size limit. #733 already split
   `Threshold.ps1` out of `Helpers.ps1` for the same reason.

**How to apply:** on any TaskMaster parallel review, sample `git log -1` twice; never trust a prior
review artifact's self-audit; and when the canonical PowerShell coverage artifact is irrelevant,
measure directly rather than recording `UNVERIFIED` — see
[[poshqc-bundled-coverage-artifact-reads-zero]].
