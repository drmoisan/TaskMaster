---
name: reconcile-derived-radius-against-branch-diff
description: Get-BlastRadius derives from plan and spec TEXT, so it cannot see files the preparation child committed but never planned — diff the item branch against origin/main and add the escaped paths as exact entries, never as a subtree glob
metadata:
  type: feedback
---

After deriving an item's declared radius with `Get-BlastRadius`, compare it against what the
item branch actually carries before writing it into the checkpoint:

```bash
git diff --name-only origin/main...origin/<item-branch>
```

Any path in that diff that the derived `paths` does not cover is a real gap. Add it.

**Corollary: a mid-run RE-DERIVATION silently destroys those reconciled paths, so treat any request
to "re-derive all radii and recompute the graph" as fail-open until proven otherwise.** Because
`.claude/agent-memory/**` sits in `mandate_reads`, the library strips those paths at derivation time
and never emits them; they exist in the checkpoint ONLY because a human or a prior session added them
back from the branch diff. Re-deriving therefore deletes real edges and manufactures concurrency on
genuinely conflicting pairs — and this contention is real, not bookkeeping: every child writes
`MEMORY.md`, which is exactly why items 285 and 287 each ended with an uncommitted `MEMORY.md` edit.

**Answer the request with a read-only COUNTERFACTUAL rather than a recompute or a flat refusal.**
Asked on 2026-09-01 to recompute after `scripts/vscode/**` was added to `mandate_reads`, the useful
move was to keep the recorded edges and ask which would survive discarding every `scripts/vscode`
reason. Result: over the 7 unstarted items, all 21 pairs survived and ZERO edges were
`scripts/vscode`-only, so the corrected config unlocked nothing on that run. The counterfactual is
cheap, settles the question with evidence instead of authority, and needs no checkpoint write. Read
`all_reasons[]` rather than the single `reason`/`detail` pair — an edge routinely carries several
reasons and judging it by the first one misclassifies it.

See [[raising-max-concurrency-is-a-noop-when-graph-is-complete]] for the same shape of question.

**Why:** The library derives from plan and specification TEXT. A file the preparation child
committed but the plan never names is structurally invisible to it — not a bug in the
derivation, a limit of its input. Observed 2026-09-01 on `/parallel-add 646`: the library
returned 32 paths from a preflight-cleared plan, and the branch diff carried two more, both
from an agent-memory commit the child made on the item branch
(`.claude/agent-memory/orchestrator/MEMORY.md` plus a new sibling note). Those files ship to
`main` through the item's own PR, so an undeclared radius under-reports what the item lands.

Under-reporting is the one direction this surface cannot tolerate: it is exactly what F8 drift
detection exists to catch, and the concurrency guarantee for every item running beside it
depends on the declared radius being truthful.

**Add the escaped paths as EXACT entries, never as a subtree glob.** `.claude/agent-memory/**`
would have contended with essentially every future item forever — the same failure mode as the
`**/evidence/**/*.md` glob recorded in
[[blast-radius-powershell-calling-convention]]. The exact `MEMORY.md` path contends only with
another item editing that same shared index, which is a real contention and correct to report.
Record the widening and its reason in a `blast_radius_note`; the six canonical keys are
enforced, so the note lives beside `blast_radius`, never inside it.

**How to apply:** Treat this as a required step between derivation and the checkpoint write,
not an optional audit. Expect the agent-memory case specifically — children routinely commit
memory at the end of a run, and it is the established pattern in this repository rather than a
child misbehaving, so the fix is to declare it, not to strip the commit. Note that a prior
item's radius (647) had the same undeclared agent-memory files and shipped anyway, so a clean
sibling precedent is not evidence the gap does not exist.

**The escape scales with the DELEGATION CHAIN, not with the item.** Expect roughly one
`MEMORY.md` plus one sibling note per agent that participated, not two files total. Confirmed
2026-09-01 on `/parallel-add 656`, the very next add: the library returned 94 paths and the
branch diff carried SIX more, one pair each from the `atomic-executor`, `atomic-planner`, and
`task-researcher` trees — none from `orchestrator`, which is the only tree the 646 case
produced. So do not grep for the orchestrator path you saw last time; diff the branch and take
whatever `.claude/agent-memory/**` files it actually carries. Two adds in a row escaped, which
makes this the expected outcome rather than an edge case.

