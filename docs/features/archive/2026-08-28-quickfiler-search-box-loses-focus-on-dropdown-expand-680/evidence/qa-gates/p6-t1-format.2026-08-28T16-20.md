# P6-T1 — Format Gate (final pass)

Timestamp: 2026-08-28T16-20

Command: `dotnet tool run csharpier check .` (recorded as `PRE_FORMAT_CHECK_EXIT`), then
`dotnet tool run csharpier format .`, then `dotnet tool run csharpier check .` again.
`git status --porcelain` is captured immediately before and immediately after the format command.

EXIT_CODE: 0

## Final pass (pass 2)

- `PRE_FORMAT_CHECK_EXIT: 0` — the formatter had nothing to rewrite. `Checked 1556 files in 4828ms.`
- Format command exit: 0. `Formatted 1556 files in 1235ms.` (This is a PROCESSED count, not a
  rewritten count; the formatter prints it on every run. `PRE_FORMAT_CHECK_EXIT` is the observation
  that distinguishes a clean pass from a repairing one.)
- Post-format `dotnet tool run csharpier check .` — `EXIT_CODE: 0`. `Checked 1556 files in 4190ms.`
- Porcelain before / after the format command: **identical** (21 entries before, 21 after,
  `Compare-Object` empty). No path entered or left the changed set, so the set-difference condition
  is vacuously satisfied and no reconciliation against the P0-T7 baseline is required.

## Pass 1 (did not count as final — recorded for audit)

The first pass reported `PRE_FORMAT_CHECK_EXIT: 1`. The formatter rewrote three files, all
introduced or edited by this plan:

- `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` — line endings
- `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` — line endings
- `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` — line endings plus one
  member-chain wrap around line 331

All three are inside the P6-T6 edited/created file set, so no pre-existing drift entered the pass.
Per the plan's rule the loop restarted from P6-T1 and pass 1 does not count as final. The porcelain
capture in pass 1 was also identical before and after, which is expected: those files were already
`M` or `??` and no commit exists until P7-T4, so a content rewrite of an already-changed path is
invisible to porcelain. That is exactly why `PRE_FORMAT_CHECK_EXIT` is the discriminating
observation.

## Output Summary

Final pass is clean: `PRE_FORMAT_CHECK_EXIT` is 0, the post-format check exits 0, and the two
porcelain captures are identical. Formatting is stable — a second format run changes nothing.
