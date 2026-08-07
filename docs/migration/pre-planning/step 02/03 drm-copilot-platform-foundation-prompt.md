# Objective

Develop a reusable **platform-foundation** capability in the `drm-copilot` repository.

The capability must let an orchestrator design, implement, validate, and review a modern application foundation whose components may include:

- a contextual host integration;
- an independently launchable local-first client;
- desktop and mobile web clients;
- a backend API;
- authentication and authorization;
- durable backend persistence;
- a scoped local replica;
- portable rules or model packages;
- a durable mutation outbox;
- synchronization and conflict handling;
- telemetry and diagnostics;
- feature flags;
- environment configuration;
- infrastructure, deployment, rollback, and operations.

The immediate consumers are `drmoisan/TaskMaster` and `drmoisan/TMW`, but the implementation must remain product-neutral. Core agents, skills, schemas, validators, hooks, templates, commands, and examples must not contain TaskMaster-, TMW-, Outlook-, email-, VSTO-, COM-, Microsoft-Graph-, or Microsoft-365-specific behavior.

The new capability must extend the existing `drm-copilot` orchestration model. Do not build a second orchestration framework.

# Required operating mode

Treat this as a large, cross-cutting feature.

Use the repository's standard large-feature or epic lifecycle:

1. Promote or create the active feature or epic folder.
2. Research the existing runtime, canonical customization source, publication model, and relevant platform-engineering practices.
3. Produce a detailed specification.
4. Produce a dependency-aware atomic plan.
5. Execute through delegated specialist agents and isolated worktrees where appropriate.
6. Run all applicable Python, TypeScript, PowerShell, C#, schema, contract, extension, MCP, and publication gates.
7. Perform feature and integrated-platform review.
8. Publish complete evidence and a consumer handoff.

The orchestrator must coordinate and integrate. It must not directly implement work assigned to an available specialist.

Persist orchestration state after every material phase and child feature transition.

Do not modify canonical policy sources through ad hoc edits. Use the repository's established policy-authoring, generation, mirroring, conversion, and push-down mechanisms.

# Architectural intent

The framework must support products that split responsibilities across several clients and services. In particular, it must be able to represent this generic pattern without hard-coding it:

```text
Contextual host client
        │
        ├── online contextual commands
        └── explicit handoff
                │
                ▼
Independent local-first client
        │
        ├── local replica
        ├── local application logic
        ├── durable outbox
        └── conflict and sync UX
                │
                ▼
Authenticated API and workers
        │
        ├── durable operations
        ├── authoritative service state
        ├── external-system adapters
        └── observability and rollout controls
```

The framework must preserve these distinctions:

1. host-integrated behavior;
2. shared application behavior;
3. local optimistic projection;
4. unsynchronized local intent;
5. authoritative remote state;
6. server-committed operation result;
7. conflict state;
8. platform capability or limitation;
9. product requirement;
10. implementation and verification status.

A local-first client must not be reduced to an opaque queue. The contracts must support substantive offline behavior against synchronized data, including local search, local business rules, local model inference where applicable, local editing, optimistic state, restart survival, undo before synchronization, and conflict handling.

# Scope

Implement a reusable capability with the working name:

```text
platform-foundation
```

Use a different final name only when repository conventions justify it.

The capability must cover the following areas.

# 1. Generic specialist agents

Create reusable agents for at least the following responsibilities.

## Platform architect

Responsibilities:

- define client, service, worker, data, and deployment topology;
- establish trust and authority boundaries;
- distinguish host adapters from shared application logic;
- identify expensive or difficult-to-reverse decisions;
- author and reconcile architecture decision records;
- identify required platform capabilities and known platform limitations;
- assess online, offline, restart, reconnect, desktop, and mobile consequences;
- prevent implementation before blocking architecture decisions are approved;
- avoid production implementation work.

## Security and identity reviewer

Responsibilities:

- OAuth/OIDC topology;
- client and API identity boundaries;
- delegated versus application permissions;
- token acquisition, refresh, expiry, revocation, and logout;
- account and tenant switching;
- API audience and authorization policy;
- downstream on-behalf-of or delegated access;
- token-cache design;
- secrets, keys, and certificates;
- least privilege;
- threat modeling;
- telemetry redaction;
- offline and mobile security;
- security acceptance evidence.

The agent must not create or commit credentials.

## Local-first and synchronization reviewer

Responsibilities:

- local replica scope;
- local storage technology and durability;
- schema migration;
- storage quota and eviction behavior;
- cache retention and purge;
- portable model or rules snapshots;
- mutation outbox semantics;
- operation identity and idempotency;
- ordering, retries, cancellation, and restart survival;
- synchronization cursors and checkpoints;
- tombstones and deletion semantics;
- conflict detection and resolution;
- account and data partitioning;
- foreground and optional background synchronization;
- local-data corruption and recovery;
- evidence that offline claims are executable rather than aspirational.

## PWA and mobile reviewer

Responsibilities:

- installability;
- service-worker application-shell caching;
- offline launch;
- structured local storage;
- storage persistence and eviction handling;
- local-first mobile UX;
- responsive and accessible behavior;
- contextual-host-to-companion handoff;
- mobile host capability classification;
- physical-device or approved-host verification;
- distinction between host-shell availability and end-to-end feature parity;
- foreground synchronization as the correctness path;
- background synchronization only as an optimization.

This agent must remain generic. It may reason about a contextual host and companion client, but must not encode Outlook or TaskMaster concepts.

## API and operations reviewer

Responsibilities:

- API versioning;
- OpenAPI or equivalent contract generation;
- standard error envelopes;
- correlation and trace propagation;
- idempotency;
- optimistic concurrency;
- pagination, filtering, and continuation;
- cancellation and retries;
- durable operation status;
- synchronization endpoints;
- health and readiness;
- persistence migrations;
- environment contracts;
- infrastructure and deployment;
- rollback and disaster recovery;
- operational supportability.

## Platform-foundation reviewer

Responsibilities:

- review the integrated platform rather than isolated components;
- validate the selected end-to-end vertical slice;
- require evidence for online, offline, restart, reconnect, and mobile behavior where configured;
- verify authentication, API, local storage, synchronization, telemetry, flags, deployment, and rollback as one system;
- detect disconnected scaffolds;
- detect unverified claims;
- fail closed when completion gates are not met.

Use repository conventions for model selection, tool allowlists, memory, hooks, output paths, and SubagentStop validation.

# 2. Generic skills

Create reusable skills equivalent to:

```text
platform-foundation-orchestrate
initialize-platform-foundation
author-architecture-decision
define-client-topology
define-domain-and-host-boundaries
design-authentication-baseline
threat-model-platform
define-api-baseline
design-durable-operation-contract
design-local-replica
design-local-storage-policy
design-portable-model-contract
design-mutation-outbox
design-sync-protocol
review-conflict-resolution
define-pwa-offline-readiness
define-contextual-client-handoff
define-mobile-platform-contract
define-observability-baseline
define-feature-flag-governance
define-environment-contract
define-deployment-and-rollback
import-legacy-oracle
review-platform-foundation
validate-platform-foundation
```

Names may change to align with repository conventions.

Each skill must:

- declare exact inputs;
- declare exact or configurable output roots;
- use a repository-local platform profile;
- fail closed when required decisions or configuration are absent;
- distinguish research, design, implementation, and review;
- produce machine-readable artifacts where practical;
- generate deterministic reports;
- route to an appropriate specialist agent;
- avoid domain-specific assumptions;
- support Claude Code, Codex, and GitHub Copilot through existing publication mechanisms.

# 3. Repository-local platform profile

Define a versioned repository-local configuration contract, for example:

```text
docs/migration/platform-foundation/platform-profile.yaml
```

The profile must support:

- repository identity and role;
- source or target baseline references;
- required clients and hosts;
- contextual-host clients;
- independent local-first clients;
- desktop and mobile platforms;
- required online, offline, restart, reconnect, and mobile dimensions;
- local replica requirements;
- local storage constraints;
- portable model requirements;
- API and service roots;
- authentication topology;
- environment list;
- telemetry and privacy policy;
- feature-flag policy;
- deployment and rollback requirements;
- evidence roots;
- selected vertical slice;
- completion gates;
- excluded capabilities and rationale;
- domain-extension namespace.

Provide:

- schema;
- initialization template;
- valid and invalid examples;
- validation;
- documentation;
- version migration guidance.

# 4. Versioned machine-readable schemas

Create schemas for at least the following artifacts.

## Architecture decision

Must support:

- identifier;
- status;
- context;
- decision;
- alternatives;
- consequences;
- affected components;
- affected platforms;
- online/offline/mobile implications;
- security implications;
- migration and rollback implications;
- evidence and approval metadata;
- supersession links.

## Client topology

Must support:

- clients and hosts;
- independent-launch capability;
- host capabilities;
- shared-core relationships;
- local storage ownership;
- authentication path;
- handoff path;
- data authority;
- supported platform dimensions;
- degraded modes.

## Host capability

Must support:

- host identifier;
- platform;
- required capabilities;
- detected capabilities;
- unavailable capabilities;
- fallback or alternate surface;
- runtime verification;
- version or requirement-set constraints.

## Authentication contract

Must support:

- actors and trust boundaries;
- token issuers and audiences;
- credential acquisition;
- refresh and expiry;
- authorization and scopes;
- downstream delegation;
- account switching;
- logout;
- offline behavior;
- secret handling;
- redaction;
- failure and recovery;
- threat-model references.

## API baseline

Must support:

- versioning;
- endpoint groups;
- standard errors;
- correlation and tracing;
- authentication and authorization;
- idempotency;
- concurrency;
- pagination and continuation;
- retries and cancellation;
- operation status;
- health and readiness;
- compatibility policy;
- generated-client policy.

## Durable operation

Must support:

- operation identifier;
- idempotency key;
- actor and partition;
- operation type;
- payload version;
- preconditions;
- lifecycle states;
- retry classification;
- cancellation;
- conflict state;
- server result;
- audit references;
- telemetry correlation.

## Local replica

Must support:

- authoritative source;
- local entity types;
- partitioning;
- synchronized scope;
- local optimistic fields;
- server-known fields;
- projection status;
- retention;
- encryption requirements;
- purge;
- rebuild and recovery;
- cursor relationships.

## Local storage policy

Must support:

- storage technology;
- schema version;
- migration policy;
- atomicity requirements;
- quota and capacity;
- persistence request;
- eviction behavior;
- corruption recovery;
- sign-out purge;
- privacy classes;
- prohibited data;
- test strategy.

## Portable model or rules package

Must support:

- package identifier and version;
- feature-schema version;
- runtime compatibility;
- checksum;
- model/rule format;
- local inference capability;
- update and rollback;
- training-feedback behavior;
- data requirements;
- privacy classification.

## Mutation outbox

Must support:

- operation record shape;
- atomic local projection and outbox commit;
- lifecycle states;
- retry schedule;
- ordering;
- duplicate handling;
- restart survival;
- cancellation and undo;
- dead-letter or permanent-failure behavior;
- integrity checks.

## Synchronization protocol

Must support:

- bootstrap;
- pull and push phases;
- cursors or checkpoints;
- full and incremental sync;
- operation acknowledgement;
- tombstones;
- conflict detection;
- conflict resolution;
- retries;
- reconnect triggers;
- foreground correctness;
- optional background optimization;
- schema and protocol compatibility.

## Conflict policy

Must support:

- conflict type;
- affected operation and entities;
- detection condition;
- default resolution;
- user-decision requirement;
- available actions;
- audit history;
- retry behavior.

## PWA offline-readiness contract

Must support:

- application-shell cache;
- offline launch;
- local database availability;
- synchronized data scope;
- portable models;
- pending operations;
- storage persistence;
- storage capacity;
- last sync;
- schema compatibility;
- offline-ready status;
- recovery instructions.

