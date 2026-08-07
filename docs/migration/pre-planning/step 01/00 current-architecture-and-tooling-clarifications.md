# Step 1 authoritative clarifications

## Purpose

This document resolves terminology and architecture decisions that became clearer after the initial Step 1 notes were drafted.

Where an older Step 1 pre-planning document conflicts with this file, this file is authoritative.

# 1. Reusable tooling versus repository-local domain knowledge

The reusable Step 1 capability belongs in `drm-copilot`, but it must remain **domain-neutral**.

`drm-copilot` should own:

- generic discovery and parity agents;
- generic skills;
- schemas;
- validators;
- hooks;
- initialization;
- reports;
- generic analyzers;
- CLI, MCP, and VS Code wrappers;
- cross-ecosystem publication.

TaskMaster-specific knowledge must remain in TaskMaster:

- domain profile;
- VSTO and Outlook inventory;
- feature contracts;
- legacy coverage ledger;
- runtime characterization;
- source evidence;
- unspecified-behavior log;
- product decisions;
- source acceptance scenarios;
- source-baseline manifest.

TMW-specific knowledge must remain in TMW:

- target profile;
- current implementation inventory;
- target implementation records;
- parity matrix;
- target acceptance scenarios;
- architecture decisions;
- migration work packages;
- target evidence.

An older statement that the extension should contain “TaskMaster-specific skills, schemas, or validators” must be read as “reusable legacy-discovery capabilities plus TaskMaster-local configuration and artifacts.” Product-specific behavior must not be pushed into unrelated repositories.

# 2. Target client topology

The approved target topology is:

1. an Outlook web add-in for contextual online interaction;
2. an installable companion PWA that is the full local-first and offline TaskMaster application on desktop and mobile; and
3. a modern backend and Microsoft Graph data plane for authoritative mailbox changes and durable shared state.

The companion PWA is not merely:

- a queue;
- a diagnostics page;
- a task-only viewer;
- a fallback error screen.

For synchronized data, the PWA should eventually support local message and folder browsing, search, recommendations, portable classifier inference, task and tag work, settings, optimistic mailbox projections, durable operations, restart survival, undo before sync, reconnect, and conflict handling.

# 3. Outlook add-in and PWA offline distinction

The Outlook-hosted add-in and the independent PWA are different clients.

The Outlook add-in:

- provides current-item context through supported host APIs;
- provides concise online actions;
- can create an explicit handoff to the PWA;
- does not own the durable local mailbox replica;
- does not provide the target offline runtime in new Outlook.

The PWA:

- launches independently;
- owns the TaskMaster local replica;
- owns substantive offline workflows;
- owns the mutation outbox;
- owns pending, failure, and conflict UX;
- synchronizes after reconnect.

The PWA cannot directly rewrite Outlook's private native OST or Outlook-managed local cache through supported Office.js or Microsoft Graph APIs. It updates its own optimistic projection. After reconnect, the API applies the operation to the authoritative mailbox, and Outlook later converges through normal mailbox synchronization.

That limitation does not weaken the required PWA offline experience.

# 4. Step 1 platform dimensions

Step 1 must not use a single undifferentiated `mobile` or `offline` field where separate outcomes matter.

For every relevant target feature, record at least the applicable dimensions below.

## Connectivity and synchronization

- online behavior;
- disconnected behavior;
- restart while disconnected;
- reconnect behavior;
- conflict behavior;
- eventual authoritative commit;
- eventual Outlook convergence.

## Outlook-integrated clients

- Outlook desktop/web add-in availability;
- Outlook desktop/web add-in end-to-end behavior;
- Outlook Mobile add-in manifest availability;
- Outlook Mobile add-in task-pane launch;
- Outlook Mobile add-in end-to-end behavior.

## Companion clients

- desktop PWA online behavior;
- desktop PWA offline behavior;
- desktop PWA restart survival;
- desktop PWA reconnect behavior;
- mobile PWA installability;
- mobile PWA online behavior;
- mobile PWA offline behavior;
- mobile PWA restart survival;
- mobile PWA reconnect behavior.

## Data availability

- data required in the local replica;
- cache policy;
- body and attachment requirements;
- local model or rules requirements;
- storage durability and quota requirements.

# 5. Parity status rules

Do not classify a feature as complete because:

- a task pane renders;
- a button exists;
- an interface exists;
- a mock returns a result;
- an API scaffold exists;
- a local database exists;
- an operation can be inserted into a queue;
- a mobile manifest exists;
- a PWA manifest exists.

A feature is complete for a dimension only when the full user outcome is implemented and verified for that dimension.

Examples:

- `Outlook Mobile shell only` is not mobile feature parity.
- `PWA installs` is not offline parity.
- `PendingOperation inserted` is not offline filing parity.
- `API accepted request` is not exactly-once synchronization.
- `IndexedDB schema exists` is not restart-safe local-first behavior.

# 6. Required Step 1 target records

The TMW Step 1 prompt must treat the companion PWA as a first-class target component even if it is not yet implemented.

For each TaskMaster feature contract, the target implementation or parity record should identify:

- required contextual Outlook surface;
- required companion surface;
- required local data;
- required local business logic;
- required server behavior;
- required authoritative mailbox behavior;
- online status;
- Outlook desktop/web status;
- Outlook Mobile add-in status;
- desktop PWA status;
- mobile PWA status;
- offline status;
- restart status;
- reconnect status;
- conflict status;
- evidence;
- product decisions;
- Step 2 dependencies.

If the current TMW repository has no companion PWA or local-first implementation, record those dimensions as missing or platform-dependent. Do not silently narrow the target requirement to the existing Office add-in scaffold.

# 7. Step 1 sequencing

The authoritative order remains:

1. Complete and release the generic Step 1 and Step 2 capabilities in `drm-copilot`.
2. Push the released customizations into TaskMaster and TMW through reviewed adoption branches.
3. Run TaskMaster Step 1 discovery.
4. Complete required human characterization and product decisions.
5. Merge and pin the TaskMaster source baseline.
6. Run TMW Step 1 reconciliation against that pinned baseline.
7. Require the TMW parity matrix to identify the full add-in-plus-PWA target and all Step 2 dependencies.
8. Merge the TMW Step 1 artifacts.
9. Run the TaskMaster Step 2 oracle prompt.
10. Run the TMW Step 2 platform-foundation prompt.

# 8. Completion statement

Step 1 is complete only when:

- the TaskMaster behavior baseline is pinned;
- every source contract has a target disposition;
- the full target client topology is represented;
- Outlook add-in and PWA requirements are not conflated;
- Outlook Mobile add-in and mobile PWA requirements are not conflated;
- offline, restart, reconnect, and conflict requirements are explicit;
- missing platform capabilities are assigned to Step 2;
- no current scaffold is represented as verified parity without evidence.