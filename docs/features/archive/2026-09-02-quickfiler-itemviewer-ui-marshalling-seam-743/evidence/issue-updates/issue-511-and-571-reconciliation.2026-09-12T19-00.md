# Issue #511 and #571 reconciliation (P5-T2)

Task: [P5-T2]
Timestamp: 2026-09-13T03-39
Command: `pwsh -Command 'gh --version'` then `pwsh -Command 'gh issue view 511 --repo drmoisan/TaskMaster --comments'`, `pwsh -Command 'gh issue view 571 --repo drmoisan/TaskMaster --comments'`, `gh issue comment 511 --repo drmoisan/TaskMaster --body-file coverage\issue-511-comment.md`, `gh issue comment 571 --repo drmoisan/TaskMaster --body-file coverage\issue-571-comment.md` (each run from the item worktree root via Set-Location inside one pwsh invocation; the two body files were written under the ignored repository-root `coverage` directory so no untracked artifact entered the tree)
EXIT_CODE: 0
Output Summary: `gh --version` printed `gh version 2.87.3 (2026-02-23)` and exited 0, which selects the posting branch; both `gh issue view` commands exited 0; both `gh issue comment` commands exited 0 and printed a comment URL.

## Branch selector

- `gh --version` EXIT_CODE: **0** (the exit code, and nothing else, selected the posting branch)

PostedAs: comment

- Issue #511 comment URL: https://github.com/drmoisan/TaskMaster/issues/511#issuecomment-5652002368
- Issue #571 comment URL: https://github.com/drmoisan/TaskMaster/issues/571#issuecomment-5652002536

## Pre-posting read of the existing comment threads

Both issues carry the same two closing comments dated 2026-08-22 ("Premise correction from the epic execution run" and "Closing as superseded by #592"). Both cite the Designer `EndInit()` pair as `:6166-6167`; the current tree carries the pair at lines 6165 and 6166 of `QuickFiler/Viewers/ItemViewer.Designer.cs` (line 6169 is the `_topicThread` `EndInit()`), and `BeginInit()` at lines 89-90 as the comments state. Both closing comments restate the `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` hypothesis as the lead carried to #592.

## Element coverage

- (a) confirms the premise correction refuting the window-handle cause still holds, quotes it, and does not claim it was in error; cites Designer lines 6165 and 6166 with the off-by-one correction called out.
- (b) replaces the stale forward pointer to #592 with #743 and its resolution.
- (c) marks the `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` hypothesis as superseded by the H-COST mechanism identified in the P1-T11 artifact, citing correction C1.

## Exact comment text posted on issue #511

```
## Reconciliation from issue #743 (2026-09-13)

This comment reconciles the closing state of #511 against the current tree and the resolution delivered under #743.

### (a) The premise correction stands

The premise correction recorded above on 2026-08-22, which refutes the window-handle root cause, still holds against the current tree and was not in error. Quoting it: "`EndInit` creates the WebView2 child window handles, and WinForms creates a parent's handle when a child's handle is created. The `ItemViewer`'s own handle therefore exists the instant construction returns." Re-verified on branch `bug/quickfiler-itemviewer-ui-marshalling-seam-743`: `ItemViewer()` calls `InitializeComponent()` at `QuickFiler/Viewers/ItemViewer.cs:25`; `InitializeComponent` runs `BeginInit()` on both WebView2 children at `QuickFiler/Viewers/ItemViewer.Designer.cs:89-90` and `EndInit()` on them at `QuickFiler/Viewers/ItemViewer.Designer.cs:6165` and `:6166`. One citation correction only: the two earlier comments cite the `EndInit()` pair as lines 6166-6167; in the current tree the pair sits at lines 6165 and 6166 (line 6169 is the `_topicThread` `EndInit()`). The window-handle root cause remains refuted, and forcing the handle remains a measured no-op.

### (b) Forward pointer

The forward pointer to #592 is stale: #592 is closed and consolidated into **#743** (`2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743`). #743 carries the resolution: `QfcItemController.ResolveControlGroupsAsync` is widened from the concrete `ItemViewer` to `IItemViewer` through two additive interface members (`DescendantControls()` and `ItemNumberLabel`), and the `AssignControlsAsync` marshal is routed through the injected `IUiDispatcher` seam with the same null tolerance the other seam sites carry. The member is therefore driven from a deterministic seam test class, `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs`, with no pump host and no concrete viewer (fail-before three of three runs with `InvalidCastException`, pass-after three of three, then 62 consecutive targeted runs with zero failures). The retained pump-hosted test `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` is unchanged. Evidence lives under `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/` on that branch.

### (c) The gate hypothesis is superseded

The `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` hypothesis restated in the closing comment above is superseded. Per correction C1 in the #743 spec, those two identifiers exist in zero `.cs` files in the current tree (they were removed under #493; the mechanism that exists today is `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction`). The mechanism was then identified by measurement rather than inference (AC1 verdict artifact `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`): three monotonic counters on the one-permit `TransactionGate` recorded, in a serial-regime run of the whole `QuickFiler.Test` assembly, `acquisitions=11 releases=10 contended=0`, with a balance test holding the permit and asserting acquisitions minus releases equals exactly 1. A serial run cannot queue a second live holder, so a contended count of zero rejects the gate-leak hypothesis (H-LEAK) by direct observation. The surviving mechanism is elapsed pump-hosted fixture cost under load (H-COST): the six `ThroughThePumpHost` tests measured 68-125 ms serially and up to 6,460 ms under class-level parallelism alone, and the recorded 6x-26x load multiplier applied to the latter exceeds the 60,000 ms `PumpTimeoutMs` bound. No expiry was reproduced during the instrumented runs; that is recorded as a negative result, and the identification rests on the counter observable rather than on an observed expiry.
```

