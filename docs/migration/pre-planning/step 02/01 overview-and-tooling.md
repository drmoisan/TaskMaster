## Conclusion

**After the legacy-discovery additions, `drm-copilot` will have enough orchestration, planning, execution, evidence, and quality-control capability to manage Step 2. It will not yet have all of the specialized platform-engineering workflows needed to make Step 2 reliable and repeatable.**

You do not need another agentic framework. You should add a **platform-foundation capability pack** to `drm-copilot`, while keeping the actual architecture, infrastructure, credentials, environments, and product implementation local to TMW.

The existing repository already provides:

* large-feature orchestration;
* atomic planning and execution;
* language-specific C#, TypeScript, Python, and PowerShell engineers;
* feature and staged review;
* persistent orchestration state;
* worktree isolation;
* per-language quality gates;
* CLI, MCP, and VS Code integration;
* evidence and completion-gate enforcement.

That is enough to coordinate the work. The main gaps concern **what must be designed, proven, and operationalized during a platform foundation phase**.

# Assessment by Step 2 deliverable

| Deliverable                 | Current capability after Step 1 additions |            Additional reusable tooling needed? |
| --------------------------- | ----------------------------------------: | ---------------------------------------------: |
| Add-in shell                |                                   Partial |                                            Yes |
| Authentication              |                                   Partial |                                            Yes |
| API baseline                |                 Mostly orchestration only |                                            Yes |
| Telemetry                   |                                   Partial |                                            Yes |
| Feature flags               |                                   Partial |                                            Yes |
| Local store scaffolding     |                                   Partial | Yes, especially for offline-first requirements |
| DevOps baseline             |              General quality support only |                                            Yes |
| Front-end foundation        |             Strong implementation support |                               Modest additions |
| Back-end foundation         |             Strong implementation support |                               Modest additions |
| Architecture governance     |                                   Partial |                                            Yes |
| Environment reproducibility |                                   Partial |                                            Yes |
| Security validation         |                Insufficiently specialized |                                            Yes |

The distinction is important:

> The current agents can write these components, but the framework does not yet appear to define the reusable contracts and gates that prove they form a coherent production platform.

# What `drm-copilot` can already do

## 1. Coordinate the platform epic

The epic orchestrator can decompose the foundation into child features, manage dependencies, use isolated worktrees, and integrate the results.

A suitable decomposition would be:

```text
Platform Foundation Epic
├── Application shell
├── Authentication and authorization
├── API conventions and baseline
├── Local persistence
├── Synchronization substrate
├── Telemetry and diagnostics
├── Feature flags
├── Environment and configuration
├── CI/CD baseline
└── Platform integration review
```

## 2. Delegate language-specific implementation

The existing roster includes TypeScript, modern C#, Python, PowerShell, and legacy C# engineers.

That is enough to implement:

* React or similar front-end shell;
* Office add-in code;
* API services;
* infrastructure scripts;
* test utilities;
* build and deployment automation.

## 3. Enforce implementation quality

The repository already has language-specific change-budget routing, QA gates, output validators, and restricted PR publication.

These are useful but mostly validate code quality and process integrity—not platform coherence.

# Additional reusable capabilities needed in `drm-copilot`

I recommend one additional cross-repository package:

```text
platform-foundation
```

This should be smaller than the discovery package. It should add specialized workflows and validation, not prescribe one technology stack.

## 1. Platform Architect agent

Add a generic `platform-architect` agent.

Responsibilities:

* convert architecture requirements into bounded platform decisions;
* establish component boundaries;
* identify cross-cutting contracts;
* produce ADRs;
* ensure front end, back end, add-in, local store, and cloud services fit together;
* verify offline and mobile implications;
* identify irreversible or expensive decisions;
* avoid implementing application features.

It should explicitly evaluate:

* deployment topology;
* trust boundaries;
* online/offline ownership;
* local versus server state;
* authentication flows;
* API boundaries;
* telemetry flow;
* feature-flag evaluation;
* failure and recovery modes.

The existing `task-researcher` is useful for investigating technologies, but its purpose is general implementation research rather than enforcing platform-level architectural consistency. Its current workflow focuses on current-state analysis, alternatives, behavioral semantics, requirements mapping, and testing implications.

## 2. Security and identity reviewer

Authentication should not be handled only as a coding task.

Add a `security-identity-reviewer` agent or skill covering:

* OAuth/OIDC flow selection;
* Microsoft Entra application topology;
* delegated versus application permissions;
* token storage;
* refresh behavior;
* multi-tenant versus single-tenant configuration;
* consent;
* logout and account switching;
* least privilege;
* offline token behavior;
* mobile authentication;
* threat modeling;
* secrets handling;
* logging redaction.

