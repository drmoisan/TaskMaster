# `outlook-store-exclusion` — User Story

- Issue: #328
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/328
- Owner: drmoisan
- Author: prd-feature (authoring)
- Status: Draft
- Last Updated: 2026-07-15T18-42
- Work Mode: full-feature (AC sources: `spec.md` and `user-story.md`)

## Story Statement

- As a TaskMaster user who has more than one mailbox/store open in Outlook, I want to exclude a
  specific store from all TaskMaster processing, so that the add-in does not enumerate or process
  items from a mailbox I do not use for tasks (for example a shared, archive, or delegate mailbox).
- As that user, I want to exclude the store precisely — by the store's stable identity rather than a
  fragile name or file-path substring — so that only the mailbox I intend is excluded and no other
  store is caught by accident.
- As that user, I want to toggle a store's exclusion on and off from the settings UI and have the
  choice persist across restarts, so that I never have to hand-edit the JSON config to manage
  exclusions.

## Problem / Why

`StoresWrapper` already implements a store deny-list (`ExcludedStoreNameContains`,
`ExcludedStoreFilePathContains`, public-folder and GWSO store exclusion) behind a `ShouldIncludeStore`
predicate and a `GetFilteredStores()` enumeration, persisted under the `"StoresWrapper"` config key.
Three problems make it hard for a user to reliably exclude a specific mailbox:

1. **Matching is imprecise.** Exclusion is DisplayName/FilePath substring only, which is brittle
   across Outlook profiles and can match more than the user intends. There is no way to exclude one
   specific mailbox by a stable identifier.
2. **The filter is bypassed in most places.** Only inbox loading routes through the filter. Four
   other enumeration sites iterate `Session.Stores` directly, so an excluded store is still processed
   by the to-do tree, to-do events, and project-data scanning:
   - `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs`
   - `ToDoModel/Data Model/ToDo/ToDoEvents.cs` (issue-cited sites at :112 and :156)
   - `ToDoModel/Data Model/Project/ProjectData.cs`
3. **There is no UI.** `StoreWrapperController` only edits per-store archive/junk folder assignments,
   so a user must hand-edit the JSON config to add or remove an exclusion.

## Personas & Scenarios

- **Persona: multi-mailbox knowledge worker.**
  - Who: a TaskMaster user with a primary mailbox plus one or more additional stores (a shared team
    mailbox, an archive PST, or a delegate account) open in the same Outlook profile.
  - What they care about: keeping the to-do tree, to-do events, and project data focused on their
    own actionable mailbox; not paying processing cost or seeing task noise from mailboxes they do
    not manage.
  - Constraints: they are not comfortable editing JSON config by hand and do not know a store's MAPI
    entry-ID; they expect the setting to survive an Outlook restart.
  - Goals and frustrations: today an excluded store is still scanned by the to-do tree and project
    data, and substring name matching occasionally excludes the wrong store or fails to exclude the
    intended one.
  - Context and motivations: they manage exclusions rarely, from the store settings dialog, and
    expect the choice to be durable and precise.

- **Scenario: exclude a shared mailbox from processing.**
  - Who is acting: the multi-mailbox user.
  - What triggered the action: the to-do tree and project data are showing items from a shared
    mailbox they do not want TaskMaster to process.
  - Steps: they open the store settings dialog (`StoreWrapperController`/`StoreWrapperViewer`), select
    the shared mailbox from the store list, check the new "Exclude this store" checkbox, and save.
  - Obstacles/decisions: they do not need to know the StoreID; the UI captures it. If the store's
    identity cannot be read, the checkbox is disabled so they cannot make an unsafe toggle.
  - Expected outcome: after saving, the shared mailbox is no longer enumerated or processed by inbox
    loading, the to-do tree, to-do events, or project-data scanning. The exclusion persists across
    restarts. They can later re-include the store by unchecking the box and saving.

## Acceptance Criteria

- [x] A specific store/mailbox can be excluded by StoreID, and once excluded it is not enumerated or processed by inbox loading, the to-do tree, to-do events, or project data scanning.
- [x] Exclusion persists across sessions via the StoresWrapper config.
- [x] A user can toggle a store on/off through the UI without hand-editing JSON.
- [x] New/changed code meets the repo's coverage thresholds; full toolchain (csharpier → analyzer build → nullable build → vstest) passes.

## Non-Goals

- Merging with the issue-#261 disabled-identity mechanism (`DisabledStoreIdentities` /
  `IStoreDisableService`). That is a separate, DisplayName-based, runtime session-vs-future disable
  feature for the lockup-resilience epic and is not changed by this feature.
- Deleting the two apparently-dead `ToDoEvents` methods (`GetListOfToDoItemsInView`,
  `GetToDoItemsInView`). This feature threads the filter through them for consistency; whether to
  delete them is deferred to the atomic plan and, if pursued, a separate issue.
- Cross-profile or cross-machine identity stability. `StoreID` is stable within an Outlook profile
  but not guaranteed across profile recreation, account re-add, or a different machine — the same
  per-profile scoping the existing substring lists already have.
- Removing the existing substring-based exclusion options; they remain as fallbacks behind the new
  authoritative StoreID check.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-15-outlook-store-exclusion-328/user-story.md`
- Total AC items: 4
- Checked off (delivered): 4
- Remaining (unchecked): 0
- Items remaining: none. The toolchain AC is now met — csharpier/analyzer/nullable gates pass and the
  vstest suite is functionally green (4611/4611 without instrumentation). The prior scope-conflict
  vstest failure was resolved by the in-scope P4-T4 fix (handled `get_StoresWrapper` fail-open case in
  the `OlObjectsProxy` test double).
