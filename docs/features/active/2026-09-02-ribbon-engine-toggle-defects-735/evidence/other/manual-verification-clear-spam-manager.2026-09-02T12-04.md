# Manual Verification Dossier — Clear Spam Manager (P2-T13)

Timestamp: 2026-09-03T02-21
Task: [P2-T13]
Finding: #735 finding 2 — the residual, coverage-exempt lines inside `ClearSpamManagerAsync`.

**This is a documentation and acceptance task, not a toolchain gate.** It must not be reported as a
passing automated check, and no coverage credit is claimed anywhere in this change for the lines it
covers.

ManualVerificationStatus: OPERATOR-ACTION-REQUIRED

Reason: this executor has no live Outlook host. The procedure below requires launching Outlook with
add-in user-interface errors shown and interacting with the Explorer ribbon during the
pre-initialization window, which cannot be performed from an automated session. Repository unit-test
policy independently forbids starting an external process or a message pump from a test, so the
procedure cannot be automated either. The two observation fields are therefore left unfilled rather
than asserted.

Consequence for acceptance tracking: acceptance criterion F2-AC8 ("The change description records
the manual verification") stays UNCHECKED, and P5-T3 records `OPERATOR-ACTION-REQUIRED` for it in
the acceptance-criteria status summary. Per the plan, this is the correct outcome for an executor
without a live Outlook host, not a plan failure.

## Required operator procedure

### Step 1 — the pre-initialization window (the defect condition)

1. Launch Outlook with "Show add-in user interface errors" enabled, so an unhandled exception from a
   ribbon event handler surfaces as a visible error rather than being swallowed.
2. Before add-in initialization completes, click **Clear Spam Manager** on the Explorer ribbon.
3. Confirm the "Are you sure you want to clear the Spam Manager? This cannot be undone" prompt by
   answering Yes.
4. Expected observation after the fix: the not-ready notice is shown — "The Spam Manager cannot be
   cleared yet because the classifier manager is still loading. Please try again once initialization
   completes." — and NO `NullReferenceException` is raised. Under the pre-fix code this same
   sequence raised an unhandled `NullReferenceException` out of the user-interface event handler.

Observed outcome: NOT PERFORMED — no live Outlook host available to this executor.

### Step 2 — after initialization completes (the no-regression condition)

1. Wait for add-in initialization to complete.
2. Click **Clear Spam Manager** again and confirm the prompt.
3. Expected observation: the reset runs end to end, exactly as before this change — the Spam Bayes
   configuration is located, a fresh classifier is created and serialized, the manager entry is
   replaced, and the Spam engine restarts. No not-ready notice appears.

Observed outcome: NOT PERFORMED — no live Outlook host available to this executor.

## Why this step exists

The roughly ten lines that remain inside `ClearSpamManagerAsync` after the extraction are not
unit-testable: they show a message box, install a WinForms synchronization context, and call
classifier creation and serialization paths that touch disk. They stay inside `RibbonController`'s
pre-existing, already-ratified type-level `[ExcludeFromCodeCoverage]`. This change adds no new
exemption attribute anywhere and widens no existing one. Repository policy requires that such
residual lines be validated by a documented manual step INSTEAD OF a coverage claim, which is what
this dossier records.

The decidable logic those lines used to contain — the whole three-way null decision and the
notification — has been extracted into `SpamManagerResetGate` and is covered by nine passing unit
tests recorded in `evidence/regression-testing/gate-tests.2026-09-02T12-04.md`. What remains for the
operator to confirm is only that the extraction is wired correctly in the live host.

## Handoff

The operator performing this verification should replace each "Observed outcome" line above with the
actual observation and change `ManualVerificationStatus:` to `PERFORMED`, then check off F2-AC8 in
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`.

Command: not applicable — this task performs no command. It writes the procedure and its outcome field.
EXIT_CODE: 0

Output Summary: The two-step operator procedure is recorded in full.
`ManualVerificationStatus: OPERATOR-ACTION-REQUIRED` because this executor has no live Outlook host,
so neither observation was made. F2-AC8 consequently stays unchecked and is reported as
OPERATOR-ACTION-REQUIRED in the P5-T8 summary.
