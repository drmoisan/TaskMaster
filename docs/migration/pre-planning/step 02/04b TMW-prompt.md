# Objective

Perform the TMW portion of Step 2: build and prove the **modern platform foundation** for TaskMaster.

The target product has three cooperating runtime surfaces:

1. an Outlook web add-in for contextual online interaction;
2. an installable companion PWA that is the full local-first and offline TaskMaster application on desktop and mobile; and
3. a modern backend and Microsoft Graph data plane for authoritative mailbox operations, durable shared state, model distribution, automation, audit, and operations.

This step establishes the platform and proves it with one TaskMaster-derived vertical slice. It does not migrate the entire TaskMaster feature inventory.

# Architectural invariant

> The companion PWA is the offline TaskMaster application. The Outlook add-in is the contextual Outlook integration.

The PWA must not be reduced to a diagnostics screen or opaque command queue. For synchronized data it must support substantive TaskMaster behavior locally, including:

- independent launch;
- local message and folder browsing;
- local search;
- recent and predicted destinations;
- portable classifier or rules inference where selected;
- task and tag workflows;
- local settings;
- optimistic mailbox projections;
- durable pending operations;
- restart survival;
- undo or cancellation before synchronization where permitted;
- sync status;
- failure and conflict handling.

The PWA cannot directly modify Outlook's private native offline cache through supported Office.js or Microsoft Graph APIs. It updates its own TaskMaster projection immediately, submits authoritative operations after reconnection, and Outlook later converges through normal Exchange synchronization.

Do not weaken this architecture because the Outlook-hosted add-in cannot run offline.

# Required framework

Use the released `drm-copilot` Step 1 discovery capability and Step 2 platform-foundation capability that have been pushed into this repository.

Use the actual released names and paths for:

- platform-profile initialization and validation;
- architecture decisions;
- client and host topology;
- authentication and threat modeling;
- API contracts;
- local replica and storage contracts;
- portable model contracts;
- durable operation, outbox, sync, and conflict contracts;
- PWA offline readiness;
- contextual-client handoff;
- mobile platform contracts;
- observability;
- feature flags;
- environments, deployment, and rollback;
- TaskMaster oracle import;
- integrated platform review;
- completion validation.

Do not recreate equivalent local agents, schemas, validators, or orchestration workflows.

# Prerequisite gate

Before authoritative implementation:

1. Verify that the released Step 1 and Step 2 agents, skills, schemas, validators, and MCP or CLI tools are installed and callable.
2. Record:
   - TMW repository identity;
   - current branch;
   - current commit SHA;
   - `drm-copilot` release version;
   - `drm-copilot` source commit SHA;
   - discovery schema version;
   - platform-foundation schema version.
3. Verify the working tree is clean apart from the intended Step 2 branch or epic worktrees.
4. Run the complete TMW baseline quality gates.
5. Locate and validate the merged TMW Step 1 parity matrix.
6. Locate the TaskMaster Step 2 oracle bundle.
7. Verify:
   - TaskMaster repository identity;
   - pinned TaskMaster commit SHA;
   - oracle manifest version;
   - oracle manifest checksum;
   - source contract checksums;
   - fixture checksums;
   - expected-result checksums;
   - TaskMaster oracle completion status.
8. Verify the TaskMaster source checkout, when used, is at exactly the pinned commit and is read-only for this workflow.
9. Confirm blocking product decisions for the selected vertical slice are approved.
10. Confirm required online, offline, restart, reconnect, desktop, Outlook Mobile, and companion-PWA acceptance scenarios exist.
11. Confirm required human-only dependencies have explicit runbooks, including as applicable:
    - Entra app registration;
    - delegated permission consent;
    - development tenant access;
    - physical mobile devices;
    - HTTPS hosting;
    - cloud subscriptions;
    - secret-store access;
    - production-like deployment permissions.
12. Initialize or validate the TMW platform profile and confirm the repository role is `modernization-target` or the released equivalent.

If the TaskMaster oracle is missing, stale, checksum-mismatched, or incomplete, stop authoritative implementation. A clearly labeled target-platform audit may proceed, but the integrated parity slice must not be marked complete.

# Required operating mode

Treat this as an epic-scale platform implementation.

Use the repository's epic planning and execution lifecycle:

