# Required sequence

For the **authoritative Step 1 run**, complete the work in this order:

1. Finish the legacy-discovery-and-parity capability in `drm-copilot`.
2. Pass its tests, validators, feature review, and cross-ecosystem publication tests.
3. Merge it to `drm-copilot/main`.
4. Publish a new `drm-copilot` extension/MCP release.
5. Install or update that released runtime locally.
6. Push the released customizations into **both** TaskMaster and TMW.
7. Review and merge separate “adopt discovery runtime” pull requests in TaskMaster and TMW.
8. Run the TaskMaster prompt.
9. Review and adjudicate the TaskMaster outputs, merge them, and record the resulting TaskMaster commit SHA.
10. Run the TMW prompt against that pinned TaskMaster commit.
11. Review and merge the TMW parity artifacts.

The release step matters because the requested `drm-copilot` capability includes not only static agents and skills but also schemas, validators, analyzers, CLI commands, MCP tools, and VS Code integration. The extension is the authoritative development surface, while the standalone npm package distributes the same MCP server and its runtime resources. The repository provides separate push-down commands for Claude, Codex/agents, and GitHub Copilot.

## Can you run the prompts before publishing?

You can conduct a **controlled preview** before release by:

* building the extension and MCP server from the `drm-copilot` feature branch;
* configuring Claude or Codex to use that local development build;
* pushing customizations directly from that source checkout;
* recording the exact `drm-copilot` commit SHA used.

Do not treat those results as the final Step 1 baseline. After the feature is merged and released, rerun initialization and validation with the released version. The runtime currently expects a built MCP bridge, and its script-backed tools require Python and PowerShell on the consumer machine.

For the final discovery artifacts, the better sequence is:

> **merge → release → install → push down → validate → run prompts**

# Push-down precautions

Both TaskMaster and TMW already contain repository-specific agent customizations. TaskMaster has an extensive `CLAUDE.md` and C# policies, while TMW has existing Claude and Codex orchestrators and skills.

The push-down publisher writes every selected source file to the corresponding destination path and classifies an existing destination file as overwritten. Therefore, do not push directly to `main`; use a dedicated adoption branch and inspect the complete diff.

Recommended adoption workflow in each consumer repository:

```text
main
  └── chore/adopt-drm-copilot-discovery-runtime
        ├── push down released Claude customizations
        ├── push down released Codex/agents customizations
        ├── reconcile repository-local policies
        ├── run customization validation
        ├── run repository baseline quality gates
        └── merge adoption PR
```

Do not combine the push-down adoption diff with the discovery artifacts. Separate pull requests make it possible to distinguish framework changes from the actual system analysis.

## Recommended push-down selections

| Repository | Claude selection                                                                 | C# variant | Memory mode | Codex/agents                                                                    |
| ---------- | -------------------------------------------------------------------------------- | ---------: | ----------: | ------------------------------------------------------------------------------- |
| TaskMaster | Core, discovery capability, legacy C#                                            |   `legacy` |     `merge` | Corresponding core, discovery, and legacy-C# selections                         |
| TMW        | Core, discovery capability, TypeScript, modern C#, and PowerShell where required |   `modern` |     `merge` | Corresponding core, discovery, TypeScript, modern-C#, and PowerShell selections |

The final pack names may differ from the working names in the development prompt. Use the names documented in the completed `drm-copilot` feature. The Claude publisher supports pack selection, modern/legacy C# variants, and overwrite/merge/skip memory modes.

For TaskMaster, the released legacy C# variant is the appropriate base because the repository targets VSTO and .NET Framework 4.8.1.

A representative Claude command, using the final released pack names, will look like:

```powershell
poetry run python -m scripts.dev_tools.push_down_claude_customizations `
  --destination <absolute-path-to-consumer-repo> `
  --packs <released-pack-list> `
  --csharp-variant <legacy-or-modern> `
  --memory-mode merge
```

Use the released VS Code command or MCP tool for the corresponding Codex/agents push-down. Push GitHub Copilot customizations as well only when GitHub Copilot will be one of the active development environments.

