# Objective

Perform the TaskMaster portion of Step 1: Discovery and Parity Definition.

This repository is the authoritative legacy source. Produce a complete, evidence-backed description of TaskMaster’s existing behavior, implementation coverage, runtime semantics, edge cases, and unresolved product questions.

This is a discovery, characterization, and documentation epic. It is not a modernization implementation epic.

Do not modify TMW. Do not design or implement the target platform. Do not refactor, repair, modernize, or replace TaskMaster production code.

# Required Framework

Use the released drm-copilot legacy-discovery-and-parity capability that has been pushed into this repository.

Use the actual released names and paths for:

- discovery initialization;
- domain-profile validation;
- legacy inventory;
- feature-contract extraction;
- runtime characterization;
- evidence linking;
- unspecified-behavior tracking;
- acceptance-scenario generation;
- coverage review;
- discovery completion validation.

Do not recreate equivalent local agents, schemas, validators, or workflows when the released framework already provides them.

# Prerequisite Gate

Before performing discovery:

1. Verify that the released discovery agents, skills, schemas, validators, analyzers, and MCP or CLI tools are present and callable.
2. Record:
   - the current TaskMaster commit SHA;
   - the drm-copilot release version;
   - the drm-copilot source commit SHA;
   - the discovery schema version;
   - the current branch.
3. Verify that the working tree is clean apart from the intended discovery branch.
4. Run the repository’s established build and test baseline.
5. Initialize or validate the repository-local discovery workspace.
6. Validate the repository-local domain profile.
7. Confirm that the profile declares this repository as the legacy source.
8. Confirm that evidence paths, exclusions, required dimensions, production roots, test roots, and solution entry points are defined.

If the discovery runtime is missing, its version cannot be identified, the profile is malformed, or required schemas and validators are unavailable, stop before creating authoritative discovery artifacts. Produce a prerequisite-failure report rather than improvising a replacement framework.

Record baseline build or test failures as pre-existing findings. Do not silently correct them during discovery.

# Required Operating Mode

Treat this as an epic-scale, research-first body of work.

Use the repository’s epic planning and execution lifecycle where available:

1. Create the epic or active feature documentation.
2. Establish scope and explicit exclusions.
3. Initialize discovery artifacts.
4. Inventory the repository.
5. Divide discovery into bounded capability workstreams.
6. Delegate deep analysis to the legacy-parity and runtime-characterization specialists.
7. Validate every artifact as it is produced.
8. Perform a complete coverage review.
9. Run the discovery completion gate.
10. Produce the immutable handoff baseline for TMW.

Persist orchestration state after each material phase and workstream.

The orchestrator must coordinate work and validate integration. It must not substitute its own broad impressions for specialist analysis.

# Repository Role and Semantic Separation

Model TaskMaster as:

- repository role: legacy-source;
- primary platform: Windows Outlook desktop VSTO;
- primary implementation: .NET Framework C# with COM/Outlook interop;
- source of truth: observable legacy behavior and its evidence.

For every relevant capability, keep these concepts separate:

1. Observed legacy behavior.
2. Inferred but unverified legacy behavior.
3. Legacy defects or accidental behavior.
4. Product decisions about what should be preserved.
5. Target requirements for the modern implementation.
6. Current TMW implementation status.

This repository may define items 1–4 and candidate target requirements. It must not assert item 6.

In particular, distinguish:

- current legacy online behavior;
- current legacy cached-mode or disconnected behavior;
- reconnect behavior;
- current legacy desktop availability;
- current legacy mobile availability;
- required target offline behavior;
- required target mobile behavior.

A lack of mobile support in the VSTO application must not be interpreted as evidence that mobile support is unnecessary in the target product.

# Discovery Scope

Start from automated inventory rather than from the README alone.

Inspect, as applicable:

- TaskMaster.sln and every included project;
- production and test projects;
- VSTO entry points;
- ThisAddIn lifecycle;
- Ribbon XML and designer-generated Ribbon surfaces;
- Ribbon controls and callback mappings;
- Explorer and Inspector integration;
- Outlook event subscriptions and unsubscriptions;
- COM object ownership and release;
- Outlook item types;
- MAPI and custom-property access;
- folder, store, PST, search-folder, and shared-mailbox behavior;
- UI forms, dialogs, viewers, keyboard handling, and command routing;
- controller and service boundaries;
- settings, defaults, migration behavior, and persistence;
- filesystem access and serialization;
- registry access;
- model training, loading, saving, and reset behavior;
- logging, diagnostics, and error reporting;
- timers, asynchronous operations, cancellation, and shutdown behavior;
- tests, fixtures, mocks, and disabled or ignored tests;
- external dependencies and host assumptions;
- documented and undocumented configuration.

At minimum, investigate the following capability families. Treat this as a starting taxonomy, not as an exhaustive list:

