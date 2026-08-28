# D3 — No Production Behaviour Change, and Collaborator Stability ([P3-T5])

Timestamp: 2026-08-28T05-40

Command:
`git grep -n 'InitializeBreadcrumbPipeline' -- 'QuickFiler/*.cs' 'TaskMaster/*.cs' 'TaskVisualization/*.cs'`;
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs`;
and `git diff 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`
EXIT_CODE: 0

---

## D3 changes no production behaviour

**Statement.** D3's fail-fast guard is unreachable from production code. It changes no user-visible
behaviour, repairs no user-visible symptom, and no reviewer should expect one.

**Reason — the callers are guarded upstream on a null breadcrumb coordinator, in a file this feature
does not own.** A repository-wide search finds exactly **one** production call site:

```
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:150:                viewer.InitializeBreadcrumbPipeline(provider);
```

It sits inside `QfcItemController.EnsureBreadcrumbPipeline`, guarded as follows:

```csharp
            if (viewer.BreadcrumbCoordinator == null)
            {
                var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
                    _globals.Ol.FolderTreeService
                );
                viewer.InitializeBreadcrumbPipeline(provider);
            }
```

The call is made **only when `viewer.BreadcrumbCoordinator` is null**, which is exactly the condition
under which the new guard's enclosing `if (BreadcrumbCoordinator != null)` block is not entered at all.
The throw therefore cannot fire on this path.

This one call site reaches both overloads: it calls the one-argument
`InitializeBreadcrumbPipeline(provider)`, which forwards to the two-argument
`InitializeBreadcrumbPipeline(provider, operations)` where the guard lives. Both are covered by the
same upstream null check. The remaining hits in the search are the two overload declarations
themselves and one comment line, all inside the owned `ItemViewer.Breadcrumb.cs`.

The spec cites this guard at `QfcItemController.ViewerSetup.cs:143`; it resolves at line **145** in the
current tree, with the call at **150**. That is expected pre-change line drift against the citation
anchor, and the guard was resolved by its enclosing member name `EnsureBreadcrumbPipeline` as the
plan's standing rule requires. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is a
**forbidden** file under constraint C1 — it is owned by sibling feature `qfc-item-controller-defects-484`
— and it was read only, never edited.

## No re-initialization branch was added

The delivered guard has exactly two outcomes when `BreadcrumbCoordinator` is non-null: **throw**
`InvalidOperationException` when the supplied provider is not reference-equal to the retained one, or
**return without effect** when it is. There is no third branch that tears down and rebuilds the
pipeline.

That omission is load-bearing, not an economy. Under fail-fast,
`InitializeBreadcrumbPipeline` never constructs a second `BreadcrumbBridgeCoordinator`, so nothing new
ever reaches `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator`'s replacement branch and
the out-of-scope replace-without-dispose defect stays dormant exactly as it is today. Substituting an
explicit re-initialization branch would make that path live and would require pulling that defect into
scope in the same change-set. Constraint C7 forbids the substitution, and no task in this plan made it.

---

## Collaborator stability

### `BreadcrumbBridgeCoordinator.cs` and `BreadcrumbMessengerHub.cs` are unmodified

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs
```

**Output: no lines.** Both files are byte-identical to their state at `BASE_SHA`. Both are forbidden
files under constraint C1.

### `SetBridgeCoordinator` and the bridge-unsubscribe helper are unchanged

`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` **is** modified by this feature, by
D2's retained-theme replay, so a whole-file diff check would be uninformative. The complete set of
changed lines in that file, from `git diff <BASE_SHA>`, is:

```
+        // Issue #488 defect D2: the last theme requested, retained so that a theme set while the
+        // ConfigureHost post is still queued is replayed onto the host that is ultimately adopted.
+        private string? _retainedTheme;
+
+
+                    // Issue #488 defect D2: replay the retained theme onto the newly adopted host.
+                    // Guarded because BreadcrumbDropDownHost.SetTheme rejects null or whitespace.
+                    // The UpdateRequestProviders branch below deliberately performs no theme call:
+                    // that host already holds the theme, and a redundant SetTheme there would be
+                    // observable to the mock-host tests that pin the replacement contract.
+                    string? retained = _retainedTheme;
+                    if (retained != null && !string.IsNullOrWhiteSpace(retained))
+                    {
+                        host.SetTheme(retained);
+                    }
+            _retainedTheme = theme;
```

Every changed line is an **addition** — there is not one deleted line in the file — and all sixteen
belong to the field declaration, the `ConfigureHost` newly-adopted branch, and the `SetTheme`
assignment. **No line of `SetBridgeCoordinator` and no line of the bridge-unsubscribe helper
`UnsubscribeBridge` appears in the diff**, so both members are unchanged from `BASE_SHA`, including
`SetBridgeCoordinator`'s reference-equality guard that D3 mirrors.

Output Summary: D3 **changes no production behaviour**: its only production call site,
`QfcItemController.ViewerSetup.cs:150` inside `EnsureBreadcrumbPipeline`, is guarded upstream on
`viewer.BreadcrumbCoordinator == null` in a 484-owned file this feature does not edit, so the new throw
is unreachable there. **No re-initialization branch was added**, per constraint C7.
`git diff --name-only <BASE_SHA>` over `BreadcrumbBridgeCoordinator.cs` and `BreadcrumbMessengerHub.cs`
produces **no output lines**, and the complete changed-line set of
`BreadcrumbItemViewerLifecycleCoordinator.cs` contains no line of `SetBridgeCoordinator` or
`UnsubscribeBridge`.
