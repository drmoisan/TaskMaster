# P2-T13 — AC-6 Verified (Scope Boundary)

Timestamp: 2026-09-01T14-52

Command:
```
git rev-parse issue-648-diff-anchor
git diff --name-only issue-648-diff-anchor
git diff --name-only issue-648-diff-anchor -- UtilitiesCS.Test UtilitiesCS
```
(all run from the checkout root)

EXIT_CODE: 0

Output Summary:

## Anchor confirmation

`git rev-parse issue-648-diff-anchor` printed `c7b4f08f6d80296840f9a351042cb2113892e95f`, which is the
hash recorded in the `DiffAnchor:` field of
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t3-fetch-base.md`.
The anchor has not moved since P0-T3 created it.

Anchoring on the merge base rather than on the ref `origin/main` still satisfies AC-6's phrase "the
branch diff against `origin/main`" at
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/issue.md:141`, because
the anchor is this branch's merge base with `origin/main` and the diff from it is exactly the set of
changes this branch made. In this execution `origin/main` was merged into the branch before any plan
task ran, so `origin/main` is an ancestor of HEAD and the merge base equals the current `origin/main`
tip; the two-dot diff against the anchor is therefore identical to the three-dot diff against
`origin/main`.

## First command — complete list

`git diff --name-only issue-648-diff-anchor` listed 55 paths. Filtering that list to paths ending in
`.cs` returns a count of **1**, and that one path is:

```
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```

The remaining 54 paths break down as:

- **42 paths beneath `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/`**
  — this feature's `issue.md`, its `plan.2026-08-31T20-07.md`, its research artifact, and the 39
  evidence artifacts this plan authored. None ends in `.cs`; two end in `.xml` (the two copied
  Cobertura documents) and the rest in `.md`.
- **11 paths beneath `.claude/agent-memory/`**, all ending in `.md`. Ten of the eleven were already
  committed to this branch before this plan's execution began and are part of the branch's inherited
  history rather than of this plan's footprint. The eleventh,
  `.claude/agent-memory/orchestrator/orchestrator-state-json-is-tracked-in-git.md`, is an uncommitted
  worktree modification made by a concurrent agent writing into this same worktree;
  `git status --porcelain -- .claude/agent-memory` reports it as ` M`. It was **not** staged by
  P2-T12, whose pathspec covers only `QuickFiler.Test` and this feature folder, and it will not be
  committed by P2-T18, whose pathspec is the same. None of the eleven ends in `.cs`, so none affects
  the AC-6 condition.
- **1 path**, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, the single changed `.cs` path.

## Second command — output

`git diff --name-only issue-648-diff-anchor -- UtilitiesCS.Test UtilitiesCS` printed no lines. Its
output is **empty**, and its exit code was 0.

## Acceptance conditions

1. **Exactly one path in the first list ends with `.cs`, and it is
   `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`.** The filtered count is 1 and the single
   path is that one.
2. **The second command's output is empty.** It is.
3. **None of the three out-of-scope mutators appears in either list.** Confirmed: neither
   `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, nor
   `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, nor
   `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` appears in the first list, and the second list
   is empty. This is consistent with the P0-T17 baseline, in which both commands were already empty
   beneath those three trees.

AC-6 holds: the branch diff changes exactly one path with a `.cs` extension,
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, and changes no path beneath `UtilitiesCS.Test/`
or `UtilitiesCS/`.