- Quick Filer and keyboard-driven filing;
- folder search, ranking, and destination selection;
- filing queues and queued operations;
- message and conversation move behavior;
- undo or restoration behavior;
- attachment and message export or save behavior;
- SpamBayes;
- triage classification;
- folder and category prediction;
- classifier training and feedback;
- classifier persistence and model lifecycle;
- tags for people, projects, and topics;
- task and to-do models;
- task trees and visualization;
- Outlook links and message-to-task capture;
- settings and configuration;
- stores, PSTs, and mailbox topology;
- startup, shutdown, reconnect, and Outlook lifecycle handling;
- logging, supportability, diagnostics, and failure recovery.

Do not assume that a named feature in documentation corresponds to one feature contract. Decompose it into independently observable behaviors where appropriate.

# Required Artifacts

Use the released framework’s canonical local paths. Do not create a parallel artifact hierarchy merely because an example path appears below.

The completed TaskMaster discovery set must include equivalents of:

- domain profile;
- discovery scope;
- repository and component inventory;
- coverage ledger;
- legacy feature contracts;
- runtime characterization scenarios;
- runtime and static evidence references;
- unspecified-behavior log;
- product-decision queue;
- legacy acceptance or characterization scenarios;
- discovery coverage report;
- discovery completion report;
- immutable source-baseline manifest for TMW.

# Component Inventory and Coverage Ledger

Generate the initial inventory with the framework’s static analyzers, then review and enrich it manually.

Every in-scope component must have a coverage disposition such as:

- inspected;
- behavior extracted;
- runtime validation required;
- duplicate or generated;
- infrastructure only;
- excluded with rationale;
- blocked with reason.

For each component, record:

- path;
- symbol or control identifier where applicable;
- component type;
- associated project;
- tests found;
- feature-contract links;
- runtime-characterization requirement;
- evidence;
- disposition.

Do not claim coverage solely because an analyzer found a file. Analyzer detection and semantic inspection are separate states.

Discovery completion must fail while any in-scope component lacks a disposition.

# Feature Contracts

Create stable feature identifiers and machine-valid feature contracts.

Each contract must describe, as applicable:

- feature name and capability family;
- user or system trigger;
- UI entry points;
- preconditions;
- inputs;
- state before execution;
- state transitions;
- observable outputs;
- mailbox side effects;
- local filesystem side effects;
- settings or model side effects;
- ordering and timing;
- repeated invocation;
- idempotency or lack of idempotency;
- cancellation;
- partial failure;
- error reporting;
- recovery behavior;
- Outlook lifecycle dependencies;
- online behavior;
- cached-mode or disconnected behavior;
- reconnect behavior;
- multi-store behavior;
- current platform availability;
- target offline relevance;
- target mobile relevance;
- source-code evidence;
- test evidence;
- documentation evidence;
- runtime evidence;
- confidence;
- unresolved questions;
- known or suspected defects.

Every behavior assertion must be linked to verifiable evidence.

Classify claims explicitly as:

- verified-static;
- verified-test;
- verified-runtime;
- inferred;
- contradictory;
- unknown;
- product-decision-required.

Do not mark inferred or contradictory behavior as verified.

# Static Evidence

Read relevant implementation paths end to end.

For each important behavior:

1. Identify the UI or event entry point.
2. Follow the call chain through controllers, models, utilities, and adapters.
3. Identify all resulting state changes and side effects.
4. Locate relevant tests.
5. Record the actual source revision, path, symbol, and line or range.
6. Record contradictions between source, tests, and documentation.
7. Identify host behavior that cannot be proven statically.

Do not rely on class names, method names, comments, or README descriptions without examining the underlying implementation.

# Runtime Characterization

Create repeatable characterization scenarios for behavior that cannot be established reliably through static inspection.

Prioritize:

- filing while online;
- filing while disconnected;
- queued filing and reconnect;
- application or Outlook restart with pending work;
- message move and conversation move;
- partial multi-message failure;
- undo or restoration;
- folder deletion or rename during a workflow;
- multiple stores;
- shared mailboxes where supported;
- search folders;
- PSTs;
- attachments;
- categories and flags;
- classifier enable, train, save, load, reset, and failure behavior;
- model-state corruption or missing files;
- item-change and selection-change events;
- Outlook startup and shutdown;
- cancellation and repeated commands.

For each scenario record:

- environment;
- Outlook version and bitness;
- account or store type;
- cached-mode state;
- network state;
- sanitized fixture;
- pre-state;
- action;
- post-state;
- logs and diagnostics;
- result;
- repeatability;
- evidence;
- confidence;
- unresolved observations.

Never mark a runtime scenario as passed unless it was actually executed and evidence was captured.

When a scenario requires human interaction, produce an exact operator run sheet and pause that scenario at AWAITING-HUMAN-CHARACTERIZATION. Do not invent the observation.

Do not commit message bodies, real addresses, tokens, tenant identifiers, or other personal data.

