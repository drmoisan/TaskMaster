---
name: project-456-f14-itemviewer-plan-seams
description: Non-obvious seam and constraint facts discovered planning epic #136 child F14 (#456) quickfiler-itemviewer-coverage — ControlHost not on the interface, S1 orphans two usings, spec D5 overrides three research STA proposals, AC9 forbids the dead-member deletion
metadata:
  type: project
---

Planning facts for `quickfiler-itemviewer-coverage` (#456, epic #136 child F14) that are not derivable
from the feature documents alone and cost real verification time.

**1. `IBreadcrumbDropDownHost` does not expose `ControlHost`.** Verified: `IBreadcrumbDropDownHost.cs:19`
declares only `: IDisposable`; `ControlHost` is on the concrete `BreadcrumbDropDownHost.cs:185`. So the
self-referential closure at `ItemViewer.Breadcrumb.cs:164`
(`() => host.ControlHost?.Control.Focus()`) **cannot** be hoisted into an injectable-factory overload —
the `host` local must stay concretely typed inside the default factory. `FocusBreadcrumbCore` and
`() => BreadcrumbCoordinator?.CancelSelector()` **can** be hoisted and passed as `Action` parameters.
Research case C20 is therefore an irreducible residual; C21 is coverable.

**2. Seam S1 (`ControlColumnTrimmer`) orphans two `using` directives in each caller.** After the three
geometry methods leave `ItemViewer.cs` and `ItemViewerExpanded.cs`, `Point`/`Size` (System.Drawing) and
`Any`/`First`/`Where`/`Select` (System.Linq) have **no remaining occurrence** in either file (verified by
grep). Both `using System.Linq;` and `using System.Drawing;` must be removed in the same task set or the
`EnforceCodeStyleInBuild` pass reports them. Pre-existing unused directives (`System.Data`,
`System.Text`) are out of scope.

**3. `spec.md` deviation D5 overrides the research artifacts in three places.** Three separate research
artifacts proposed `*.StaTests.cs` homes — `ItemViewerExpanded.StaTests.cs`,
`ControlColumnTrimmer.StaTests.cs`, and `ItemViewerBreadcrumbGeometry.StaTests.cs` for case C22. D5 and
the Non-Goals prohibit creating the first `*.StaTests.cs` in `QuickFiler.Test`. Plan all three as plain
`[TestClass]`, and record C22 (`Control.RectangleToScreen` / `Screen.FromControl`, both need a real
handle) as a named residual instead.

**4. Deviation D11 must resolve to Option B, not Option A.** AC9 enumerates the permitted production
edits (seam addition, visibility widening, verbatim-duplicate extraction, comment correction, attribute
removal). Member **deletion** is not in that list, so the three unreferenced private members of
`ItemViewer.cs` (`:171-175`, `:177-187`, `:205`) are kept and covered through an `internal`/`internal
static` widening, not deleted. Do not add the missing designer wiring either — the wired path in
`ItemViewerExpanded` is the defective one (#486). **All three need their own widening task.** The first
draft widened only `:171` and `:177` and still wrote a covering test for `MoveOptionsMenu_Click` (`:205`),
which no task made reachable — a blocking preflight finding. `:177` is reachable only from `:171`, so it
is transitively dead too; state that explicitly in the phase preamble.

**5. `BreadcrumbDropDownHost` is constructible in a plain test.** `BreadcrumbDropDownHostTests.cs:312`
does `new BreadcrumbDropDownHost(anchor, environment, initializer, "html", noOp, noOp, noOp)` with
`environment = FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))`. That is what
makes the `ConfigureBreadcrumbDropDown` idempotence **true** arm reachable (`Environment` is a get-only
auto-property assigned at `BreadcrumbDropDownHost.cs:156`).

**6. `ItemViewer.WebViewThread.cs` has a 9-line denominator — seam defaults must be static method
groups.** A lambda field initializer (`= item => item.ShowDropDown();`) puts the never-executed lambda
body on the same source line as the executed initializer, so the line only reads covered if the harness
unions `<class>` elements taking max hits per line. On a 9-line file that is an unacceptable dependency
on an unverified harness property. Use `= ShowMoveOptionsMenuCore;` with a `private static` default and
accept one permanently-uncovered line per seam.

Related: [[project-136-wave1-nonhalting-f1-dependency]],
[[csharp-seam-default-cs0236-and-intermediate-consumers]],
[[planner-mcp-validator-not-in-tool-surface]].