1. Audit existing TMW platform components.
2. Approve architecture decisions.
3. Establish shared platform contracts.
4. Implement independent foundation workstreams in dependency order.
5. Integrate the workstreams through one TaskMaster-derived slice.
6. Run failure injection and physical or approved-host verification.
7. Deploy to an approved nonproduction environment.
8. Verify rollback.
9. Perform integrated platform review.
10. Update the Step 1 parity matrix with verified results.

Use isolated child worktrees and an epic integration branch where supported.

Persist orchestration state after each material phase and child feature.

Do not merge disconnected scaffolds to the epic completion state merely because their individual unit tests pass.

# Step 2 baseline record

Create and validate a baseline equivalent to:

```yaml
schemaVersion: 1

drmCopilot:
  version: "<released-version>"
  commit: "<sha>"

taskMaster:
  repository: "drmoisan/TaskMaster"
  commit: "<pinned-oracle-sha>"
  oracleManifest: "<path>"
  oracleChecksum: "<sha256>"

tmw:
  repository: "drmoisan/TMW"
  commit: "<step-1-merged-sha>"
  parityMatrix: "<path>"
  parityChecksum: "<sha256>"

platformFoundation:
  selectedVerticalSlice: "ifile"
  clientTopology: "outlook-addin-plus-local-first-pwa"
  authoritativeMailbox: "microsoft-365"
  offlineHost: "companion-pwa"
```

Use the actual released schema and canonical paths.

Do not use floating branches as source references.

# Phase 0 — current-platform audit

TMW is not a blank repository. Inventory the current code and classify each relevant component as:

- retain unchanged;
- retain and harden;
- replace;
- migrate behind a new interface;
- prototype only;
- remove.

At minimum inspect:

- Office add-in manifests;
- Outlook desktop/web command and task-pane surfaces;
- Outlook Mobile add-in-only manifest;
- task-pane and iFile host wiring;
- host-neutral TypeScript modules;
- API client generation;
- ASP.NET Core API;
- application, domain, classifier, and infrastructure projects;
- Microsoft Identity Web and Graph integration;
- correlation middleware;
- OpenAPI generation;
- user-settings persistence;
- training-feedback persistence;
- iFile workflow;
- current mobile Dev Tunnel tooling;
- TypeScript and .NET test infrastructure;
- architecture tests;
- quality tiers and CI;
- deployment, signing, and release tooling;
- active and archived architecture documents.

Expected starting classifications include, subject to actual evidence:

| Existing component | Expected treatment |
|---|---|
| Office.js task pane | Retain; isolate behind a host adapter |
| Outlook Mobile add-in-only manifest | Retain; add canonical generation or parity validation |
| Unified/JSON manifest | Retain for supported desktop/web surfaces |
| Host-neutral iFile logic | Reuse where contracts remain valid |
| ASP.NET Core API | Retain and harden |
| Microsoft Identity Web and Graph OBO | Retain; productionize credentials and token cache |
| Correlation middleware | Retain and extend into distributed tracing |
| OpenAPI | Retain, version, and check compatibility |
| Current iFile server workflow | Reuse as the platform vertical slice |
| In-memory training feedback | Replace for production |
| JSON-file user settings | Keep only as development/test support or replace |
| Dev Tunnels | Development-only |
| Existing CI | Extend for PWA, local-store, sync, deployment, security, and offline tests |

Produce a machine-readable audit and a human-readable report.

# Phase 1 — mandatory architecture decisions

Do not begin major implementation until the following ADRs are approved or explicitly marked nonblocking by policy.

## 1. Client topology

Decide and document:

- Outlook add-in clients;
- independently installed companion PWA;
- desktop and mobile browser/PWA scope;
- shared application core;
- host-specific adapters;
- transitional legacy fallback;
- unsupported clients.

## 2. Shared application and host boundaries

Define:

- pure domain modules;
- application workflows;
- host adapters;
- local-storage adapters;
- API adapters;
- model adapters;
- UI sharing policy;
- prohibited imports and references.

Shared domain and application modules must not depend on Office.js, browser globals, service-worker globals, ASP.NET hosting, or product infrastructure directly.

## 3. Offline semantic contract

Define strong offline parity for synchronized data:

- what can be viewed locally;
- what can be searched locally;
- what business rules run locally;
- which classifiers run locally;
- which edits commit locally;
- which mailbox operations are optimistic and pending;
- restart behavior;
- undo/cancel behavior;
- reconnect behavior;
- conflict behavior;
- user-visible state labels;
- explicit limitations.

Do not define offline parity as “commands can be queued.”

## 4. Local replica scope and retention

Decide:

- folders synchronized;
- message time range and count limits;
- message metadata;
- body preview and full-body policy;
- attachment prefetch policy;
- task/tag/settings scope;
- local model packages;
- retention and purge;
- privacy classes;
- user controls.

## 5. Local storage technology

Conduct a technology spike on supported desktop and mobile targets.

Verify:

- availability;
- transaction behavior;
- quota;
- persistence requests;
- eviction;
- migrations;
- performance;
- corruption recovery;
- private/restricted mode behavior;
- testability.

## 6. Authentication topology

Decide:

- Outlook add-in token path;
- PWA token path;
- API audience;
- tenant and account behavior;
- Graph on-behalf-of behavior;
- consent;
- token-cache implementation;
- logout and purge;
- development identity;
- production credentials;
- mobile authentication.

## 7. API and operation contract

Decide:

- versioning;
- endpoint grouping;
- errors;
- correlation;
- idempotency;
- optimistic concurrency;
- durable operation status;
- generated clients;
- compatibility policy;
- sync endpoints;
- health and readiness.

## 8. Backend persistence

Decide:

- database technology;
- schema ownership;
- operation repository;
- settings and metadata storage;
- classifier/model storage;
- migrations;
- retention;
- backup and restore;
- concurrency.

## 9. Synchronization and conflict policy

Decide:

- bootstrap and incremental sync;
- folder/message cursor model;
- push/pull sequence;
- outbox lifecycle;
- retry and ordering;
- tombstones;
- duplicate handling;
- remote deletion and moves;
- conflict classes;
- automatic versus user resolutions;
- foreground sync;
- optional background optimization.

## 10. Portable classifier or rules strategy

For the selected slice, decide whether local inference uses:

- TypeScript;
- WebAssembly;
- a portable model runtime;
- a synchronized rules package;
- another approved mechanism.

Define package versioning, checksum, feature-schema compatibility, update, rollback, and feedback synchronization.

## 11. Outlook add-in to PWA handoff

Define an explicit protocol.

Do not assume the Outlook-hosted webview and installed PWA share the same IndexedDB or local-storage partition.

Prefer:

- an opaque short-lived single-use token;
- explicit API registration and redemption;
- data minimization;
- replay prevention;
- fallback to browser when the PWA is not installed;
- audit and telemetry.

## 12. Mobile support

Separate:

- Outlook Mobile add-in behavior;
- installed companion-PWA behavior;
- mobile browser fallback;
- physical-device verification;
- storage and foreground-sync behavior;
- platform-specific limitations.

## 13. Telemetry and privacy

Define:

- event catalog;
- logs, traces, metrics;
- client/server/operation correlation;
- prohibited data;
- redaction;
- sampling;
- offline buffering;
- retention;
- diagnostic export;
- dashboards and alerts.

## 14. Feature flags

Define:

- provider;
- local deterministic test provider;
- cached snapshot;
- offline defaults;
- safe startup defaults;
- owners;
- expiry;
- cleanup;
- telemetry;
- kill switches;
- authorization prohibition.

## 15. Environment, deployment, and rollback

Define:

- local, test, staging, and production environments;
- configuration schema;
- secret references;
- static PWA/add-in hosting;
- API hosting;
- database provisioning;
- migration execution;
- release identity;
- smoke tests;
- rollout;
- rollback;
- disaster recovery;
- support ownership.

# Phase 2 — shared platform contracts

Create a foundational change that introduces contracts and architecture tests before broad implementations diverge.

Recommended TypeScript interfaces include equivalents of:

```typescript
interface HostContext
interface ConnectivityMonitor
interface LocalStore
interface LocalReplicaRepository
interface MutationOutbox
interface SyncCoordinator
interface TokenBroker
interface TelemetrySink
interface FeatureFlagProvider
interface ModelPackageProvider
interface Clock
interface OperationIdProvider
```

