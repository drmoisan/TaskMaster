# efc-item-controller-keyboard-registration-defects (Issue #459)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-item-controller-keyboard-registration-defects/ (Issue #459)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #459
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/459
- Last Updated: 2026-08-08
## Summary

Three related defects in `EfcItemController` keyboard-action registration: the `KbdActions<>` indexer
setter silently drops unregistered keys, the async expansion path never registers or removes the
`'B'`/`'D'` jump keys that the sync path does, and the resulting asymmetry can make a later sync
expansion throw `ArgumentException`.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Controllers/EfcItemController.cs` expansion and keyboard-registration paths
- Data source or fixture: n/a

## Steps to Reproduce

Defect B is the user-visible one:

1. Open the Email Filer and expand an item through the **async** expansion path (`ToggleExpansionAsync`).
2. Press `B` or `D` to jump to the message body / detail.
3. Observe that the jump keys are not registered.
4. Now expand through the **sync** path (`ToggleExpansion(ToggleState.On)`), collapse through the async
   path, then expand through the sync path again.
5. Observe an `ArgumentException` from `KbdActions<>.Add`.

## Expected Behavior

- `RegisterActions` registers the actions it is given.
- The async and sync expansion paths produce the same keyboard-action state.
- Repeated expand/collapse cycles in any order do not throw.

## Actual Behavior

**A — `RegisterActions` registers nothing (`EfcItemController.cs:691`).**
`_keyboardHandler.CharActions[action.Key] = action.Value` uses the `KbdActions<>` indexer setter
(`KbdActions.cs:38-47`), which performs a `Find(key)` and only assigns when the element is **non-null**.
A missing key is a silent no-op, not an insert. Combined with the `!overwriteDuplicates` filter at
`:687-690` — which removes exactly the keys that *are* present — the `overwriteDuplicates: false` path
is guaranteed to register nothing. `RegisterActions` currently has zero call sites, so this is latent.

**B — async and sync expansion paths are not equivalent (`EfcItemController.cs:931-956` vs `:862-905`).**
`ToggleExpansion(ToggleState.On)` registers `'B'`/`'D'` in `CharActions` (`:879-888`) and
`ToggleExpansion(Off)` removes them (`:902-903`). `ToggleExpansionOn()` (`:944-956`) and
`ToggleExpansionOff()` (`:931-942`) — the bodies dispatched by `ToggleExpansionAsync` (`:913`, `:922`) —
do neither.

**C — duplicate registration throws (`EfcItemController.cs:879-888`).**
`KbdActions<>.Add` throws `ArgumentException` when the `(sourceId, key)` pair already exists
(`KbdActions.cs:92-98`). Because of B, a sync-On -> async-Off -> sync-On sequence leaves the
`"Item"`/`'B'` and `"Item"`/`'D'` entries in place and the second sync-On throws on a UI-thread call path.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Defect C is an unhandled exception on a UI-thread call path. Defect B silently removes documented
keyboard navigation. Defect A is latent today but will surface the moment `RegisterActions` is wired up.

## Suspected Cause / Notes

The indexer-setter semantics in `KbdActions<>` (assign-only-if-present) differ from the usual
`IDictionary` indexer contract (upsert), which is the likely origin of defect A. Defects B and C stem
from the async expansion path being added later without mirroring the sync path's registration
bookkeeping.

Related open issue: #444 `Bug: kbdactions-enumerable-ctor-bypasses-duplicate-guard` covers a separate
`KbdActions<>` guard gap.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Decide and document the intended `KbdActions<>` indexer-setter contract, then align `RegisterActions`
- [ ] Make `ToggleExpansionOn`/`ToggleExpansionOff` mirror the sync path's `'B'`/`'D'` registration
- [ ] Unit coverage: register-into-empty; async expand then key press; sync/async interleaved expand cycles
- [ ] Manual verification: `B` and `D` jump keys work after expanding through either path

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