This should produce a threat model and security acceptance report, not credentials or tenant-specific settings.

## 3. Offline-first architecture skill

This is the most important missing capability.

A generic skill should require explicit answers to:

* What data is stored locally?
* Which store is authoritative?
* Which operations are permitted offline?
* How are local writes recorded?
* How are operations ordered?
* How are operations made idempotent?
* What happens after retries?
* What happens after application termination?
* How are conflicts identified?
* How are conflicts resolved?
* How are tombstones represented?
* How is schema migration handled?
* How is local data encrypted?
* How is cache invalidation handled?
* What is visible on mobile?
* Which data must be prefetched?
* How is storage bounded?

The Step 1 framework will classify offline behavior, but Step 2 needs to convert those requirements into an actual local-first substrate.

Add skills such as:

```text
design-offline-data-model
design-sync-protocol
review-conflict-resolution
validate-offline-foundation
```

## 4. API baseline contract skill

Add a workflow to establish and validate:

* API versioning;
* error envelope;
* request correlation IDs;
* idempotency keys;
* pagination;
* filtering and sorting;
* concurrency tokens;
* optimistic concurrency;
* retry semantics;
* cancellation;
* authentication;
* authorization;
* rate limiting assumptions;
* OpenAPI generation;
* API compatibility checks;
* health and readiness endpoints.

The implementation can remain TMW-specific, but the contract and validator should be reusable.

Recommended reusable tools:

```text
dev.platform.validate-openapi
dev.platform.check-api-breaking-changes
dev.platform.validate-error-contract
dev.platform.validate-idempotency-contract
```

## 5. Observability baseline skill

“Telemetry” should not merely mean adding a logging library.

Create a reusable observability contract requiring:

* structured logging;
* traces;
* metrics;
* correlation across add-in, front end, API, and background processing;
* environment and release identifiers;
* privacy classification;
* redaction rules;
* sampling policy;
* offline telemetry buffering;
* retry and drop behavior;
* client crash reporting;
* health signals;
* support diagnostic bundle;
* alert ownership.

The reusable framework should validate that telemetry events have:

* stable names;
* documented properties;
* no prohibited data;
* severity;
* ownership;
* retention classification.

## 6. Feature-flag governance

Feature flags require more than a client library.

Add a generic contract covering:

* flag identifier;
* owner;
* purpose;
* default;
* targeting;
* offline default;
* mobile behavior;
* failure behavior;
* expiration date;
* cleanup issue;
* telemetry;
* security implications.

Add a validator that rejects:

* flags without owners;
* flags without offline fallback;
* permanent flags without explicit justification;
* flags used as authorization controls;
* stale flags past expiration.

## 7. Platform integration review

The existing feature review validates an individual feature. Step 2 requires a review that checks the foundation as an integrated platform.

Add a `platform-foundation-review` skill or extend epic review to validate:

* add-in starts and authenticates;
* front end reaches API;
* API validates identity;
* local store works offline;
* queued operations survive restart;
* reconnect triggers synchronization;
* telemetry correlates end to end;
* feature flags work online and offline;
* environment configuration is reproducible;
* secrets are absent from source;
* CI builds and tests all components;
* deployment produces identifiable artifacts;
* rollback is documented.

# What should remain local to TMW

The reusable extension should not implement the product platform itself.

TMW must own the following.

## Add-in shell

* Office manifest
* Ribbon commands
* Task pane
* mobile-form-factor behavior
* initialization lifecycle
* Office.js integration
* capability detection
* degraded-mode UI
* host-specific error handling

## Authentication implementation

* Entra tenant and app registration identifiers
* redirect URIs
* scopes and permissions
* environment configuration
* token-cache implementation
* account-selection behavior
* authorization policy
* backend identity validation
* deployment secrets

## API implementation

* actual endpoints;
* domain models;
* persistence layer;
* business authorization;
* API hosting;
* database;
* queues;
* migrations;
* production configuration.

## Telemetry implementation

* telemetry provider;
* instrumentation keys or connection data;
* event catalog;
* dashboards;
* alerts;
* retention;
* environment-specific sampling.

## Feature flags

* provider;
* actual flag inventory;
* targeting rules;
* environment values;
* rollout plan;
* cleanup schedule.

## Local store and sync implementation

* database technology;
* schemas;
* migrations;
* operation queue;
* tombstones;
* conflict logic;
* encryption;
* cache limits;
* synchronization protocol;
* test fixtures.

## DevOps