Recommended server-side interfaces include equivalents of:

```csharp
public interface ICurrentUser
public interface IOperationRepository
public interface IIdempotencyStore
public interface ISyncCursorRepository
public interface IConflictRepository
public interface IConflictResolver
public interface IFeatureFlagEvaluator
public interface IModelPackageRepository
```

Names must follow repository conventions.

This phase should provide:

- contracts;
- schemas;
- state machines;
- architecture tests;
- deterministic test fixtures;
- minimal adapters only where needed to prove contracts.

Do not implement broad TaskMaster features in this phase.

# Phase 3 — parallel platform workstreams

After contracts merge, execute dependency-aware child features.

## A. Shared application core and client shells

Create a structure equivalent to:

```text
src/
  core/
    domain/
    application/
    classifiers/
    filing/
    tasks/
    tags/

  platform/
    auth/
    local-store/
    sync/
    telemetry/
    feature-flags/
    models/

  hosts/
    outlook-addin/
      desktop/
      mobile/
      handoff/

    companion-pwa/
      shell/
      service-worker/
      install/
      offline-readiness/
```

Adapt to current repository conventions rather than forcing these exact paths.

Deliver:

- shared routing and state model;
- Outlook desktop/web bootstrap;
- Outlook Mobile bootstrap;
- independent PWA bootstrap;
- capability detection;
- connectivity state;
- error boundaries;
- release/schema compatibility;
- responsive and accessible UI foundation;
- canonical or parity-validated add-in manifests;
- PWA manifest and service-worker entry point.

Do not duplicate domain logic across clients.

## B. Authentication foundation

Harden existing identity rather than replacing it without evidence.

Deliver:

- Outlook add-in token-broker adapter;
- PWA token-broker adapter;
- authenticated generated API client;
- API bearer validation;
- authorization policies;
- Graph on-behalf-of path;
- required-scope checks;
- consent-error handling;
- expired/revoked token handling;
- logout and account switching;
- deterministic test identity provider;
- production credential abstraction;
- token-redaction tests;
- sign-out local-data purge integration.

Test at least:

- valid token;
- absent token;
- wrong audience;
- wrong tenant where applicable;
- missing scope;
- expired token;
- revoked consent;
- multiple accounts;
- API authentication succeeds but downstream delegation fails;
- no tokens in telemetry or local domain storage.

## C. API baseline hardening

Retain the existing API direction and establish a durable versioned contract.

Deliver:

- explicit API versioning;
- standard error envelope;
- trace and correlation identifiers;
- idempotency-key support;
- optimistic concurrency;
- pagination and continuation conventions;
- retry and cancellation semantics;
- health endpoint;
- readiness endpoint;
- capabilities and version endpoint;
- generated OpenAPI;
- generated TypeScript client;
- compatibility check in CI;
- authorization policy tests;
- durable operation contract.

Provide platform endpoints equivalent to:

```text
POST /api/v1/operations
GET  /api/v1/operations/{operationId}
POST /api/v1/sync/push
GET  /api/v1/sync/pull?cursor=...
GET  /api/v1/capabilities
```

Use task-oriented endpoints, not raw external-service passthrough.

## D. Durable backend persistence

Replace production dependence on in-memory feedback and single JSON-file settings.

Deliver:

- production database integration;
- migrations;
- operation repository;
- idempotency store;
- settings and metadata repository;
- sync cursor/checkpoint repository;
- conflict repository;
- model package repository or object-store abstraction;
- retention and purge;
- backup/restore strategy;
- concurrency tests;
- development/test implementation where needed.

No production path may silently fall back to process-memory durability.

## E. Local replica and storage

Implement the central local-first foundation.

The logical model must include equivalents of:

```text
Account
Mailbox
Folder
CachedMessage
TaskMetadata
TagMetadata
UserSettings
ModelPackage
PendingOperation
OperationAttempt
SyncCursor
Conflict
FeatureFlagSnapshot
TelemetryEnvelope
SchemaMetadata
```

Partition all data by an approved account/tenant/mailbox boundary.

Deliver:

- explicit schema version;
- forward migrations;
- migration tests;
- atomic transaction for local projection plus outbox insertion;
- bounded caches;
- storage quota reporting;
- persistent-storage request and status;
- sign-out purge;
- corruption recovery;
- prohibited-data enforcement;
- deterministic repository abstractions;
- fixture seeding;
- storage-health and offline-readiness UI.

