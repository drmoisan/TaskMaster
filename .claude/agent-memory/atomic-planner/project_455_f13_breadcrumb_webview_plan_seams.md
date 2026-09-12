---
name: project-455-f13-breadcrumb-webview-plan-seams
description: epic #136 child F13 (#455) planning seams — separate-type (never partial) exemption extraction, class-vs-method-level attribute lambda leak, 8-of-11 files already passing, preparation-mode repo-relative paths
metadata:
  type: project
---

Planning facts for `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455` (epic #136 child F13, 15 files under `QuickFiler/Viewers/`). Not derivable from the code alone.

- **Eight of eleven production files already clear both gates** (>= 80% line, >= 75% branch). Lowest branch is 85.71% (`BreadcrumbCollapsedSurfaceController.cs`). Planning them as under-covered is wrong; the bar is retain-or-improve plus a bounded named-outcome list. Real work sits in exactly three places: the `BreadcrumbPopupUiOperations.cs` exemption restructure, de-exempting `WebView2Messenger.cs` + `WebView2BreadcrumbHost.cs` (both start at zero, absent from the report entirely), and ~12 named residual branch outcomes.
- **`[ExcludeFromCodeCoverage]` on a *method* does not suppress lambdas lifted out of it; on a *type* it does.** Measured both directions in one Cobertura report. Consequence: extracting exempt forwarders must produce a **separate type**, never a `partial` of the covered class — an attribute on one partial declaration applies to the whole type and would silently exempt all 234 covered lines (Blocking under `epic.md:223`).
- `<class>` `line-rate`/`branch-rate` attributes are inflated (issue #441) by per-method duplicate `<line>` blocks. Key on `filename=`, sum deduplicated class-level `<line>` children with `max(hits)`. Three F13 files report under a `<class>` name that differs from their filename, so a name-keyed harness loses them.
- **Preparation mode.** This plan is authored in one worktree and executed later by `epic-orchestrator` in a different one: every path must be repo-relative, and the F1 (#432) ledger gate must be worded as an execution-time read (see [[project-136-wave1-nonhalting-f1-dependency]]).
- Determinism vehicle is **scheduler control, not clock control** — manually-pumped fake `SynchronizationContext` with explicit `Drain()`. There is no `DateTime`/`Stopwatch`/`Timer`/`TimeProvider` anywhere in these files, so an injected clock or fake-timer task is out of scope and must be rejected. No STA anywhere; `BreadcrumbPopupControlDispatchTests.cs` already builds `Panel`/`ToolStripDropDown`/`ToolStripControlHost` in a plain `[TestClass]`.
- Chaining `WebView2Messenger`'s internal ctor naively flips argument evaluation order and would report `"dispatcher"` instead of `"coreWebView"` for a both-null call. One dedicated regression task is required.
- Related: [[csharp-pure-move-extraction-pattern]], [[partial-class-seam-declaration-and-consumption-same-phase]], [[literal-call-clauses-block-file-size-tightening]].
