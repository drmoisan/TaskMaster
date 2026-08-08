---
name: plan-csproj-line-ranges-shift-during-execution
description: A plan task that cites a csproj block by line range goes stale mid-execution when earlier tasks add Compile entries to that same block; cite blocks by name instead
metadata:
  type: project
---

A plan task that locates a `<Compile Include>` block by line range (e.g. "the breadcrumb block at
lines 58-91", "remove the entry from line 150") is only valid at *authoring* time. Every earlier
task that adds an entry to that csproj shifts the range for every later task.

Measured on `#455` F13: `QuickFiler.Test/QuickFiler.Test.csproj` baseline has the breadcrumb block
at lines 58-91 and `Controllers\WebView2CoreInitializerTests.cs` at line 150. Phases 1-3 add 12 test
entries to that block, so by Phase 4 the block is ~58-103 and the Controllers entry is at ~162.

**Why:** This is the same defect class as the stale-locator problem in prose docs
([[project_plan_line_locators_stale_after_doc_edit]]), but it fires *during* execution rather than
across a doc revision, so an executor that trusts the number edits the wrong line.

**How to apply:**
- At preflight, for each line-range csproj citation, count how many entries earlier tasks add to
  that file before the citing task runs. Non-zero means the range is stale.
- The correct plan wording is a content-based block name ("inside the F13 block of
  `QuickFiler/QuickFiler.csproj`"), not a range.
- Not automatically blocking: it is advisory when the acceptance is content-based (entry counts,
  CRLF intact, no unrelated entry moved) and the `Edit` target string is unique, because the Edit
  tool matches by string, not by line.
- A *baseline-state* citation that describes the pre-change tree captured by a Phase 0 task is
  correct to leave as a range — it is a measurement, not a locator.
