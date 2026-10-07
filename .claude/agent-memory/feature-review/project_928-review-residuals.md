---
name: 928-review-residuals
description: #928 (scoped coverage-runner threshold skip) - cycle-1 reduced audit 1 blocking (AC6 uncredited entry-point line 408), remediation relocated the gate into the path-loaded part file, cycle-1 exit re-audit 2026-09-29T11-15 PASS 7/7 AC, 0 blocking; no-Bash review mechanics incl. reading gitignored JaCoCo <sourcefile> nodes directly; promotion candidates P-1..P-4
metadata:
  type: project
---

Cycle 1 (2026-09-29T10-00): REMEDIATION REQUIRED, blocking 1 (AC6 PARTIAL). Population 94.49% -> 94.46%;
entry point 113/126 -> 115/129 with changed line 408 (scoped-arm `Write-Warning`) at 0 hits; new part file 5/5.
Mechanism: [[pester-breakpoint-coverage-binds-to-first-parsefile-copy]].

Cycle-1 exit (2026-09-29T11-15): PASS, 7/7 AC, 0 blocking, 1 non-blocking. The executor moved the
`if scoped {warn; return} ; line-assert; branch-assert` block into `Assert-CoberturaCoverageThresholdForRun`
in the path-loaded `Invoke-MSTestWithCoverage.Scope.ps1` behind one unconditional entry-point call, and
folded CR-2 (`ValidateNotNullOrEmpty` + `IsPathRooted` guards, 4 negative tests). Result by the identical
breakpoint-mode command: population 94.53% (1626/94) vs baseline 94.49%; entry point back to 113/126 exactly
(the 2 moved assertion lines out, dot-source + call in); part file 13/13. The plan's P0-T7 diagnostic gate
measured the crediting premise BEFORE the edit using Threshold.ps1's throw lines as the subject (control run
leaves 53/54/123/124 uncovered; ordered AssemblyDiscovery-then-Scope run credits them).

**Why the mechanics matter next time:**
- Review ran with NO Bash tool both cycles. Read/Grep/Glob were enough: Glob hides gitignored paths but
  Read/Grep on the explicit path work. At the exit review I read the gitignored direct-Pester JaCoCo documents
  under `<wt>/coverage/*.jacoco.xml` at the `<sourcefile>` level (Grep `<sourcefile name=` for line numbers,
  then Read the region) and verified the uncovered-line set, the changed-line `ci` values and the report-level
  counter myself instead of trusting the executor's `FILE_LINE` transcription. Do this whenever the executor
  leaves the raw document in the worktree.
- Branch head is readable without git: `<session>/.git/worktrees/<wt>/HEAD` -> `ref: refs/heads/<branch>` ->
  `<session>/.git/refs/heads/<branch>` (loose ref; would need packed-refs if absent).
- Worktree lives INSIDE the session checkout (`<session>/.claude/worktrees/<wt>`); the item worktree has NO
  `.claude/worktrees/` dir, so the hook-satisfying advertised path is THREE `..` from the session cwd:
  `docs/features/active/../../../.claude/worktrees/<wt>/docs/features/active/<feature>/policy-audit.<ts>.md`.
- Session-root `artifacts/pr_context.summary.txt` belonged to a sibling parallel item (#930, C# branch) but
  its bullets were all `.md` (collect_pr_context misclassifies C# as docs), so the hook's language checks are
  disarmed from the session cwd. In the worktree cwd my hand-authored summary lists `.ps1`, and the canonical
  `artifacts/pester/powershell-coverage.xml` there reads 0/9294 (bundled route instruments only `.claude`/
  `.codex`), so I wrote an honest FAIL row on that artifact (non-blocking, pre-existing) plus a PASS row on the
  Route C measurement, on separate single lines, each carrying label + coverage keyword + verdict.
- Hook trap: never write "Not applicable"/"N/A"/"UNVERIFIED" on any line that mentions PowerShell/Pester
  together with a coverage keyword (or Pester alone); grep the draft with the hook's narrowing regex before
  finishing.
- No clock available without Bash: assigned the ts label later than every executor label and disclosed it.

**Residuals / promotion candidates (owed by the orchestrator):**
- P-1 repo-wide breakpoint under-crediting of entry-point lines (avoided structurally, not fixed).
- P-2 `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines 41, 42, 104, 124 embed the
  developer account name in literal fixture paths (pre-existing, outside the diff).
- P-3 script-level comment-based help for the coverage runner (AC4 satisfied via function help only).
- P-4 the bundled PoshQC test route's coverage document never covers `scripts/`, so the review hook's
  canonical PowerShell artifact can only force FAIL rows.
- Plan D9 finding: the issue's "formatter would rewrite both scripts" claim was measured with bare
  `Invoke-Formatter`; the repository MCP route rewrote nothing, so `Invoke-MSTest.ps1` is unchanged.