**The escape is not universal, and the exception identifies its cause precisely.** On
`/parallel-add 633` (2026-09-01) the library returned 99 paths and the branch diff carried NOTHING
beyond the feature folder, so no widening was needed. The reason is the mechanism, not luck: that
preparation child died on a rate limit before making its own memory commit, and the parent — which
completed the commit on its behalf — cannot commit `.claude/agent-memory/**` at all, because the
pre-implementation gate's operand exemption covers only `docs/features` and
`artifacts/orchestration` paths. So the escape appears exactly when the CHILD finishes and commits,
and disappears when the PARENT closes the gap. Do not skip the diff on that expectation; run it and
let it tell you which case you are in.

**Three adds in a row now, so treat the escape as certain unless the child died before its
memory commit.** Confirmed again 2026-09-01 on `/parallel-add 670`: the library returned 103
paths and the branch diff carried TEN more — `MEMORY.md` plus siblings from the
`atomic-executor`, `atomic-planner` and `orchestrator` trees. The one add that escaped nothing
was 633, and only because its preparation child was rate-limited before committing memory; the
parent could not make that commit on its behalf, since the pre-implementation gate's operand
exemption covers only `docs/features` and `artifacts/orchestration` paths. So a clean
reconciliation is the signal of an interrupted child, not of a well-behaved one.

These paths are now load-bearing in the graph rather than incidental: 4 of the run's 36 edges
rest on a `.claude/agent-memory/` path, and every one of them exists ONLY because the
reconciliation was performed. `.claude/agent-memory/**` is a `mandate_reads` exclusion, so the
library can never derive such a path from plan text — the reconciliation is the sole producer
of that whole class of edge.

**The escape is PREVENTABLE from the delegation prompt, which is cheaper than reconciling it
away.** On `/parallel-add 678` (2026-09-01) the library returned 116 paths and the branch diff
carried nothing beyond the feature folder — and unlike 633, the child did NOT die: it ran to
completion and cleared preflight. The difference was the prompt. It carried two clauses together:
"commit nothing outside `docs/features/`" and "if you write agent memory, say so explicitly and
name the exact paths, because the parent must reconcile the derived radius against the branch
diff". The child reasoned from the first clause that `.claude/agent-memory/` is tracked in this
repository (609 files, not gitignored), so writing there would either breach the footprint
constraint or leave the worktree dirty for the execution child — and it therefore wrote no memory
at all, reporting its findings in its final message instead.

That is a real lever, not a coincidence: the escape exists because children commit memory by
default, and naming the consequence in the prompt makes a well-behaved child decline. Use it when
you want a clean radius. Note the tradeoff — the child's reusable findings then live only in its
report, so relay anything worth keeping rather than letting it evaporate. And do not invert the
earlier signal into a rule: a clean reconciliation now means EITHER an interrupted child OR a
prompt that suppressed the memory commit. Run the diff either way; it is the only thing that
distinguishes them.

**The prompt lever is now confirmed twice, so treat it as reliable rather than anecdotal.** Repeated
2026-09-01 on `/parallel-add 287`, whose delegation prompt carried the same two clauses: the library
returned 92 paths and the branch diff carried nothing beyond the feature folder. As on 678 the child
ran to completion and cleared preflight, so this is the suppression working, not an interrupted
child. Both confirmations came from a RESUMED preparation as well as a fresh one, so the lever
survives the resume path. Keep running the diff regardless — it is still the only thing that
distinguishes a suppressed child from a dead one.

**Re-test the widened radius before writing it.** Adding paths can in principle resolve a new
shared surface or a new edge, and a radius that silently gained contention is worth knowing
about before it reaches the checkpoint rather than after. On 656 the re-test came back clean —
no shared surface added, no edge or reason kind changed — and that was recorded here as "the
expected result for agent-memory paths".

