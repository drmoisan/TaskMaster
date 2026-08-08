# qfc-collection-move-diagnostics-defects (Issue #469)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-move-diagnostics-defects/ (Issue #469)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #469
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/469
- Last Updated: 2026-08-08
## Summary

Four related defects in `QfcCollectionController`'s move and move-diagnostics path: a null guard
placed after the dereference it protects, a trailing null element in the returned array, positional
indexing into an unordered `ConcurrentDictionary`, and a declared parameter that the method body
never reads.

## Environment

- OS/version: n/a (logic defects, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

**Defect 1 — unreachable null guard in `GetMoveDiagnostics` (`:2288-2322`)**

1. Line 2288: `var qf = TryGetItemGroupByIndex(k)?.ItemController;` — `qf` may be `null` by
   construction of the null-conditional.
2. Line 2289 dereferences it immediately: `qf.ItemHelper`.
3. Line 2312 dereferences it again: `xComma(qf.ItemHelper.Subject)`.
4. Only at line 2313 does `if (qf is not null)` appear, so the `else` branch at `:2318-2322` is dead
   code and a null `qf` throws a `NullReferenceException` at 2289 before reaching the guard.
5. Note the issue-#97 guard documented at `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:77-80`
   protects the `olAppointment` parameter, not this path.

**Defect 2 — trailing null element (`:2284-2286`)**

1. Line 2284 allocates `new string[_itemGroupsToMove.Count + 1]`.
2. The loop at 2286 fills indices `0 .. Count-1`.
3. `strOutput[Count]` is therefore always `null`.
4. Consumers are `QuickFiler/Controllers/QfcHomeController.Metrics.cs:75` and `:144`.

**Defect 3 — positional access into a `ConcurrentDictionary` (`:2260-2270`)**

1. `_itemGroupsToMove` is declared `ConcurrentDictionary<QfcItemGroup, int>` at `:71`.
2. `TryGetItemGroupByIndex` does `_itemGroupsToMove.ElementAt(index).Key` at `:2264`.
3. `ConcurrentDictionary` enumeration order is unspecified and not stable across mutations.
4. `MoveEmailsAsync` (`:2220-2223`) and `GetMoveDiagnostics` (`:2286-2288`) each walk `0..Count-1`
   independently, so the two walks can observe different orders.

**Defect 4 — `MoveEmailsAsync` ignores its parameter (`:2206-2228`)**

1. `stackMovedItems` (`SloStack<IMovedMailInfo>`) is declared on the interface at
   `QuickFiler/Interfaces/IQfcCollectionController.cs:50`.
2. It is supplied by `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` as `_movedItems`.
3. The method body at `:2206-2228` never reads the parameter.

## Expected Behavior

1. The null guard should precede every dereference of `qf`, so a missing item controller produces the
   intended diagnostic line rather than an exception.
2. `GetMoveDiagnostics` should return exactly `_itemGroupsToMove.Count` elements.
3. Index-to-group resolution should use a stable, explicitly ordered collection so that a diagnostic
   line is attributed to the message it describes.
4. Either `MoveEmailsAsync` populates the undo stack it is handed, or the parameter is removed from
   the contract.

## Actual Behavior

1. A null `ItemController` throws `NullReferenceException` at `:2289`; the `else` branch is dead.
2. Callers receive an array whose last element is always `null`.
3. A diagnostic line can be attributed to the wrong message when the dictionary is mutated between
   the two independent walks.
4. The undo record supplied by the caller is silently dropped, unless it is populated elsewhere.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at the line numbers above. Discovered during preparation
  research for issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  sections E4, E5, E6, and E15.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Defects 1 and 3 can produce wrong or failed move diagnostics. Defect 4 needs triage before a final
severity can be assigned: if the undo record is genuinely dropped, undo-after-move is broken, which
would be High.

## Suspected Cause / Notes

Defects 1 and 2 share a shape with `SetVisualDigits` (`:138-143`), suggesting a systematic
guard-placement habit rather than isolated slips. Defect 3 is a consequence of using a
`ConcurrentDictionary` where an ordered list is required. Defect 4 should be triaged first, since its
resolution may be "remove the parameter" rather than "populate it".

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `GetMoveDiagnostics` with a null `ItemController`; array length equals
      `Count`; `TryGetItemGroupByIndex` stability across a mutation; `MoveEmailsAsync` populates or
      does not require `stackMovedItems`.
- [x] Integration scenario to retest: move a multi-message selection and confirm each diagnostic line
      matches its message, then exercise undo.
- [x] Manual verification notes: triage defect 4 before fixing; the correct resolution may be a
      contract change coordinated with the `IQfcCollectionController` owner.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
