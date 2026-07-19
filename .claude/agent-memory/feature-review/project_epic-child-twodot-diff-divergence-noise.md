---
name: epic-child-twodot-diff-divergence-noise
description: reviewing an epic child feature against the integration branch — use three-dot merge-base diff, not two-dot, or sibling-merge divergence shows as spurious adds/deletes
metadata:
  type: project
---

When feature-review scope is an epic child branch (e.g. #307 F2) diffed against the epic
integration branch (`origin/epic/...`), the caller may hand you a two-dot range
`base..HEAD`. If the integration branch tip has advanced with sibling merges (F1/#306, F3/#309,
F4/#310) that the child branch is behind, the two-dot diff surfaces those siblings' doc folders and
files (e.g. `ScoSortedDictionary.cs`) as spurious additions/deletions that are NOT the child's work.

**Why:** two-dot compares tips directly; three-dot compares merge-base→HEAD (standard PR/GitHub
semantics) and cancels content common to both sides.

**How to apply:** compute `git merge-base HEAD origin/epic/<branch>` and audit the three-dot
`origin/epic/<branch>...HEAD` scope. This is the authoritative base per `pr-base-branch-merge-base`
and is NOT a scope narrowing (it is the widest legitimate scope for the branch's own changes) — but
document the resolution in policy-audit §Scope Resolution so the two-dot/three-dot difference is on
record. See [[stale-caller-merge-base]] for the related "recompute the merge-base, don't trust the
supplied SHA" rule.
