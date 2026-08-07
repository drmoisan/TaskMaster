# Objective

Develop a reusable legacy-system discovery and parity-definition capability within the `drm-copilot` repository.

The capability must support repositories migrating an existing application to a modern architecture by enabling agentic discovery of:

* current system behavior;
* feature and workflow inventory;
* legacy implementation coverage;
* runtime characterization;
* undocumented and contradictory behavior;
* source-to-target parity;
* product decisions;
* executable acceptance scenarios.

The implementation must remain domain-neutral. It must not contain TaskMaster-, TMW-, Outlook-, VSTO-, email-, or task-management-specific behavior in the core reusable framework.

The immediate consumers will be:

* `drmoisan/TaskMaster`, which will provide legacy-system context, feature contracts, runtime evidence, characterization scenarios, and coverage information;
* `drmoisan/TMW`, which will provide the modern implementation, target architecture decisions, parity status, and verification tests.

The work in this repository must provide reusable workflow mechanics, schemas, agents, skills, validators, hooks, templates, analyzers, CLI commands, MCP surfaces, publishing support, and documentation.

# Required Operating Mode

Treat this as a large, cross-cutting feature.

Use the repository’s standard large-feature orchestration lifecycle:

1. Promote or create the active feature folder.
2. Perform repository and external research.
3. Produce a detailed specification.
4. Produce an atomic implementation plan.
5. Execute the plan task by task through delegated specialist agents.
6. Run all applicable language and repository quality gates.
7. Perform feature review.
8. Produce complete evidence and handoff documentation.

Do not implement directly in the orchestrator when an appropriate specialist agent exists.

Persist orchestration state after every material phase and task transition.

Do not modify canonical policy files except through the repository’s established policy-authoring and push-down mechanisms.

# Scope

Implement a generic capability with the working name:

`legacy-discovery-and-parity`

Use a better final name if repository naming conventions indicate one, but preserve the intent.

The capability must include the following functional areas.

## 1. Generic Agent Roles

Create reusable specialist agents for at least the following responsibilities.

### Legacy Parity Analyst

Responsibilities:

* inspect a legacy repository systematically;
* identify observable features and workflows;
* extract behavior without proposing implementation;
* distinguish verified, inferred, contradictory, unknown, and intentionally unspecified behavior;
* maintain traceability to source, tests, documentation, and runtime evidence;
* update feature contracts and coverage records;
* avoid product decisions;
* avoid source-code implementation changes.

### Runtime Characterization Analyst

Responsibilities:

* define repeatable runtime characterization scenarios;
* identify required fixtures, preconditions, actions, and observations;
* compare pre-action and post-action state;
* capture runtime evidence references;
* classify behavior confidence;
* identify environment-specific behavior;
* support manual, semi-automated, and automated characterization;
* avoid inventing expected results where legacy behavior is not yet known.

### Requirements Reconciler

Responsibilities:

* reconcile legacy behavior contracts with target implementation records;
* produce or update parity entries;
* identify missing, partial, conflicting, intentionally changed, or retired behavior;
* ensure that each parity claim is evidence-backed;
* preserve separation between observed legacy behavior and approved target behavior.

### Migration Coverage Reviewer

Responsibilities:

* verify discovery completeness;
* inspect coverage ledgers;
* identify uninspected or undispositioned components;
* verify that every required feature has evidence and acceptance scenarios;
* verify that unknown behavior is represented in an unresolved-behavior or product-decision record;
* fail closed when required discovery coverage is incomplete.

Use repository conventions for model selection, tool allowlists, memory scope, hooks, and output validation.

## 2. Generic Skills

Create reusable skills for at least:

* `inventory-legacy-system`
* `extract-feature-contract`
* `characterize-runtime-behavior`
* `reconcile-parity-matrix`
* `review-discovery-coverage`
* `adjudicate-unspecified-behavior`
* `generate-acceptance-scenarios`
* `initialize-discovery-workspace`
* `validate-discovery-artifacts`

Each skill must:

* declare explicit inputs;
* declare exact permitted output locations or repository-provided configurable output roots;
* fail closed when required configuration is absent;
* avoid hard-coded domain assumptions;
* produce deterministic artifacts where practical;
* include clear routing to the appropriate specialist agent;
* be translatable or publishable across Claude Code, Codex, and GitHub Copilot using existing repository mechanisms.