Do not store access or refresh tokens in the domain database.

## F. Portable model or rules packages

For the selected vertical slice, deliver a versioned package contract and at least one useful local decision path, such as folder ranking or another approved recommendation.

Deliver:

- model/rules package identifier and version;
- feature-schema version;
- checksum verification;
- runtime compatibility check;
- local inference adapter;
- model download and activation;
- rollback to prior package;
- local feedback recording;
- server feedback synchronization;
- failure and stale-package behavior;
- privacy classification.

Do not claim general offline classifier parity from a placeholder rule.

## G. Mutation outbox and synchronization

Implement a durable state machine equivalent to:

```text
pending
  → sending
  → acknowledged
  → completed

pending/sending
  → retryable-failure
  → pending

pending/sending
  → conflict

pending/sending
  → permanent-failure

eligible nonterminal state
  → cancelled
```

Each operation must include:

- stable operation identifier;
- stable idempotency key;
- account/mailbox partition;
- type and payload version;
- preconditions;
- created timestamp through an injected clock;
- attempt count;
- lifecycle state;
- last error;
- conflict reference;
- server result.

Required guarantees:

- local projection and outbox record commit atomically;
- operation survives process and device restart;
- deterministic replay order;
- safe duplicate submission;
- bounded exponential retry;
- foreground reconnect synchronization;
- explicit permanent failure;
- explicit conflict records;
- server acknowledgement before local outbox cleanup;
- user-visible pending, failed, and conflict states;
- safe account/mailbox switching;
- schema migration with pending operations.

Correctness must not depend on browser background synchronization.

## H. Contextual add-in to PWA handoff

Implement and test an explicit handoff.

A typical sequence is:

1. Contextual client reads an approved normalized item reference.
2. Client registers a handoff with the API.
3. API returns an opaque short-lived single-use token and app URL.
4. Client opens the installed PWA or browser fallback.
5. PWA redeems the token.
6. PWA persists normalized context in its local replica.
7. Token expires and cannot be replayed.

Requirements:

- no access token in URL;
- no message body in URL;
- no real subject or address in URL;
- single use;
- short expiry;
- account/mailbox validation;
- replay prevention;
- cancellation and expiry UX;
- browser fallback;
- audit and telemetry;
- automated contract tests;
- physical or approved-host test.

## I. Telemetry and diagnostics

Implement telemetry before the integrated slice so the slice is observable.

Deliver:

- event catalog;
- structured client/server logs;
- distributed trace propagation;
- operation and handoff correlation;
- metrics;
- release and environment identity;
- privacy classes;
- redaction;
- sampling;
- bounded local offline telemetry buffer;
- upload retry and drop behavior;
- diagnostic export;
- health dashboards;
- alert definitions and owners;
- local test sink.

Include events equivalent to:

```text
application_started
host_detected
authentication_started
authentication_succeeded
authentication_failed
local_store_opened
local_store_migrated
offline_readiness_changed
handoff_created
handoff_redeemed
operation_queued
sync_started
operation_sent
operation_completed
operation_conflicted
sync_failed
feature_flag_evaluated
```

Do not log subjects, message bodies, addresses, attachment names, tokens, or raw provider identifiers unless an approved privacy design explicitly permits a protected representation.

## J. Feature flags

Deliver:

- provider-neutral interface;
- deterministic local/test provider;
- production adapter;
- cached flag snapshot;
- offline fallback;
- safe startup defaults;
- owner;
- purpose;
- expiry;
- cleanup reference;
- telemetry;
- environment targeting;
- kill-switch behavior.

Initial platform flags may include equivalents of:

```text
platform.companion-enabled
platform.local-store-enabled
platform.sync-enabled
platform.offline-mutations-enabled
platform.telemetry-upload-enabled
platform.vertical-slice-v2-enabled
```

Flags must never be used as authorization controls.

## K. Environment and deployment skeleton

Deliver:

- prerequisite verifier;
- one-command local bootstrap;
- deterministic data seed;
- local identity/test provider;
- local telemetry sink;
- local feature-flag provider;
- local backend persistence;
- environment schema validation;
- infrastructure as code;
- static add-in/PWA hosting;
- API hosting;
- database provisioning;
- secret-store integration;
- migration execution;
- artifact and release versioning;
- security and dependency scanning;
- deployment workflow;
- smoke tests;
- rollback workflow;
- support diagnostics;
- operations runbook.

Do not postpone deployability until after all feature work.

# Phase 4 — integrated TaskMaster-derived vertical slice

Use the selected TaskMaster oracle slice. The default recommendation is filing/iFile.

The vertical slice must pass through:

- Outlook add-in;
- companion PWA;
- shared application core;
- authentication;
- API;
- durable backend persistence;
- local replica;
- local recommendation or classifier path;
- mutation outbox;
- synchronization;
- telemetry;
- feature flags;
- deployment and rollback.

# Required online flow

For a filing slice:

1. Open the add-in in a supported Outlook desktop/web host.
2. Authenticate.
3. Read selected-message context through the host adapter.
4. Load cached folders immediately when available.
5. Refresh data through the API.
6. Produce search and recommendation results.
7. Select a destination.
8. Submit a filing operation with an idempotency key.
9. Apply the operation through the backend and external mailbox adapter.
10. Persist the durable result.
11. Reconcile the local projection.
12. Verify correlated telemetry.
13. Verify the feature flag can disable or roll back the new slice safely.

# Required desktop offline PWA flow

1. Install the PWA.
2. Authenticate and complete initial synchronization.
3. Verify offline readiness.
4. Disconnect the network.
5. Close the Outlook host.
6. Launch the PWA independently.
7. Browse cached messages and folders.
8. Search folders locally.
9. Produce a local recommendation or approved rule result.
10. Choose a destination.
11. Commit the local projection and outbox atomically.
12. Display the message in the projected destination with `pending synchronization` state.
13. Close the PWA.
14. Restart it while still offline.
15. Verify the same projection and pending operation remain.
16. Exercise undo or cancellation before submission where supported.
17. Restore connectivity.
18. Bring the PWA to the foreground.
19. Synchronize the operation.
20. Verify the backend applies it exactly once.
21. Verify the local cursor/checkpoint advances.
22. Verify the pending state becomes committed.
23. Verify the Outlook client eventually reflects the server-side result after its own mailbox synchronization.

Do not claim that the PWA updated Outlook's native cache during the disconnected period.

# Required mobile flow

Validate both mobile surfaces separately.

## Outlook Mobile add-in

Verify:

- add-in-only manifest installation;
- message-read activation;
- full-screen task-pane behavior;
- selected-item context;
- authentication;
- online API access;
- concise contextual actions;
- “Open in TaskMaster” or equivalent handoff;
- capability checks;
- failure messages.

## Companion mobile PWA

Verify:

1. Install the PWA on an approved iOS or Android device.
2. Complete initial sync.
3. Launch TaskMaster from Outlook Mobile through the handoff.
4. Persist the normalized item locally.
5. Enable airplane mode.
6. Close Outlook.
7. Launch the PWA independently.
8. Find the synchronized item.
9. Search or rank destinations locally.
10. Perform the filing decision.
11. Verify optimistic local projection and pending status.
12. Force-close the PWA.
13. Reopen while offline.
14. Verify restart survival.
15. Restore connectivity.
16. Bring the PWA to the foreground.
17. Verify exactly-once synchronization.
18. Open Outlook Mobile.
19. Verify Outlook reflects the server-side result after normal sync.
20. Verify end-to-end telemetry correlation.

Foreground synchronization is mandatory. Background synchronization may improve latency but must not be required for correctness.

# Required conflict and failure scenarios

Use the TaskMaster oracle and target contracts to test at least the applicable cases:

- no network;
- intermittent network;
- API timeout before acknowledgement;
- API timeout after server commit;
- duplicate submission;
- out-of-order response;
- local process termination;
- device restart;
- schema upgrade with pending operations;
- stale cursor/checkpoint;
- message moved by another client;
- message deleted by another client;
- destination renamed;
- destination deleted;
- account switched;
- consent revoked;
- permissions reduced;
- token expired;
- telemetry provider unavailable;
- feature-flag provider unavailable;
- local storage quota pressure;
- local storage persistence denied;
- service-worker update during pending work;
- model package stale or incompatible;
- partial attachment/export failure;
- rollback to prior application release.