# Offline and Reconnect Analysis

For every behavior that changes mailbox, local, model, or task state, answer:

- Can the command be initiated while disconnected?
- Does Outlook expose enough cached data to complete it?
- Is the operation completed locally, rejected, or deferred?
- What state represents pending work?
- Does pending work survive Outlook restart?
- In what order is deferred work replayed?
- What identifies duplicate replay?
- What happens when the destination or source changed remotely?
- What happens after partial success?
- What feedback does the user receive?
- What occurs after reconnection?
- Is the result observable from another client?

Record unknown answers as unknown. Do not infer future target behavior from current VSTO or Outlook caching mechanics.

# Unspecified and Contradictory Behavior

Create an unspecified-behavior record whenever:

- source and tests disagree;
- documentation and implementation disagree;
- behavior depends on timing;
- partial failure is not clearly defined;
- more than one outcome is plausible;
- runtime characterization is inconclusive;
- legacy behavior appears defective;
- preserving the exact behavior would conflict with the modernization goals.

Each record must include:

- linked feature contract;
- precise question;
- evidence;
- competing interpretations;
- impact;
- confidence;
- recommended experiment, when applicable;
- product decision required;
- proposed target outcome, if one can be recommended without treating it as approved.

Do not make final product decisions on behalf of the product owner.

# Product-Owner Decision Queue

Produce a concise decision queue for the human product owner.

For every decision include:

- identifier;
- linked legacy feature;
- question;
- current observed behavior;
- available evidence;
- user-visible impact;
- offline impact;
- mobile impact;
- security or privacy impact;
- options;
- recommendation;
- consequence of deferral.

Separate required decisions from low-priority informational questions.

# Acceptance and Characterization Scenarios

Generate scenarios only from:

- verified legacy behavior; or
- explicitly approved product decisions.

Label scenarios to distinguish:

- legacy characterization;
- required target outcome;
- online;
- cached-mode offline;
- reconnect;
- desktop;
- mobile;
- manual;
- automatable;
- pending decision.

An unresolved behavior must generate a pending scenario or decision reference, not a fabricated expected result.

# Source Baseline Handoff

At completion, generate an immutable source-baseline manifest for TMW containing:

- TaskMaster repository identity;
- TaskMaster commit SHA;
- discovery schema version;
- drm-copilot release and commit;
- domain-profile version;
- feature-contract identifiers;
- contract paths;
- contract checksums;
- evidence-index checksum where supported;
- completion-report path and status;
- unresolved-decision count;
- runtime-characterization coverage summary.

The baseline must permit the TMW repository to detect when a referenced TaskMaster contract has changed.

Do not include mutable branch-only references without commit SHAs.

# Non-Goals

Do not:

- implement modern architecture;
- modify TMW;
- implement Step 2 platform foundation;
- refactor TaskMaster;
- fix legacy defects;
- replace COM or VSTO;
- add Microsoft Graph integration;
- create target-side APIs;
- claim mobile parity;
- add product requirements without identifying them as proposed;
- mark runtime behavior verified without execution evidence;
- commit personal mailbox content;
- create a parity matrix that claims knowledge of TMW implementation.

If discovery reveals a code defect or instrumentation need, record it as a separate proposed issue. Do not widen this work into implementation.

# Completion Criteria

The TaskMaster portion of Step 1 is complete only when:

- every in-scope project and component has a coverage disposition;
- every discovered user-facing control or event entry point maps to one or more feature contracts or has an exclusion rationale;
- all feature contracts validate;
- every verified behavior has evidence;
- inferred, unknown, and contradictory behavior is classified correctly;
- every unknown or contradictory behavior has an unspecified-behavior record;
- online, offline, reconnect, desktop, and target-mobile relevance are explicit where configured;
- all required characterization scenarios are either executed or visibly blocked awaiting human evidence;
- generated acceptance scenarios contain no invented expected behavior;
- the product-owner decision queue is complete;
- the discovery coverage report is generated;
- the discovery completion validator passes, or reports an explicit incomplete status with all blockers;
- the immutable source-baseline manifest is generated and validates.

Do not report PASS while required runtime characterization or product decisions remain unresolved. Report CONDITIONAL or INCOMPLETE with exact blockers.

# Final Response

Provide:

1. Baseline TaskMaster commit and drm-copilot version.
2. Scope and exclusions.
3. Component and UI coverage summary.
4. Feature-contract count by capability family.
5. Runtime-characterization status.
6. Online, offline, and reconnect coverage.
7. Unspecified and contradictory behavior summary.
8. Product-owner decisions required.
9. Acceptance-scenario count and status.
10. Validation and quality-gate results.
11. Exact artifact paths.
12. Source-baseline manifest path and checksum.
13. Remaining blockers.
14. Exact handoff instructions for the TMW discovery run.

Do not claim that Step 1 as a whole is complete. Only the TaskMaster legacy-source portion is in scope.