## 3. Repository-Local Configuration Contract

Define a generic repository-local configuration file that consuming repositories can provide.

Use a path consistent with repository conventions, such as:

```text
docs/migration/discovery/domain-profile.yaml
```

or another clearly justified path.

The configuration contract must support:

* system name;
* system role, such as legacy source or modernization target;
* production roots;
* test roots;
* solution or project entry points;
* documentation roots;
* evidence roots;
* required platform or operating modes;
* domain extensions;
* analyzer configuration;
* excluded paths;
* artifact output roots;
* target or source repository references;
* required feature dimensions;
* completion-gate configuration.

The core framework must read this configuration rather than hard-code repository-specific paths.

Provide:

* schema;
* example;
* validation;
* initialization template;
* clear documentation.

## 4. Machine-Readable Schemas

Create versioned schemas for at least the following artifacts.

### Feature Contract

Must support:

* schema version;
* feature identifier;
* name;
* domain or capability grouping;
* summary;
* user or system trigger;
* preconditions;
* inputs;
* state transitions;
* observable outputs;
* side effects;
* ordering and timing semantics;
* cancellation behavior;
* failure behavior;
* offline or disconnected behavior;
* mobile or alternate-client applicability;
* environment constraints;
* security or permissions considerations;
* source evidence;
* runtime evidence;
* confidence classification;
* unresolved questions;
* product-decision status;
* target relevance;
* extensible domain-specific metadata.

### Coverage Ledger

Must support:

* repository revision;
* discovered components;
* component type;
* path and symbol references;
* inspection status;
* disposition;
* linked feature contracts;
* tests found;
* runtime validation requirement;
* exclusions and rationale;
* responsible workflow or agent;
* last-updated evidence.

### Runtime Characterization Scenario

Must support:

* scenario identifier;
* linked feature identifiers;
* environment;
* fixture;
* preconditions;
* action;
* observation plan;
* pre-state capture;
* post-state capture;
* expected result when known;
* result status;
* deviations;
* evidence;
* repeatability;
* confidence;
* unresolved findings.

### Parity Matrix

Must support:

* source repository and pinned revision;
* target repository and pinned revision;
* feature identifier;
* source contract reference;
* target implementation reference;
* overall parity state;
* dimension-specific status;
* offline status;
* mobile status;
* evidence;
* intentional deviations;
* retirement decision;
* blockers;
* validation status.

### Unspecified Behavior Record

Must support:

* behavior identifier;
* linked feature;
* question;
* evidence;
* competing interpretations;
* impact;
* status;
* decision owner;
* decision;
* rationale;
* approved target behavior;
* acceptance-test implications.

### Product Decision Record

Must support:

* decision identifier;
* linked features or unspecified behaviors;
* decision;
* rationale;
* alternatives;
* affected platforms;
* required migration behavior;
* approval metadata;
* resulting acceptance criteria.

### Evidence Reference

Define a reusable evidence structure supporting:

* evidence type;
* repository;
* revision;
* path;
* symbol;
* line range;
* runtime artifact;
* timestamp;
* environment;
* checksum where applicable;
* author or agent;
* confidence;
* notes.

Use JSON Schema unless repository conventions strongly support another machine-validatable format.

Schemas must allow domain-specific extension without weakening core validation.

## 5. Validators

Implement deterministic validators for all schemas and cross-artifact invariants.

At minimum provide commands equivalent to:

```text
dev.discovery.validate-profile
dev.discovery.validate-feature-contract
dev.discovery.validate-coverage-ledger
dev.discovery.validate-runtime-scenario
dev.discovery.validate-parity-matrix
dev.discovery.validate-decisions
dev.discovery.validate-all
```

Validation must detect at least:

* malformed artifacts;
* unsupported schema versions;
* duplicate identifiers;
* broken references;
* missing evidence;
* invalid confidence or status values;
* unresolved behavior marked as verified;
* parity entries without source contracts;
* required-parity features without acceptance scenarios;
* missing platform-mode classifications;
* coverage records without disposition;
* excluded components without rationale;
* conflicting product decisions;
* stale repository revision references where deterministically detectable.