* cloud resources;
* deployment definitions;
* environment topology;
* GitHub environments;
* secrets;
* infrastructure-as-code;
* release process;
* rollback procedure;
* production support ownership.

# Additional TMW-local tooling needed

Even with the extension additions, TMW should contain local engineering tools.

## 1. Development environment bootstrap

For example:

```text
tools/dev/
  bootstrap.ps1
  reset-local-environment.ps1
  seed-development-data.ps1
  verify-prerequisites.ps1
```

The agentic framework can invoke these, but TMW must define them.

## 2. Identity emulator or test identity support

You need a repeatable way to test:

* authenticated user;
* expired token;
* revoked consent;
* insufficient scope;
* multiple accounts;
* offline token availability.

This may use mocks in automated testing and a documented development tenant for integration testing.

## 3. API test harness

Include:

* generated client;
* contract test suite;
* in-memory or containerized backend;
* test fixtures;
* deterministic clock;
* deterministic identifiers;
* failure injection.

## 4. Local-store and synchronization harness

This is essential.

It should simulate:

* no network;
* intermittent network;
* API timeouts;
* duplicate responses;
* out-of-order responses;
* stale version tokens;
* local process termination;
* partial synchronization;
* server deletion;
* simultaneous mobile and desktop edits;
* schema upgrade with pending operations.

## 5. Telemetry test sink

Provide a local sink that verifies emitted events without sending development data to production telemetry.

## 6. Feature-flag test provider

Provide deterministic local flag values and tests for:

* enabled;
* disabled;
* provider unavailable;
* stale cached values;
* offline fallback;
* user-targeted variation.

## 7. End-to-end smoke environment

A single command should be able to:

1. Start the API.
2. Start required dependencies.
3. seed test data;
4. configure the front end;
5. install or sideload the add-in;
6. run smoke checks;
7. produce diagnostics.

# Suggested reusable additions to `drm-copilot`

I would add the following—not another broad system.

## Agents

```text
platform-architect
security-identity-reviewer
offline-sync-reviewer
platform-foundation-reviewer
```

## Skills

```text
platform-foundation-orchestrate
author-architecture-decision
design-authentication-baseline
threat-model-platform
define-api-baseline
design-offline-data-model
design-sync-protocol
define-observability-baseline
define-feature-flag-governance
define-environment-contract
review-platform-foundation
```

## Schemas

```text
platform-profile.schema.json
architecture-decision.schema.json
authentication-contract.schema.json
api-baseline.schema.json
observability-contract.schema.json
feature-flag.schema.json
offline-data-contract.schema.json
sync-contract.schema.json
environment-contract.schema.json
platform-foundation-report.schema.json
```

## Validators

```text
dev.platform.validate-profile
dev.platform.validate-adrs
dev.platform.validate-auth
dev.platform.validate-api
dev.platform.validate-observability
dev.platform.validate-feature-flags
dev.platform.validate-offline
dev.platform.validate-sync
dev.platform.validate-environments
dev.platform.validate-foundation
```

# Platform foundation completion gate

Step 2 should not be considered complete merely because each component exists.

The gate should require a vertical slice demonstrating:

```text
Outlook add-in
    ↓
user signs in
    ↓
front end calls API
    ↓
API authenticates and returns data
    ↓
data is stored locally
    ↓
user performs one supported action offline
    ↓
action survives application restart
    ↓
reconnection synchronizes the action
    ↓
telemetry links the client action to the API operation
    ↓
feature flag can enable or disable the slice safely
```

That single vertical slice is more valuable than six disconnected scaffolds.

It should be tested in at least:

* desktop online;
* desktop cached/offline;
* disconnect followed by reconnect;
* mobile or mobile-equivalent surface where the selected Office add-in capability is supported;
* telemetry-provider unavailable;
* feature-flag-provider unavailable.

# Recommended answer

**Yes, add further tooling—but only a focused platform-foundation layer.**

After the discovery work, `drm-copilot` will already have the correct control plane:

* orchestration;
* agent delegation;
* planning;
* implementation;
* review;
* evidence;
* validation;
* cross-agent publishing.

It still needs reusable platform-engineering contracts and reviewers for:

1. architecture decisions;
2. authentication and threat modeling;
3. API conventions;
4. offline-first local storage;
5. synchronization and conflict resolution;
6. observability;
7. feature-flag governance;
8. environment reproducibility;
9. integrated platform acceptance.

The actual add-in shell, authentication configuration, API, telemetry provider, flags, local database, synchronization implementation, and DevOps resources should remain in **TMW**, not `drm-copilot`.

The largest risk is not missing a coding agent. It is completing six scaffolds independently without proving the offline-capable end-to-end vertical slice.
