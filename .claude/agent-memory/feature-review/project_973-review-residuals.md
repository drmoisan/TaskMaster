---
name: 973-review-residuals
description: '#973 (15 stale binding-redirect pairs, ADAL deletion, System.Linq.AsyncEnumerable aliased install, Graph-using removal, CategoryClassifierGroup partial split, CLAUDE.md premise) full-bug review 2026-10-06T19-30 REMEDIATION_REQUIRED 21/23 AC, 2 blocking (AC17 autonomous spec amendment; AC18 human manual runbook); no-Bash; split-coverage arithmetic; bundled PoshQC artifact FAIL row beside vacuous PASS'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, item worktree `repos/TaskMaster/.claude/worktrees/agent-a24d410b914bcefd7`,
head `9b0d54217`, merge base `993fdd015`): REMEDIATION_REQUIRED, 21/23 AC, 2 blocking (B-1 AC17 class autonomous, B-2 AC18
class human_decision_required per caller), 3 non-blocking, 5 observations. Caller forbade Bash; all four artifacts validated
with `mcp__drm-copilot__validate_orchestration_artifacts` using the #968 shape (see [[968-review-residuals]]); policy-audit
passed first try, one post-validation edit (a `Not applicable ... Pester` cell, banned by the SubagentStop hook regex) re-validated.

**Reusable verification points:**
- AC premise defect pattern: an AC that asserts "request X redirects to an assembly in the add-in output directory" fails when
  the requester is never deployed there. Three agreeing observations closed it: executor `Test-Path` probes, the caller's
  System.Reflection.Metadata read of the referencing DLL, and a reviewer Grep for the family's identifiers over the sources
  (0 hits; `using` directives that bind nothing emit no assembly reference, and RAR copies ProjectReference dependencies from
  the referenced DLL's metadata). Classed autonomous because the item itself carried orchestrator-ruled AC amendments (spec
  Planner Amendments 4 and 5), with the reclassification route to human_decision_required stated in the finding.
- Pure-move partial split coverage check from Cobertura `<class>` nodes keyed on `filename=`: derive counts from line-rate
  fractions (0.720126 = 229/318; 0.669145 = 180/269; new file line-rate 1 = 49/49; branch 0.775862 = 45/58 = 24/34 + 21/24)
  and show original + new == baseline for both lines and branches; package-level deltas elsewhere are run-to-run variance.
  The new partial's `complexity` equals baseline minus the original's (70 = 44 + 26) as a second corroboration.
- Bundled PoshQC `artifacts/pester/powershell-coverage.xml` again reads 0/9294 with 13 packages all under `.claude/` and
  `.codex/` (Grep `^  <package name=` and `^  <counter`); wrote an honest FAIL row on it plus `PASS by vacuity` on changed
  lines (test-only PowerShell diff; module hash identical in the format artifact) and named the CI `_pester.yml` job as the
  scripts/ source. Its JaCoCo `sessioninfo start` epoch is local-time-as-UTC (4 h behind the reflog); do not use it as a clock.
- Verify caller "plan-text defect" claims against the plan by Grep: here every wildcard pathspec in the plan was single-quoted;
  the unquoted forms were executor-issued and corroborated by quoted re-captures, so no plan remedy was owed.
- CRLF check without a shell: Grep `\r$` in count mode equals the line count for a CRLF file (402, 442, 106 here).
- Topology and hook path identical to #968 (session cwd `TaskMaster-wt/<ts>`, five-`..` traversal); session-cwd
  `artifacts/pr_context.summary.txt` belonged to #959 and enumerated `.cs` and `.ps1`, so both language rows had to be
  hook-clean; no canonical C# or PS artifact existed in the session cwd, so no forced FAIL.

**Follow-ups owed to the orchestrator:** B-1 spec amendment of AC17 (and Proposed Fix trace steps 1 and 4) then check-off;
B-2 maintainer runbook run (designer-load-<ts>.md); optional CR-1 plan P0-T20 pattern narrowing; file CR-3 (classifier build
path and Triage_OlLogic sub-floor coverage) and P-4 (bundled PoshQC coverage never instruments scripts/) if untracked;
canonical `artifacts/csharp/coverage.xml` still absent (recurring O-2).