Every conflict must produce a deterministic machine state and understandable user state.

# Step 1 parity update

After the integrated slice passes, update the TMW Step 1 parity artifacts.

Do not mark a feature fully implemented merely because the platform exists.

For the selected source contracts, update:

- online status;
- offline-PWA status;
- reconnect status;
- desktop status;
- Outlook Mobile add-in status;
- mobile PWA status;
- test status;
- runtime evidence;
- approved semantic differences;
- remaining blockers.

Preserve the pinned TaskMaster oracle reference and checksums.

# Testing requirements

Run the repository's complete existing quality gates plus new platform gates.

Required test classes include:

- shared-domain and application unit tests;
- architecture-boundary tests;
- authentication tests;
- API contract and compatibility tests;
- backend persistence and migration tests;
- local-store transaction and migration tests;
- portable model compatibility tests;
- outbox state-machine tests;
- idempotency tests;
- sync bootstrap and incremental tests;
- cursor/checkpoint tests;
- tombstone tests;
- conflict tests;
- handoff expiry/replay tests;
- service-worker and offline-launch tests;
- storage quota/persistence/recovery tests;
- feature-flag failure tests;
- telemetry privacy tests;
- deployment smoke tests;
- rollback tests;
- desktop end-to-end tests;
- physical or approved-host mobile tests;
- TaskMaster oracle comparison tests.

Use deterministic clocks, identifiers, network seams, and external-service adapters.

No automated test may depend on personal mailbox data, production credentials, or an uncontrolled external service.

# Security and privacy requirements

At minimum:

- no secrets in source;
- no tokens in logs;
- no access or refresh tokens in the local domain database;
- least-privilege scopes;
- explicit authorization policies;
- account/mailbox partition isolation;
- sign-out purge;
- local data classification;
- body/attachment retention controls;
- encrypted transport;
- approved at-rest strategy;
- short-lived single-use handoff;
- replay protection;
- audit trail for privileged operations;
- redacted diagnostic export;
- threat-model review with no blocking findings.

# Non-goals

Do not:

- edit TaskMaster;
- introduce a production dependency on TaskMaster binaries;
- reproduce VSTO, COM, WinForms, Ribbon, or Outlook object-model mechanics;
- directly modify Outlook's private offline cache;
- implement every TaskMaster feature;
- claim full classifier parity from a placeholder model;
- claim mobile parity because the task pane renders;
- claim offline parity because a database exists;
- claim synchronization correctness without idempotency and restart evidence;
- rely on background sync for correctness;
- store tokens in the local domain store;
- use feature flags as authorization;
- merge cloud credentials or tenant-specific secrets;
- replace existing TMW components without an evidence-backed audit;
- weaken TaskMaster oracle requirements to match incomplete TMW behavior.

# Completion criteria

Step 2 is complete only when all applicable items pass.

## Architecture and contracts

- [ ] Mandatory ADRs are approved.
- [ ] Shared-core and host boundaries are enforced.
- [ ] The platform profile and baseline validate.
- [ ] The pinned TaskMaster oracle validates.

## Clients

- [ ] Outlook desktop/web add-in launches and authenticates.
- [ ] Outlook Mobile add-in launches for the approved scenario.
- [ ] Companion PWA installs and launches independently.
- [ ] PWA launches offline from cached application assets.
- [ ] Shared application logic is not duplicated across hosts.

## Authentication

- [ ] Supported clients acquire API credentials.
- [ ] API authentication and authorization pass.
- [ ] Downstream delegated access passes.
- [ ] expiry, revocation, consent, logout, and account switching are tested.
- [ ] no token is stored or logged improperly.

## API and persistence

- [ ] Versioned OpenAPI validates.
- [ ] Generated clients are current.
- [ ] API breaking-change gate passes.
- [ ] Standard errors and correlation work.
- [ ] Durable operations and idempotency work.
- [ ] Production persistence and migrations work.
- [ ] Health and readiness are distinct.

## Local-first foundation

