---
name: project-400-partial-class-headroom-placement
description: "#400 breadcrumb plans: add new coverage cases to existing [TestClass] partial .Part2.cs files, not topically-correct near-limit files, to keep the 17-class filter intact"
metadata:
  type: project
---

When a QuickFiler breadcrumb remediation plan needs new test cases to close a coverage
shortfall, allocate them to an existing `*.Part2.cs` `[TestClass] partial` continuation
that still has headroom to the 480-line bound — even when a different file is the
topically correct home.

**Why:** the downstream instrumented gate pins an exact 17-class filter and per-class case
counts. A `.Part2.cs` partial shares its original `[TestClass]` name, so class count and
filter string stay byte-identical and only the per-class case total changes. Creating a new
test file would require a new `QuickFiler.Test.csproj` Compile include, an 18th class, and a
rewrite of the filter and every downstream count assertion. In the 2026-07-22 revision,
`BreadcrumbDropDownLifecycleCoverageTests.cs` (468 lines) and
`BreadcrumbDropDownCoverageThresholdTests.cs` (479 lines) had effectively zero headroom, so
Host-lambda coverage was placed in `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` instead.

**How to apply:** before allocating cases, capture current physical line counts for every
candidate test file, exclude any above roughly 460 lines, and prefer `.Part2.cs` partials.
State the case arithmetic explicitly (old total + added = new total) so the executor has an
exact expected count rather than a stale one. If a post-format count would exceed 480, stop
for replanning rather than silently adding a file.

Related: [[plan-validator-task-id-sequential-constraint]], [[project_351_quickfiler_breadcrumb_plan_seams]]
