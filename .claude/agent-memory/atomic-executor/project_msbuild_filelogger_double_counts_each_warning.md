---
name: msbuild-filelogger-double-counts-each-warning
description: An MSBuild /flp:verbosity=normal log records every warning TWICE (inline + end-of-build summary), so a whole-log Select-String count gate expecting one-per-project fails at exactly 2x
metadata:
  type: project
---

An MSBuild file logger at `/flp:logfile=<path>;verbosity=normal` writes each warning
**twice**: once inline as the project builds, and once again in the end-of-build
warning summary block. A plan gate of the form
`(Select-String -Path <logfile> -Pattern '<token>' -SimpleMatch).Count` run over the
whole log therefore returns exactly **2x** the number of distinct warnings.

**Why:** Observed on #730 (2026-09-02). The plan gated
`RxCheck_analyzers_pre == 5` (one System.Reactive.PackagesConfigCheck warning per
affected project) using a whole-log `Select-String ... .Count`. Measured value was
`10` — five inline matches at log lines 7665/8764/10028/10725/11842 (UtilitiesCS,
ToDoModel, QuickFiler, TaskMaster, UtilitiesCS.Test) and the same five again in the
summary block at lines 11851-11871. MSBuild's own summary line read `5 Warning(s)`
/ `0 Error(s)`. The underlying fact the plan asserted was true; only the measurement
method was wrong, and the gate was unsatisfiable as written.

The same doubling hits a generic `": warning "` / `": error "` count, so a delta
gate written as `W_post == W_pre - 5` is also arithmetically wrong — the true delta
is `-10`. One bad counting method therefore breaks the baseline task AND the
post-change task that subtracts from it.

**How to apply:** When a plan gates a warning COUNT read out of an msbuild file log,
check whether the count is per-occurrence or per-distinct-warning before accepting
the threshold. Robust alternatives, in preference order:
1. Parse MSBuild's own summary line (`^\s*(\d+) Warning\(s\)`), which is emitted once.
2. Restrict the search to the summary block, or pipe through
   `Select-Object -Unique` on the matched line text.
3. Keep the whole-log count but state the expected value as `2 x <distinct>`.
Raise this at preflight; once execution starts it is a hard stop, because
adjusting the threshold is forbidden replanning. See
[[project_preflight_selfderived_gate_thresholds_are_blind]] and
[[project_plan_authoring_time_token_counts_are_undercounts]].
