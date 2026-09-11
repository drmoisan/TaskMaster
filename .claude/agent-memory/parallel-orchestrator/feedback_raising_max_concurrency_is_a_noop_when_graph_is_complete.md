---
name: raising-max-concurrency-is-a-noop-when-graph-is-complete
description: An operator asking for N concurrent orchestrations gets the cap raised on the checkpoint only — never the manifest — plus a plain statement that the per-edge barrier, not the cap, is the binding constraint when the conflict graph is complete
metadata:
  type: feedback
---

When the operator asks to raise `max_concurrency`, set it on the CHECKPOINT, leave the MANIFEST
alone, and say plainly whether it will change anything. On an all-pairs conflict graph it will not.

**Why:** `max_concurrency` and the cohort barrier are two independent controls and only one of them
is usually binding. `.claude/rules/parallel-orchestration.md` states it directly: under the per-edge
barrier the cap "is a pure throughput throttle", it "can never co-schedule two conflicting items",
and raising it "changes only how many independent lanes advance at once". So the cap is an upper
bound on lanes, not a source of them. Observed 2026-09-01 on run `bugs-638-644-647`: 14 items, all
91 of 91 possible unordered pairs present in `conflict_edges`, zero independent pairs, every
current-generation cohort a singleton. Raising 2 to 3 was therefore a no-op — one item can be
in flight whatever the cap says. Silently accepting the request would have implied a throughput
change that cannot occur, and the operator would have discovered it only by watching nothing happen.

**How to apply:**

- **Write the checkpoint field; never the manifest.** `max_concurrency` lives in both. The manifest
  is static input authored by `parallel-planner` and is read-only here, so it keeps its original
  value and the two legitimately diverge. Record the divergence in a note rather than "fixing" it.
- **Measure the lanes before answering, do not assume.** Compare `len(conflict_edges)` against
  `C(n,2)` over the non-withdrawn items and list the missing pairs. An empty missing-pair list means
  exactly one lane. Max cohort size is the same fact read from the other side.
- **Never buy concurrency by shrinking the graph.** The obvious way to honour the request is to
  narrow a blast radius or reinterpret an edge, and the `parallel-orchestrate` skill forbids exactly
  that "in order to combine two cohorts or widen a launch batch". The relation fails closed by
  design. Point at the upstream derivation causes instead — see
  [[mandate-reads-omits-scripts-vscode]] for the `scripts/vscode/**` gap, and note that
  `.claude/agent-memory/MEMORY.md` edges are real contention added by radius reconciliation, not a
  derivation artifact (see [[reconcile-derived-radius-against-branch-diff]]).
- This is the same posture as [[decide-scope-calls-yourself]]: carry out the instruction, then
  report the decision and the rationale, including when the instruction cannot achieve its evident
  goal.

**A C# run is NOT automatically a complete graph — measure it rather than predicting it.** The
completeness on `bugs-638-644-647` came from every plan citing the mandated coverage script (see
[[mandate-reads-omits-scripts-vscode]]), which makes every pair contend through a shared path. That
mechanism does not fire when two items live in disjoint assemblies. Observed 2026-09-08 admitting
item 811 to run `bugs-2026-09-06`: 811 (UtilitiesCS / UtilitiesCS.Test) and 796 (QuickFiler /
QuickFiler.Test) share NO module and NO path, so `Test-BlastRadiusConflict` returned False on both
the narrow and the widened radius — the run's first non-adjacent pair, at 13 of 15 possible pairs.
The practical consequence is that a genuine second lane can exist on a C# run, so the
missing-pair count is worth computing every time and the earlier all-pairs result should be read as
one run's property rather than as a property of the language.