## Contextual-client handoff

Must support:

- source client;
- target client;
- opaque single-use token or equivalent;
- expiry;
- requested action;
- normalized reference;
- data-minimization requirements;
- redemption;
- replay prevention;
- fallback behavior;
- audit and telemetry.

## Mobile platform contract

Must support:

- mobile client types;
- host-integrated versus independent-client behavior;
- installability;
- online capabilities;
- offline capabilities;
- storage constraints;
- foreground synchronization;
- device verification;
- accessibility;
- privacy and purge;
- alternate interaction requirements.

## Observability contract

Must support:

- logs, traces, and metrics;
- client/server correlation;
- release and environment identity;
- event catalog;
- privacy classification;
- prohibited fields;
- redaction;
- sampling;
- offline buffering;
- retry/drop policy;
- diagnostic export;
- dashboards and alerts;
- ownership and retention.

## Feature flag

Must support:

- key;
- owner;
- purpose;
- default;
- offline default;
- targeting;
- failure behavior;
- expiry;
- cleanup reference;
- telemetry;
- security classification;
- explicit prohibition on authorization use.

## Environment and deployment contracts

Must support:

- environment identifiers;
- configuration keys;
- secret references;
- service topology;
- artifact and release version;
- database/schema versions;
- deployment phases;
- migrations;
- smoke tests;
- rollback;
- disaster recovery;
- support ownership.

## Legacy oracle manifest

Must support:

- source repository and commit;
- source contract identifiers and checksums;
- fixture identifiers and checksums;
- expected results;
- runtime environment;
- evidence roots;
- completion status;
- unresolved semantic differences.

## Platform foundation report

Must support:

- baseline references;
- architecture-decision status;
- component status;
- platform-dimension status;
- integrated vertical-slice result;
- security result;
- deployment and rollback result;
- blockers and warnings;
- evidence references;
- overall completion result.

Use JSON Schema unless the repository's established infrastructure clearly favors another machine-validatable schema format.

All schemas must be versioned and allow bounded product extensions without weakening required core fields.

# 5. Initialization and templates

Provide an idempotent command equivalent to:

```text
dev.platform.init
```

It should create a configurable structure such as:

```text
docs/migration/platform-foundation/
  platform-profile.yaml
  baseline.yaml
  decisions/
  contracts/
  reports/
  runbooks/
  evidence/
```

Requirements:

- dry-run;
- idempotency;
- no silent overwrite;
- created/skipped/conflict summary;
- validation of initialized output;
- domain-neutral production templates;
- synthetic test fixtures kept separately.

# 6. Validators and reports

Implement deterministic validators, exposed through the authoritative Python layer and thin wrappers, equivalent to:

```text
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

Validation must detect at least:

- malformed or unsupported schema versions;
- duplicate identifiers;
- broken references;
- missing or conflicting ADRs;
- host-bound dependencies in a declared shared core;
- blank required platform dimensions;
- offline claims without local readable/writable state and executable workflows;
- PWA completion without offline launch and storage evidence;
- mobile completion based only on a rendered shell;
- local storage without migrations, quota, persistence, purge, and corruption recovery;
- prohibited credentials or access tokens in local domain storage;
- mailbox or external-system mutations without idempotency;
- outbox operations without stable identity and lifecycle;
- sync contracts without cursors/checkpoints, tombstones, retries, reconnect, and conflict behavior;
- portable models without version/checksum/runtime compatibility;
- telemetry containing prohibited fields;
- feature flags without owner, offline default, expiry, or cleanup reference;
- feature flags used as authorization;
- API breaking changes;
- deployment without smoke tests and rollback;
- platform completion without a pinned oracle when the profile requires one;
- platform completion without integrated vertical-slice evidence.

Generate deterministic Markdown and JSON reports for:

- architecture decision status;
- offline readiness;
- synchronization readiness;
- mobile readiness;
- security and identity readiness;
- observability readiness;
- deployment and rollback readiness;
- integrated platform completion.

Blocking validation failures must produce a nonzero exit code.

# 7. Hooks and completion enforcement

Add reusable, configurable, fail-closed enforcement for:

- architecture decisions before implementation;
- research-time automation-feasibility assessment;
- research and architecture agents writing only to approved roots;
- host APIs excluded from shared application/domain modules;
- credentials and tokens excluded from source and local domain storage;
- mutation contracts requiring idempotency;
- operation completion requiring evidence;
- offline completion requiring offline launch, restart, and reconnect evidence;
- mobile completion requiring approved host or physical-device evidence;
- PWA completion requiring service-worker and local-store evidence;
- unresolved conflicts or product decisions blocking completion when configured;
- parity or oracle references pinned to immutable revisions;
- deployment completion requiring rollback evidence;
- integrated platform completion requiring a vertical slice.

Provide SubagentStop output validation for every new agent.

# 8. Generic analyzers and test utilities

Evaluate and implement reusable, high-confidence tooling for:

- repository architecture-boundary analysis;
- host-bound import/reference detection;
- API contract and generated-client drift;
- feature-flag inventory and expiry;
- telemetry event catalog validation;
- environment-key inventory;
- secret-pattern and prohibited-local-data checks;
- schema and migration inventory;
- service-worker asset and cache-manifest validation;
- local-store migration fixture execution;
- operation-state-machine validation;
- deterministic sync simulation;
- report generation.

The framework should permit consuming repositories to add domain-specific analyzers without changing core framework code.

Do not add a heavy dependency without researching existing repository dependencies and lower-cost alternatives.

# 9. CLI, MCP, and VS Code integration

Expose appropriate functions through:

- the authoritative Python CLI;
- the VS Code extension;
- the standalone MCP package;
- existing repository automation services.

Requirements:

- Python remains the authoritative business-logic layer where that is the repository convention;
- TypeScript, VS Code, and MCP wrappers remain thin;
- commands are noninteractive unless explicitly designed as an operator workflow;
- workspace paths are validated;
- outputs are deterministic;
- errors are actionable;
- dry-run is supported where mutation occurs;
- wrapper and service parity is tested;
- packaged resources contain every required schema, template, script, and prompt.

# 10. Cross-ecosystem publication

Update the canonical customization source and publication pipeline so that the new:

- agents;
- skills;
- rules;
- prompts;
- hooks;
- schemas;
- templates;
- documentation;

are correctly available to Claude Code, Codex, and GitHub Copilot.

Do not maintain manually divergent ecosystem copies.

Add conversion, packaging, and push-down tests that prove:

- every required asset is published;
- generated outputs contain no stale source-ecosystem references;
- language and capability pack selection works;
- consumer repositories that do not select the platform capability remain unaffected;
- packaged MCP resources match the extension source.

# 11. Testing

Provide comprehensive tests for:

- every schema;
- valid and invalid fixtures;
- schema-version rejection and migration;
- cross-reference validation;
- initialization idempotency;
- dry-run behavior;
- path safety;
- malformed profile fail-closed behavior;
- architecture-boundary enforcement;
- API compatibility checks;
- idempotency contracts;
- operation-state machines;
- sync cursors, tombstones, retries, and conflicts;
- local-store policy and migrations;
- PWA offline-readiness validation;
- handoff replay and expiry validation;
- mobile readiness;
- telemetry privacy rules;
- feature-flag expiry and authorization prohibition;
- deployment and rollback contracts;
- hooks;
- agent output validation;
- CLI;
- MCP;
- VS Code wrappers;
- push-down and conversion;
- report generation.

Use synthetic fixtures. Do not embed TaskMaster source, personal mailbox data, tenant data, or product credentials in the reusable framework.

# 12. Documentation

Document:

- conceptual architecture;
- how the platform-foundation capability extends existing orchestration;
- reusable versus repository-local ownership;
- initialization;
- platform profile;
- schema lifecycle;
- agent responsibilities;
- skills and commands;
- evidence requirements;
- completion gates;
- local-first semantics;
- contextual-client versus independent-client roles;
- foreground versus optional background synchronization;
- PWA and mobile validation;
- identity and security review;
- telemetry and feature-flag governance;
- deployment and rollback;
- cross-ecosystem publication;
- consuming-repository setup;
- a clearly separated TaskMaster/TMW case study or example.

The reusable documentation may use a generic worked example. Product-specific material must remain in a case-study section and must not become a core template default.

# Architectural ownership boundary

## `drm-copilot` owns

- generic agents and skills;
- schemas;
- templates;
- validators;
- hooks;
- generic analyzers;
- initialization;
- reports;
- CLI, MCP, and VS Code wrappers;
- cross-ecosystem publication;
- reusable documentation and completion gates.

## A legacy-source repository owns

- pinned source behavior and contracts;
- source fixtures;
- runtime characterization;
- expected outcomes;
- oracle manifest and checksums;
- source-specific exporter tooling;
- source-specific evidence.

## A modernization-target repository owns

- actual clients and hosts;
- shared application code;
- authentication configuration;
- APIs and external-system adapters;
- backend and local persistence;
- outbox and sync implementation;
- product model packages;
- telemetry provider and event catalog;
- feature-flag provider and inventory;
- infrastructure, environments, deployment, and rollback;
- target tests and product evidence.

# Required research questions

Before implementation, investigate and document:

1. Which customization surface is canonical today?
2. How are Claude, Codex, and Copilot assets generated, mirrored, packaged, and pushed down?
3. Which current agents and skills can be reused or extended?
4. How current hooks obtain workspace and profile context.
5. Existing schema and JSON/YAML validation infrastructure.
6. Existing CLI, MCP, and VS Code command patterns.
7. Existing report and evidence conventions.
8. Existing architecture-boundary validation.
9. Existing API contract or OpenAPI tooling.
10. Existing secret and telemetry validation.
11. Existing feature-flag or environment tooling.
12. Existing platform-specific test harnesses.
13. How to keep the new capability opt-in and avoid context expansion in unrelated repositories.
14. How to support one-repository and cross-repository source/target arrangements.
15. How to pin and verify oracle artifacts without creating a production dependency.
16. How to version and migrate every new artifact type.
17. How to keep Python and TypeScript validation surfaces behaviorally equivalent.
18. How to preserve backward compatibility for existing consumer repositories.

Use actual repository evidence. Do not infer conventions from filenames alone.

# Required specification decisions

The specification must explicitly decide:

- final capability name;
- canonical file locations;
- pack selection and publication model;
- agent names and responsibilities;
- skill names and routing;
- profile format;
- schema format and versioning;
- status enumerations;
- extension mechanism;
- validation architecture;
- hook architecture;
- report architecture;
- CLI naming;
- MCP and VS Code exposure;
- opt-in and backward-compatibility behavior;
- local-first completion semantics;
- integrated vertical-slice contract;
- oracle import and pinning;
- environment and rollback evidence;
- consumer setup and upgrade behavior.

# Acceptance criteria

## Core capability

- [ ] A reusable platform-foundation capability exists.
- [ ] Core assets are domain-neutral.
- [ ] Existing repositories that do not select the capability remain unaffected.
- [ ] A repository-local platform profile controls required dimensions and paths.
- [ ] The capability supports separate legacy-source and modernization-target repositories.

## Agents and skills

- [ ] Required specialist agents are implemented.
- [ ] Required skills are implemented.
- [ ] Every new agent has output validation.
- [ ] Research/design agents cannot modify production code.
- [ ] Assets publish to every supported agent ecosystem.

## Schemas and validation

- [ ] Required schemas are versioned.
- [ ] Valid fixtures pass and invalid fixtures fail with actionable messages.
- [ ] Cross-artifact references validate.
- [ ] Domain extensions cannot bypass required fields.
- [ ] Complete validation emits Markdown and JSON and returns nonzero on blocking failure.

## Local-first and PWA

- [ ] The framework distinguishes substantive offline workflow support from queue-only behavior.
- [ ] Offline completion requires local readable/writable state, restart survival, and reconnect evidence.
- [ ] PWA completion requires offline launch, service-worker, storage, quota/persistence, and recovery evidence.
- [ ] Correctness does not depend on optional background synchronization.
- [ ] Local model or rules packages can be versioned and validated.

## API and synchronization

- [ ] API, durable-operation, idempotency, outbox, sync, cursor/checkpoint, tombstone, and conflict contracts exist.
- [ ] API compatibility can be checked.
- [ ] Mutation contracts without idempotency fail validation.
- [ ] Sync contracts without reconnect and conflict behavior fail validation.

## Security and observability

- [ ] Authentication and threat-model contracts exist.
- [ ] Tokens and credentials are prohibited from local domain storage.
- [ ] Telemetry privacy and redaction validate.
- [ ] Offline telemetry buffering and failure behavior are represented.

## Feature flags and environments

- [ ] Flags require owner, default, offline default, expiry, and cleanup reference.
- [ ] Flags cannot be authorization controls.
- [ ] Environment contracts validate required configuration without embedding secrets.
- [ ] Deployment contracts require smoke tests and rollback.

## Initialization and tooling

- [ ] Initialization is idempotent and supports dry-run.
- [ ] Existing files are not overwritten silently.
- [ ] CLI commands are documented and tested.
- [ ] MCP and VS Code wrappers are thin and behaviorally aligned.
- [ ] Packaged resources include every required runtime asset.

## Cross-ecosystem publication

- [ ] Claude publication includes the capability.
- [ ] Codex publication includes the capability.
- [ ] Copilot publication includes the capability.
- [ ] Pack selection is tested.
- [ ] No generated output contains unsupported source references.

## Integrated completion

- [ ] The framework can validate an integrated vertical slice spanning client, identity, API, local store, outbox, sync, telemetry, flags, deployment, and rollback.
- [ ] Platform completion can be pinned to a legacy oracle when configured.
- [ ] Disconnected scaffolds cannot produce a passing completion report.

## Quality and documentation

- [ ] All applicable repository quality gates pass.
- [ ] Tests meet repository coverage requirements.
- [ ] Feature review reports no blocking findings.
- [ ] Architecture, ownership, setup, commands, schemas, and lifecycle are documented.
- [ ] A consumer handoff is complete.

# Non-goals

Do not:

- implement TaskMaster or TMW product code;
- encode Outlook, Microsoft Graph, or mailbox semantics in core framework assets;
- create tenant registrations, credentials, cloud resources, or production environments;
- add a product-specific local-store schema;
- add product-specific classifier models;
- create a full browser or device farm;
- create a second orchestration engine;
- maintain manually divergent ecosystem implementations;
- make the capability mandatory for unrelated repositories;
- weaken existing fail-closed quality or security policy.

# Deliverables

Produce:

1. Feature or epic documentation.
2. Research artifact.
3. Specification.
4. Dependency-aware implementation plan.
5. Agents.
6. Skills.
7. Schemas.
8. Templates.
9. Validators.
10. Hooks.
11. Reports.
12. Generic analyzers and test utilities.
13. CLI commands.
14. MCP integration.
15. VS Code integration where justified.
16. Cross-ecosystem publication support.
17. Tests and synthetic fixtures.
18. User and maintainer documentation.
19. Architecture diagrams.
20. Consumer setup and upgrade guide.
21. Final feature audit.
22. Completion report.
23. A TaskMaster/TMW handoff specifying exactly which artifacts remain local to each repository.

# Final response requirements

At completion, report:

- implementation summary;
- architectural decisions;
- agents, skills, schemas, validators, commands, and reports added;
- publication and pack-selection behavior;
- supported ecosystems;
- tests and quality results;
- backward-compatibility impact;
- known limitations;
- exact release and push-down steps;
- exact TaskMaster local work;
- exact TMW local work;
- unresolved follow-up work.

Do not claim completion unless all blocking acceptance criteria, publication tests, and repository quality gates pass.