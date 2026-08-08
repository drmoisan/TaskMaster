---
name: excludefromcodecoverage-partial-type-trap
description: An [ExcludeFromCodeCoverage] on one partial suppresses EVERY partial of the type, including the generated .Designer.cs — so removing it exposes thousands of unmeasured generated lines in the same edit
metadata:
  type: project
---

`[ExcludeFromCodeCoverage]` on a WinForms partial class applies to the **whole type**, not the file it
appears in. C# merges attributes across partial declarations, so an attribute on `Foo.cs` also
suppresses `Foo.Designer.cs`. There is no way to attribute only one half.

**Why:** Discovered preparing epic child F9 (#452) of `quickfiler-per-file-coverage` (#136).
`EfcViewer.cs:20` carries the attribute; `EfcViewer.Designer.cs` (4,277 lines) carries none, yet is
absent from every Cobertura report. Removing the attribute to satisfy a per-file coverage AC would
have dropped ~2,000 uncovered generated lines into the denominator in the same commit, breaking the
repository-wide "retain or improve" gate. A file inventory cannot see this — the Designer looks
clean.

**How to apply:** Before removing `[ExcludeFromCodeCoverage]` from any `Form`/`UserControl` partial,
count the sibling `*.Designer.cs` lines and treat them as entering the denominator. Two dispositions:

- Construct the type once (unshown, STA, disposed in `finally`) — measured designers in this repo land
  at 99-100% line (`BayesianPerformanceViewer.Designer.cs` 99.14%, `ConfigViewer.Designer.cs` 99.60%,
  `ItemViewerExpanded.Designer.cs` 99.5%). This turns the liability into a coverage *gain*.
- Or add method-level attributes to `InitializeComponent`/`Dispose(bool)` — but there is ZERO
  precedent for that in this repo, and Visual Studio silently drops them on regeneration.

Designer branch rate is ~0.50 by construction regardless of effort: `Dispose(bool)`'s
`disposing && (components != null)` can only go one way because `components` is initialized to `null`
and never reassigned. So a generated file can never pass a 75% branch gate — it needs a
"measured but not gated" ledger bucket, not a `testable` classification.

This affects every Designer-backed child in the epic, not just F9. See
[[cobertura-per-file-rates-corrupted-441]] for the separate measurement hazard.
