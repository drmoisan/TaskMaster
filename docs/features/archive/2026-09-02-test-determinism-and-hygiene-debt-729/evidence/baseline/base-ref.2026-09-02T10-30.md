# Merge-base reference (P0-T14)

Timestamp: 2026-09-03T01-31

Command: `$base = (git merge-base origin/main HEAD).Trim()`

EXIT_CODE: 0

BaseRef: 8be5a6aac3b5a82c86241fbbf989fd9118602c56

HeadAtCapture: 3c1fb73c6df4c99aad05404c96d3281fe5937999
Branch: bug/test-determinism-and-hygiene-debt-729

## Re-anchor record (2026-09-03T02-05)

PriorBaseRef: 687f15fbf164d5aeff044a5ec17de18bc8622b27
CurrentBaseRef: 8be5a6aac3b5a82c86241fbbf989fd9118602c56
Reason: This item's run was interrupted. While it was stopped, sibling parallel item #564
merged to `main` as merge commit `8be5a6aa` (PR #745). The branch was reconciled by merging
`origin/main` into `bug/test-determinism-and-hygiene-debt-729` at resume, before Phase 2
execution continued. That merge moved `git merge-base origin/main HEAD` from the prior value
to the current value.

Command: `git merge-base origin/main HEAD` (re-run after `git fetch origin main` and
`git merge origin/main --no-edit`)
EXIT_CODE: 0

Effect on the plan: D11 requires every `$base`-anchored task to assert `$base` equals the
`BaseRef:` recorded here. The authoritative value for every remaining task is the
`CurrentBaseRef` above. Re-anchoring to the merge commit is required rather than optional:
with the prior value still recorded, `git diff 687f15fb HEAD` would attribute the 20 files
that sibling item #564 delivered to this item's footprint, because those files are reachable
from `HEAD` through the reconciliation merge. Anchoring to `8be5a6aa` yields this item's own
changes only.

Content impact of the reconciliation on this plan's citations: the only non-`docs/`,
non-`artifacts/` file `main` changed in that range is `CLAUDE.md`, and all three changed lines
are prose citations repointed from `.github/workflows/ci.yml` to the split reusable workflow
files `_format-check.yml`, `_build-analyzers.yml`, and `_build-nullable.yml`. No approved
command text changed. Every toolchain command this plan runs (`dotnet tool run csharpier
check .`, the two `msbuild TaskMaster.sln /t:Rebuild` invocations, and the coverage run) is
character-for-character unchanged, so no plan task, decision record, or acceptance condition
is invalidated by the reconciliation.

Output Summary: The merge base of `origin/main` and `HEAD` is the 40-character hexadecimal SHA
recorded in `BaseRef:` above. `origin/main` was merged into this branch twice: once during
Phase 0 (merge commit `3c1fb73c`) and once at resume after sibling item #564 landed on `main`.
Every later `$base`-anchored git acceptance in this plan re-derives `$base` per task, per D11,
and asserts equality against the `BaseRef:` value recorded here before proceeding.
