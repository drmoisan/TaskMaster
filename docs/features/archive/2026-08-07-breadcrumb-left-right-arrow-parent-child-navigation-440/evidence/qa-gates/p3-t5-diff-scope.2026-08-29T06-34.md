# Phase 3 — Diff-Scope Gate (issue #440, plan task P3-T5)

Timestamp: 2026-08-29T06-34

`BASE` is `b56400ab663a85b6039139d4548f408821e957ce` throughout. Every span carries an
explicit pathspec per Global rule 9.

Staging command run first:

```
git add -A -- UtilitiesCS UtilitiesCS.Test
```

EXIT_CODE: 0

---

## Span 1 — anchored name-listing diff over the two owned roots

Command: `git diff --name-only b56400ab663a85b6039139d4548f408821e957ce -- UtilitiesCS UtilitiesCS.Test`
EXIT_CODE: 0

Full output:

```
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
```

Exactly the three declared paths and no others. This is the AC-12 file list within
the two owned roots. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is absent, which is
the diff half of AC-10.

---

## Span 2 — porcelain status over the two owned roots

Command: `git status --porcelain -- UtilitiesCS UtilitiesCS.Test`
EXIT_CODE: 0

Full output:

```
M  UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
M  UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
M  UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
```

Exactly three entries. Every entry's two-character status field is `M `, which is one
of the three permitted modification forms (`M `, ` M`, `MM`). No entry's status field
begins with `A`, `?`, or `R`, so no file was created, added, or renamed under either
owned root. Together with span 1 this confirms no new test file was added, which is
the AC-10 requirement.

The status-code form is what makes this discriminating: the preceding `git add -A`
converts any untracked entry into a staged entry, so an assertion phrased on the
absence of untracked markers could not fail.

---

## Span 3 — hunk-level content diff of the production file

Command: `git diff b56400ab663a85b6039139d4548f408821e957ce -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`
EXIT_CODE: 0

Full output:

```diff
diff --git a/UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs b/UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
index 0cb41e25..5f829bfb 100644
--- a/UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
+++ b/UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
@@ -224,15 +224,15 @@ namespace UtilitiesCS.OutlookObjects.Folder
             {
                 return false;
             }
-            // #440: attempt the tree transition first (decision D1 handling order). It selects the
-            // parent of the leaf-anchored node; once a non-leaf node is selected, or while a child
-            // of the open expansion is selected, no parent-select is available and the pre-existing
-            // behavior runs unchanged.
+            // #440: attempt the tree transition first (decision D1 handling order). Each press
+            // selects the parent of the currently active node, so repeated Left walks the ancestor
+            // chain to the root. No index test is needed here: ActivateSegment refuses a negative
+            // index, which is what preserves the root boundary and the pre-existing fall-through.
+            // While a child of the open expansion is selected, no parent-select is available.
             int? activeIndex = row.ActiveSegmentIndex;
             if (
                 _selectedSubfolderIndex < 0
                 && activeIndex.HasValue
-                && activeIndex.Value == row.Chain.Count - 1
                 && row.ActivateSegment(activeIndex.Value - 1)
             )
             {
```

### Count of removed lines whose content is not a comment

The diff removes five lines in total. Four of them begin with `// ` and are the
superseded `#440` explanatory comment block. Exactly **one** removed line is not a
comment:

```
-                && activeIndex.Value == row.Chain.Count - 1
```

That is the single leaf-anchored conjunct AC-2 names for removal. No other
conditional in the method changed: the `if (` opener, the closing `)`, the
`int? activeIndex = row.ActiveSegmentIndex;` assignment, the `return true;` body, the
`_selectedSubfolderIndex >= 0` reset block and the `return row.TryCollapseLeaf();`
tail all appear as unchanged context.

### The `_selectedSubfolderIndex < 0` conjunct is retained as context

In the diff above it is rendered as

```
                 _selectedSubfolderIndex < 0
```

with a **leading space** as its first character, which is the diff context marker.
A deleted line would carry a leading `-`. The conjunct is therefore unchanged, which
is the diff half of AC-3.

---

## Span 4 — repository-wide anchored name-listing diff

Command: `git diff --name-only b56400ab663a85b6039139d4548f408821e957ce -- . ":(exclude)docs" ":(exclude).claude"`
EXIT_CODE: 0

Full output:

```
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
```

Exactly the same three paths as span 1. This is the span that makes the
whole-repository absence claims decidable, because spans 1 to 3 are pathspec-scoped
to `UtilitiesCS` and `UtilitiesCS.Test` and can never list a path outside them:

- **AC-12**: the change touches exactly three repository source files.
- **AC-4**: QuickFiler/Controllers/KeyboardHandler.cs is absent from this list.
- **AC-9**: QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs and
  QuickFiler/Controllers/BreadcrumbBridgeRouter.cs are both absent from this list.
  UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs is likewise absent, and that
  half is also observable in span 1, which covers `UtilitiesCS`.

The two exclusions are the only ones applied and both are load-bearing:
`.claude/agent-memory/` is tracked and can carry unrelated modifications written by
other agents in sibling worktrees, and `docs` carries this feature folder.

Untracked bootstrap and build output cannot make this span fail, because `git diff`
reports only tracked paths.

---

## Span 5 — porcelain status over the two QuickFiler roots

Command: `git status --porcelain -- QuickFiler QuickFiler.Test`
EXIT_CODE: 0

Full output: (empty)

This catches a file created rather than modified under either QuickFiler root, which
a name-listing diff is blind to. The output is empty, so no such file exists.

Ignored paths cannot make this span fail, because porcelain status omits them and
every bootstrap or build path this plan produced is matched by `.gitignore`: the
repository-root `.dotnet-sdk` directory that P0-T5 created is matched by `.dotnet*/`;
the repository-root packages directory that P0-T7 restored and provisioned into is
matched by `**/[Pp]ackages/*`; and the per-project build output directories are
matched by `[Bb]in/` and `[Oo]bj/`.

---

## Verdict

All five spans pass. The change footprint is exactly the three declared source paths
plus this feature folder's own documentation and evidence artifacts.