## Exact comment text posted on issue #571

Identical to the #511 text except for two issue-specific phrases: the opening sentence reads "reconciles the closing state of #571", and element (b) reads "The retained pump-hosted test `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`, which #571 exists to stabilize, is unchanged." The full text follows.

```
## Reconciliation from issue #743 (2026-09-13)

This comment reconciles the closing state of #571 against the current tree and the resolution delivered under #743.

### (a) The premise correction stands

The premise correction recorded above on 2026-08-22, which refutes the window-handle root cause, still holds against the current tree and was not in error. Quoting it: "`EndInit` creates the WebView2 child window handles, and WinForms creates a parent's handle when a child's handle is created. The `ItemViewer`'s own handle therefore exists the instant construction returns." Re-verified on branch `bug/quickfiler-itemviewer-ui-marshalling-seam-743`: `ItemViewer()` calls `InitializeComponent()` at `QuickFiler/Viewers/ItemViewer.cs:25`; `InitializeComponent` runs `BeginInit()` on both WebView2 children at `QuickFiler/Viewers/ItemViewer.Designer.cs:89-90` and `EndInit()` on them at `QuickFiler/Viewers/ItemViewer.Designer.cs:6165` and `:6166`. One citation correction only: the two earlier comments cite the `EndInit()` pair as lines 6166-6167; in the current tree the pair sits at lines 6165 and 6166 (line 6169 is the `_topicThread` `EndInit()`). The window-handle root cause remains refuted, and forcing the handle remains a measured no-op.

### (b) Forward pointer

The forward pointer to #592 is stale: #592 is closed and consolidated into **#743** (`2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743`). #743 carries the resolution: `QfcItemController.ResolveControlGroupsAsync` is widened from the concrete `ItemViewer` to `IItemViewer` through two additive interface members (`DescendantControls()` and `ItemNumberLabel`), and the `AssignControlsAsync` marshal is routed through the injected `IUiDispatcher` seam with the same null tolerance the other seam sites carry. The member is therefore driven from a deterministic seam test class, `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs`, with no pump host and no concrete viewer (fail-before three of three runs with `InvalidCastException`, pass-after three of three, then 62 consecutive targeted runs with zero failures). The retained pump-hosted test `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`, which #571 exists to stabilize, is unchanged. Evidence lives under `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/` on that branch.

### (c) The gate hypothesis is superseded

The `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` hypothesis restated in the closing comment above is superseded. Per correction C1 in the #743 spec, those two identifiers exist in zero `.cs` files in the current tree (they were removed under #493; the mechanism that exists today is `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction`). The mechanism was then identified by measurement rather than inference (AC1 verdict artifact `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`): three monotonic counters on the one-permit `TransactionGate` recorded, in a serial-regime run of the whole `QuickFiler.Test` assembly, `acquisitions=11 releases=10 contended=0`, with a balance test holding the permit and asserting acquisitions minus releases equals exactly 1. A serial run cannot queue a second live holder, so a contended count of zero rejects the gate-leak hypothesis (H-LEAK) by direct observation. The surviving mechanism is elapsed pump-hosted fixture cost under load (H-COST): the six `ThroughThePumpHost` tests measured 68-125 ms serially and up to 6,460 ms under class-level parallelism alone, and the recorded 6x-26x load multiplier applied to the latter exceeds the 60,000 ms `PumpTimeoutMs` bound. No expiry was reproduced during the instrumented runs; that is recorded as a negative result, and the identification rests on the counter observable rather than on an observed expiry.
```