- [ ] Local replica is partitioned correctly.
- [ ] Local schema migrations pass.
- [ ] Data survives restart.
- [ ] Storage persistence and quota state are visible.
- [ ] Purge and corruption recovery are tested.
- [ ] At least one meaningful local recommendation or classifier path works.
- [ ] Offline behavior is more than queue insertion.

## Outbox and synchronization

- [ ] Projection and outbox commit atomically.
- [ ] Pending operations survive restart.
- [ ] Duplicate submission is harmless.
- [ ] Reconnect synchronization works in foreground.
- [ ] Cursors/checkpoints and tombstones work.
- [ ] Conflicts are persisted and visible.
- [ ] Partial failure is recoverable.
- [ ] schema upgrade with pending work is safe.

## Handoff and mobile

- [ ] Contextual-client handoff is single-use, short-lived, and redacted.
- [ ] PWA receives and stores approved normalized context.
- [ ] Mobile PWA offline flow passes on an approved physical device.
- [ ] Outlook Mobile eventually reflects the server-side result after reconnect.
- [ ] Host-shell rendering alone is not used as parity evidence.

## Observability and flags

- [ ] Client and server traces correlate.
- [ ] Offline telemetry buffering works.
- [ ] Privacy/redaction tests pass.
- [ ] Feature flags work online and offline.
- [ ] Provider failure results in safe startup behavior.
- [ ] Every flag has owner, expiry, and cleanup reference.

## Deployment and rollback

- [ ] A clean nonproduction environment can be provisioned reproducibly.
- [ ] Secrets are externalized.
- [ ] Database migrations are controlled.
- [ ] Deployment smoke tests pass.
- [ ] Rollback is executed and verified.
- [ ] Release and schema versions are visible in diagnostics.

## Integrated proof

- [ ] The TaskMaster-derived online slice passes.
- [ ] The desktop offline PWA slice passes.
- [ ] Restart and reconnect pass.
- [ ] The mobile add-in and mobile PWA flows pass.
- [ ] The operation is applied exactly once.
- [ ] The oracle comparison passes or cites an approved semantic difference.
- [ ] The Step 1 parity matrix is updated with verified evidence.
- [ ] Integrated platform review reports no blocking findings.

Do not report PASS when any required platform dimension is blank, any source checksum is stale, the PWA cannot launch offline, the operation does not survive restart, synchronization is unverified, mobile evidence is absent, or rollback has not been demonstrated.

# Deliverables

Produce:

1. Step 2 baseline manifest.
2. Current-platform audit.
3. Approved ADR set.
4. Platform contracts and schemas.
5. Shared application and host boundaries.
6. Outlook add-in shell updates.
7. Companion PWA shell and service worker.
8. Authentication foundation.
9. Versioned API and generated client.
10. Durable backend persistence.
11. Local replica and storage migrations.
12. Portable model or rules package.
13. Mutation outbox.
14. Sync and conflict engine.
15. Add-in-to-PWA handoff.
16. Telemetry and diagnostics.
17. Feature-flag foundation.
18. Environment and deployment infrastructure.
19. Desktop and mobile test harnesses.
20. Integrated TaskMaster-derived vertical slice.
21. Failure-injection evidence.
22. Deployment and rollback evidence.
23. Updated parity artifacts.
24. Operations and support runbooks.
25. Final integrated platform audit.
26. Step 2 completion report.

# Final response

Provide:

1. TMW baseline commit and `drm-copilot` version.
2. Pinned TaskMaster oracle commit and checksum.
3. Architecture decisions.
4. Existing TMW components retained, hardened, replaced, or removed.
5. Client and host topology.
6. Authentication implementation and test result.
7. API and persistence result.
8. Local replica and PWA offline-readiness result.
9. Portable model or local recommendation result.
10. Outbox, synchronization, and conflict result.
11. Outlook add-in to PWA handoff result.
12. Desktop offline/restart/reconnect evidence.
13. Outlook Mobile add-in evidence.
14. Mobile PWA offline evidence.
15. Telemetry and feature-flag result.
16. Deployment and rollback result.
17. Integrated vertical-slice result.
18. TaskMaster oracle comparison result.
19. Updated parity-matrix paths.
20. Quality-gate and review results.
21. Remaining limitations and follow-up work.
22. A clear statement of whether Step 2 is complete.

Do not begin broad Step 3 feature migration unless the integrated Step 2 completion gate passes.