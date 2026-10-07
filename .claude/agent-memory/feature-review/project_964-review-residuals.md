---
name: 964-review-residuals
description: '#964 (EngineToggleStateCoordinator split into three partials + one shared TryInvokeSink guard; #947 residuals) minor-audit review 2026-10-03T08-50 PASS 8/8, then remediation-cycle-1 exit re-audit 2026-10-03T09-50 PASS 8/8, 0 blocking; a pure-move split isolated a pre-existing uncovered branch arm (closed by one data-driven test); composed-then-re-stamped evidence Timestamps classed Minor non-blocking; no-Bash review mechanics'
metadata:
  type: project
---

Minor-audit review (parallel cohort bugs-2026-09-28, item worktree `repos/TaskMaster/.claude/worktrees/agent-a3fb26aa2afc7c52c`,
origin/main `993fdd015` merged at `981abef77`). Cycle 0 review (label 08-50, head `2fed92d2f`): PASS, 8/8 AC, 0 blocking, CR-1 Minor,
CR-2..CR-4 Informational. The orchestrator opened remediation cycle 1 for CR-1 + CR-4 under the related-defect directive (test-only,
+41/-0 in the SinkGuard partial, commit `b27cf3bd2`). Cycle-exit re-audit (label 09-50, head `01dcbe119`): PASS, 8/8, 0 blocking,
R-1/R-2 closed, new CR-5 Informational (type-level summary omits the new region) and P-1 Minor (timestamps, below). Caller forbade
Bash both times; all six artifacts validated first try with `mcp__drm-copilot__validate_orchestration_artifacts` using the #968
policy-audit shape (see [[968-review-residuals]]).

**Reusable verification points:**
- A pure-move partial split can make a NEW file read sub-floor on branches without any regression: `RenderEngineName`'s
  null-or-empty arm became the only branch in `Messages.cs`, so its `<class>` node read `branch-rate="0.5"` while the type read
  43/44. Disposition used: PASS on the type aggregate + no-regression limb, non-blocking Minor with a one-test remedy. The remedy
  (`[DataTestMethod]` null + "" rows on `HandleToggleClickAsync` with engines unavailable, assert `"(null)"` in the notice) moved
  the node to `branch-rate="1"` at the SAME line (230924) of the pre/post documents, and the TaskMaster package counters moved by
  exactly one branch with zero line change: the arithmetic signature that proves the test, and only the test, closed the arm.
- Executor-side composed timestamps: the cycle-1 executor first wrote estimated `Timestamp:` labels into 14 artifacts, then
  re-stamped all 14 to one host-clock reading `09-23` with the in-field note `(host clock read at correction; the label first
  written was composed, not read)`. Classed as P-1, Minor, non-blocking, no task owed: disclosed in-field, no figure depends on a
  label, labels monotone and bracketed by the cycle-open and cycle-commit reflog epochs, and the host-clock labels that followed
  matched the Cobertura root epoch (09:26:44 vs 09-30 label) and the commit epoch to the minute. Per-artifact write times are
  unrecoverable; say so. Count the note by Grep for `composed` (caller said 15, tree showed 14; report the discrepancy, do not
  reconcile it by guessing).
- Caller-supplied review labels (08-50, 09-50) are themselves not clock reads; state that in every artifact header and show
  they sort after every evidence label and reflog epoch.
- Phrase censuses over XML doc comments wrap across `///` lines; confirm absence by Grep, presence by Read.
- `using System.IO;` in `EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` is USED (`new IOException(...)` as a second
  fault kind); do not flag it.
- The SubagentStop coverage hook reads `artifacts/pr_context.summary.txt` from the SESSION cwd; at both reviews the session pair
  belonged to the #968 branch (head 78e24a68c), which enumerates C#, so C#-labelled coverage lines had to stay free of
  `N/A` / `UNVERIFIED` / `out of scope`. Five-`..` path form from `TaskMaster-wt/<ts>` as at #968.
- Clock without a shell: worktree reflog `repos/TaskMaster/.git/worktrees/<wt>/logs/HEAD` epochs at -0400; Cobertura root
  `timestamp=` sits 2-3 minutes before the executor's artifact label when the label is a real clock read.
- Remediation-plan header drift (`Status: Authored, awaiting preflight`, stale `Last Updated:`) after all tasks are checked is an
  orchestrator-side observation, not an executor finding; no cycle preflight record under `evidence/other/` is observable from
  the tree when the orchestrator keeps it in gitignored state.

**Follow-ups owed to the orchestrator:** CR-5 one-clause summary addition (related, optional); O-1 primary fixture 481/500 (move
`Harness`/`LoggedError` to a `.Harness.cs` partial at the next addition); O-3 canonical `artifacts/csharp/coverage.xml` still
absent (recurring); O-5 `quality-tiers.yml` absent (promoted at #956); O-6 plan header refresh; O-7 whitespace-only keys render
literally in notices (pre-existing #505 contract, not a defect).
