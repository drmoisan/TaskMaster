# AC-22 Manual Verification Checklist (MANUAL-ONLY)

Timestamp: 2026-08-08T21-44

Status: PENDING MAINTAINER EXECUTION

Issues: #505, #506, #518. Branch: `bug/ribbon-engine-toggle-state-guards-505`.

> **AC-22 is MANUAL-ONLY.** It must remain `- [ ]` in `spec.md` and must never be checked off on
> the strength of unit tests, source inspection, or any automated artifact. Only a maintainer's
> recorded live-Outlook outcome below can satisfy it. Automated evidence exists for the seam
> behavior, but VSTO callback *binding* cannot be observed outside a live Outlook process, which is
> precisely why the #505 defect went undetected.

## Preparation

- Build and install the add-in from this branch.
- In Outlook, enable **File > Options > Advanced > "Show add-in user interface errors"**. Without
  it, Office reports nothing for a signature-incompatible callback.
- Have both the Spam Manager and Triage configuration menus available on the Taskmaster ribbon tab.

---

## Step 1 — No callback-binding error is reported for either toggle

Proves the corrected synchronous `getPressed` signature actually binds. VSTO silently ignores a
mismatch, so the absence of an error dialog is the observable signal.

1. Start Outlook with "Show add-in user interface errors" enabled and let initialization complete.
2. Open the **Spam Manager** configuration menu so Office queries `SpamBayesEnabled_GetPressed`.
3. Open the **Triage** configuration menu so Office queries `TriageEnabled_GetPressed`.
4. Confirm **no** add-in user-interface error dialog appears for either callback.

- Expected: no callback-binding error for `SpamBayesEnabled_GetPressed` or
  `TriageEnabled_GetPressed`.
- **Outcome (maintainer to complete):**
- **Date / Outlook build / signature:**

---

## Step 2 — Each toggle's state updates after a click and survives a menu reopen

Proves the awaited toggle, the cache update, and the update-before-invalidate ordering work against
the real ribbon.

1. Open the Spam Manager configuration menu and note the **SpamBayes Enabled** checkbox state.
   (On the very first open of a session the box may briefly render unchecked and then correct
   itself once the asynchronous prime completes — this is the designed lazy-prime behavior, not a
   defect.)
2. Click the checkbox. Confirm its rendered state flips to match the new configuration value.
3. Close the menu and reopen it. Confirm the state shown is the new state, not the old one.
4. Click it again to restore the original setting and confirm it flips back.
5. Repeat steps 1-4 for the **Triage Enabled** checkbox.
6. Confirm that an engine that was configured **off** can be re-enabled by this toggle (the toggle
   is deliberately not gated on engine readiness, so a disabled engine must still be re-enablable).

- Expected: both checkboxes reflect real engine activation state, update after a click, and survive
  a menu reopen; a disabled engine can be re-enabled.
- **Outcome (maintainer to complete):**

---

## Step 3 — The ten callbacks invoked before initialization completes produce no `NullReferenceException`

Proves the #518 graceful degradation on the real pre-`SetGlobals` window.

1. Reload the add-in (or restart Outlook) and, **before initialization completes**, open the Spam
   Manager and Triage configuration menus.
2. Confirm **no** `NullReferenceException` is raised by any of the ten callbacks:
   `SpamBayesEnabled_Click`, `SpamBayesEnabled_GetPressed`, `SpamSaveNetwork_Click`,
   `SpamSaveLocal_Click`, `GetSaveLocation_Click`, `TriageEnabled_Click`,
   `TriageEnabled_GetPressed`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`,
   `TriageGetSaveLocation_Click`.
3. Confirm both **enable-toggle checkboxes render unchecked** rather than throwing.
4. Click each enable-toggle checkbox while the engines are still loading. Confirm **exactly one**
   "not available" notification appears per click, and that nothing else happens.
5. Confirm the six save/info buttons — **Network**, **Local**, and **Current Location** under both
   the Spam and Triage **Save Options** menus — render **disabled** while their engine is still
   loading. This is the intended UX change (B7): previously they were always enabled and silently
   did nothing.
6. Let initialization finish. Confirm those six buttons **re-enable** automatically (driven by the
   existing post-load refresh) without reopening Outlook, and that each still performs its original
   action.

- Expected: no `NullReferenceException` from any of the ten callbacks; unchecked toggles; exactly
  one notification per blocked toggle click; six buttons disabled during load and re-enabled after.
- **Outcome (maintainer to complete):**

---

## Sign-off

- Verified by:
- Date:
- Outlook version / build:
- Overall result (PASS / FAIL / PARTIAL):
- Notes:

Once all three steps are recorded as passing, AC-22 may be checked off in
`<FEATURE>\spec.md` **by the maintainer**, citing this file.
