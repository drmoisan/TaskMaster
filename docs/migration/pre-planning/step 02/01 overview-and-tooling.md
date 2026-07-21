# Step 2 — Platform foundation overview and reusable tooling

## Conclusion

After the Step 1 discovery additions, `drm-copilot` will have the general orchestration, planning, execution, evidence, and review control plane needed to manage Step 2. It still needs a focused **platform-foundation capability pack** so that authentication, API, local-first storage, synchronization, telemetry, feature flags, PWA behavior, mobile behavior, environment configuration, and deployment are governed as one coherent platform rather than as unrelated code features.

No second agentic framework is required.

The implementation split is:

- `drm-copilot` owns reusable platform-engineering workflow, schemas, validation, and enforcement.
- TaskMaster owns the pinned legacy oracle, fixtures, characterization, and expected outcomes.
- TMW owns the product architecture and implementation.

## Critical architecture clarification

Step 2 must build around this client topology:

> **The Outlook add-in is the contextual online integration. The installable companion PWA is the full local-first and offline TaskMaster application.**

The companion PWA is not a diagnostics-only surface or a thin queue. For synchronized data it should support local message and folder browsing, search, recommendations, portable classifier inference, tasks, tags, settings, optimistic mailbox projections, durable pending operations, undo where permitted, synchronization, and conflict handling.

The PWA cannot directly modify Outlook's private native offline cache through supported Office.js or Microsoft Graph APIs. Outlook add-ins also do not run while the new Outlook is offline. The PWA therefore maintains its own TaskMaster projection, submits authoritative mailbox writes after reconnection, and Outlook later converges through its normal Exchange synchronization.

The platform foundation must make that temporary divergence explicit and safe.

## Preconditions before Step 2 implementation

Do not begin authoritative Step 2 implementation until:

1. The Step 1 and Step 2 reusable capabilities are merged into `drm-copilot`.
2. The extension and MCP package have been released.
3. The released customizations have been pushed into TaskMaster and TMW through reviewed adoption branches.
4. TaskMaster Step 1 discovery is merged and pinned to a commit.
5. TMW Step 1 parity reconciliation is merged.
6. The source feature contracts and parity matrix validate.
7. Blocking unspecified-behavior decisions are resolved.
8. Online, offline, reconnect, desktop, Outlook Mobile, and companion-PWA requirements are explicit.
9. The selected Step 2 vertical slice has approved acceptance scenarios.
10. Baseline quality gates pass in both repositories, or pre-existing failures are documented.

## Assessment by Step 2 deliverable

| Deliverable | Existing support | Reusable tooling addition | Product implementation location |
|---|---:|---:|---|
| Outlook add-in shell | Partial in TMW | Host-capability and integration contracts | TMW |
| Companion PWA shell | Missing/incomplete | PWA and offline-readiness contracts | TMW |
| Shared application core | Partial | Architecture-boundary validation | TMW |
| Authentication | Partial | Identity contract and security review | TMW |
| API baseline | Partial | API conventions and compatibility gates | TMW |
| Durable backend persistence | Prototype only | Persistence decision and migration gates | TMW |
| Local mailbox/task replica | Missing | Offline data and storage contracts | TMW |
| Mutation outbox | Missing | Operation and idempotency schemas | TMW |
| Synchronization/conflicts | Missing | Sync-protocol and conflict validators | TMW |
| Local classifier/model snapshots | Missing | Portable-model and compatibility contracts | TMW |
| Telemetry | Partial | Observability and privacy contracts | TMW |
| Feature flags | Missing/partial | Flag governance and expiry validation | TMW |
| DevOps/environment | Partial | Environment and deployment contracts | TMW |
| Legacy oracle | Step 1 inputs only | Oracle bundle schema and validator | TaskMaster |
| Integrated platform proof | Missing | Platform-foundation completion review | TMW using TaskMaster oracle |

## What `drm-copilot` already provides

The existing runtime is already suitable for coordinating an epic with:

- dependency-aware child features;
- isolated worktrees;
- persistent checkpoints;
- atomic planning and execution;
- TypeScript, modern C#, legacy C#, Python, and PowerShell specialists;
- feature, staged, and epic review;
- quality gates;
- evidence-location enforcement;
- MCP and VS Code wrappers;
- cross-ecosystem publication.

Those mechanisms should be extended, not duplicated.

## Reusable platform-foundation agents

### Platform architect

Responsibilities:

- define client and service topology;
- establish trust and data boundaries;
- make expensive or irreversible decisions explicit;
- maintain architecture decision records;
- enforce separation between Outlook host adapters, shared application logic, local storage, sync, API, and infrastructure;
- evaluate desktop, mobile, online, offline, and reconnect consequences;
- prevent implementation before required architecture decisions are approved.

### Security and identity reviewer

Responsibilities:

