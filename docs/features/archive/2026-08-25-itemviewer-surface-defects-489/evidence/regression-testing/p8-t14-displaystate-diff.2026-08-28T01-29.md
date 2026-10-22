# P8-T14 — ItemViewer.DisplayState.cs and ItemViewer.Designer.cs diff shapes

Timestamp: 2026-08-28T01-29
Command: git diff --numstat <BASELINE_SHA> -- QuickFiler/Viewers/ItemViewer.DisplayState.cs QuickFiler/Viewers/ItemViewer.Designer.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

```
0	1	QuickFiler/Viewers/ItemViewer.Designer.cs
1	1	QuickFiler/Viewers/ItemViewer.DisplayState.cs
```

`ItemViewer.DisplayState.cs` reports exactly `1` added and `1` deleted; `ItemViewer.Designer.cs`
reports `0` added and `1` deleted. Neither diff introduces a new focus target.

## ItemViewer.DisplayState.cs — the FocusSubject return type only

```
@@ -76,6 +76,6 @@ namespace QuickFiler
             remove => TxtboxBody.DoubleClick -= value;
         }

-        public void FocusSubject() => LblSubject.Focus();
+        public bool FocusSubject() => LblSubject.Focus();
```

One hunk, one changed line, one changed token: `void` became `bool`. The expression body is
byte-identical — the same `LblSubject.Focus()` call against the same control. `Control.Focus()`
already returns `bool`, so no conversion, cast or wrapper was needed and the runtime behaviour of the
call is unchanged; only the declared return type, and therefore what a caller may observe, changed.

`LblSubject` is a `System.Windows.Forms.Label`. `Label` sets `ControlStyles.Selectable = false`, so
`Focus()` returns `false`. Surfacing that `false` is the whole of the fix: it makes the failure
observable without inventing a focus target, which research open item U5 records as not determinable
from source.

## No new focus target, and no selectability change

A filter over the **added** lines of both diffs for `TabStop`, `ControlStyles`, `SetStyle`,
`Selectable`, `.Focus()` and `Select()` returns exactly one line:

```
+        public bool FocusSubject() => LblSubject.Focus();
```

That is the same `LblSubject.Focus()` call that the deleted line already contained, so it is not a
new focus target — it is the pre-existing one carried across an unrelated one-token edit. There is no
added `TabStop`, `ControlStyles`, `SetStyle` or `Selectable` line in either file. The same filter run
over the whole feature diff (`QuickFiler/ QuickFiler.Test/ UtilitiesCS/`) for those three
selectability tokens returns no line at all, so `LblSubject`'s `TabStop`, control styles and
selectability are unchanged everywhere, not merely in these two files.

## ItemViewer.Designer.cs — only the deleted ParentChanged wiring

```
@@ -253,7 +253,6 @@ namespace QuickFiler
             this._l0v2h2_WebView2.Size = new System.Drawing.Size(1144, 358);
             this._l0v2h2_WebView2.TabIndex = 40;
             this._l0v2h2_WebView2.ZoomFactor = 1D;
-            this._l0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);
             //
             // LblConvCt
             //
```

A single deletion and no addition: the `ParentChanged` subscription removed by P4-T4 for issue #487.
The adjacent `TabIndex = 40` assignment is context, not a change — it appears with a leading space,
not a `+` or `-`. This Phase 8 task touches the designer file not at all; the row is carried forward
from Phase 4 and is asserted here to prove no Phase 8 edit leaked into it.

Output Summary: `ItemViewer.DisplayState.cs` reports exactly `1` added and `1` deleted — the single
`void` to `bool` token change on `FocusSubject`, with the `LblSubject.Focus()` expression body
untouched. `ItemViewer.Designer.cs` reports `0` added and `1` deleted — the Phase 4 `ParentChanged`
wiring removal and nothing else. Neither diff introduces a new focus target, and no `TabStop`,
`ControlStyles`, `SetStyle` or `Selectable` line is added anywhere in the feature diff.
