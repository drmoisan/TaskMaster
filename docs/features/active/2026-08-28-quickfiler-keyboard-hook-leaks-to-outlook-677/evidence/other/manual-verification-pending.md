# Manual Live-Outlook Verification — Pending (P6-T7)

Timestamp: 2026-08-28T16-12
Command: N/A (verification-status record)
EXIT_CODE: 0

## Status

Spec acceptance criteria **AC-1**, **AC-2**, and the **manual half of AC-3** remain **UNCHECKED**.
They require a live Outlook session with the add-in loaded and cannot be satisfied by any automated
run in this environment.

The **automated half of AC-3** — "verified by existing tests remaining green" — is already
evidenced: the whole `QuickFiler.Test` assembly ran 1218/1218 green with the pre-existing
`BreadcrumbDropDownHost` and breadcrumb pipeline tests byte-unmodified
(`evidence/regression-testing/p4-t3-summary.md`), and the in-form focus-return control cases
`FinishClose_PredicateTrue_FocusAnchorInvoked` and `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked`
assert the #438/#400 caret-return behavior directly. Only the manual half is outstanding.

## Why live verification is mandatory

Per `spec.md` Test Strategy ("Manual validation steps (required)") and the research artifact's
**Automation Feasibility** section, the failure exists only in the composition of three things,
none of which can be exercised in a headless MSTest run without violating the repository's
determinism and no-external-process test policies:

1. Outlook's native windows and message pump;
2. real WebView2 runtime child windows and their focus behavior — MicrosoftEdge/WebView2Feedback
   issue #951 is a runtime defect and is not reproducible with mocks;
3. real Win32 activation and focus transitions driven by user clicks.

The unit tests verify the isolated logic (the focus-permission predicate, the deactivate parking
handler, and the popup-close-on-deactivate ordering) through existing seams. They cannot verify
that the WebView2 runtime actually releases thread keyboard focus, nor that Outlook actually
receives the keystrokes.

## Manual checklist (verbatim from `spec.md` Test Strategy)

> Manual validation steps (required): live WebView2-runtime focus retention and actual Outlook
> keystroke delivery cannot be unit-tested (per the research artifact's Automation Feasibility
> section — the failure exists only in the composition of Outlook's native message pump, real
> WebView2 runtime child windows, and real Win32 activation transitions). Verify manually in a live
> Outlook session: type into native Outlook windows with QuickFiler open in each internal state
> (navigation on/off, popup open/closed, mid-search); confirm click-back restores QuickFiler
> navigation; confirm Escape/commit still returns the caret to the breadcrumb anchor.

> Integration scenario to retest (manual): open QuickFiler, click into a native Outlook
> Explorer/Inspector window, confirm normal typing; return to QuickFiler and confirm its own
> keyboard navigation still functions before closing.

> Manual verification notes: verify no regression to QuickFiler's own keyboard-driven filing
> workflow (arrow keys, character actions, string filter actions) after the fix.

## Mapping to the unchecked acceptance criteria

| AC | Text (abbreviated) | Manual step that closes it |
|---|---|---|
| AC-1 | Typing into a native Outlook window operates Outlook normally, with QuickFiler open in any internal state | Open QuickFiler; for each of navigation on/off, popup open/closed, and mid-search, click into an Outlook Explorer, an open Inspector, and the search box, and type |
| AC-2 | Returning to QuickFiler by click restores QuickFiler's own keyboard navigation | After each AC-1 excursion, click back into QuickFiler and exercise arrow keys, character actions, and string filter actions |
| AC-3 (manual half) | Escape/commit inside QuickFiler still returns the caret to the breadcrumb anchor | With the breadcrumb selector open, press Escape and separately commit a selection; confirm the caret returns to the breadcrumb anchor each time |

## Follow-up recorded elsewhere

The live session should also reconfirm or rule out the secondary WinForms modal-menu-mode
contributor, per `spec.md` "Rollout & Follow-up". That item is owned by the project maintainer.
