# [P14-T10] Scope-lock audit (AC-21)

Timestamp: 2026-08-26T16-37

Command:

```
git diff --name-only 61edc19b..HEAD                       # <MERGE_BASE>..HEAD, the mandated gate
git diff --name-only 61edc19b 48c9ad8f                    # this feature's own contribution
git show --name-only --format='' 5f8026aa                 # the one feature commit after the merges
git diff 61edc19b 48c9ad8f -- QuickFiler.Test/QuickFiler.Test.csproj
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**The out-of-scope set for this feature's own contribution is empty.** Every path this feature wrote
is a member of the owned file set, lives under `docs/`, or lives under `.claude/agent-memory/`.

The three files named as must-not-write — `QuickFiler/Controllers/KbdActions.cs`,
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, and
`QuickFiler/Controllers/EfcFormController.cs` — appear **zero** times, not only in this feature's own
contribution but in the entire `61edc19b..HEAD` range including the sibling merges.

The mandated `<MERGE_BASE>..HEAD` diff is 510 paths, of which 457 are under `docs/` and 4 under
`.claude/agent-memory/`, leaving 49 code paths. **Only 10 of those 49 were written by this feature.**
The other 39 arrived through two merges of `origin/epic/quickfiler-bug-family-integration` and belong
to sibling epic children. The reconciliation is set out in full below, because a bare reading of the
mandated diff would otherwise attribute sibling work to this branch.

---

## 1. This feature's own contribution

`git diff --name-only 61edc19b 48c9ad8f` — the range from the merge base to the last commit made
before the first integration merge — yields 141 paths: 130 under `docs/`, 1 under
`.claude/agent-memory/`, and 10 code paths.

Commit `5f8026aa`, the one feature commit made after the merges, adds 5 paths, all under
`docs/features/active/qfc-collection-controller-defects-468/`.

### The 10 code paths, classified

| Path | Classification |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | **owned** — first entry of `### Files this feature owns` |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | **owned** — second entry |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **owned** — listed as existing |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | **owned** — listed as existing |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` | **owned** — new file matching `QuickFiler.Test/Controllers/QfcCollectionController*` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | **owned** — same pattern |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | **owned** — same pattern |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | **owned** — same pattern |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` | **owned** — same pattern |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **owned, conditionally** — permitted only to add `Compile Include` entries |

**Out-of-scope set: empty.**

Two owned files were declared but never written, which is permitted:
`QuickFiler/Controllers/IQfcFormController.cs` and `QuickFiler/Interfaces/IFilerFormController.cs`.
Both were owned defensively; option (a) for #474 defect 1 was rejected, so neither needed an edit.

### The csproj change is `Compile Include` entries only

`git diff 61edc19b 48c9ad8f -- QuickFiler.Test/QuickFiler.Test.csproj` is a single hunk of five added
lines and zero removed lines:

```
     <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
+    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
     <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

The five entries sit immediately after the `QfcCollectionControllerDarkModeTests.cs` entry and
immediately before the `QfcDatamodelTests.cs` entry, which is the exact insertion point decision D13
specifies, and they appear in the D12 order. No `PackageReference`, no `Reference`, no `Analyzer`
element, and no property was touched.

### The one `.claude/agent-memory/` path

`.claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` was carried into `48c9ad8f`. It
is agent memory, not product code, and the P14-T10 classification admits it explicitly.

---

## 2. The mandated `<MERGE_BASE>..HEAD` diff, and why it is larger

`git diff --name-only 61edc19b..HEAD` returns 510 paths.

| Bucket | Count |
|---|---|
| `docs/**` | 457 |
| `.claude/agent-memory/**` | 4 |
| code paths | 49 |
| **total** | **510** |

Of the 49 code paths, 10 are this feature's (listed above). The remaining **39** entered through two
merge commits:

- `7f0e7a2b` — merge of `origin/epic/quickfiler-bug-family-integration`
- `ef907908` — merge of `origin/epic/quickfiler-bug-family-integration`

Those merges were performed by the orchestrator to bring this branch level with the epic integration
branch before the final QA loop, so that the QA loop runs against the tree that will actually be
reviewed and merged. `<MERGE_BASE>` remains pinned at `61edc19b` per P0-T10, so the mandated diff now
spans both this feature's work and everything the integration branch accumulated in the meantime.

### The 39 sibling-derived code paths

Sibling feature `bug/breadcrumb-router-navigation-defects-498` (merged at `8c8f7695`, PR #626) and
sibling feature `bug/quickfiler-bug-family-446` (merged at `902e5ce2`, PR #625):

```
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs
QuickFiler.Test/Controllers/QfcDatamodelTests.cs
QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs
QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
QuickFiler/Controllers/QfcDatamodel.cs
QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
QuickFiler/Controllers/QfcFormController.Actions.cs
QuickFiler/Controllers/QfcHomeController.Iteration.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs
QuickFiler/Interfaces/IQfcDatamodel.cs
QuickFiler/QuickFiler.csproj
QuickFiler/Resources/FolderBreadcrumb.html
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs
UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
UtilitiesCS/UtilitiesCS.csproj
```

Thirty-nine paths. Two of them —
`QuickFiler.Test/QuickFiler.Test.csproj` shared entries and `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` —
sit adjacent to this feature's own edits in the csproj item group, which is why decision D13 pinned an
exact contiguous insertion point: it minimises the merge-conflict surface with exactly these siblings.
No conflict occurred; both merges were clean.

**These 39 paths are not attributable to this feature.** They are already reviewed and merged work on
the integration branch. This audit records them so the mandated diff is reproducible and so no reader
mistakes them for scope creep here.

---

## 3. The three must-not-write files

`git diff --name-only 61edc19b..HEAD | grep -x <path>` for each:

| Path | Occurrences in the full `<MERGE_BASE>..HEAD` diff |
|---|---|
| `QuickFiler/Controllers/KbdActions.cs` | **0** |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | **0** |
| `QuickFiler/Controllers/EfcFormController.cs` | **0** |

None appears — not in this feature's contribution, and not in the sibling-derived set either. The
scope lock held on the strictest possible reading.

This matters for three specific decisions:

- **D2** — removing `WireUpKeyboardHandler` deletes a caller and deletes zero lines in
  `KbdActions.cs`. The zero above is the measurement that confirms it.
- **D11** — `#469` defect 4 keeps the `stackMovedItems` parameter precisely so that
  `QfcFormController.EventHandlers.cs:225` need not change.
- **#474 defect 1 option (a)** — adding `SkipGroupAsync` to `IFilerFormController` was rejected
  because it would have forced an edit to `EfcFormController.cs`.

---

## Acceptance verification

- The artifact exists.
- The verbatim path list is recorded: this feature's 10 code paths in full, the 39 sibling-derived
  code paths in full, and the bucket counts for all 510 paths in the mandated diff.
- **The out-of-scope set is empty** for every path this feature wrote.
- `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
  and `QuickFiler/Controllers/EfcFormController.cs` do not appear.
