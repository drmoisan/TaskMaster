# Manual live-host validation record ([P6-T1])

Timestamp: 2026-09-01T23-36

Command:

```
pwsh -NoProfile -Command '$p = @(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue); "OUTLOOK_PROCESS_COUNT=" + $p.Count; "USER_INTERACTIVE=" + [Environment]::UserInteractive'
```

EXIT_CODE: 0

## Measured probes

| Probe | Returned value |
|---|---|
| `Get-Process -Name OUTLOOK` — returned count | **0** |
| `[Environment]::UserInteractive` | **True** |

No Outlook process is running, so the VSTO add-in is not loaded and the QuickFiler form surface cannot be
opened. The session is nominally interactive, but there is no human at the keyboard to press a chord and
observe a menu, and the repository test policy forbids this agent from showing a WinForms form or
starting a message pump to substitute for a live host.

## Per-gesture record

| Gesture | Status | Detail |
|---|---|---|
| Alt (bare) | **MANUAL_CHECK_DEFERRED** | Not observed on a live host. No Outlook process is running (probe count 0), so the QuickFiler form surface could not be opened. |
| Alt+M | **MANUAL_CHECK_DEFERRED** | Not observed on a live host, for the same reason. |
| Alt+F4 | **MANUAL_CHECK_DEFERRED** | Not observed on a live host, for the same reason. |

No Outlook build is named, because no Outlook process was running to read a build from. Recording a
pass on assertion is not permitted by AC-15 and is not done here.

## What the automated tests do establish

- `ClaimsAltChord(handler, Keys.Alt)` returns `true`, pinned by
  `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue`.
- `ClaimsAltChord(handler, Keys.Menu | Keys.Alt)` returns `true`, pinned by
  `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue`. That is the key-data shape a physical bare Alt
  press produces, since `Keys.Menu` is documented as "The ALT key".
- `ClaimsAltChord(handler, Keys.Alt | Keys.M)` returns `false`, pinned by
  `ClaimsAltChord_WithAltM_ReturnsFalse`, so the guard no longer consumes the chord and control reaches
  `base.ProcessCmdKey`.
- `ClaimsAltChord(handler, Keys.Alt | Keys.F4)` returns `false`, pinned by
  `ClaimsAltChord_WithAltF4_ReturnsFalse`.
- `QfcFormViewer.ProcessCmdKey` routes its entire claim decision through that predicate and contains no
  independent Alt test, recorded by `[P5-T2]`.

Together these establish the **claim decision** for each of the three gestures, at the level of the
predicate and of the single call site that consumes it.

## What the automated tests do NOT establish

- That a bare Alt press actually reaches `ProcessCmdKey` on a live host. This is corroborated in-repo
  rather than proven: the keyboard-navigation dialog is opened today by pressing Alt alone, which is only
  possible if the existing override runs for that gesture.
- That returning `false` for Alt+M results in the `&Move Options` drop-down actually opening. That depends
  on WinForms mnemonic resolution running in `ProcessDialogChar`/`ProcessMnemonic` downstream of the
  suppressed dispatch, which no unit test in this repository exercises.
- **Which** of the several `"&Move Options"` owners WinForms selects. With N loaded rows there are N+2
  controls owning the mnemonic — the `ItemViewer` and `ItemViewerExpanded` Designer templates plus one
  queue-manufactured `ItemViewer` per row. WinForms cycles focus among controls sharing a mnemonic and
  offers a mnemonic only to a control whose whole parent chain is visible and enabled, which is expected
  to exclude the hidden templates, but the runtime visibility and enabled state of those templates is not
  statically determinable. If the wrong row's menu opens, that is a distinct defect in mnemonic ownership
  and belongs in a follow-up issue rather than in this fix.
- That returning `false` for Alt+F4 results in the window actually closing.

## Disposition

This mirrors the disposition recorded for the same class of check under feature #464 for the Email Filer
twin, which likewise recorded `MANUAL_CHECK_DEFERRED` together with the probes that justified it and an
explicit statement of what the automated tests do and do not establish. AC-15 is written to accept a
deferral. What it does not accept is the check being marked passed on an executor's assertion, or a
gesture being omitted silently; neither has been done here.

**Recommended next action for a human reviewer:** open the QuickFiler form in a live Outlook session with
at least one loaded row and press Alt, then Alt+M with a row focused, then Alt+F4, recording the observed
outcome and the Outlook build in this file.

Output Summary: All three gestures — bare Alt, Alt+M and Alt+F4 — are recorded with the status
`MANUAL_CHECK_DEFERRED`, accompanied by both measured probes: `Get-Process -Name OUTLOOK` returned a count
of 0 and `[Environment]::UserInteractive` returned `True`. No gesture is omitted and none is recorded as
a pass on assertion. The record states explicitly what the automated tests establish, which is the claim
decision at the predicate and its single call site, and what they do not, which is the downstream WinForms
mnemonic and window-close behaviour on a live host.