## Reconcile rather than blindly overwrite

During the adoption pull requests:

* Preserve repository-specific architectural rules and toolchain commands.
* Move repository-specific instructions into the local extension points established by the completed discovery framework.
* Do not maintain manual divergence in files designated as generated.
* Confirm that local settings and secrets remain local.
* Confirm that repository-specific agent memories do not become general distributable memories.
* Run the new discovery framework’s self-validation before merging.

The Claude publisher excludes `settings.local.json` and allows agent-memory merge behavior, but that does not protect every other colliding file.

# Before running the TaskMaster prompt

TaskMaster is the **legacy source-of-truth repository**. It is a Windows Outlook VSTO add-in targeting .NET Framework 4.8.1, with multiple projects covering Quick Filer, classifiers, tags, task visualization, shared utilities, and MSTest suites.

Complete these preparations first:

### Repository preparation

* Merge the TaskMaster discovery-runtime adoption PR.
* Start from a clean `main`.
* Create a dedicated branch such as:

```text
docs/taskmaster-legacy-discovery
```

* Record:

  * TaskMaster baseline commit SHA;
  * `drm-copilot` release version;
  * `drm-copilot` source commit SHA;
  * discovery schema version.
* Run the current TaskMaster build and test baseline.
* Do not begin discovery with unexplained pre-existing build or test failures.

TaskMaster’s current documented baseline uses `TaskMaster.sln`, Visual Studio/MSBuild, and MSTest/vstest.

### Runtime-characterization preparation

Prepare a safe characterization environment:

* A non-production Outlook profile or test mailbox.
* Representative folders, messages, conversations, categories, flags, tasks, attachments, and PST/store combinations.
* Cached Exchange Mode enabled.
* A repeatable way to disconnect and reconnect network access.
* Copies or backups of existing classifier state and TaskMaster settings.
* A written data-handling rule that prohibits committing message bodies, addresses, tenant identifiers, access tokens, or other personal information.
* A redaction policy for screenshots, logs, mailbox snapshots, and paths.

TaskMaster processes Outlook items locally and persists classifier state and diagnostic information to the filesystem, so runtime evidence needs explicit privacy controls.

The prompt can complete static discovery without every runtime experiment, but it must not declare Step 1 complete while required runtime characterization remains unperformed.

# Prompt 1 — TaskMaster legacy discovery

Copy this prompt into an orchestrator session opened at the TaskMaster repository root after the preparations above.

```text
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
```

# Between the TaskMaster and TMW prompts

Do not immediately proceed to TMW when the TaskMaster agent stops.

Perform this review cycle first:

1. Review the coverage report.
2. Perform required manual Outlook characterization.
3. Add the resulting sanitized evidence.
4. Re-run TaskMaster discovery validation.
5. Review every product-decision item.
6. Approve, reject, defer, or request another experiment.
7. Generate or update acceptance scenarios from approved decisions.
8. Obtain a TaskMaster discovery completion result.
9. Merge the TaskMaster discovery pull request.
10. Record the merged TaskMaster commit SHA.
11. Check out that exact commit in a local read-only TaskMaster clone for the TMW run.

The TMW prompt should not create an authoritative parity matrix from unmerged TaskMaster contracts. It can perform an early, non-authoritative TMW inventory in parallel, but final reconciliation must use a pinned TaskMaster source baseline.

# Before running the TMW prompt

TMW is already more than an empty prototype. It has a No-COM Office.js task-pane architecture, TypeScript code, an iFile capability, commands, tests, CI, a .NET API path, and desktop/mobile host wiring.

It also has an existing seven-stage quality model and local development commands.

Its current mobile evidence proves that the add-in shell opens and reacts to message changes, but the classification and feedback workflow is not wired; the parity analysis must not equate host-shell availability with feature parity.

Complete these preparations:

* Merge the TMW discovery-runtime adoption PR.
* Start from clean `main`.
* Create a branch such as:

```text
docs/taskmaster-parity-definition
```

