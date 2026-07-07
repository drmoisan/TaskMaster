# Issue #254 — QuickFiler dark/light toggle leaves sender/subject labels stale

Root-cause research. Scope: the "High Confidence" QuickFiler view where switching dark/light
mode leaves several email rows with the previous theme's sender/subject label colors while the
rest of each row re-themes. Distinct from issue #251/#252 (unsubscribe-on-cleanup of the
dark-mode handler), which is verified intact below.

All file:line citations are against the current worktree HEAD
(`TaskMaster-wt-2026-07-07-12-28`, tip `026de853`).

---

## 1. Confirmation that #251/#252 is intact and unrelated

`QfcCollectionController.DarkMode_CheckedChanged`
(`QuickFiler/Controllers/QfcCollectionController.cs:2118`) retains the #251 fix:

- Defensive null guard on `_formViewer` (lines 2123-2126).
- Sender-carried dark-mode read preferring `IOlObjects senderOl` over `_globals` (lines 2131-2143).
- Unsubscribe in both `CleanupAsync()` (line 2182) and `Cleanup()` (line 2196).

Issue #254 is a different defect: the handler fires correctly and reaches every item; the failure
is downstream, inside per-item theme application. The #251 regression tests
(`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`) remain valid.

---

## 2. End-to-end toggle trace (both directions, read/unread, Active/Normal)

Toggle entry and fan-out (all confirmed reachable):

1. `DarkMode_CheckedChanged` → `SetDarkMode(async: true)` / `SetLightMode(async: true)`
   (`QfcCollectionController.cs:2145-2152`).
2. `SetDarkMode`/`SetLightMode` loop every `_itemGroups` entry and call
   `itemGroup.ItemController.SetThemeDark(async)` / `SetThemeLight(async)`
   (`QfcCollectionController.cs:2156-2170`). The `async` argument is `true`.
3. `SetThemeDark(bool)` / `SetThemeLight(bool)`
   (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:275-316`) select the theme key by the
   Active/Normal test `(_activeTheme is null) || _activeTheme.Contains("Normal")`, call
   `_themes[key].SetQfcTheme(async)`, then set `_activeTheme`.
4. `Theme.SetQfcTheme(bool async)` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:414-432`): with
   `async == true` it runs `_uiDispatcher.InvokeAsync(() => SetQfcTheme())` (line 418). The returned
   `Task` is **discarded** by `SetThemeDark`/`SetThemeLight` — fire-and-forget.
5. Private `Theme.SetQfcTheme()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:8-103`)
   applies colors in a fixed order:
   - lines 11-12: `_lblItemNumber` (nav) colors
   - lines 15-18: every `TableLayoutPanel` back color
   - lines 21-31: tips-details / tips-expanded labels
   - lines 33-41: **mail read/unread label coloring** — `if (!MailRead()) SetMailUnread(); else SetMailRead();`
   - lines 44-102: buttons, menu items, menu strip, search box, body box, folder combo, topic thread,
     WebView2, viewer defaults.

### Hypothesis (a) — wrong theme key for Active variants: REFUTED
`SetThemeDark`/`SetThemeLight` branch on `_activeTheme.Contains("Normal")`, mapping
`LightActive→DarkActive` and `LightNormal→DarkNormal` (and the reverse) correctly. The
`ToggleFocus*` state machine (`FocusAndTheme.cs:27-166`) keeps `_activeTheme` in the four-value set.
No key-selection defect.

### Hypothesis (c) — the labels are themed only in `SetMailRead`/`SetMailUnread`: CONFIRMED (structural)
`_lblSender` and `_lblSubject` back/fore colors are assigned in exactly two places:
`Theme.SetMailRead()` (`Theme.cs:370-373`) and `Theme.SetMailUnread()` (`Theme.cs:408-411`). A
repo-wide search for `LblSender`/`LblSubject` background assignment finds no other setter in the QFC
theme path. Both methods set the two labels together, so an affected row has **both** labels stale
(the narrow sender strip and the wide subject bar both retain the prior color; the subject bar is
simply the more visible of the two in the screenshot). Consequently, if the code path that reaches
`SetMailRead()`/`SetMailUnread()` is skipped for an item, that item's sender/subject labels keep the
previous theme's colors while everything else on the row (nav, panel, tips, buttons, textboxes)
re-themes — which is exactly the screenshot symptom.

