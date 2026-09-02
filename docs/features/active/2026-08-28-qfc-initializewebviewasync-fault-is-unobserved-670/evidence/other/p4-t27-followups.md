# P4-T27 — Follow-up items not delivered by #670

Timestamp: 2026-09-01T20-22
Command: source verification of each cited location against the current tree
EXIT_CODE: 0

These three items are required by `spec.md` and are deliberately **not** delivered by issue #670. Each is recorded here with its preconditions so the orchestrator can route it through the promotion lifecycle. Recording them as prose inside a feature folder is not sufficient: a feature folder's prose disappears at merge, whereas a promoted issue persists.

---

## Follow-up 1 — `EfcItemController` fire-and-forget sites

**Suggested slug:** `efc-item-controller-initializewebviewasync-fault-is-unobserved`

**Locations, re-verified against the current tree:**

    QuickFiler/Controllers/EfcItemController.cs:97   Task.Run(() => InitializeWebViewAsync());
    QuickFiler/Controllers/EfcItemController.cs:153  Task.Run(() => InitializeWebViewAsync());
    QuickFiler/Controllers/EfcItemController.cs:174  internal async Task InitializeWebViewAsync()
    QuickFiler/Controllers/EfcItemController.cs:25   [ExcludeFromCodeCoverage]

Both sites discard the returned `Task` against that class's own same-named member, so they carry the identical defect #670 fixes in `QfcItemController`.

**Preconditions that must be addressed before that fix can be regression-tested:**

1. **Class-level `[ExcludeFromCodeCoverage]` at line 25.** The whole type is exempt from coverage measurement, so a fix there yields zero covered lines and no measurable coverage evidence. Either the attribute must be narrowed or the fix must be justified without a coverage delta.
2. **No injectable WebView2 seam.** `EfcItemController.InitializeWebViewAsync` calls `CoreWebView2Environment.CreateAsync` directly, with no `IWebViewCoreInitializer` equivalent. #670's regression tests were only possible because that seam already existed in `QfcItemController` and could be mocked to fault deterministically. Without an equivalent seam there is no way to produce a controlled fault, and therefore no deterministic regression test — a live CoreWebView2 runtime is an external process barred by the unit-test policy.

The seam extraction is the substantive prerequisite and is likely larger than the fix itself.

---

## Follow-up 2 — `TaskScheduler.UnobservedTaskException` backstop

**Status:** optional. A process-wide safety net for unobserved task exceptions at the add-in boundary.

**Preconditions and reasons it was excluded from #670:**

1. It fires only at **finalization**, so it is not a substitute for observing a fault at the boundary where it occurs; it is a backstop, not a fix.
2. It is **process-global**, so it would affect every task in the add-in rather than the paths under repair.
3. It would land in `TaskMaster/ThisAddIn.cs`, outside #670's file scope, which the spec's Scope and Non-Goals section fixes.
4. It has **no in-repo precedent** and **cannot be regression-tested deterministically**, because finalization timing is not controllable from a test.

This should be promoted only if a process-wide safety net is actually wanted; it is not a prerequisite for anything delivered here.

---

## Follow-up 3 — coverage-floor divergence between repository authorities

**Status:** governance item. Not a code defect.

Two authorities in this repository state different repository-wide coverage floors:

- `CLAUDE.md` (General Unit Test Policy §UT2) and `.claude/rules/csharp.md` state **`>= 80%`** line coverage, with a ratified COM/VSTO/WinForms testable-denominator exemption and `>= 90%` for new modules.
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state **`>= 85%` line and `>= 75%` branch** across all tiers, together with a Coverage Exclusion Policy that forbids excluding production files at all.

The two are in direct tension. The second's exclusion policy also conflicts with the `[ExcludeFromCodeCoverage]` attributes that #670 depends on for its scope reasoning — notably the one on `InitializeWebViewAsync` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:47` and the class-level one on `EfcItemController`.

**How #670 avoided the divergence rather than resolving it:** no acceptance criterion in this delivery asserts a repository-wide percentage as a pass or fail number. AC13 uses the unambiguous new-module `>= 90%` rule, and AC14 uses a no-regression comparison against a baseline captured in the same run. Both are well-defined under either authority.

The measured figure in this delivery run is 85.3771%, which happens to satisfy both the 80% and the 85% floors, so the divergence was not forced in either direction here. That is a property of this tree at this moment, not a resolution.

Resolving the divergence is out of scope for #670 and should be raised as its own governance item. Until it is resolved, any future plan that needs a repository-wide coverage gate has no single authority to cite.

---

## Routing note

All three items are reported to the orchestrator for promotion. This executor does not run promotion, research, or planning, and did not create issues for these items.