The complete validation command must produce both:

* human-readable Markdown summary;
* machine-readable JSON results.

It must return a nonzero exit code on blocking failures.

## 6. Completion Gates and Hooks

Add reusable hooks following the repository’s existing enforcement model.

Required gates should include:

* research agents cannot modify production code;
* feature contracts marked verified require evidence;
* runtime scenarios marked passed require runtime evidence;
* unknown or contradictory behavior requires an unspecified-behavior record;
* discovery completion requires all in-scope components to have a coverage disposition;
* required features must have acceptance scenarios;
* parity completion requires explicit online, offline, and mobile status where configured;
* implementation planning cannot begin when configured discovery gates remain incomplete.

Do not force repositories to require mobile or offline analysis unless their local domain profile declares those dimensions required.

Hook behavior must be configurable through the repository-local profile while preserving fail-closed behavior for malformed configuration.

Provide SubagentStop validation for each new agent.

## 7. Initialization and Templates

Provide a command that initializes the discovery structure in a consuming repository without overwriting existing content.

Equivalent behavior:

```text
dev.discovery.init
```

It should be capable of creating templates for:

```text
docs/migration/discovery/
  domain-profile.yaml
  scope.yaml
  coverage-ledger.json
  unspecified-behaviors.yaml
  product-decisions.yaml
  feature-contracts/
  characterization/
  evidence/
  reports/
```

The actual default structure may differ if repository conventions indicate a better standard.

Initialization must:

* be idempotent;
* support dry-run;
* report created, skipped, and conflicting files;
* validate the resulting structure;
* avoid domain-specific sample values in production templates;
* optionally include a separate documented example fixture for tests.

## 8. Generic Static Analyzers

Implement reusable analyzers that can seed discovery inventories.

Prioritize generic analyzers with high confidence and deterministic output.

At minimum evaluate and, where practical, implement:

* repository tree and project inventory;
* .NET solution and project inventory;
* C# type and symbol inventory;
* event subscription extraction;
* configuration and settings extraction;
* file-system access detection;
* registry access detection;
* COM interop usage detection;
* UI callback extraction;
* test-to-production-component reference mapping;
* dependency inventory.

Evaluate whether a reusable Office or VSTO analyzer should be included.

A VSTO analyzer may understand general constructs such as:

* Ribbon XML;
* Ribbon callbacks;
* Office interop references;
* `ThisAddIn`;
* COM event subscriptions;
* Outlook item types;
* MAPI property access.

It must not understand TaskMaster-specific feature semantics.

Analyzer outputs must:

* be machine-readable;
* include source references;
* be deterministic;
* distinguish detected facts from inferred interpretations;
* be consumable by the coverage-ledger workflow;
* support dry-run or read-only operation.

Do not introduce a heavy compiler or parsing dependency without evaluating existing repository dependencies and simpler alternatives.

## 9. CLI and MCP Integration

Expose the reusable discovery functions through the repository’s established Python CLI and MCP surfaces.

Provide a coherent namespace, for example:

```text
dev.discovery.init
dev.discovery.inventory
dev.discovery.new-contract
dev.discovery.new-scenario
dev.discovery.link-evidence
dev.discovery.validate
dev.discovery.coverage-report
dev.discovery.parity-report
dev.discovery.generate-acceptance
```

Naming may change to align with repository conventions.

The VS Code extension and standalone MCP package must expose appropriate workspace-facing commands or tools.

Requirements:

* the Python implementation is authoritative;
* VS Code and MCP wrappers remain thin;
* outputs are deterministic;
* paths are workspace-relative and validated;
* error messages are actionable;
* commands support noninteractive agent use;
* behavior is tested at the Python layer and wrapper layer as appropriate.

## 10. Cross-Ecosystem Publishing

Update the repository’s canonical customization sources and generation or push-down workflows so that the new:

* agents;
* skills;
* rules;
* prompts;
* hooks;
* templates;
* documentation;

are correctly available to Claude Code, Codex, and GitHub Copilot.

Do not maintain three manually divergent implementations.

Use the repository’s existing canonical-source, conversion, validation, and push-down architecture.

Add conversion or publishing tests that prove the new assets are represented correctly across supported ecosystems.

