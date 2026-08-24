---
name: webview2-endinit-creates-handles-at-construction
description: ItemViewer construction already creates both WebView2 child handles AND the viewer's own handle via Designer ISupportInitialize.EndInit, so any "force the handle" remedy for QuickFiler pump tests is a measured no-op
metadata:
  type: project
---

`QuickFiler.ItemViewer`'s constructor runs `InitializeComponent()`, and
`QuickFiler/Viewers/ItemViewer.Designer.cs` routes `_l0v2h2_WebView2` and `_l0vhBreadcrumb_WebView2`
through the `ISupportInitialize` `BeginInit`/`EndInit` protocol. `EndInit` on the third-party
`Microsoft.Web.WebView2.WinForms.WebView2` control **creates the child window handle**. WinForms
creates a parent's handle whenever a child's handle is created, so **the `ItemViewer`'s own handle
also exists the moment construction returns**.

Measured four ways during issue #511/#571 execution (see
`docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`):
a bare `new QuickFiler.ItemViewer()` on the pump thread — no harness, no `SaveParameters`, no
`.Handle` read — already reports **both** children as `IsHandleCreated == true`.

**Why this matters:** it falsifies the whole premise of "the viewer can reach the act with no window
handle." Any remedy of the shape
`_ = await host.InvokeAsync(() => viewer.Handle).ConfigureAwait(false)` forces a handle that already
exists and changes nothing observable. It also explains why the two named pump tests passed on
20 of 20 pre-fix runs.

**Do not confuse the two failure signatures.** A genuinely missing handle makes `Control.Invoke`
throw `InvalidOperationException` *immediately*. The failures actually observed in #511 are
**60,000 ms `PumpTimeoutMs` expiries under machine load** (reproduced with ~17 idle MSBuild
node-reuse processes present; clearing them restored green). A timeout is a different root cause
from an absent handle, and a handle-forcing change cannot address it.

**How to apply:** before accepting any "force/establish the window handle" plan for the QuickFiler
pump fixtures, measure `IsHandleCreated` on a bare `ItemViewer` first. If it is already `true`, the
plan's premise is dead and the plan needs re-scoping, not execution. Also weigh post-fix green runs
against the pre-fix base rate: at roughly 1 failing run in 21, thirty consecutive green runs has
about a 23% chance of occurring with no fix at all, so it is not evidence of efficacy.
See [[project_winformspumphost_tests_load_flaky]].
