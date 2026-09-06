# quickfiler-high-confidence-scan-bounds-configurable (Potential)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Draft

## Problem / Why

Issue #791 made the High Confidence first-batch deadline advisory and bounded the zero-acceptance scan with two gate-internal constants: `DefaultMaxScanWithoutAcceptance` (250 scored candidates) and `DefaultZeroAcceptanceCeiling` (120 seconds). Both values are engineering estimates derived from the observed scoring rate of roughly 2 to 3 items per second; they were not measured in a live session (see the #791 research artifact, Provenance and Unknowns). Following the #424 precedent, no settings surface was added. If live use shows the bounds are too tight (empty dialog on large low-yield views) or too loose (long waits before the dialog appears), the only remedy today is a code change.

## Proposed Behavior

Expose the two bounds as user settings alongside the existing High Confidence threshold, with the current constants as defaults: an `AppQuickFilerSettings` pair backed by `Settings.Designer.cs`, surfaced on the ribbon next to the threshold edit box, and passed into `QfcStreamingDequeueConfidenceGate` through the constructor seam that #791 already added. The launch log line already records both bounds, so tuning is observable without further logging changes.

## Acceptance Criteria (early draft)

- [ ] The scan cap and the zero-acceptance ceiling are persisted user settings with defaults 250 and 120 seconds, read by the datamodel's gate construction.
- [ ] Out-of-range or non-numeric ribbon input is rejected with the same guard pattern as the threshold edit box.
- [ ] The gate's launch log line reflects the configured values.
- [ ] Existing #791 gate tests remain green with the defaults; new tests cover the settings-to-gate plumbing.

## Constraints & Risks

- #424's ratified criterion explicitly refused a settings surface for the deadline; adopting one here reverses that decision and should be recorded as superseding it.
- Live measurement of the bounds should precede the change; the #791 live-Outlook runbook records the observed wait and is the natural input.
- Ribbon plumbing touches `RibbonViewer.cs`, `RibbonController.Intelligence.cs`, `AppQuickFilerSettings.cs`, `IAppQuickFilerSettings`, `app.config`, and `Settings.settings`.

## Test Conditions to Consider

- [ ] Unit coverage areas: settings round-trip; gate construction receives configured values; ribbon input validation.
- [ ] Integration scenarios: launch with modified bounds and confirm the launch log line.
- [ ] CLI/API examples: none.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-high-confidence-scan-bounds-configurable/` folder from the template