## 11. Acceptance-Scenario Generation

Implement a generic workflow that converts confirmed feature contracts and product decisions into acceptance scenarios.

Support at least:

* Gherkin-style Markdown or `.feature` output;
* stable linkage to source feature IDs;
* scenario identifiers;
* platform-mode tags;
* offline tags where applicable;
* mobile tags where applicable;
* traceability to evidence and product decisions;
* preservation of unresolved behavior as pending rather than silently defining expected results.

The workflow must distinguish:

* legacy characterization scenarios;
* target acceptance scenarios;
* implementation-specific automated tests.

It should generate test specifications, not implementation-specific test code unless explicitly invoked by a consuming repository’s later implementation workflow.

## 12. Reports

Provide deterministic reports for:

### Discovery Coverage Report

Include:

* total components;
* inspected;
* excluded;
* blocked;
* undispositioned;
* feature-contract coverage;
* runtime-characterization coverage;
* evidence completeness;
* unresolved behavior count.

### Parity Report

Include:

* total features;
* full parity;
* partial parity;
* missing;
* intentionally changed;
* retired;
* online verification;
* offline verification;
* mobile verification;
* blockers;
* unresolved decisions.

### Discovery Completion Report

Include:

* pass or fail;
* blocking findings;
* warnings;
* stale or conflicting references;
* incomplete artifacts;
* next required actions.

Reports should support Markdown and JSON.

## 13. Documentation

Document:

* conceptual architecture;
* reusable-versus-local responsibility boundary;
* installation and push-down behavior;
* initialization;
* domain-profile configuration;
* artifact lifecycle;
* agent responsibilities;
* workflow examples;
* validation commands;
* hook behavior;
* schema extension;
* evidence requirements;
* source-versus-target repository model;
* TaskMaster/TMW usage as a noncanonical example or case study.

Do not embed TaskMaster-specific content into reusable templates or standing instructions.

A consuming repository should be able to understand that:

* the extension provides workflow, schemas, enforcement, generic analyzers, and commands;
* a legacy source repository provides observed behavior, evidence, coverage, and characterization;
* a target repository provides architecture decisions, implementation status, parity verification, and target tests.

## 14. Testing

Provide comprehensive tests appropriate to each layer.

Required test categories:

* schema validation tests;
* valid and invalid fixture tests;
* cross-reference validation tests;
* identifier uniqueness tests;
* initialization idempotency tests;
* dry-run tests;
* report-generation snapshot or structural tests;
* analyzer fixture tests;
* hook validation tests;
* agent-output validation tests;
* CLI tests;
* MCP wrapper tests;
* VS Code extension command tests where applicable;
* cross-ecosystem conversion or push-down tests;
* path-safety tests;
* malformed-config fail-closed tests;
* Windows and cross-platform behavior tests as applicable.

Use synthetic test fixtures rather than TaskMaster source material unless a small, clearly licensed example is deliberately added under tests.

## 15. Non-Goals

Do not:

* implement TaskMaster feature contracts;
* inspect or modify TaskMaster or TMW as part of this feature;
* encode Outlook-specific product requirements;
* define the TaskMaster target architecture;
* create a full automated Outlook UI-testing framework;
* add domain-specific feature IDs;
* make the extension the authoritative store for migration evidence;
* store consuming-repository feature catalogs inside `drm-copilot`;
* duplicate orchestration logic already present in the repository;
* create manually divergent Claude, Codex, and Copilot implementations;
* weaken existing fail-closed quality or security policies.

# Architectural Boundaries

Apply the following ownership boundary.

## `drm-copilot` Owns

* reusable agents;
* reusable skills;
* reusable standing instructions and rules;
* schemas;
* generic validators;
* hooks;
* generic templates;
* generic analyzers;
* initialization;
* CLI commands;
* MCP tools;
* VS Code command wrappers;
* cross-ecosystem publishing;
* documentation of the reusable framework.

## Consuming Legacy Repository Owns

* domain profile;
* scope;
* legacy feature contracts;
* legacy coverage ledger;
* runtime characterization scenarios;
* legacy evidence;
* unspecified-behavior records;
* legacy architecture documentation;
* repository-specific characterization tools.

## Consuming Target Repository Owns

