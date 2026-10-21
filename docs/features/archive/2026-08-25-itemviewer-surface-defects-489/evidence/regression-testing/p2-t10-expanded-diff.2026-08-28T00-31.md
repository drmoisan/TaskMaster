# P2-T10 — Cumulative ItemViewerExpanded diffs after Phase 2

Timestamp: 2026-08-28T00-31
Command: git diff --numstat cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Viewers/ItemViewerExpanded.cs QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
EXIT_CODE: 0
ExpectedExitCode: 0

```
0	4	QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
0	22	QuickFiler/Viewers/ItemViewerExpanded.cs
```

`git diff --stat` over the same pathspec: `2 files changed, 26 deletions(-)` — and no insertions.

## Acceptance

`0` added for **both** files. Every change Phase 2 made to the `ItemViewerExpanded` pair is a
deletion; not one line was inserted or rewritten in place, which is the strongest available form of
the "no wholesale reformat" guarantee.

## Composition of the 26 deleted lines

`ItemViewerExpanded.cs` — 22 deleted:

- **4** at P2-T1: the constructor calls `MenuItem_CheckedChanged(this.ConversationMenuItem);`,
  `…(this.SaveAttachmentsMenuItem);`, `…(this.SaveEmailMenuItem);` and
  `…(this.SavePicturesMenuItem);`. The constructor now retains exactly `InitializeComponent();`,
  `_context = SynchronizationContext.Current;`,
  `_uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();` and `InitControlGroups();`.
- **18** at P2-T2: both `MenuItem_CheckedChanged` overloads with their blank separator lines — the
  `(object sender, EventArgs e)` form and the typed `(ToolStripMenuItem menuItem)` form whose
  `else` branch was the `menuItem.Image = null;` that cleared the image the
  `ToolStripMenuItemCb.Checked` setter had just assigned.

`ItemViewerExpanded.Designer.cs` — 4 deleted, all at P2-T3: the four
`CheckedChanged += new System.EventHandler(this.MenuItem_CheckedChanged);` wiring statements for
`ConversationMenuItem`, `SaveAttachmentsMenuItem`, `SaveEmailMenuItem` and `SavePicturesMenuItem`,
at the baseline lines `:171`, `:180`, `:189` and `:198`.

## The generated file was not reformatted

`ItemViewerExpanded.Designer.cs` is 821 lines at baseline, already far above the 500-line ceiling,
which P0-T15 recorded as pre-existing out-of-scope finding O5. Its diff is exactly `0 4`, so the
6-plus-hundred untouched lines are byte-identical. This is the outcome the P0-T10 U1 gate exists to
protect: CSharpier 1.2.6 skips `*.Designer.cs` through its generated-file detection — proved there
by a `Checked 0 files` result on this exact file — so the repo-wide `format .` invocation Phase 11
runs cannot re-wrap the 110-column line at `:274` or any other line here.

`ItemViewerExpanded.cs` was formatted explicitly after the P2-T2 deletion and CSharpier's only
change was to collapse the blank line the deletion left before the class-closing brace, which is
counted inside the 22 and still yields `0` added.

## Intra-file shift, restated for the phases that follow

`ItemViewerExpanded.cs` lost 4 lines at P2-T1 from `:24-27`, so every citation into this file from
P2-T2 onward is four lower than the research document prints. P4-T3 locates its
`L0v2h2_WebView2_ParentChanged` deletion by matching the quoted member signature, not by a line
number, so the shift is recorded rather than compensated. `ItemViewerExpanded.Designer.cs` lost 4
lines at P2-T3, and P4-T4 already prints its `:274` as the pre-P2-T3 number and anchors on the
quoted `+=` statement text.

Output Summary: Both `ItemViewerExpanded` files report **`0` added**. The cumulative Phase 2 diff is
`0 4` for `ItemViewerExpanded.Designer.cs` and `0 22` for `ItemViewerExpanded.cs` — 26 deletions,
zero insertions across the pair. The 22 comprise the four redundant constructor calls from P2-T1 and
both `MenuItem_CheckedChanged` overloads from P2-T2; the 4 are the designer `+=` wirings from P2-T3.
The 821-line generated designer file was not reformatted, which is the guarantee the P0-T10 U1
answer was captured to secure.
