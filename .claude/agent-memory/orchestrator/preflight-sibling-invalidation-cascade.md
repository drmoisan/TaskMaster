---
name: preflight-sibling-invalidation-cascade
description: Preflight rounds cascade when a delta changes a taxonomy or count other tasks reference; bundle the sibling fix into the same delta, and treat a planner's flagged-but-declined residual as a probable defect
metadata:
  type: feedback
---

On a citation-dense plan with cross-task invariants, the `atomic-plan-contract` two-round target is
aspirational. Issue #670's plan took **5 preflight rounds** (1 by a prior orchestrator child, 4 by the
resumed child) and closed **27 defects** — 11 / 9 / 4 / 3 / 0. Rounds 3, 4 and 5 existed almost
entirely because the *previous round's own fix* invalidated a sibling.

**Why:** the cascade mechanism is specific and recognizable. A delta that changes a **taxonomy, a
count, or a stated fact that other tasks reference** silently falsifies every sentence built on the old
value. Worked examples from this run:

- Round 2 replaced a two-outcome stage-4 taxonomy with a three-outcome one. P4-T22 still read "one of
  the **two** admissible outcomes" — a gate against a set that no longer existed.
- Round 2 added two `:\Program Files` sweeps. P3-T14 still derived only **four** substitution tokens,
  so nothing would substitute the path the new sweep searched for: the sweep could never return zero.
- Round 3's fix to P0-T2's host-path handling falsified P4-T28's justification sentence, which asserted
  "P0-T2 rewrites only the repo-local entry".

**How to apply:**

1. **Instruct the reviewer to bundle the consequential sibling fix into the same delta.** Round 4 did
   this explicitly (its defect 3 was the P4-T28 sentence its own defect 1 would falsify) and round 5
   cleared with zero defects. Rounds that fix only the reported defect guarantee another round.
2. **When a delta changes a count or taxonomy, grep the whole plan for the OLD value before accepting
   the revision.** `grep -c "two admissible|four substitution tokens"` returning 0 is a cheap,
   decisive check the orchestrator can run itself in seconds.
3. **Treat a planner's "I found this but did not act on it" as a probable defect, not a judgment to
   respect.** The planner flagged a `p0-t2-sdk-bootstrap.md` residual and declined; it was real. It
   then *reasoned explicitly* that P0-T3 and P0-T5 were fine for the same class; round 4 proved P0-T3
   was **not** fine (`scripts/vscode/Invoke-Restore.ps1:32` echoes a resolved Program Files path). Both
   times the decline was wrong. Close it yourself in the next delta rather than letting a later round
   find it.
4. **Ground-truth the empirical claim a defect rests on before commissioning the fix.** Two blocking
   defects here turned on what a command actually prints. `dotnet --list-sdks` printing
   `10.0.400 [C:\Program Files\dotnet\sdk]` and `Invoke-Restore.ps1:32` being
   `Write-Host "Using MSBuild: $msbuildPath"` were each one tool call to confirm, and confirming them
   is what made the delta safe to apply verbatim. See
   [[reconcile-plan-numbers-against-your-own-measurements]].

**The dominant defect class in a plan that commits evidence mid-run** is host-path sanitisation
ordering: a `git add <feature-folder>` in Phase 3 commits every Phase 0 artifact, so a Phase 4 sweep
cannot recover a literal already in a commit. The fix is always to sanitise **at capture time** in the
Phase 0 task that writes the artifact, not to widen a later sweep. Related:
[[multi-location-fact-residuals-drive-preflight-rounds]], [[preflight-catches-vacuous-gates]],
[[never-embed-absolute-host-paths]].