* target architecture decisions;
* target implementation records;
* parity matrix or authoritative target-side parity status;
* target acceptance tests;
* offline synchronization tests;
* mobile tests;
* implementation evidence;
* target-specific test tools.

The core implementation must enforce or document this separation clearly.

# Required Research Questions

Before implementation, investigate and document:

1. How current `drm-copilot` agents, skills, hooks, schemas, CLI commands, MCP tools, and VS Code commands are authored and published.
2. Which customization surface is canonical.
3. How current cross-ecosystem conversion handles agents, skills, rules, hooks, and templates.
4. How current feature-folder and evidence-location conventions should apply.
5. How hooks currently receive context and configuration.
6. Whether JSON Schema infrastructure already exists.
7. Whether YAML parsing and validation dependencies already exist.
8. Whether the current MCP server supports file creation, validation, reporting, and analyzer invocation patterns needed here.
9. How current output validators are structured.
10. Which existing skills can be reused rather than duplicated.
11. Whether current atomic planning and acceptance-criteria workflows need extension for discovery-created criteria.
12. How to preserve backward compatibility for repositories that do not use discovery tooling.
13. How to version schemas and migrate older artifacts.
14. How to make analyzers deterministic across Windows, Linux, and macOS where applicable.
15. Whether Roslyn or another compiler-backed analyzer is justified for C# analysis, compared with lower-cost parsing approaches.
16. How to avoid excessive context injection into repositories that do not invoke the discovery workflows.

Use actual repository evidence. Do not infer repository conventions from filenames alone.

# Required Specification Decisions

The specification must explicitly decide:

* final capability name;
* canonical file locations;
* agent names and responsibilities;
* skill names and inputs;
* schema formats and versioning;
* status and confidence enumerations;
* domain-extension mechanism;
* repository-local configuration contract;
* evidence-reference format;
* validation architecture;
* hook architecture;
* analyzer architecture;
* CLI naming;
* MCP exposure;
* VS Code exposure;
* publishing model;
* backward compatibility;
* migration strategy for future schema versions;
* completion-gate defaults;
* how optional dimensions such as offline and mobile become required;
* how acceptance scenarios are generated;
* how discovery criteria differ from implementation acceptance-criteria check-off;
* how target parity records reference immutable source revisions.

# Required Acceptance Criteria

The completed feature must satisfy all of the following.

## Core framework

* [ ] A reusable legacy discovery and parity framework exists in `drm-copilot`.
* [ ] Core framework content contains no TaskMaster- or TMW-specific behavior.
* [ ] Repository-specific configuration is loaded from a local domain profile.
* [ ] Repositories not using the capability remain unaffected.

## Agents and skills

* [ ] Required generic agents are implemented and validated.
* [ ] Required generic skills are implemented and validated.
* [ ] Agents are prevented from writing outside authorized paths.
* [ ] Every new agent has a SubagentStop output validator.
* [ ] Agent and skill assets are available across supported coding-agent ecosystems.

## Schemas and artifacts

* [ ] All required schemas exist and are versioned.
* [ ] Valid examples pass validation.
* [ ] Invalid examples fail with actionable messages.
* [ ] Cross-artifact references are validated.
* [ ] Domain-specific extensions are supported without bypassing core required fields.

## Initialization

* [ ] A consuming repository can initialize a discovery workspace through a deterministic command.
* [ ] Initialization is idempotent.
* [ ] Dry-run is supported.
* [ ] Existing files are not overwritten silently.
* [ ] The initialized workspace validates successfully.

## Validation and reporting

* [ ] Individual artifact validators exist.
* [ ] A complete validation command exists.
* [ ] Blocking failures return a nonzero exit code.
* [ ] Markdown and JSON reports are generated.
* [ ] Completion validation checks coverage, evidence, unresolved behavior, acceptance scenarios, and configured platform dimensions.

## Hooks

* [ ] Verified claims require evidence.
* [ ] Unknown behavior requires an unresolved-behavior record.
* [ ] Runtime pass results require runtime evidence.
* [ ] Discovery completion fails when in-scope components lack disposition.
* [ ] Required platform dimensions cannot remain unspecified.
* [ ] Implementation planning can be gated on discovery completion.