### Hypotheses (b) mail branch not reached / (d) async divergence — CONFIRMED as the trigger mechanism
The mail branch (`Theme.Rendering.cs:33-41`) is guarded by `MailRead()`, the injected
`Func<bool>` whose production value is `() => !controller.Mail.UnRead`
(`QuickFiler/Helper Classes/QfcThemeHelper.cs:89`). `MailRead()` is evaluated at line 34, **before**
`SetMailUnread()`/`SetMailRead()` run. `controller.Mail` is the Outlook `MailItem` COM object;
reading `.UnRead` on a stale/moved/deleted item throws (`COMException` "item has been moved or
deleted", or an NRE if `Mail` is null). In High-Confidence mode the visible set is rebuilt by
post-hoc removal and conversation reshuffling
(`QfcCollectionController.RemoveBelowThresholdAsync`, per prior research on the dual high-confidence
pipeline), so a subset of item controllers can hold a `MailItem` reference that is no longer
resolvable.

If `MailRead()` throws:
- Private `SetQfcTheme()` has already applied lines 11-31 (nav, panel, tips → new theme) but throws
  at line 34, so lines 34-102 never run → `_lblSender`/`_lblSubject` keep the prior theme's colors
  and later controls (buttons, textboxes) also keep prior colors.
- Because step 4 used `async: true` (`InvokeAsync`, fire-and-forget), the throw surfaces as a faulted
  `DispatcherOperation` **that no one awaits** — it does not abort the `SetDarkMode`/`SetLightMode`
  loop and does not abort the other items' dispatched renders. Each item's render faults or succeeds
  independently, so **several** rows (only those whose `MailRead()` faults) retain stale label colors
  while the rest re-theme. This precisely explains "some rows, not all."

Note a latent inconsistency in the read-timer path (not the toggle path but adjacent): the public
`SetMailRead(bool)` dispatches via `_uiDispatcher.BeginInvoke` (`Theme.cs:348`) while
`SetMailUnread(bool)` dispatches via `_lblSender.BeginInvoke` (`Theme.cs:386`). This does not cause
#254 but is worth flagging to the planner as a nearby smell.

---

## 3. Ground-truth reachability

- **Structural root cause — CONFIRMED by code reading.** Labels are re-themed only inside the
  `MailRead()`-guarded branch of `SetQfcTheme()`; a throw at the `MailRead()` probe (line 34) leaves
  panels/nav/tips re-themed but sender/subject labels stale; `async: true` fire-and-forget dispatch
  isolates the fault to individual items. The observed screenshot state (dark/light panels correct,
  scattered blue `MediumBlue = LightNormal/LightActive.mailUnreadBackColor` subject bars) is the
  deterministic result of that path. Screenshot: `artifacts/Screenshot 2026-07-07 123101.png`.
- **Exact production trigger — LIKELY, not runtime-verified here.** The specific fault source
  `!controller.Mail.UnRead` throwing on a stale/moved COM `MailItem` is consistent with High-Confidence
  remove/reshuffle behavior and with the "several rows" symptom, but was not reproduced against a live
  Outlook process in this research (out of scope for a No-COM environment). Any per-item fault in the
  read-state probe produces the same symptom; the fix does not depend on the exact fault source.

---

## 4. Minimal fix recommendation (1 production file)

Make the mail read/unread label recoloring resilient so a fault in the read-state probe cannot leave
`_lblSender`/`_lblSubject` at the previous theme's colors.

- **File:** `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, mail branch (lines 33-41).
- **Change:** evaluate `MailRead()` defensively and always drive one of
  `SetMailUnread()`/`SetMailRead()` so the labels are re-themed on every toggle regardless of the
  probe outcome. Sketch (for the planner; exact form subject to CSharpier + analyzer gates):

  ```csharp
  // Mail item colors — a fault in the read-state probe (a stale/moved Outlook MailItem) must not
  // prevent the sender/subject labels from being re-themed (issue #254). Default to unread coloring
  // when the read state cannot be determined so the labels still adopt the current theme's colors.
  bool isRead;
  try
  {
      isRead = MailRead();
  }
  catch (System.Runtime.InteropServices.COMException)
  {
      isRead = false;
  }
  if (!isRead) { SetMailUnread(); } else { SetMailRead(); }
  ```

Design notes / policy alignment:
- Catch the specific `COMException` (the documented failure of reading `MailItem.UnRead` on a
  disconnected item) rather than a broad `catch (Exception)`, to respect the fail-fast/no-broad-catch
  rule in the General Code Change Policy. If the planner determines `Mail` can also be null in this
  path, add `NullReferenceException` explicitly rather than widening to `Exception`.
- Defaulting to unread coloring is a minor, deliberate semantic tradeoff: an item whose read state
  cannot be probed is rendered with the current theme's unread colors — visually within the correct
  theme family, which is strictly better than retaining the wrong theme's colors.
- Alternative (rejected as less robust): make the `MailRead` closure defensive at its construction
  site (`QfcThemeHelper.BuildProductionControlSet`, `QuickFiler/Helper Classes/QfcThemeHelper.cs:89`).
  This fixes only the production closure and leaves `Theme.SetQfcTheme()` fragile for any other
  caller/injected Func; the label-theming guarantee belongs in the renderer. Keep this out of scope.

No change is required in `QfcCollectionController` or `QfcItemController.FocusAndTheme.cs`; the fan-out
and key selection there are correct. Expected blast radius: one production file.

---

## 5. Regression test approach (deterministic, seam-based, no live Outlook/COM/WinForms host)

The `Theme` big constructor already accepts handle-less WinForms doubles and an injected
`Func<bool> mailRead`, proven by
`UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.DispatcherTests.cs:87-146`
(`Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher`). Reuse that exact
construction shape. Key seam facts:

- Handle-less `Label` returns `InvokeRequired == false`, so `SetQfcTheme(async: false)`
  (`Theme.cs:414-427`) falls through to run the private `SetQfcTheme()` synchronously on the test
  thread — no dispatcher, no STA thread, no live handle needed. This is the same technique used by
  `QfcItemController.FocusAndThemeTests.EnableHandlelessThemeInvoke`
  (`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:136-158`).
- `MailRead` is a plain `Func<bool>` private field; inject a throwing delegate to simulate the
  stale-COM probe without any COM object.

Proposed new test class (host it beside the existing dispatcher tests, e.g.
`UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, to keep files under the
500-line cap). Use MSTest + FluentAssertions; no Moq needed for the Theme itself.

