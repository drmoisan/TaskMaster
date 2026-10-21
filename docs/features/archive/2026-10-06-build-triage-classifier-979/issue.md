# Build Triage Classifier (Issue #979)

- Date captured: 2026-10-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/build-triage-classifier/ (Issue #979)

- Issue: #979
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/979
- Last Updated: 2026-10-06
- Work Mode: full-feature

## Problem / Why

`IItemInfo` captures a Triage label, but `MinedMailInfo` does not retain that
value when mail is mined or staged. As a result, mined mail cannot serve as
the durable input for rebuilding the Triage classifier.

## Proposed Behavior

Add nullable Triage storage and copy behavior to `MinedMailInfo`. Provide a
rebuild operation that constructs the Triage classifier from mined mail using
only valid A, B, and C labels, then expose that operation through the ribbon at
`TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier`.

## Acceptance Criteria

- [x] `MinedMailInfo` declares nullable Triage data and every applicable mined-mail construction and staging-load mapping preserves A, B, C, and null values from `IItemInfo`.
- [x] A Triage classifier can be rebuilt from `MinedMailInfo` records using only valid A, B, and C labels; null and invalid labels do not contribute training data.
- [x] Rebuild initializes aggregate counts and token-base state, persists resulting classifier configuration, and replaces the active classifier without changing existing classifier behavior.
- [x] The ribbon exposes `Build Triage Classifier` at `TaskMaster -> Settings -> Folder Classifier`, and invoking it reaches the Triage rebuild operation.
- [x] Unit tests cover mined-mail Triage preservation, valid-label filtering, classifier reconstruction state, and the ribbon command path.

## Constraints & Risks

The rebuild must retain the existing A/B/C classifier contract and must not
coerce missing Triage values into a class. Existing configuration persistence
and classifier-manager replacement behavior must continue to work after the
new rebuild path is used.

## Test Conditions to Consider

- [x] Mined-mail copy and staging mappings preserve A, B, C, and null Triage values.
- [x] Rebuild includes only valid A/B/C labels and initializes aggregate counts and the token base.
- [x] Configuration persistence and active-classifier replacement retain existing behavior.
- [x] Ribbon XML, controller, and viewer wiring place and invoke the named command.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create the active feature folder and complete the feature documents.