- OAuth/OIDC and Entra topology;
- delegated versus application permissions;
- Outlook add-in and PWA authentication;
- API audience and authorization;
- Graph on-behalf-of behavior;
- consent, expiry, revocation, logout, and account switching;
- token-cache design;
- secrets and certificates;
- threat modeling;
- telemetry redaction;
- mobile and offline security;
- least privilege.

### Local-first and synchronization reviewer

Responsibilities:

- local replica scope;
- storage technology and durability;
- schema migration;
- cache policy and quotas;
- local classifiers and model snapshots;
- mutation outbox;
- idempotency;
- ordering and retries;
- conflict detection and resolution;
- tombstones;
- restart survival;
- foreground and background synchronization;
- account/mailbox partitioning;
- local-data purge and recovery.

### PWA and mobile reviewer

Responsibilities:

- installability;
- service-worker cache behavior;
- offline launch;
- IndexedDB or selected local-store behavior;
- storage persistence and eviction handling;
- Outlook add-in to PWA handoff;
- responsive and accessible mobile UX;
- Outlook Mobile manifest and host limitations;
- foreground sync;
- physical-device verification;
- distinction between Outlook Mobile add-in parity and companion-PWA parity.

### API and operations reviewer

Responsibilities:

- API versioning;
- OpenAPI generation;
- errors;
- correlation;
- idempotency;
- concurrency;
- operation status;
- sync endpoints;
- health and readiness;
- telemetry;
- deployment;
- database migration;
- rollback;
- supportability.

### Platform-foundation reviewer

Responsibilities:

- review the integrated platform rather than isolated features;
- verify the TaskMaster-derived vertical slice;
- require evidence for desktop, offline, restart, reconnect, and mobile;
- verify telemetry and feature flags;
- verify deployability and rollback;
- fail closed on disconnected scaffolds or unverified claims.

## Reusable skills

The platform-foundation pack should include workflows equivalent to:

```text
platform-foundation-orchestrate
initialize-platform-foundation
author-architecture-decision
define-client-topology
define-domain-and-host-boundaries
design-authentication-baseline
threat-model-platform
define-api-baseline
design-local-replica
design-portable-model-contract
design-mutation-outbox
design-sync-protocol
review-conflict-resolution
define-pwa-offline-readiness
define-addin-pwa-handoff
define-mobile-platform-contract
define-observability-baseline
define-feature-flag-governance
define-environment-contract
define-deployment-and-rollback
import-legacy-oracle
review-platform-foundation
validate-platform-foundation
```

Each skill must:

- declare exact inputs and outputs;
- consume repository-local configuration;
- remain product-neutral;
- produce machine-readable artifacts;
- support deterministic validation;
- fail closed when required decisions or evidence are absent;
- route to an appropriate specialist;
- publish across Claude Code, Codex, and GitHub Copilot through existing mechanisms.

## Repository-local configuration

Consuming repositories should provide a versioned platform profile, for example:

```text
docs/migration/platform-foundation/platform-profile.yaml
```

The profile should support:

- repository role;
- source or target baseline references;
- required clients and hosts;
- online/offline/reconnect dimensions;
- mobile platforms;
- local storage requirements;
- identity topology;
- API roots;
- environment list;
- evidence roots;
- feature-flag policy;
- telemetry policy;
- selected vertical slice;
- completion gates;
- excluded capabilities and rationale.

TaskMaster's profile should identify it as a legacy oracle. TMW's profile should identify it as the modernization target.

## Required schemas

At minimum:

```text
platform-profile.schema.json
architecture-decision.schema.json
client-topology.schema.json
host-capability.schema.json
authentication-contract.schema.json
api-baseline.schema.json
api-error.schema.json
idempotent-operation.schema.json
local-replica.schema.json
local-storage-policy.schema.json
portable-model.schema.json
mutation-outbox.schema.json
sync-protocol.schema.json
conflict-policy.schema.json
pwa-offline-readiness.schema.json
addin-pwa-handoff.schema.json
mobile-platform.schema.json
observability-contract.schema.json
telemetry-event.schema.json
feature-flag.schema.json
environment-contract.schema.json
deployment-contract.schema.json
legacy-oracle-manifest.schema.json
platform-foundation-report.schema.json
```

Schemas must be versioned and allow carefully bounded product extensions without weakening core required fields.

## Required validators

Commands should be available through the authoritative Python implementation and thin CLI/MCP/VS Code wrappers. Equivalent commands include:

```text
dev.platform.init
dev.platform.validate-profile
dev.platform.validate-adrs
dev.platform.validate-client-topology
dev.platform.validate-host-capabilities
dev.platform.validate-auth
dev.platform.validate-api
dev.platform.check-api-breaking-changes
dev.platform.validate-idempotency
dev.platform.validate-local-replica
dev.platform.validate-local-storage
dev.platform.validate-model-contract
dev.platform.validate-outbox
dev.platform.validate-sync
dev.platform.validate-conflicts
dev.platform.validate-pwa
dev.platform.validate-handoff
dev.platform.validate-mobile
dev.platform.validate-observability
dev.platform.validate-feature-flags
dev.platform.validate-environments
dev.platform.validate-deployment
dev.platform.validate-oracle
dev.platform.validate-foundation
```

