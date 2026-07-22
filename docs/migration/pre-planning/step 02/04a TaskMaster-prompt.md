# Objective

Perform the TaskMaster portion of Step 2: create the **pinned legacy oracle** consumed by the TMW platform-foundation work.

This repository is the legacy source of truth. Its Step 2 responsibility is limited to:

- freezing the approved Step 1 source baseline;
- selecting the TaskMaster-derived vertical-slice scenarios;
- creating sanitized deterministic fixtures;
- creating a read-only legacy reference exporter;
- recording runtime characterization for online, cached-mode, disconnect, restart, and reconnect behavior;
- publishing a versioned oracle bundle with checksums and expected outcomes.

This is not a modern platform implementation.

Do not add authentication, an API, a PWA, a sync engine, feature flags, cloud deployment, modern telemetry infrastructure, or any production dependency on TMW.

Do not refactor or intentionally change TaskMaster production behavior.

# Required framework

Use the released `drm-copilot` Step 1 discovery capability and Step 2 platform-foundation capability that have been pushed into this repository.

Use the actual released names and paths for:

- platform-profile initialization and validation;
- source-baseline registration;
- architecture and oracle artifacts;
- legacy-oracle manifest validation;
- fixture and evidence validation;
- runtime characterization;
- checksum generation;
- platform-foundation reporting;
- human-exception runbooks;
- final oracle review.

Do not recreate local copies of reusable agents, schemas, validators, or workflows when the released framework already supplies them.

# Prerequisite gate

Before creating authoritative Step 2 artifacts:

1. Verify the released Step 1 and Step 2 agents, skills, schemas, validators, and MCP or CLI tools are installed and callable.
2. Record:
   - the TaskMaster repository identity;
   - current branch;
   - current commit SHA;
   - `drm-copilot` release version;
   - `drm-copilot` source commit SHA;
   - discovery schema version;
   - platform-foundation schema version.
3. Verify the working tree is clean apart from the intended Step 2 branch.
4. Locate and validate the merged Step 1 TaskMaster source-baseline manifest.
5. Verify that all selected source contracts are pinned to the current approved Step 1 commit and that their checksums match.
6. Verify that blocking Step 1 product decisions for the selected vertical slice are resolved.
7. Run the established TaskMaster build and test baseline.
8. Record pre-existing build, test, coverage, or environment failures rather than silently repairing them.
9. Initialize or validate the TaskMaster platform profile and confirm the repository role is `legacy-oracle` or the released equivalent.
10. Confirm the profile identifies:
    - selected feature contracts;
    - selected runtime scenarios;
    - required environments;
    - evidence roots;
    - privacy and redaction policy;
    - oracle output root;
    - completion gates.

If the framework is missing, the Step 1 baseline is invalid, source checksums do not match, or blocking product decisions remain unresolved, stop authoritative work and produce a prerequisite-failure report.

# Required operating mode

Treat this as an epic-scale, evidence-first body of work, but keep production scope narrow.

Use the repository's epic or large-feature lifecycle:

1. Establish the immutable Step 2 baseline.
2. Select the vertical-slice source contracts and scenarios.
3. Define sanitized fixture schemas and privacy rules.
4. Implement the reference exporter as test-support tooling.
5. Generate deterministic fixtures and expected-result records.
6. Conduct required runtime characterization.
7. Publish the oracle bundle.
8. Validate checksums, source references, and completion status.
9. Perform an independent oracle review.
10. Produce the TMW handoff.

Persist orchestration state after every material phase.

Delegate research, characterization, C# tooling, and review to the appropriate specialists.

# Repository role and invariants

Model TaskMaster as:

- repository role: legacy oracle;
- runtime: classic Outlook VSTO on Windows;
- source of truth: observed legacy user outcomes and runtime behavior;
- output: immutable fixtures, expected results, and evidence for the selected target slice.

Preserve these distinctions:

1. observed TaskMaster behavior;
2. inferred TaskMaster behavior;
3. approved source behavior contract;
4. approved semantic change for the target;
5. source fixture;
6. expected legacy result;
7. target acceptance requirement;
8. current TMW implementation status.

This repository may own items 1 through 7. It must not claim item 8.

The oracle must not force the target to reproduce VSTO, COM, WinForms, Outlook object-model, Ribbon, threading, OST, or local-file implementation mechanics. It must capture the user-visible and state-transition behavior that the target must preserve or intentionally change.

# Selected vertical slice

Use the selected Step 2 vertical slice from the platform profile. The default recommendation is the filing/iFile corridor because both repositories already contain meaningful implementation and evidence.

For a filing-oriented slice, include the approved subset of:

- current item context;
- folder hierarchy;
- folder search;
- recent or predicted destination behavior;
- message filing;
- conversation filing where in scope;
- attachment export behavior;
- email-copy or picture-save behavior where in scope;
- optimistic or immediate user feedback;
- partial failure;
- undo or restoration;
- cached-mode behavior;
- disconnect and reconnect;
- Outlook restart;
- destination rename or deletion;
- message moved or deleted elsewhere;
- duplicate invocation;
- multi-store or shared-mailbox behavior where approved.

Do not expand the scope to every TaskMaster feature merely because the exporter can observe it. Later feature waves can publish later oracle versions.

# Required Step 2 artifact structure

Use the released framework's canonical paths. Do not create a parallel structure merely because the following is illustrative.

The result must contain equivalents of:

```text
docs/migration/platform-foundation/
  platform-profile.yaml
  source-baseline.yaml
  selected-scenarios.yaml
  fixture-scope.yaml
  privacy-and-redaction.md
  reports/
  runbooks/
  evidence/

artifacts/platform-foundation/
  taskmaster-oracle-v1/
    manifest.json
    contracts/
    fixtures/
    expected-results/
    environment/
    evidence-index.json
    checksums.json
```

# 1. Freeze the source baseline

Create an immutable oracle baseline containing:

- repository identity;
- source commit SHA;
- Step 1 source-baseline path and checksum;
- selected feature-contract identifiers;
- selected contract paths and checksums;
- approved product-decision references;
- runtime environment identifiers;
- TaskMaster build and test baseline;
- `drm-copilot` release and schema versions;
- oracle version;
- creation timestamp;
- evidence roots;
- unresolved-but-accepted limitations.

Do not use a floating branch as an authoritative source reference.

If TaskMaster production behavior changes later, publish a new oracle version rather than silently modifying the prior bundle.

# 2. Fixture privacy and redaction

Before exporting data, define a fail-closed fixture policy.

The oracle must not contain:

- real message bodies;
- real email addresses;
- real tenant identifiers;
- access or refresh tokens;
- app-registration secrets;
- personal folder paths;
- personal attachment contents;
- personal calendar or task content;
- production mailbox identifiers unless transformed into nonreversible synthetic keys;
- diagnostic logs containing prohibited data.

Prefer:

- fully synthetic mailbox fixtures;
- deterministic stable synthetic identifiers;
- normalized display values;
- redacted or hashed references only when the privacy decision explicitly allows them;
- checksums for integrity;
- a data-classification record for every fixture field.

If a runtime observation necessarily contains sensitive data, store only a sanitized derived result in the repository and record where the protected raw evidence is held outside source control.

# 3. Read-only reference exporter

Create a separate test-support or tooling project, not production add-in code.

A suitable repository-local shape is:

```text
tools/
  TaskMaster.ReferenceExporter/

tests or existing test-project structure/
  TaskMaster.ReferenceExporter.Tests/
```

Use the actual repository conventions for project placement and tests.

The exporter must:

- be read-only with respect to TaskMaster and Outlook state;
- accept explicit fixture or scenario inputs;
- produce deterministic machine-readable output;
- normalize volatile identifiers and timestamps through injected seams;
- support dry-run or preview where appropriate;
- separate host-bound capture from pure normalization;
- identify source path, symbol, and contract references;
- record exporter version;
- produce checksums;
- fail explicitly on unsupported or ambiguous data;
- avoid dependence on TMW;
- avoid writing into the user's normal TaskMaster settings or classifier locations.

Where direct Outlook access is required, isolate it behind a narrow adapter. Keep all normalization, comparison, and rendering logic host-neutral and unit-testable.

# 4. Fixture model

For the selected slice, fixture records should include only approved fields such as:

- synthetic account and mailbox key;
- synthetic store key;
- folder identifier, display name, path, and hierarchy;
- folder type and store relationship;
- synthetic message key;
- subject or body placeholder when required by a rule;
- sender/recipient class rather than real address;
- received timestamp through a deterministic fixture clock;
- source folder;
- categories and flags;
- conversation membership where relevant;
- attachment metadata and synthetic content hash;
- TaskMaster classifier or ranking result;
- TaskMaster settings affecting the scenario;
- pre-state;
- action;
- post-state;
- user feedback;
- error or partial-failure result;
- evidence references.

For move operations, record the identity semantics actually observed. Do not assume identifiers remain stable.

# 5. Legacy scenario pack

Create machine-valid scenarios linked to Step 1 feature contracts.

For a filing slice, include at least the applicable approved scenarios:

```text
TM-FILING-ONLINE-001
TM-FILING-CACHED-OFFLINE-001
TM-FILING-DISCONNECT-DURING-ACTION-001
TM-FILING-RECONNECT-001
TM-FILING-OUTLOOK-RESTART-001
TM-FILING-DUPLICATE-INVOKE-001
TM-FILING-DESTINATION-RENAMED-001
TM-FILING-DESTINATION-DELETED-001
TM-FILING-MESSAGE-MOVED-ELSEWHERE-001
TM-FILING-MESSAGE-DELETED-ELSEWHERE-001
TM-FILING-PARTIAL-ATTACHMENT-FAILURE-001
TM-FILING-UNDO-001
```

Add only scenarios supported by selected scope and available evidence.

Each scenario must record:

- scenario identifier;
- linked source contract;
- environment;
- fixture;
- preconditions;
- user action;
- pre-state;
- observed behavior;
- post-state;
- ordering and timing;
- user-visible feedback;
- failure behavior;
- repeatability;
- evidence;
- confidence;
- approved target requirement;
- approved semantic difference, if any;
- automation status;
- human-runbook reference when needed.

Do not invent an expected result when legacy behavior is unknown or contradictory.

# 6. Runtime characterization

Static inspection and unit tests are not sufficient for cached Outlook and VSTO behavior.

Prioritize runtime characterization for:

- online filing;
- cached Exchange mode;
- fully disconnected behavior;
- connectivity lost during a workflow;
- reconnect and eventual Exchange synchronization;
- Outlook process termination and restart;
- pending or queued legacy work, if any;
- message and conversation moves;
- partial attachment or export failure;
- destination-folder changes;
- message changes from another client;
- duplicate user actions;
- undo behavior;
- multiple stores;
- shared mailbox or delegated mailbox where approved;
- PST or search-folder behavior where approved.

For every runtime execution record:

- Windows version;
- Outlook version and bitness;
- account/store type;
- cached-mode state;
- network state;
- sanitized fixture identifier;
- exact operator action;
- before snapshot;
- after snapshot;
- TaskMaster logs or diagnostics after redaction;
- duration and ordering;
- repeatability;
- result;
- evidence path;
- confidence.

When human interaction is unavoidable, create the required runbook before execution. Mark the scenario `AWAITING-HUMAN-CHARACTERIZATION` until evidence is captured. Do not mark it passed based on instructions alone.

# 7. Expected-result records

Expected results must be machine-readable and derived from:

- verified source behavior;
- verified tests; or
- an approved semantic target decision.

Each result must distinguish:

- exact legacy outcome;
- user-visible outcome required in the target;
- implementation detail not required in the target;
- approved behavior change;
- platform-specific expectation;
- offline expectation;
- reconnect expectation;
- mobile relevance;
- evidence and confidence.

For example, a target may be required to preserve the outcome “message is filed to the chosen destination and the user sees a durable pending state while offline” without reproducing the legacy Outlook-object-model operation sequence.

# 8. Oracle bundle publication

Publish a versioned bundle beneath the canonical artifact root.

The bundle manifest must include:

- oracle schema version;
- oracle version;
- TaskMaster repository and commit;
- `drm-copilot` version and commit;
- source-baseline path and checksum;
- contract list and checksums;
- fixture list and checksums;
- expected-result list and checksums;
- environment records;
- evidence index;
- exporter version;
- build and test baseline;
- runtime-characterization summary;
- unresolved and accepted limitations;
- completion status.

The bundle must be reproducible from the same source revision and approved fixture inputs.

TMW must be able to validate the bundle without running TaskMaster or reading a personal mailbox.

# 9. Validation

Run the released validators for:

- platform profile;
- source baseline;
- selected contracts;
- oracle manifest;
- fixture schemas;
- expected results;
- evidence references;
- checksums;
- privacy policy;
- runtime-characterization status;
- completion report.

The completion gate must fail when:

- source references are floating or stale;
- checksums do not match;
- a required scenario lacks a result or explicit approved deferral;
- runtime evidence is claimed but absent;
- prohibited data appears in the bundle;
- expected behavior is inferred but marked verified;
- target behavior is changed without an approved decision;
- TMW-specific implementation status appears in the source oracle;
- the existing TaskMaster build/test baseline regresses because of the exporter work.

# Non-goals

Do not:

- modify TaskMaster production behavior;
- refactor the VSTO host;
- migrate application features;
- introduce a modern API;
- introduce authentication;
- introduce Microsoft Graph as a TaskMaster runtime dependency;
- add a companion PWA;
- add a local-first target store;
- add a target sync engine;
- add production telemetry or feature flags;
- add cloud infrastructure;
- edit TMW;
- create a production dependency on TMW;
- copy personal mailbox data into source control;
- claim mobile behavior that TaskMaster does not possess;
- reinterpret unknown behavior as a passing oracle result.

# Completion criteria

The TaskMaster Step 2 oracle work is complete only when:

- [ ] The approved Step 1 source baseline is pinned and validates.
- [ ] The selected vertical-slice contracts are explicit.
- [ ] The privacy and redaction policy is enforced.
- [ ] The reference exporter is read-only, deterministic, and tested.
- [ ] Every fixture is synthetic or approved and sanitized.
- [ ] Every required scenario has verified evidence or an explicit approved deferral.
- [ ] Cached-mode, disconnect, restart, and reconnect behavior are characterized where required.
- [ ] Expected results separate source outcome from target implementation mechanics.
- [ ] The oracle manifest and all checksums validate.
- [ ] The existing TaskMaster build and test baseline passes without product-behavior changes.
- [ ] An independent oracle review reports no blocking findings.
- [ ] TMW can consume the bundle by pinned commit and checksum without TaskMaster runtime access.

Do not report full completion while required human characterization is still pending. Report `INCOMPLETE` or the framework's equivalent with exact blockers.

# Final response

Provide:

1. TaskMaster repository and source commit.
2. `drm-copilot` release and schema versions.
3. Selected feature contracts and scenario count.
4. Reference-exporter location and behavior.
5. Build and test results.
6. Runtime-characterization summary.
7. Privacy/redaction result.
8. Oracle bundle path.
9. Oracle manifest checksum.
10. Contract, fixture, and expected-result checksum summary.
11. Accepted semantic differences.
12. Deferred or blocked scenarios.
13. Validation and review results.
14. Exact TMW handoff instructions, including the pinned TaskMaster commit and oracle checksum.

Do not begin TMW implementation from this repository.