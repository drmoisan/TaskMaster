# `2026-10-06-build-triage-classifier` — User Story

- Issue: #979
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-10-06T19-29

## Story Statement

- As a TaskMaster user who has labelled mail for triage, I want to rebuild the Triage classifier from mined mail, so that prior labels can restore the classifier's training state.
- As a TaskMaster maintainer, I want mined mail to retain nullable Triage data, so that classifier reconstruction uses the same data semantics as item information.

## Problem / Why

Triage labels are available on `IItemInfo` but are lost from `MinedMailInfo`.
This prevents the mined-mail data set from rebuilding the classifier and leaves
users without a ribbon command to initiate that recovery workflow.


## Personas & Scenarios

- Persona: TaskMaster user with a mined-mail collection that includes prior triage decisions.
  - The user needs classifier behavior to reflect retained mail labels.
  - The user has no need to manually transform mined mail into item-information records.
  - The user expects missing or invalid labels to remain outside classifier training.
- Scenario: A user needs to restore a Triage classifier from mined mail.
  - The user opens `TaskMaster -> Settings -> Folder Classifier`.
  - The user selects `Build Triage Classifier`.
  - The command rebuilds from the available mined-mail records.
  - The rebuild uses valid A/B/C labels only, ignores missing or invalid labels, and makes the rebuilt classifier active through the existing manager behavior.


## Acceptance Criteria

- [x] `MinedMailInfo` exposes nullable Triage data and all applicable mined-mail creation and staging-load mappings preserve A, B, C, and null values from `IItemInfo`.
- [x] Rebuilding from mined mail uses valid A/B/C Triage labels only; null and invalid labels are excluded from classifier training and aggregate counts.
- [x] The rebuild initializes the existing aggregate-count and token-base state before publishing the rebuilt classifier.
- [x] Rebuild persistence and active-classifier replacement use existing configuration and manager behavior without altering established classifier behavior.
- [x] `Build Triage Classifier` appears at `TaskMaster -> Settings -> Folder Classifier` and invokes the mined-mail rebuild flow.
- [x] Unit tests cover label preservation, null and invalid-label exclusion, reconstructed classifier state, persistence/manager behavior, and the ribbon command path.


## Non-Goals

- Changing the established A/B/C classifier contract.
- Assigning a synthetic Triage label to mined mail with no label.
- Adding a CLI command, external service, or new configuration format.
- Backfilling Triage values for historic unlabelled mined-mail records.
