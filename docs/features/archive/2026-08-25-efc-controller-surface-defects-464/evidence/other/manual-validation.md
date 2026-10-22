# [P11-T13] Manual validation checks

Timestamp: 2026-08-28T02-11
Task: [P11-T13]
EXIT_CODE: 0

`spec.md` §`Manual validation` (`spec.md:898-903`) names two checks a reviewer performs by hand,
"because no automated instrument exists". Each is recorded below at the strength of the evidence
actually obtained. Neither is recorded as a pass on an executor's assertion.

---

## Check 1 — Alt+F and Alt+M open the `Filters` and `Move Options` menus

> Open the Email Filer form, press Alt+F, then Alt+M; both menus open.

**Status: `MANUAL_CHECK_DEFERRED`**

**Reason.** The check requires a running Outlook host with the VSTO add-in loaded and the Email Filer
form open, and a human at the keyboard to press the two chords and observe the two menus. Measured on
this machine:

| Probe | Result |
|---|---|
| `Get-Process -Name OUTLOOK` count | **0** — no Outlook process is running |
| `Test-Path HKLM:\SOFTWARE\Classes\Outlook.Application` | True — Outlook is installed |
| `[Environment]::UserInteractive` | True |

Outlook is installed but not running, the VSTO add-in is not loaded, and the repository's own test policy
prohibits this agent from showing a WinForms form or starting a message pump in order to substitute for
the check. No automated instrument exists for it, which is precisely why `spec.md` classifies it as
manual.

**This is not recorded as a pass.** `[P11-T15]` records it as an outstanding manual item.

**What automated evidence does exist, and what it does and does not establish.** The RC10 correction is
`EfcViewer.ClaimsAltChord`, which narrows the viewer's `ProcessCmdKey` claim to a **bare** Alt press. Its
five delivered tests all pass in the `[P10-T6]` run:

| Test | Asserts |
|---|---|
| `ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue` | bare Alt is still claimed |
| `ClaimsAltChord_WithAltF_ReturnsFalse` | **Alt+F is not claimed**, so it falls through to `base.ProcessCmdKey` |
| `ClaimsAltChord_WithAltM_ReturnsFalse` | **Alt+M is not claimed**, so it falls through to `base.ProcessCmdKey` |
| `ClaimsAltChord_WithNonAltChord_ReturnsFalse` | non-Alt chords are unaffected |
| `ClaimsAltChord_WithNullHandler_ReturnsFalse` | no claim without a keyboard handler |

These establish that `EfcViewer` no longer intercepts Alt+F or Alt+M and that both reach
`base.ProcessCmdKey`, which is where WinForms mnemonic processing opens a menu. They do **not** establish
that the two menus actually open in a live Outlook session — that depends on the mnemonic assignments on
the live `ToolStrip`, on the add-in loading, and on nothing else in the host intercepting the chord
first. The residual gap is exactly what check 1 exists to close, and it remains open.

---

## Check 2 — the replacement bytes are two U+002D characters

> Confirm the replacement bytes at `QfcItemController.ViewerSetup.cs:55` are two U+002D characters.

**Status: VERIFIED BY INSTRUMENT — not confirmed by a human reviewer**

The check is a byte-level fact about a source file, not a host-dependent observation, so it was measured
directly rather than deferred. The measurement is stronger than visual inspection, which is the failure
mode that produced the original defect: an en dash and a double hyphen are visually similar.

Locator correction: `spec.md` cites `:55`. On this execution base the literal is at **`:61`**, as the
base-drift addendum records; merged feature #484 added 69 lines above it. `[P9-T5]` resolves the site by
content, not by line number.

Delivered line 61, verbatim:

```csharp
            CoreWebView2EnvironmentOptions options = new("--incognito ");
```

`od -c` over that line renders the two characters preceding `incognito` as `-` `-`. Read as Unicode code
points they are **`0x2d`** and **`0x2d`** — two **U+002D HYPHEN-MINUS** characters. The pre-change line
carried a single **U+2013 EN DASH**, which is the defect.

Corroborating automated assertion: the delivered test
`IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace` passes in the `[P10-T6]` run and
asserts the same property over the `EfcItemController.IncognitoArgument` constant.

**Caveat stated plainly:** the plan's wording asks for "the reviewer's confirmation". No human reviewer
performed this check. What is recorded is an instrument measurement of the same fact, and it is labelled
as such rather than presented as a reviewer sign-off.

---

## Summary of dispositions

| Check | Status | Recorded as a pass? |
|---|---|---|
| 1 — Alt+F / Alt+M open the two menus in a live host | `MANUAL_CHECK_DEFERRED` | **no** |
| 2 — replacement bytes are two U+002D | VERIFIED BY INSTRUMENT (`od -c`, code points `0x2d 0x2d`) | recorded as measured, not as a reviewer sign-off |

Output Summary: Check 1 is `MANUAL_CHECK_DEFERRED` because no Outlook process is running, the VSTO
add-in is not loaded, and the test policy forbids substituting a live form; it is **not** recorded as a
pass and `[P11-T15]` carries it as an outstanding manual item. Five passing `ClaimsAltChord` tests
establish that `EfcViewer` no longer claims Alt+F or Alt+M and that both fall through to
`base.ProcessCmdKey`, but not that the menus open in a live host. Check 2 is verified by instrument: the
two bytes preceding `incognito` at `QfcItemController.ViewerSetup.cs:61` are code points `0x2d` and
`0x2d`, two U+002D characters; no human reviewer confirmed it, and that limitation is stated rather than
glossed.