* Run the TMW baseline quality gates.
* Place or clone TaskMaster at a known read-only sibling path.
* Check out the exact merged TaskMaster discovery commit.
* Validate the TaskMaster source-baseline manifest and contract checksums.
* Record the TMW baseline commit.
* Initialize or validate the TMW target-side discovery profile.
* Configure online, offline, reconnect, desktop, and mobile as required parity dimensions.
* Confirm that the orchestrator can read TaskMaster contracts but cannot write to the TaskMaster checkout.

# Prompt 2 — TMW target inventory and parity reconciliation

Copy this prompt into an orchestrator session opened at the TMW repository root after the TaskMaster baseline has been merged and pinned.

```text
# Objective

Perform the TMW portion of Step 1: Discovery and Parity Definition.

Use the completed, pinned TaskMaster legacy-source baseline to:

- inventory the current TMW implementation;
- map existing TMW behavior to TaskMaster feature contracts;
- produce the authoritative source-to-target parity matrix;
- identify implemented, partial, missing, conflicting, intentionally changed, and candidate-retired behavior;
- define target acceptance scenarios;
- identify architecture and platform gaps affecting online, offline, reconnect, desktop, and mobile operation;
- create a product-owner decision queue and an implementation handoff.

This is a target inventory and parity-definition epic.

Do not implement Step 2 platform foundation. Do not perform broad feature implementation. Do not edit TaskMaster.

# Required Framework

Use the released drm-copilot legacy-discovery-and-parity capability that has been pushed into this repository.

Use the actual released names and paths for:

- target discovery initialization;
- domain-profile validation;
- target implementation inventory;
- source-baseline import or registration;
- parity reconciliation;
- product-decision tracking;
- target acceptance-scenario generation;
- coverage and parity reporting;
- completion validation.

Do not recreate local equivalents of the released schemas, validators, agents, or workflows.

# Prerequisite Gate

Before producing authoritative parity artifacts:

1. Verify that the released discovery agents, skills, schemas, validators, analyzers, and MCP or CLI tools are present and callable.
2. Record:
   - the current TMW commit SHA;
   - the drm-copilot release version;
   - the drm-copilot source commit SHA;
   - the discovery schema version;
   - the current branch.
3. Verify that the TMW working tree is clean apart from the intended discovery branch.
4. Run the repository’s established baseline quality gates.
5. Initialize or validate the repository-local target discovery workspace.
6. Validate the TMW domain profile.
7. Confirm that the profile declares this repository as the modernization target.
8. Locate the TaskMaster source-baseline manifest.
9. Validate:
   - TaskMaster repository identity;
   - pinned TaskMaster commit SHA;
   - schema version;
   - contract list;
   - contract checksums;
   - TaskMaster completion-report status.
10. Verify that the TaskMaster checkout is at exactly the pinned commit and is read-only for this workflow.

If the TaskMaster source baseline is absent, mutable, unmerged, invalid, checksum-mismatched, or incomplete, stop authoritative reconciliation.

You may produce a clearly labeled NON-AUTHORITATIVE TMW CURRENT-STATE INVENTORY, but do not create or mark complete the final parity matrix.

Do not silently use TaskMaster main, a floating branch, or a different commit.

# Required Operating Mode

Treat this as an epic-scale, research-first reconciliation.

Use the repository’s epic planning and execution lifecycle:

1. Establish source-baseline integrity.
2. Inventory the current TMW platform and implemented capabilities.
3. Divide reconciliation by TaskMaster capability family.
4. Delegate source-to-target mapping to the requirements-reconciliation specialist.
5. Validate each target implementation record and parity entry.
6. Review architecture and platform constraints.
7. Generate target acceptance scenarios.
8. Perform a complete parity-coverage review.
9. Run the target discovery completion gate.
10. Produce the handoff for Step 2 and later feature migration.

Persist orchestration state after each material phase and capability workstream.

# Repository Role and Semantic Separation

Model TMW as:

- repository role: modernization-target;
- primary client: Office.js Outlook add-in;
- current front end: TypeScript task pane and command surfaces;
- current or emerging backend: modern .NET API;
- architectural constraint: No-COM target implementation;
- source requirements: pinned TaskMaster feature contracts plus approved product decisions.

Keep these concepts separate:

1. TaskMaster observed behavior.
2. Approved target requirement.
3. Current TMW implementation.
4. TMW implementation evidence.
5. Planned but unimplemented capability.
6. Product decision.
7. Platform limitation.
8. Candidate retirement.

Do not classify planned documentation, an interface, a placeholder button, a mock, or a scaffold as implemented product behavior.

Do not classify an add-in shell that opens on mobile as mobile feature parity unless the complete feature workflow is implemented and verified.

# Source Baseline

Register or import the pinned TaskMaster source baseline using the released framework.

For every source reference retain:

- source repository;
- TaskMaster commit SHA;
- feature-contract identifier;
- contract path;
- contract checksum;
- relevant evidence reference;
- approved product-decision reference where applicable.

Do not copy and independently edit TaskMaster feature contracts in TMW.

Where a local snapshot is required by the framework, record its source checksum and treat it as generated, immutable input.

If a TaskMaster source contract changes, the parity validator must detect that the TMW baseline is stale.

# TMW Current-State Inventory

Inventory the actual current repository rather than relying on previous migration plans.

Inspect, as applicable:

- Office add-in manifests;
- desktop and mobile form factors;
- task-pane entry points;
- command handlers;
- Office.js host integration;
- item-context and item-change handling;
- iFile modules and workflows;
- dialog and inline presentation paths;
- host-neutral controllers and services;
- HTTP clients and generated clients;
- API projects and endpoints;
- authentication and authorization;
- Microsoft Graph usage;
- storage and caching;
- local persistence;
- synchronization or queued-operation code;
- feature flags;
- telemetry and diagnostics;
- environment configuration;
- mobile development and tunnel scripts;
- tests, fakes, MSW handlers, and integration fixtures;
- architecture rules and dependency boundaries;
- CI and contract validation;
- incomplete or placeholder workflows;
- active, archived, and potential feature documentation.

For each component record:

- path;
- symbol;
- layer;
- platform;
- feature associations;
- tests;
- runtime evidence;
- maturity;
- source-contract links;
- parity relevance;
- disposition.

Separate generic platform-foundation capability from TaskMaster feature parity. For example, the existence of authentication, an API host, CI, or a mobile shell may be useful platform evidence without satisfying any legacy feature contract.

# Target Implementation Records

For each TaskMaster feature contract, create or update a target implementation record.

Each record must include:

- source feature identifier;
- pinned source reference;
- approved target requirement;
- current target status;
- target components;
- user entry points;
- current implementation behavior;
- tests;
- runtime evidence;
- online status;
- offline status;
- reconnect status;
- desktop status;
- mobile status;
- architecture compatibility;
- known limitations;
- intentional deviations;
- unresolved decisions;
- implementation blockers;
- next migration work package.

Use statuses such as:

- implemented-and-verified;
- implemented-unverified;
- partial;
- scaffold-only;
- missing;
- conflicting;
- blocked-by-platform;
- intentionally-changed;
- candidate-retirement;
- retired-by-approved-decision;
- not-applicable-with-rationale.

A source contract must not be marked fully implemented unless its required dimensions are independently satisfied.

# Parity Matrix

Create the authoritative parity matrix in TMW.

Each entry must include:

- TaskMaster feature identifier;
- source repository and commit;
- source contract path and checksum;
- approved target requirement;
- current TMW implementation reference;
- overall parity status;
- online status;
- offline status;
- reconnect status;
- desktop status;
- mobile status;
- test status;
- runtime-verification status;
- intentional deviations;
- product-decision references;
- blockers;
- migration priority;
- validation result.

Do not reduce parity to one overall percentage. Preserve the dimension-specific statuses.

Examples of acceptable distinctions include:

- online implemented, offline missing, mobile partial;
- desktop verified, mobile shell only;
- source behavior verified, target requirement undecided;
- implementation present, runtime evidence missing;
- behavior intentionally changed by approved decision.

# Offline and Synchronization Assessment

For every required state-changing feature, determine whether TMW currently provides:

- local readable state;
- local writable state;
- pending-operation persistence;
- restart survival;
- deterministic replay;
- idempotency;
- ordering;
- retry;
- conflict detection;
- conflict resolution;
- tombstones or deletion semantics;
- reconnect triggering;
- user-visible pending state;
- user-visible conflict state;
- bounded local storage;
- data encryption;
- schema migration;
- telemetry buffering.

Record only what exists and is evidenced.

Do not design or implement the missing Step 2 platform foundation in this work. Express missing capabilities as platform dependencies or Step 2 requirements.

# Mobile Assessment

For every target-mobile feature, distinguish:

- add-in manifest availability;
- task-pane launch;
- item-context acquisition;
- UI visibility;
- user action availability;
- API connectivity;
- authentication;
- local or cached data availability;
- offline behavior;
- mutation behavior;
- feedback;
- recovery;
- end-to-end verification on a physical or supported mobile host.

A feature is not mobile-complete solely because its controls render.

Where the mobile host cannot provide a required desktop capability directly, record:

- the platform constraint;
- the user outcome that remains required;
- candidate alternate interaction;
- required product decision;
- required backend or local-store dependency.

Do not unilaterally waive mobile parity.

# Architecture Fit

Evaluate each legacy capability against TMW’s No-COM architecture.

Classify the migration approach as:

- reuse current TMW implementation;
- extend current TMW implementation;
- adapt a legacy domain concept without reusing implementation;
- replace with Office.js behavior;
- replace with Microsoft Graph behavior;
- place behind the API;
- implement in a companion web or mobile surface;
- retire by approved decision;
- unresolved.

Identify:

- COM-only assumptions;
- desktop-only assumptions;
- unavailable Office.js APIs;
- Microsoft Graph requirements;
- backend requirements;
- local-store requirements;
- synchronization requirements;
- security or privacy implications;
- licensing or dependency concerns.

Do not add dependencies or modify architecture during this work.

# Existing TMW Work

Inspect current TMW code and completed feature evidence before assigning status.

For any existing feature, determine whether it:

- satisfies the same user outcome as the source contract;
- satisfies only part of the outcome;
- changes semantics;
- supports desktop only;
- supports mobile only at the shell level;
- requires the API;
- requires network connectivity;
- contains mocks or placeholders;
- has automated tests;
- has runtime evidence.

Do not assume feature parity from matching names such as “iFile,” “classify,” “tags,” or “task.”

# Product-Owner Decision Queue

Create a target-side product-decision queue for:

- legacy defects that should not be preserved;
- behavior impossible or impractical in Office.js;
- features requiring a companion application;
- mobile interaction changes;
- offline conflict behavior;
- privacy and local-data retention;
- features that may be retired;
- differences between TaskMaster and current TMW behavior;
- ambiguous source contracts;
- inconsistent platform requirements.

Each decision record must reference the relevant source contract and parity entry.

Do not mark a feature intentionally changed or retired until the corresponding decision is approved.

# Target Acceptance Scenarios

Generate target acceptance scenarios from:

- verified TaskMaster contracts;
- approved product decisions;
- explicit target platform requirements.

Scenarios must retain source traceability and include appropriate tags for:

- online;
- offline;
- reconnect;
- desktop;
- mobile;
- API;
- local store;
- synchronization;
- manual verification;
- automated verification;
- pending platform foundation;
- pending product decision.

Do not generate implementation-specific test code in this work.

Separate:

- source characterization scenarios;
- target acceptance scenarios;
- current TMW verification scenarios;
- future automated tests.

# Migration Work Packages

Produce a dependency-aware migration work-package catalog suitable for later planning.

Each work package must include:

- linked source feature contracts;
- current parity gaps;
- platform dependencies;
- product decisions;
- expected target surfaces;
- online requirement;
- offline requirement;
- mobile requirement;
- acceptance-scenario references;
- suggested delivery phase;
- blockers.

Do not execute the work packages.

Do not absorb Step 2 platform-foundation implementation into this prompt. Instead, identify which parity entries depend on:

- add-in shell;
- authentication;
- API baseline;
- telemetry;
- feature flags;
- local store;
- synchronization;
- environment or deployment foundation.

# Required Artifacts

Use the released framework’s canonical paths. Do not create a parallel hierarchy merely because an example name appears below.

The completed TMW discovery set must include equivalents of:

- target domain profile;
- pinned source-baseline registration;
- target component inventory;
- target implementation records;
- authoritative parity matrix;
- parity coverage report;
- product-decision queue;
- target acceptance scenarios;
- migration work-package catalog;
- Step 2 dependency summary;
- target discovery completion report.

# Non-Goals

Do not:

- edit TaskMaster;
- alter TaskMaster contracts;
- implement Step 2 platform foundation;
- implement missing TaskMaster features;
- refactor TMW product code;
- add local-store or synchronization infrastructure;
- change authentication;
- add API endpoints;
- add telemetry;
- add feature flags;
- change manifests;
- claim mobile parity from shell availability;
- mark planned behavior as implemented;
- retire behavior without approved product decisions;
- replace pinned source references with floating branches;
- generate a single undifferentiated parity percentage.

If a blocking product or architecture issue is discovered, record it. Do not widen the work into implementation.

# Completion Criteria

The TMW portion of Step 1 is complete only when:

- the TaskMaster source baseline is pinned and validates;
- every TaskMaster feature contract has a parity entry or an explicit justified disposition;
- every parity entry has a target implementation record;
- every implementation claim has code, test, or runtime evidence;
- online, offline, reconnect, desktop, and mobile statuses are explicit where required;
- scaffold-only behavior is not marked implemented;
- platform-foundation dependencies are identified;
- architecture conflicts are documented;
- all intentional-change and retirement statuses reference approved decisions;
- unresolved decisions are in the product-owner queue;
- target acceptance scenarios preserve source and decision traceability;
- migration work packages are generated without implementation;
- parity and discovery validators pass;
- the completion report contains no hidden or undispositioned gaps.

Do not report PASS when source checksums are stale, parity entries are missing, required dimensions are blank, or unapproved product changes are represented as final.

# Final Response

Provide:

1. TMW baseline commit and drm-copilot version.
2. Pinned TaskMaster source commit and baseline-manifest checksum.
3. Current TMW component and platform inventory summary.
4. Total source feature contracts.
5. Parity counts by full, partial, scaffold-only, missing, conflicting, intentionally changed, candidate-retired, and unverified.
6. Online, offline, reconnect, desktop, and mobile summaries.
7. Existing TMW capabilities that can be reused.
8. Existing TMW components that should be replaced or substantially revised.
9. Product-owner decisions required.
10. Step 2 platform dependencies.
11. Target acceptance-scenario count and status.
12. Migration work-package summary.
13. Validation and quality-gate results.
14. Exact artifact paths.
15. Remaining blockers.
16. A clear statement of whether Step 1 is complete and, if not, the exact reasons.

Do not begin Step 2.
```

# Definition of Step 1 completion

Step 1 is complete only when all of the following are true:

* The released discovery capability is installed and validated in both repositories.
* TaskMaster’s feature contracts and behavior catalog are complete enough for the agreed scope.
* Required runtime characterization is finished or explicitly accepted as deferred.
* You have adjudicated all blocking unspecified behaviors.
* TaskMaster discovery artifacts are merged and pinned to a commit.
* TMW references that exact TaskMaster commit.
* Every TaskMaster contract has a TMW parity disposition.
* Online, offline, reconnect, desktop, and mobile statuses are explicit.
* Target acceptance scenarios exist.
* The remaining platform dependencies are clearly assigned to Step 2.
* Both repository completion validators pass, or the remaining exceptions are explicitly accepted and documented.

Only after that gate should the platform-foundation prompt be run.
