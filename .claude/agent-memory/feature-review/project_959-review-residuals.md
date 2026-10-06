---
name: 959-review-residuals
description: '#959 (SortEmail latent logic defects L1-L4 plus folded #966 residuals, EfcDataModel finally, ToDoModel duplicate) full-bug review: cycle 1 2026-10-06T15-30 PASS 25/27 (CR-1..CR-7); cycle 2 (re-review after in-item Phase 7 remediation) 2026-10-06T17-40 PASS 25/27, 0 blocking, CR-1/CR-3/G-4 closed, new CR-8 stale plan Status line; AC6/AC27 pending PR body; validator artifact_path workspace-relative; sync/async ternary arms both pinned'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, item worktree `repos/TaskMaster/.claude/worktrees/agent-ae9faf6e1bf21ac17`, merge base
`942873699` = origin/main at cut, no main merge yet). Cycle 1 at head `0fab75ed6` (label 15-30): PASS, 25/27 AC, AC6 and AC27 PENDING
on the PR body (caller said "pending, not FAIL" with NO remediability class, so PASS and no remediation-inputs, the #950 shape), 0
blocking, CR-1..CR-7, G-1..G-7, U-1..U-3. Cycle 2 at head `2d7e338f4` (label 17-40) after the orchestrator ran an in-item "Phase 7"
(plan revision 2.0 appended in place; CR-1 and CR-3 remediated; full toolchain re-run; three fixed-name projections rewritten at
ITERATION 2 with `SUPERSEDES:` commit SHAs): PASS 25/27, 0 blocking, CR-2 and CR-4..CR-7 carried per the orchestrator's PD-16, CR-8 new.
Both cycles ran with no Bash; all six artifacts validated first try (cycle 2 after one wording edit, see below).

**Reusable verification points:**
- `mcp__drm-copilot__validate_orchestration_artifacts` JOINS `workspace_root` and `artifact_path`: pass
  `docs/features/active/<feature>/<stem>.<ts>.md` relative to the worktree root.
- A seamed sync/async core PAIR with the same `IsImage ? pictures : attachments` ternary: read the `<class>` node for every new core,
  not just per-member line percentages (cycle 1 CR-1: sync ternary `50% (1/2)` at 100% lines). Closure proof in cycle 2: the same
  `<line number="143" ...>` node inside the file's class region (Grep the pattern, pick the hit between the class's document line and
  the next class) reads `100% (2/2)` and the method node `branch-rate="1"`; the executor's P7-T1 "false-before" reading of the Phase 6
  document is the RED-equivalent for a coverage-only test, disclosed as "already-correct behaviour, no fail-before run".
- A re-review after in-item remediation: the fixed-name projections are OVERWRITTEN (Phase 6 document gone), so a one-branch
  repo-wide drift outside the changed family (13681 -> 13680 covered) cannot be localised; record it as an observation under the
  #511 run-to-run band, cite the family class rows being identical and the non-exempt hash equal to baseline, and do not gate on it.
- A new `throw;` inside an async `catch` reads `branch="True" condition-coverage="50% (1/2)"` and the brace after it `hits="0"`;
  branch-rate drops from such arms are not regressions.
- Modified file below every floor at baseline (EfcDataModel.cs 75.69/73.08 -> 76.34/73.08, changed lines hit, QuickFiler interop
  controller within CLAUDE.md UT2 exemption (c)): FAIL on the per-file threshold limb, PASS on the no-regression limb, non-blocking.
  The async method's changed lines live in the `d__N` MoveNext node, not under `name="MoveToFolderAsync"`; cite the executor's
  E-CHANGED-LINES rows plus the synchronous seam's method node.
- Same-root-cause call site in an UNCOMPILED file (`QuickFiler/Legacy/QfcController.cs:792`): Minor, two dispositions, orchestrator
  chose "for filing with the folder cleanup" (PD-16); carry it verbatim, do not re-litigate.
- After an in-place plan revision that appends phases, check the plan's `- **Status:**` header line against the task boxes: at #959
  all 16 P7 boxes were `[x]` and committed while the Status line still said "P7-T1 to P7-T16 pending" (CR-8, Minor documentation drift).
- Plan self-review prose may name the worktree LEAF (`agent-<hex>`) as a `.git/worktrees/` path; that is repository-relative, not a
  host/account identifier, and the executor's sweep tokens do not match it. Record as an observation, not a finding.
- Clock without a shell: worktree reflog `repos/TaskMaster/.git/worktrees/<wt>/logs/HEAD`; choose the label strictly after the last
  epoch and say it is not a clock read. Cobertura root `timestamp=` sat 2 min before the 17-19 label and 121 s before the commit.
- Hook-safe policy-audit wording: a literal "out of scope" inside a quoted caller statement under `## Rejected Scope Narrowing` is
  harmless to the validator (no language label + `coverage` on that line) but I paraphrased it anyway; the TS/PS/Python 1.2.1 bullets
  and metrics rows may carry `N/A`.
- AC wording drift to evaluate on intent: "has exactly one catch clause" and "gains one DataRow row" — Informational (CR-7).

**Follow-ups owed to the orchestrator after cycle 2:** CR-8 plan Status line; CR-2 + U-2 QfcController/`QuickFiler/Legacy/` filing;
G-1/U-1 EfcDataModel coverage uplift (file); G-5/U-3 `quality-tiers.yml` absent (recurring); AC6 and AC27 close at the pr-author step;
Phase 8 (origin/main merge, union resolution of QuickFiler.Test.csproj) is gated on this re-review passing.
