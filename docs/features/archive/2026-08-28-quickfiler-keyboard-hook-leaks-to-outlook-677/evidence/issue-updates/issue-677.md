# POSTING BLOCKED — Issue #677 Update Mirror (P6-T8)

Timestamp: 2026-08-28T16-13
PostedAs: unknown

## Reason not posted

Nothing in this plan authorizes a GitHub write. P6-T8's stated scope is to update the local
`issue.md` and to mirror the update text here; it contains no posting step. P7-T1 states explicitly
that PR creation goes through the `pr-author` skill and is orchestrator-gated, and that this plan
does not run `gh pr create`. Posting a comment or editing the issue body on
https://github.com/drmoisan/TaskMaster/issues/677 would be an external side effect outside the
plan, so it was not performed. The `gh` CLI is authenticated and available in this environment, so
this is a scope decision, not a tooling limitation.

The local `issue.md` at
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/issue.md` HAS been
updated with the text below and is committed with the change.

## Exact text applied to `issue.md`

### Section "Proposed Fix / Validation Ideas"

```
- [x] Unit coverage areas: **delivered**. The seeded `KeyboardHandler`/`KbdActive` coverage area is
  superseded — root-cause analysis confirmed `KeyboardHandler` is correctly scoped to QuickFiler's
  own control tree and is not changed by this fix
  (`evidence/qa-gates/keyboardhandler-unchanged.md`). The correct coverage area is the
  **window-activation / focus-transfer logic**, and seventeen regression tests now cover it at 100%
  changed-line coverage: eight in `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`
  for the execution-time focus-permission predicate, seven in
  `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` for the `Form.Deactivate`
  focus-parking and selector-cancel handler, and two in
  `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` for the fan-out
  hop.
- [ ] Integration scenario to retest: ... — **still open**; requires a live Outlook session. See
  `evidence/other/manual-verification-pending.md`.
- [ ] Manual verification notes: ... — **still open**; requires a live Outlook session. See
  `evidence/other/manual-verification-pending.md`.
```

### Section "Next Step"

```
- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
- [x] Fix implemented: the two-part focus fix (execution-time focus-permission predicate on
  `BreadcrumbDropDownHost`, plus `Form.Deactivate`-driven focus parking and selector cancellation
  through `QfcFormViewer`/`QfcFormController`). Full toolchain green: CSharpier check 0 violations,
  analyzer rebuild 0 errors, nullable rebuild 0 errors, full suite 6838/6838 passing, repo line
  coverage 85.28% (up from 85.27%).
- [ ] **Manual live-Outlook verification pending** — acceptance criteria AC-1, AC-2 and the manual
  half of AC-3 in `spec.md` remain unchecked until a maintainer runs the checklist in
  `evidence/other/manual-verification-pending.md`. The same session should reconfirm or rule out
  the secondary WinForms modal-menu-mode contributor recorded in `spec.md` Rollout & Follow-up.
```

## Issue reference

- Issue: #677
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/677
