# Callers of ResolveControlGroupsAsync (P1-T3)

Task: [P1-T3]
Timestamp: 2026-09-13T02-39
Command: `pwsh -Command 'Select-String -Path (Get-ChildItem -Recurse -Filter *.cs -Path QuickFiler,QuickFiler.Test | Select-Object -ExpandProperty FullName) -SimpleMatch -Pattern "ResolveControlGroupsAsync" | ForEach-Object { $_.Path + ":" + $_.LineNumber + ": " + $_.Line.Trim() }'` Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical).
EXIT_CODE: 0
Output Summary: six matching lines. The command prints absolute paths; the worktree-root prefix is replaced by `<worktree>\` below per the evidence-hygiene rule and nothing else is altered.

```
<worktree>\QuickFiler\Controllers\QfcItemController.Initialization.cs:216: await ResolveControlGroupsAsync((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
<worktree>\QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:275: // QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_*.
<worktree>\QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:276: internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)
<worktree>\QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs:417: /// #230 (de-exempted): <c>ResolveControlGroupsAsync(ItemViewer)</c> is the pure pump case -
<worktree>\QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs:426: public async Task ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups()
<worktree>\QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs:448: await controller.ResolveControlGroupsAsync(viewer).ConfigureAwait(false);
```

Classification:

- Declaration: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` line 276 — exactly one.
- Production invocation: Initialization controller partial line 216 — exactly one. The argument is `(ItemViewer)_itemViewer`, an expression already typed as the concrete viewer.
- Test invocation: ViewerSetup test file line 448 — exactly one. The argument `viewer` is declared at line 432 of that file as `QuickFiler.ItemViewer viewer = await host.InvokeAsync(() => new QuickFiler.ItemViewer())`, an expression already typed as the concrete viewer.
- The remaining three lines (ViewerSetup.cs 275, ViewerSetupTests.cs 417 and 426) are a comment, a doc comment and a test-method name; none is an invocation.

Source-compatibility statement: both invocations pass an expression already typed as the concrete viewer `ItemViewer`, which implements `IItemViewer`; widening the parameter to `IItemViewer` therefore compiles both call sites unchanged (implicit reference conversion from the concrete type to the interface). The explicit cast at Initialization.cs line 216 remains valid and becomes redundant rather than erroneous. Neither the Initialization controller partial (497 lines) nor the ViewerSetup test file (498 lines) needs to be edited.
