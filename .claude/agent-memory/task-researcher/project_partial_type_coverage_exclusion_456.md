---
name: partial-type-coverage-exclusion-456
description: "Epic #136/F14 (#456): type-level [ExcludeFromCodeCoverage] hides ALL partials incl. Designer (proven by QfcFormViewer positive control); method-level does not (issue #457); Cobertura <line> nodes expand per physical source line, so a 6,224-line designer adds ~6,000 near-fully-covered lines"
metadata:
  type: project
---

Three findings established 2026-08-07 while researching `QuickFiler/Viewers/ItemViewer.cs` and
`ItemViewer.Designer.cs` for epic child F14 (issue #456) of epic #136.

**1. Type-level `[ExcludeFromCodeCoverage]` on one partial hides every partial, including `.Designer.cs`
and nested compiler-generated closure types. Method-level does not (open issue #457).**
Positive control that settles it without running anything: `QfcFormViewer.cs:17` is attributed,
`QfcFormViewer.Designer.cs:3` is not, `QfcFormViewer.Designer.cs:42` is *provably executed* (it is the sole
construction site of `ItemViewerExpanded`, whose designer shows `hits="1"`), yet neither file produces a
`<class>` element in the committed Cobertura. Executed-but-absent is conclusive. Docs corroborate:
`AllowMultiple = false`, and "Placing this attribute on a class... excludes all the members of that class".
Negative controls in the same folder: `ToolStripMenuItemCb` and `BayesianPerformanceViewer` carry no
attribute and both their partials appear.

**Why:** the whole F14 plan (and any future de-exemption of a WinForms partial family) hinges on this, and
it is cheap to get wrong in either direction — the CS0579 `AllowMultiple = false` rule also means a
Designer partial can never be exempted independently once the hand-written partial's attribute is removed.

**How to apply:** when a de-exemption is proposed for a partial type, use the executed-but-absent positive
control pattern rather than arguing from docs alone. After removing a type attribute, never re-add
method-level exclusions — #457 shows hoisted lambdas leak back into the denominator.

**2. This repo's Cobertura emits one `<line>` per physical source line spanned by a coverage block, not one
per statement.** Verified on multi-line `AddRange(new T[]{...})` statements. Validated line-count model
(±2 lines on an 821-line designer): `coverable ≈ physical − blank − comment − field-decls-without-initialiser
− ~10 structural`. Consequence: a 6,224-line generated designer is ~6,000 coverable lines, ~86% of which is
six inlined `byte[]` SVG payloads, and it reaches ~99.9% line coverage from a single control construction.
So **de-exempting a big WinForms designer usually IMPROVES repo-wide coverage; exempting it is what hurts.**
Do not assume "a 6,000-line designer will enter the denominator at 0%".

**3. Class-level `line-rate` attributes are unusable** — one element declared `0.995098` (=203/204) against
612 enumerated `<line>` children with 3 uncovered. Related to open issue #441, whose root cause is verified
at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (`.//class` then `.//lines/line`; the
descendant axis double-counts by matching both the class-level and per-method line lists). Derive counts
from `<line>` children, or from XML-span arithmetic when the block is too large to read.

See also [[quickfiler-percoverage-epic-136]], [[quickfiler-interface-only-files-433]].
