# 2026-10-06-build-triage-classifier — Spec

- **Issue:** #979
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-06T19-29
- **Status:** Draft
- **Version:** 0.1

## Overview

`IItemInfo` currently contains Triage data, but `MinedMailInfo` does not
declare or retain it. This feature makes mined mail sufficient input to rebuild
the existing Triage classifier and supplies a ribbon command for that rebuild.


## Behavior

When item information is transformed into or loaded as `MinedMailInfo`, its
nullable Triage value is retained unchanged. The rebuild operation reads mined
mail records, uses only valid A, B, and C labels as training input, initializes
aggregate counts and the token base through the existing classifier workflow,
persists classifier configuration, and replaces the active classifier through
the existing manager path. The ribbon command invokes this operation from
`TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier`.

## Acceptance Criteria

- [x] `MinedMailInfo` declares nullable Triage data, and all applicable construction and staging-load mappings copy A, B, C, and null values from `IItemInfo` without conversion.
- [x] Rebuild from mined mail trains only on valid A/B/C labels; null or invalid labels are excluded and do not affect aggregate counts or token-base input.
- [x] The rebuild uses the established initialization workflow for aggregate class counts and token-base state before the classifier becomes available.
- [x] The rebuilt classifier configuration is persisted and the active classifier is replaced through existing manager behavior without regressing existing classification behavior.
- [x] The exact ribbon path `TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier` is present, and its command invokes the mined-mail rebuild operation.
- [x] Unit tests verify the mined-mail field and mappings, label filtering, rebuild state, persistence/manager integration, and ribbon command path.


## Inputs / Outputs

- Inputs: existing mined-mail records with nullable Triage values; the ribbon command selected by a user.
- Outputs: a rebuilt active Triage classifier and its persisted configuration.
- Config keys and defaults: reuse existing classifier configuration and persistence mechanisms; no new external configuration is required.
- Versioning or backward-compatibility constraints: retain existing classifier behavior and the A/B/C label contract.

## API / CLI Surface

The feature adds a ribbon command rather than a CLI or public external API.

- Command: `Build Triage Classifier`
- Location: `TaskMaster -> Settings -> Folder Classifier`
- Contract: process mined-mail records with valid A/B/C Triage labels only; omit null or invalid labels from training input.

## Data & State

Triage flows from `IItemInfo` to nullable `MinedMailInfo` storage through each
applicable creation and staging-load mapping. The rebuild consumes the mined
representation directly, filters it to valid A/B/C labels, initializes the
classifier's aggregate counts and token base, persists the rebuilt
configuration, and replaces the active classifier through the existing
manager.

- Data transformations and invariants: preserve Triage unchanged during mapping; do not convert null to a class; only A/B/C labels are classifier input.
- Caching or persistence details: use existing classifier configuration persistence and manager replacement behavior.
- Migration or backfill requirements: no historic backfill is required; unlabelled records remain excluded from rebuild input.

## Constraints & Risks

The implementation must reuse established rebuild initialization rather than
creating a second classifier state model. Ribbon wiring must follow existing
controller, viewer, and XML patterns so the command is available at the stated
location. Tests must isolate classifier and UI behavior from external services.


## Implementation Strategy

- Implementation scope: extend the mined-mail model and its mappings; add a focused mined-mail-based Triage rebuild entry point; wire the ribbon XML, controller, and viewer command path.
- New classes/functions/commands to add or update: a Triage rebuild operation that accepts mined-mail records and the `Build Triage Classifier` menu command.
- Dependency changes: none.
- Logging/telemetry additions and locations: follow existing classifier operation logging if present; no new telemetry requirement is identified.
- Rollout plan: include the command in the existing ribbon; the existing classifier workflow remains the operational fallback.

## Definition of Done

- [x] Each acceptance criterion is mapped to a unit test or direct verification.
- [x] Mined-mail Triage preservation and valid-label filtering are verified for A, B, C, null, and invalid input cases.
- [x] Rebuild initialization, persistence, and classifier-manager replacement are verified without regression to existing classifier behavior.
- [x] The ribbon command's XML/menu placement and controller/viewer invocation path are verified.
- [x] C# unit tests follow MSTest, Moq, and FluentAssertions repository conventions.
- [x] The final C# toolchain pass completes: CSharpier, analyzer build, nullable build, and VSTest with coverage. Aggregate coverage was 65.2006 percent; the user authorized a one-time exception to the 80 percent threshold for issue #979 only, recorded in `evidence/other/coverage-exception.2026-10-06T21-37.md`.

## Seeded Test Conditions (from potential)
- [ ] Unit coverage areas: model, mappings, rebuild filtering and state, persistence/manager behavior, and ribbon command wiring.
- [ ] Integration scenarios: executing the ribbon command rebuilds from mined mail and publishes the active classifier through the existing manager path.
- [ ] CLI/API examples: not applicable; the exposed surface is the specified ribbon command.
