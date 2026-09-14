# P0-T2 — Diff anchor and pre-existing worktree paths

Timestamp: 2026-09-13T14-50
ReAnchoredAt: 2026-09-13T14-50
ReAnchorReason: merge commit 8213826f brought origin/main into this branch, superseding the pre-merge
anchor. The pre-merge BASE_SHA named a commit that is no longer this branch's tip, so every anchored
diff in this plan would have reported the merged-in upstream commits as though this item had produced
them. The anchor is therefore re-derived against the post-merge tip.
Command: git rev-parse HEAD ; git status --porcelain --untracked-files=all
EXIT_CODE: 0

BASE_SHA: 8213826f695439e86e3ed34faa575de493a11ec7
SupersededBaseSha: d44b51932c6094dd54dc60e7c0f8d5f14d9e088d

## Why the anchor moved

The superseded anchor was recorded at 2026-09-13T04-50 against the pre-merge branch tip. Between that
capture and this one, the fix for issue 877 (pull request 880) was merged into this branch as merge
commit 8213826f695439e86e3ed34faa575de493a11ec7, whose first parent is the superseded anchor and whose
second parent is origin/main at a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5. That upstream change installs
a process-wide `AssemblyResolve` fallback in the QuickFiler test assembly's own assembly initializer via
the new shared TestSupport/TestAssemblyResolver.cs, which is the blocker that stopped this item at
P0-T12.

Had the superseded sha been retained, every anchored diff in this plan would have enumerated the 91
files the merge brought in as changes of this item, and the Scope-lock rule would have reported each of
them as a scope-lock failure. Re-anchoring to the merge commit is what keeps the anchored diffs scoped
to the work this item performs.

BASE_SHA verified by `git rev-parse HEAD` in this worktree at the time of this capture. The value is a
40-character hexadecimal sha.

## Capture ordering

The porcelain span below was executed as the first git action of this resumed run, before any file was
created or edited in this session. The Phase 0 artifacts produced by the earlier run are already
committed, so they do not appear in it.

## PreExistingWorktreePaths:

```
```

The porcelain span produced no output. The worktree carried no modified, staged, renamed or untracked
path at the re-derived anchor, and therefore the second carve-out of the Scope-lock rule contributes an
empty set for the remainder of this plan. This is unchanged from the superseded record, which also
measured an empty set.

## Consequence for the Scope-lock rule

The plan preamble anticipated that this worktree might carry promotion-lifecycle residuals — a staged
rename of a potential entry into the promoted subdirectory, a staged addition under the potential
features directory, and modified and untracked files under the tracked agent-memory directory. None of
those are present here. The measured anchor set is empty. The Scope-lock rule is therefore applied for
the remainder of this plan with its `PreExistingWorktreePaths:` clause contributing nothing, which makes
the rule strictly narrower than the plan assumed and admits no path that the plan did not already admit
through the Write Set, the three evidence directories or the tracked agent-memory directory of this
worktree.

## Structural citations re-verified against the post-merge tree

Each of the following was re-measured in this worktree after the merge and is unmoved from the value the
plan cites:

- QuickFiler/Controllers/QfcQueue.cs measures 507 physical lines.
- QuickFiler/Controllers/QfcQueue.Enqueue.cs measures 200 physical lines.
- The Tlp Manipulation region opens at line 230 and closes at line 453 of QuickFiler/Controllers/QfcQueue.cs.
- The Helper Methods region opens at line 472 and closes at line 505 of the same file.
- The two QfcQueue Compile items in QuickFiler/QuickFiler.csproj sit at lines 348 and 349.
- The Interfaces Compile block in that project begins at line 363, where the first item names the
  move-monitor interface file.
- The three QfcQueue test Compile items in QuickFiler.Test/QuickFiler.Test.csproj sit at lines 119, 120 and 215.

Output Summary: BASE_SHA re-anchored to 8213826f695439e86e3ed34faa575de493a11ec7, a 40-character
hexadecimal sha, superseding d44b51932c6094dd54dc60e7c0f8d5f14d9e088d. The porcelain status was empty,
so `PreExistingWorktreePaths:` is an empty block and the anchor carve-out of the Scope-lock rule remains
empty. Every structural citation the plan makes was re-measured against the post-merge tree and none
moved. Acceptance met.
