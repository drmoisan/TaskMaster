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