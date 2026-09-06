---
name: qfc791-deadline-and-cancel-teardown
description: "#791 research: #424 AC:231/239 and #608 AC:184 ratified the empty-at-deadline result #791 now supersedes; an item-count cap alone cannot bound the pre-UI wait; the gate test helper's exact 9-param ctor lookup fails closed"
metadata:
  type: project
---

Issue #791 (High Confidence empty dialog + Cancel teardown), researched 2026-09-06 against `7c8ac9ae`.
Six findings a planner working from the issue body alone would miss.

**1. Two closed features explicitly ratified the behavior #791 changes.** `#424` spec AC
(`docs/features/archive/2026-08-06-...-424/spec.md:231`) states the zero-accepted deadline result is an
empty list plus an empty first group, and `#608` spec AC
(`docs/features/active/2026-08-25-...-608/spec.md:184`) states that behavior is retained. Both are
superseded by #791 AC1 and must be named as superseded in the spec. #446 AC-6 (`CompleteAddingAsync`
only under `SourceExhausted`, `QfcHomeController.Iteration.cs:39-47`) is *preserved* — route any new
stop reason away from that branch.

**2. #424 already refused a settings surface for this bound.** Its AC at `spec.md:239` says the deadline
is an internal constant with an internal test seam, "no new `QfSettings`/`IAppQuickFilerSettings`
member, no `Settings.Designer.cs` change, and no ribbon plumbing". Put any hard scan cap in the same
place, not in `AppQuickFilerSettings` (`Settings.Designer.cs:1-9` is auto-generated).

**3. A scanned-item cap alone does not bound the pre-UI wait.** The gate's empty-queue branch
(`QfcStreamingDequeueConfidenceGate.cs:185-196`) waits `timeOut` ms and retries while
`_remainingLoadActive` is true, and `scanned++` (`:205`) only runs after a score. Removing the deadline
as a terminator therefore needs BOTH an item cap and a wall-clock ceiling.

**4. The gate test helper fails closed on an exact constructor shape.**
`QfcStreamingDequeueConfidenceGateTests.cs:27-92` does one `GetConstructor` with the exact nine-type
list and asserts it is non-null. Any added ctor parameter breaks every gate test until the helper is
updated — this is by design (#446 replaced a fallback chain that failed open).

**5. `ActionCancelAsync` is also the normal-completion path.** `MoveAndIterate` calls it at
`QfcFormController.EventHandlers.cs:169` (error) and `:208` ("Finished Moving Emails"), so the missing
`KbdActive` reset / focus parking / ordering affects successful completion too, not just the button.
`ButtonCancel_Click` (`:70-82`) rethrows from `async void`; the existing cancel test
`QfcFormControllerTests.cs:392-403` awaits and asserts nothing.

**6. File-size routing.** `QfcDatamodel.cs` is 480/500 (put new members in
`QfcDatamodel.QueueProcessing.cs`, 298); `QfcCollectionController.cs` is 2329 (call the public
`UnregisterNavigation()` from the form controller instead of editing it); `QfcFormControllerTests.cs`
792 and `QfcFormControllerSeamTests.cs` 496 — new cancel tests need a new file plus a
`<Compile Include>` entry in the legacy `QuickFiler.Test.csproj`.

**Why:** items 1 and 2 protect the plan from "regressing" ratified ACs or inventing a settings knob;
3 through 6 are the traps that turn a plausible fix into a red build or an unbounded startup.

**How to apply:** read before planning any change to the high-confidence dequeue bound or the
QuickFiler Cancel/teardown chain.

Related: [[qfc424-high-confidence-startup-stall]], [[qfc-lifecycle-disposal-731]] (Cleanup() is
UI-thread — no blocking wait), [[qfc677-webview2-focus-hold-outlook-keyboard]] (the park-focus routine
this reuses), [[qfc678-predictor-carry]].