1. **Regression (fails pre-fix, passes post-fix):** construct a `Theme` with distinct
   `mailUnreadBackColor`/`mailReadBackColor` sentinels, real handle-less `_lblSender`/`_lblSubject`
   pre-set to a "previous theme" sentinel color, and `mailRead: () => throw new COMException()`.
   Act: `Action act = () => theme.SetQfcTheme(async: false);`. Assert: `act.Should().NotThrow()` **and**
   `lblSender.BackColor`/`lblSubject.BackColor` equal the theme's unread color (re-themed), not the
   previous-theme sentinel. Pre-fix this throws (and the labels stay stale); post-fix it re-themes.
2. **Positive — unread:** `mailRead: () => false` → both labels adopt `mailUnread*` colors.
3. **Positive — read:** `mailRead: () => true` → both labels adopt `mailRead*` colors.

These three cases give full branch coverage of the changed try/catch block (try-success-read,
try-success-unread, catch-default). Optionally add a collection-level test mirroring
`QfcCollectionControllerDarkModeTests` (mock `IQfcItemController`, verify `SetThemeDark`/`SetThemeLight`
invoked per group) to lock in that the fan-out still reaches every item; the existing #251 tests
already cover the fan-out reachability, so this is optional.

---

## 6. Coverage implications

- Changed lines are confined to the mail branch of `Theme.SetQfcTheme()`
  (`Theme.Rendering.cs`), an assembly (`UtilitiesCS`) that is not COM-exempt and is subject to the
  repository line floor and the >= 90% floor for new/changed code.
- The three test cases above cover every changed branch (probe returns true, probe returns false,
  probe throws), meeting the changed-line and new-code coverage floors with no regression on
  surrounding lines.
- No coverage exclusion is introduced; the fix adds only reachable, test-covered branches.

---

## Automation Feasibility

- **Fix implementation:** fully autonomous. Single-file edit in host-neutral `UtilitiesCS`; no
  Outlook/COM/VSTO/WinForms host required to compile or format. Respects No-COM architecture rules
  (the change touches WinForms `Label` color properties already present in `Theme`, adds no new
  Outlook-interop dependency).
- **Regression test:** fully autonomous and deterministic. The `Theme` big constructor with
  handle-less WinForms doubles plus an injected `Func<bool>` (throwing / true / false) exercises the
  exact defective branch with no live Outlook, no dispatcher, and no STA thread — the pattern is
  already proven in `Theme.DispatcherTests.cs` and `QfcItemController.FocusAndThemeTests.cs`.
- **Toolchain verification:** autonomous via the standard C# loop (CSharpier → analyzer build →
  nullable build → `vstest.console.exe /EnableCodeCoverage`).
- **Human interaction required:** none for implement-and-verify. The only item outside the autonomous
  loop is optional end-user visual confirmation against a live Outlook profile that the scattered blue
  bars no longer appear; this is confirmatory, not a gate, since the deterministic test reproduces the
  underlying branch fault.

Conclusion: implementable and verifiable fully autonomously via existing test seams.
