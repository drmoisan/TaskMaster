# qfc-collection-eliminate-space-sign-error (Issue #471)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-eliminate-space-sign-error/ (Issue #471)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #471
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/471
- Last Updated: 2026-08-08
## Summary

`QfcCollectionController.EliminateSpaceForItems` computes a negative `heightChange` and then
subtracts it, so the item panel **grows** when rows are removed instead of shrinking.

## Environment

- OS/version: n/a (logic defect, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

1. Inspect `QuickFiler/Controllers/QfcCollectionController.cs:2013-2027`
   (`EliminateSpaceForItems(int removalInex, int removalCount)`).
2. Note line 2017 assigns a negative magnitude:
   `var heightChange = -(int)Math.Round(_template.Height * removalCount, 0);`
3. Note lines 2020 and 2025 then **subtract** that value:
   `... MinimumSize.Height - heightChange` and `... Size.Height - heightChange`.
4. Subtracting a negative number adds. Removing `removalCount` rows therefore increases the panel
   height by `_template.Height * removalCount`.
5. Compare with the sibling `MakeSpaceForItems` at `:2029-2042`, which computes a **positive**
   magnitude and uses `+`. The two methods are not symmetric.
6. The reachable production path is `ToggleGroupConv` at `:1779`, i.e. collapsing a conversation.

## Expected Behavior

Removing `removalCount` rows should reduce the panel's `MinimumSize.Height` and `Size.Height` by
`_template.Height * removalCount`, mirroring `MakeSpaceForItems` in the opposite direction.

## Actual Behavior

The panel height increases by `_template.Height * removalCount` on every removal, so the item table
grows each time a conversation is collapsed.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `QuickFiler/Controllers/QfcCollectionController.cs:2017`
  (negative assignment), `:2020` and `:2025` (subtraction), and `:2029-2042` (the asymmetric sibling
  `MakeSpaceForItems`). Discovered during preparation research for issue #454 (epic #136, child F11);
  full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  section E3.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

A visual layout defect on the conversation-collapse path. It does not corrupt data or lose mail, but
it degrades the QuickFiler item panel cumulatively over a session.

## Suspected Cause / Notes

A sign error introduced when `EliminateSpaceForItems` was written as the inverse of
`MakeSpaceForItems`: the author negated the magnitude **and** kept the subtraction, applying the
inversion twice.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: assert `EliminateSpaceForItems` reduces both `MinimumSize.Height` and
      `Size.Height` by exactly `_template.Height * removalCount`, and that
      `MakeSpaceForItems` followed by `EliminateSpaceForItems` with the same count is height-neutral.
- [x] Integration scenario to retest: collapse and re-expand a conversation repeatedly and confirm
      the panel height returns to its original value.
- [x] Manual verification notes: the fix is either negating line 2017 or changing lines 2020/2025 to
      `+`, not both.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