Validation must detect:

- missing or conflicting ADRs;
- host-bound dependencies in the shared core;
- blank required platform dimensions;
- offline claims without local data and executable workflows;
- mobile claims based only on task-pane rendering;
- use of feature flags for authorization;
- flags without owners, offline defaults, expiry, or cleanup references;
- mailbox mutations without idempotency;
- outbox entries without deterministic identity and lifecycle;
- sync contracts without cursor, tombstone, retry, and conflict behavior;
- storage designs without migrations, quota, persistence, purge, and corruption recovery;
- telemetry that contains prohibited data;
- API changes that break approved contracts;
- deployment contracts without rollback;
- completion reports without a TaskMaster oracle reference.

## Required hooks and gates

Add reusable enforcement for:

- architecture decisions before implementation;
- research-time automation-feasibility assessment;
- no production changes by architecture/research agents;
- no Outlook COM dependencies in target modules;
- no host APIs in shared application/domain modules;
- no access tokens in local domain storage;
- no mailbox mutation contract without idempotency;
- no offline-complete status without restart and reconnect evidence;
- no mobile-complete status without physical or approved host evidence;
- no PWA-complete status without offline launch and storage evidence;
- no parity status without a pinned TaskMaster oracle reference;
- no platform completion without integrated deployment and rollback evidence.

## What remains local to TaskMaster

TaskMaster should own:

- the Step 1 source baseline;
- a versioned legacy oracle manifest;
- sanitized fixture data;
- legacy characterization scenarios;
- expected outcomes;
- a read-only reference exporter;
- environment descriptions;
- behavior and fixture checksums;
- evidence from classic Outlook cached mode and reconnect;
- explicit semantic-difference decisions.

TaskMaster should not acquire:

- modern authentication;
- a new API;
- a PWA;
- feature flags;
- a modern sync engine;
- target deployment infrastructure;
- a production dependency on TMW.

## What remains local to TMW

TMW should own:

- the shared TaskMaster application core;
- Outlook desktop/web/mobile host adapters;
- the installable companion PWA;
- service worker and application-shell cache;
- local storage and migrations;
- local model snapshots and inference adapters;
- mutation outbox;
- synchronization and conflicts;
- add-in to PWA handoff;
- authentication;
- API and Graph adapters;
- durable backend persistence;
- telemetry;
- feature flags;
- environments and infrastructure as code;
- deployment and rollback;
- all target tests and parity status.

## Additional TMW-local test tools

TMW should contain:

- development environment bootstrap;
- local identity/test provider;
- API contract harness;
- local-store migration harness;
- sync and conflict simulator;
- network failure injection;
- deterministic Graph adapter or emulator;
- telemetry test sink;
- feature-flag test provider;
- PWA installation/offline harness;
- service-worker update tests;
- storage pressure and eviction tests where feasible;
- mobile viewport and physical-device runbooks;
- end-to-end deployed smoke environment.

## Correct integrated completion gate

Step 2 must prove this sequence:

```text
Outlook add-in online
    -> authenticate
    -> acquire selected-message context
    -> create single-use handoff
    -> open installed companion PWA
    -> persist message/folder scope locally
    -> disconnect
    -> launch PWA independently
    -> search and classify locally
    -> perform filing action offline
    -> atomically update local projection and outbox
    -> close/restart PWA
    -> pending state survives
    -> reconnect
    -> synchronize through API
    -> API applies operation exactly once through Graph
    -> PWA reconciles delta and operation state
    -> Outlook later displays server result
    -> telemetry correlates the full workflow
    -> feature flag can disable the slice safely
```

Required platforms:

- desktop Outlook add-in online;
- desktop installed PWA offline;
- desktop restart and reconnect;
- Outlook Mobile add-in online;
- installed mobile PWA offline;
- mobile foreground reconnect;
- telemetry provider unavailable;
- feature-flag provider unavailable;
- API timeout and ambiguous response;
- remote move/delete conflict;
- rollback or kill-switch exercise.

## Publication and adoption sequence

1. Complete the `drm-copilot` platform-foundation feature.
2. Run all language, MCP, extension, conversion, and publication tests.
3. Merge to `drm-copilot/main`.
4. publish a new extension and MCP release;
5. push released customizations to TaskMaster and TMW on adoption branches;
6. reconcile local policies and generated files;
7. run customization validation;
8. merge adoption pull requests;
9. complete Step 1 in TaskMaster and TMW;
10. run the Step 2 TaskMaster oracle prompt;
11. run the Step 2 TMW platform-foundation prompt.

## Step 2 documents

- `02 repository-work-architecture-and-sequencing.md` defines the repository work and dependency waves.
- `03 drm-copilot-platform-foundation-prompt.md` is the reusable tooling prompt.
- `04a TaskMaster-prompt.md` creates the legacy oracle and fixtures.
- `04b TMW-prompt.md` implements and validates the target platform foundation.
