# P7-T4 — Finding-3 and binding-scope exclusions

Timestamp: 2026-09-04T02-36

Command:

```
git status --porcelain
git diff -U0 origin/main...HEAD -- QuickFiler/Controllers/EfcFormController.cs
git diff --name-only origin/main...HEAD -- .claude .codex .agents config QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/QfcFormController.SetupDisposal.cs QuickFiler/Controllers/QfcCollectionController.cs TaskMaster/Ribbon ":(exclude).claude/agent-memory/**"
```

EXIT_CODE: 0

## `ActionOkAsync` post-change line span

`ActionOkAsync` is declared at `QuickFiler/Controllers/EfcFormController.cs` line **838** and its
closing brace is at line **872**. Post-change span: **838 through 872**.

## Hunk headers of the anchored controller diff

The six hunks the anchored `origin/main...HEAD` diff reports, with their new-side line ranges:

| Hunk header | New-side range | Intersects 838-872? |
|---|---|---|
| `@@ -129 +129,13 @@` | 129-141 | no |
| `@@ -157,0 +170,61 @@` | 170-230 | no |
| `@@ -921 +994,7 @@` | 994-1000 | no |
| `@@ -923,2 +1002,24 @@` | 1002-1025 | no |
| `@@ -929,2 +1030,5 @@` | 1030-1034 | no |
| `@@ -1022 +1126 @@` | 1126 | no |

**No hunk header names a line range intersecting 838 through 872.** Finding 3, the `ActionOkAsync`
disposal reordering, is therefore untouched by this item and remains owned by a sibling item. The
nearest hunk below the span begins at 994 and the nearest above ends at 230, so the span is clear by
607 lines on one side and 122 on the other.

## `.Dispose()` occurrence audit

The file's `.Dispose()` occurrence count is **3**, at lines 227, 869 and 954. P0-T9 recorded a
pre-change count of **2**, so exactly one occurrence was added.

| Line | Enclosing member | Status |
|---|---|---|
| 227 | `ShowModelessFaultNotice` (declared 201, closing brace 229) | **the single added occurrence** |
| 869 | `ActionOkAsync` | pre-existing, untouched — outside every hunk range above |
| 954 | `Cleanup` | pre-existing, untouched — outside every hunk range above |

The single added occurrence at line 227 lies inside `ShowModelessFaultNotice`, within the
`@@ -157,0 +170,61 @@` hunk that adds the whole notifier seam. It is
`notice.FormClosed += (sender, args) => notice.Dispose();` — a **self-disposing notification form**,
which releases the modeless notice when the user closes it. It is **not a disposal-ordering change**:
it introduces no ordering relationship with either pre-existing `.Dispose()` call, both of which sit
in unrelated members that no hunk touches.

## Second anchored name-only diff

```
```

The diff **printed no lines**. It covers `.claude`, `.codex`, `.agents`, `config`, the three named
out-of-scope QuickFiler controller files, and `TaskMaster/Ribbon`, none of which this item changed.

That diff's pathspec carries an explicit `":(exclude).claude/agent-memory/**"` term, **without which
it could not print no lines**: the agent-memory paths this run's earlier agents wrote were committed
to this branch before Phase 0 and are therefore inside `origin/main...HEAD` from the outset, and
P7-T2's `git add -A` adds any written since, so without the exclusion every one of them would be
reported here as a scope violation it is not. The exclusion is exactly co-extensive with the
enumeration P7-T2 records — all 16 paths — so nothing is silently dropped: every agent-memory path
removed from this diff's view is named individually in that artifact.

## `git status --porcelain` span

```
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/other/p7-t2-commit.md
MM docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/plan.2026-09-02T12-02.md
?? docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p7-t3-scope-containment.md
```

**Every line names a path under this feature folder.** No line names a path under `TaskMaster/`,
`TaskMaster.Test/`, `QuickFiler/`, or `QuickFiler.Test/`, and none names a path under
`.claude/agent-memory/`, though such a line would have been permitted.

As in P7-T3, an empty porcelain span is not assertable here: P7-T3's own evidence artifact is
uncommitted when this command runs — the third line above — alongside P7-T2's artifact and this
plan file's checkbox updates. The negative clause is what makes both empty diff results non-vacuous:
it proves that no uncommitted edit to any of the four code trees is hiding a change from the two
anchored, committed-only diffs above, which is the property the empty-span form was standing in for.
The second anchored diff additionally covers `.claude`, `.codex`, `.agents`, `config`, and the four
named out-of-scope source locations, whose emptiness the negative clause does not speak to and which
that diff establishes directly.

Output Summary: `ActionOkAsync` occupies lines 838 through 872 and no hunk of the anchored controller
diff intersects that span, so finding 3 is untouched. The file's `.Dispose()` count is 3 against a
pre-change 2; the single added occurrence is at line 227 inside `ShowModelessFaultNotice`, a
self-disposing notification form rather than a disposal-ordering change. The second anchored
name-only diff printed no lines. Every line of the `git status --porcelain` span names a path under
this feature folder, and none names a path under any of the four code trees.
