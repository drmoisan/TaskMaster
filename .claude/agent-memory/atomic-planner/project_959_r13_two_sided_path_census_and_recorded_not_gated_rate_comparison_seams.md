---
name: project-959-r13-two-sided-path-census-and-recorded-not-gated-rate-comparison-seams
description: Preflight round 13 on the #959 revision 2.0 plan (post-merge fan-in gate and Phase 7 no-regression gate) - an additions-only fan-in gate must enumerate every two-sided path mechanically, a same-tree two-decimal first-party comparison is recorded not gated, FILES floors must equal their own breakdown sums, a "no Write Set edits" phase rule contradicts a conflict-resolution branch
metadata:
  type: project
---

Preflight round 13 (2026-10-06) on the #959 plan revision 2.0 reported five defects; all were textual gaps in gates authored without a git channel. The deltas were applied verbatim with one sibling correction, no version bump (the 1.8 delta-pass convention: a note in the existing log entry plus a "delta pass" bullet in the self-review record).

**Why:** the fan-in gate `CMD-FANIN` proved "additions-only" for the one overlap the planner knew about (the csproj) while subtracting `.claude/agent-memory/` as an own path, so a second two-sided file (`.claude/agent-memory/orchestrator/MEMORY.md`, both sides inserting one index line thirty-six lines apart) was never checked and a merge that dropped main's line would have passed. The reviewer, who had git, found it; the planner, who did not, had asserted "the one known overlap".

**How to apply:**
- A fan-in "additions-only" gate must compute the two-sided set mechanically: `$shared = @($mainOnly | Where-Object { $paths -ccontains $_ })` (main-only paths since the anchor intersected with the branch's paths relative to the fetched main SHA), print every `SHARED-NUMSTAT` row, count `SHARED-WITH-LOSS` with the predicate `-notmatch "^[0-9]+\t0\t"` over each row's numstat, and add a `LOSS-CHECK-CONTROL` over a path known to delete a line (`0 1`) so the predicate is proven to fire. Never name "the one overlap" from memory when a git channel is unavailable; attribute the observation to whoever ran git.
- A same-tree two-decimal first-party line/branch comparison against an earlier phase has a margin of about six lines and two branches and trips on run-to-run variance in untouched assemblies. Record it (`PHASE6-*-NOT-LOWER:` as observations with a named `Output Summary:` string) and hold the changed-line no-regression rule on the family rows instead: quote the `SORTEMAIL-AGG` aggregate and every `SORTEMAIL-CLASS` row from the earlier phase's artifact and gate identity (`PHASE 7 FAMILY COVERAGE CHANGED`). Re-derive the quoted values from the evidence file in the same pass.
- A `FILES:` floor must equal the sum of its own parenthetical breakdown (89+1+3+10 = 103, not 100; 103+1+8 = 112, not 108). Count the on-disk feature folder with Glob when git is unavailable: it matched the reviewer's `git ls-files` figure (93).
- A phase rule that says "no task edits a Write Set path" contradicts a conflict branch that resolves a Write Set csproj with the Edit tool; name the admitted path in the rule.
- Re-count every "all N required" after an insertion; the P8-T3 insertion brought a stated nine up from an actual eight, and P7-T1 stated seven for six.
- A sibling note that says "the one overlapping file" is invalidated by a delta that names two; narrow it to the path the control actually tests and report the sibling correction explicitly.
- Self-review "0 lines" claims for a retired phrase self-hit once the log entry and the record quote it; state "only the log entry's quotation and this record's pattern text" instead (see [[project-959-r7-ac-by-reference-spec-correction-and-delta-backtick-seams]]).

Related: [[project-959-r10-append-phases-review-residuals-and-pr-time-merge-seams]] (the fan-in gate this round corrected), [[project-964-r4-post-merge-footprint-reanchor-with-negative-controls]].
