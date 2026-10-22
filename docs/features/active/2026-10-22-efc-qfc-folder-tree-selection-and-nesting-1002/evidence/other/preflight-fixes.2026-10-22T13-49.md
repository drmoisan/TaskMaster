Timestamp: 2026-10-22T13-49
Plan: docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/remediation-plan.2026-10-22T13-43.md

Fixes:

1. Phase 3 preamble: the sentence said the `_globals.Ol` argument is evaluated first. In `BindBreadcrumbRowsAsync` the score projection is computed first, but it contains no await. Reworded to state that the `_globals.Ol` argument throws before the `BindRowsAsync` call is made and that the preceding projection has no await. Intent and assertions unchanged.