## Analyzers

* [ ] At least a generic repository/project inventory analyzer is implemented.
* [ ] At least one meaningful C# or .NET analyzer is implemented.
* [ ] Analyzer output is deterministic and machine-readable.
* [ ] Analyzer findings include source references.
* [ ] Facts are distinguished from inference.
* [ ] Analyzer fixtures and tests exist.

## CLI, extension, and MCP

* [ ] Python is the authoritative implementation layer.
* [ ] CLI commands are documented and tested.
* [ ] MCP tools expose appropriate discovery functions.
* [ ] VS Code commands or wrappers are added where justified.
* [ ] Wrappers do not duplicate business logic.
* [ ] Workspace path safety is enforced.

## Cross-ecosystem publishing

* [ ] Canonical source assets publish correctly to Claude Code.
* [ ] Canonical source assets publish correctly to Codex.
* [ ] Canonical source assets publish correctly to GitHub Copilot.
* [ ] Conversion or push-down validation covers the new assets.
* [ ] No unsupported references remain in generated outputs.

## Acceptance scenarios

* [ ] Confirmed contracts can generate linked acceptance scenarios.
* [ ] Platform-mode tags are supported.
* [ ] Offline and mobile tags are supported when configured.
* [ ] Unresolved behavior is not converted into a false expected result.
* [ ] Generated scenarios preserve traceability to contracts and decisions.

## Documentation

* [ ] Architecture and ownership boundaries are documented.
* [ ] A consuming-repository setup guide exists.
* [ ] Schema and status semantics are documented.
* [ ] Extension-versus-local placement is documented.
* [ ] A source-and-target repository workflow is documented.
* [ ] TaskMaster/TMW may appear only as a clearly separated example or case study, not as core configuration.

## Quality

* [ ] All applicable Python quality gates pass.
* [ ] All applicable TypeScript quality gates pass.
* [ ] All applicable PowerShell quality gates pass.
* [ ] All applicable C# quality gates pass for any C# additions.
* [ ] All tests pass.
* [ ] Feature review finds no blocking defects.
* [ ] Required evidence is stored in canonical feature evidence locations.
* [ ] Documentation and generated customization outputs are current.

# Deliverables

Produce:

1. Active feature documentation.
2. Research artifact.
3. Specification.
4. Atomic implementation plan.
5. New or updated agents.
6. New or updated skills.
7. Schemas.
8. Validators.
9. Hooks.
10. Templates.
11. CLI tooling.
12. Generic analyzers.
13. MCP integration.
14. VS Code integration where justified.
15. Cross-ecosystem publication support.
16. Tests.
17. User and maintainer documentation.
18. Discovery framework architecture diagram.
19. Example consuming-repository structure.
20. Final feature audit.
21. Completion report with acceptance-criteria status.
22. A follow-on handoff describing the exact work that must be performed locally in TaskMaster and TMW after this reusable framework is published.

# Implementation Guidance

Prefer a thin, composable framework over a monolithic discovery engine.

Use structured artifacts as the source of truth. Markdown reports should be generated views where practical.

Separate:

* observed legacy facts;
* inferred behavior;
* product decisions;
* target requirements;
* implementation status.

Never allow one status field to blur these categories.

Prefer deterministic, local, read-only analysis.

Avoid requiring external services for core validation and reporting.

Design analyzers so that repositories can add local plugins or domain-specific enrichment without modifying core framework code.

Do not place large domain catalogs into standing instructions. Load discovery context only when the relevant skill or agent is invoked.

Ensure the framework supports repositories with:

* one repository containing both source and target;
* separate source and target repositories;
* no runtime characterization capability;
* manual runtime evidence;
* automated runtime evidence;
* optional offline or mobile dimensions;
* more than one target client.

# Final Response Requirements

At completion, provide:

* concise implementation summary;
* architectural decisions;
* files and components added;
* commands available;
* supported agent ecosystems;
* test and quality results;
* known limitations;
* backward-compatibility impact;
* exact steps to publish or push the capability into TaskMaster and TMW;
* exact local artifacts TaskMaster must create;
* exact local artifacts TMW must create;
* unresolved follow-on work.

Do not claim completion unless all blocking acceptance criteria and repository quality gates pass.
