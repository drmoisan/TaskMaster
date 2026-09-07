# Issue #663 update mirror

Timestamp: 2026-09-01T23-40

POSTING BLOCKED

Reason: this executor did not post to GitHub. Posting the update is not a task in the approved plan —
`[P6-T17]` requires the local mirror and permits the `POSTING BLOCKED` disposition — and no
`gh issue comment` or `gh issue edit` invocation was authorised or performed. The text below is the update
intended for issue #663 and is recorded here so that a maintainer can post it verbatim.

PostedAs: unknown

## Exact text of the intended update

> ### Fixed: `QfcFormViewer.ProcessCmdKey` no longer claims every Alt chord
>
> **Correction to the issue body.** The issue text states that "Alt+F and Alt+M are swallowed". **Alt+M is
> correct. Alt+F is not.** The QuickFiler form Designer declares no `MenuStrip`, no `ToolStripMenuItem`,
> and no ampersand in any of its six `.Text =` assignments. The counterpart of the Email Filer's
> "&Filters" caption is `ButtonFilters.Text`, which is the plain string `"Filters"` with no ampersand, so
> there is no Alt+F mnemonic on this surface and there never was. The "Alt+F" wording was carried over
> from the Email Filer twin, whose form does carry a "&Filters" mnemonic. No acceptance criterion asserts
> that Alt+F opens a menu here.
>
> **The mnemonic that was actually swallowed is Alt+M**, for the `&Move Options` menu on the hosted
> `ItemViewer` and `ItemViewerExpanded` controls. `Alt+F4`, the standard window-close chord, was also
> consumed, because it reaches `ProcessCmdKey` as `WM_SYSKEYDOWN` before the default window procedure can
> translate it into the close command.
>
> **Root cause.** `QfcFormKeyHandler.IsAltKeyCommand` tests only the Alt modifier bit and never inspects
> the key-code half of the key value. A `ProcessCmdKey` override that returns `true` suppresses message
> dispatch before WinForms mnemonic resolution runs, so the mnemonic carried by the consumed chord never
> fires.
>
> **Fix.** A new `internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` was added
> to `QfcFormKeyHandler`. It returns `true` only when the handler is non-null, the `Keys.Alt` flag is set,
> and `keyData & Keys.KeyCode` is `Keys.Menu` or `Keys.None` — that is, only for a bare Alt press, which
> is the sole gesture the parameterless `ToggleKeyboardDialogAsync()` dispatch can encode. The
> `QfcFormViewer.ProcessCmdKey` guard now routes through that predicate. Every other Alt chord reaches
> `base.ProcessCmdKey`.
>
> `IsAltKeyCommand` is deliberately left unchanged, with its four existing tests unmodified: its breadth
> remains meaningful for the `KeyEventArgs` dispatch contract that the uncompiled viewer variants
> reference, and narrowing it in place would silently change that contract.
>
> **Verification.** Seven MSTest methods were added to the existing `QfcFormKeyHandlerTests` fixture.
> Three of them — `ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse` and
> `ClaimsAltChord_WithAltLeft_ReturnsFalse` — failed before the fix and pass after it. The full suite runs
> 6934 of 6934 passing, up from a 6927 baseline, with no regression. `ClaimsAltChord` measures 100% line
> and branch coverage in the Cobertura output.
>
> **Not covered by automation.** The live-host check of bare Alt, Alt+M and Alt+F4 is recorded as
> `MANUAL_CHECK_DEFERRED` with the probes that justify the deferral. In particular, which of the several
> `&Move Options` owners WinForms selects on the first Alt+M press cannot be determined statically and
> needs a human at a live Outlook session.

## Local `issue.md` mirroring

Not applicable. `PostedAs` is `unknown` rather than `body`, and no issue-body update was performed, so
there is nothing to mirror into the local
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/issue.md`. That file reproduces the
original GitHub issue body verbatim and is left unmodified.

Output Summary: The intended issue-#663 update is recorded verbatim above under a `POSTING BLOCKED`
header, with `PostedAs: unknown`. The text states the corrected symptom: Alt+M is the swallowed mnemonic
on this surface and Alt+F is not, because `ButtonFilters.Text` is the plain string `"Filters"` with no
ampersand. No GitHub posting was performed by this executor.