**That expectation was wrong, so run the re-test rather than assuming its outcome.** On 670
the widening changed the reason kinds on two edges: 285 and 647 had contended by
`module_overlap` alone and gained `path_overlap`, because those items' radii carry the same
shared memory index. No conflict VERDICT changed, so nothing about the schedule moved, but the
recorded reasons would have been wrong had the pre-widening values been written. The clean 656
result was a property of that particular pair of radii, not of agent-memory paths in general.
It is one command; do not skip it on the strength of a prior clean run.

**Confirmed again on 662, and the effect is growing rather than shrinking.** The widening changed
the reason kinds on FOUR edges this time — 285, 646, 648 and 656 each moved from `module_overlap`
alone to `module_overlap` plus `path_overlap` — versus two on 670 and zero on 656. The cause is
cumulative: every add that reconciles agent-memory paths into its radius raises the chance that the
NEXT add shares one, so the run now has 13 of 78 edges resting on a `.claude/agent-memory/` path
where it had 8 of 66. Expect the re-test to matter more as a run lengthens, not less. As before no
VERDICT moved, so the schedule was unaffected — the re-test protects the recorded REASONS, which is
the thing a later reader uses to judge whether an edge is real contention or a tooling artifact.

**Run the re-test as narrow-versus-widened, not just widened.** The delta is only visible if you
compute both. One extra invocation of the same harness over the narrow radius gives you the exact
list of edges whose reasons changed, which is what the status doc should record; computing only the
widened result tells you the final state but not that anything moved.

**Do NOT re-normalize the widened radius through `Get-NormalizedDeclaredRadius` — it strips the very
paths you just added.** That helper looks like the right way to re-resolve modules and shared
surfaces over the new path list, and it is the obvious move because it is exactly what
`Get-BlastRadius` calls internally. But its body runs `Get-NonMandateReadEntry` over the paths
first, and `.claude/agent-memory/**` is a `mandate_reads` exclusion, so every reconciliation-added
path is silently filtered back out and the widened radius collapses to the narrow one. Caught by
inspection on `/parallel-add 663` before it was run; nothing downstream would have flagged it,
because the result is a perfectly valid radius that merely under-reports — the exact failure the
reconciliation exists to prevent.

Build the widened radius by hand instead:

- `paths` = sorted union of the narrow paths and the escaped paths.
- `modules` / `shared_surfaces` = the narrow values unioned with
  `Get-BlastRadiusFromObservedPaths -ObservedPaths $escaped -Config $cfg -ComputedAt $ts`. That
  entry point resolves modules and surfaces from paths with NO mandate-read filter (it uses
  `Get-ConcreteEntry`), so it is the one library call that will tell you what the escaped paths
  actually resolve to. On 663 the answer was "nothing" — no module, no surface — which is worth
  confirming rather than assuming, since it is what licenses keeping the narrow values.
- `contracts`, `source`, `computed_at` = carried from the narrow radius.

**Exact-containment misses glob coverage, so expect one or two harmless false escapes.** The
reconciliation compares diff paths against `paths` by string equality, so a file already covered by
the item's own `docs/features/active/<slug>/**` glob still reports as escaped. Adding it is
harmless and matches precedent (several items carry both the folder glob and exact evidence paths),
but do not read the count as evidence of that many undeclared files — on 663, 1 of the 13 was this.

**A three-dot diff anchored to a PINNED SHA silently degenerates to two-dot.** `A...B` means
`merge-base(A,B)..B`, so when `A` is an ancestor of `B` the merge base IS `A` and the three-dot form
collapses to the two-dot form, reporting every path that landed since `A` — including a whole
reconciliation merge with `main`. Items 656 and 662 both shipped plans whose footprint gates pinned
such an ancestor: they reported 299 and 22 paths against true values of 51 and 4, before either
executor had made a single edit. **Three-dot syntax is not by itself protection against a stale
base; only the freshness of the left operand is.** Always anchor to `origin/main` (or an explicit
`git merge-base origin/main HEAD`), never to a SHA a plan pins. Two consecutive items carried the
same construction, so treat a pinned base SHA in a footprint gate as a plan defect to report rather
than a condition to satisfy — the gate is unsatisfiable as written.
