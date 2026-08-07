The clean split is:

**Put reusable orchestration mechanics and cross-repository policy in `drm-copilot`. Put domain knowledge, migration evidence, feature contracts, and executable characterization assets in TaskMaster/TMW.**

That follows the repository’s own architecture: `drm-copilot` is designed to package and publish common agentic customizations across repositories, while each target repository supplies its own source tree, feature documents, checkpoints, evidence, and project-specific rules.

# What belongs in `drm-copilot`

These assets should be generic enough to work on another legacy migration without mentioning TaskMaster, Outlook, VSTO, or TMW.

## 1. Generic discovery agents

Add reusable agents such as:

* `legacy-parity-analyst`
* `runtime-characterization-analyst`
* `requirements-reconciler`
* `migration-coverage-reviewer`

Their prompts should define responsibilities and evidence standards, but not TaskMaster-specific outputs.

For example, the generic agent may say:

> Identify observable behavior, trace it to evidence, assign confidence, distinguish verified behavior from inferred behavior, and record unresolved semantics.

It should not say:

> Inspect QuickFiler, SpamBayes, Outlook categories, or TaskTree.

The current repository already treats agents as reusable specialist personas with tool allowlists, hooks, model selection, and memory scope.

## 2. Generic discovery skills

These belong in the extension:

```text
inventory-legacy-system
extract-feature-contract
characterize-runtime-behavior
reconcile-parity-matrix
review-discovery-coverage
adjudicate-unspecified-behavior
generate-acceptance-scenarios
```

Each skill should accept repository-provided configuration rather than hard-code paths or concepts.

For example:

```yaml
inventory_roots:
  - src
  - tests

contract_schema:
  schemas/discovery/feature-contract.schema.json

domain_profile:
  docs/migration/discovery/domain-profile.yaml
```

The existing skills architecture is already intended for reusable, user-invocable workflows with their own tool and agent routing.

## 3. Generic artifact schemas

The shape of the artifacts should be centrally governed:

* Feature contract schema
* Coverage ledger schema
* Parity matrix schema
* Runtime scenario schema
* Product decision schema
* Evidence reference schema
* Discovery completion report schema

These schemas should not prescribe TaskMaster feature IDs or Outlook concepts. They should define extensible fields.

Example:

```yaml
feature_id: string
name: string
domain: string
triggers: array
preconditions: array
state_changes: array
observable_results: array
platform_modes: object
evidence: array
confidence: enum
decision_status: enum
extensions: object
```

TaskMaster can then use `extensions.outlook`, `extensions.vsto`, or `extensions.mailbox`.

## 4. Generic validators

The implementation of validation belongs in `drm-copilot`:

```text
validate-feature-contract
validate-coverage-ledger
validate-parity-matrix
validate-runtime-scenario
validate-discovery-completion
```

The validator should enforce general invariants such as:

* Every claim has evidence.
* Every in-scope component has a coverage disposition.
* Unknown behavior is logged.
* Every required-parity feature has acceptance scenarios.
* Blank platform-mode fields are prohibited.
* Contradictory evidence is not marked verified.

The target repo should provide the policy configuration that tells the validator which modes matter.

## 5. Generic hooks and enforcement

The hook scripts and hook registration belong in `drm-copilot` because enforcement should be consistent everywhere.

Examples:

* Block completion when discovery artifacts are malformed.
* Prevent research agents from editing production code.
* Require evidence before a behavior is marked verified.
* Require unresolved behavior to appear in the decision log.
* Require parity-matrix coverage before promotion to implementation planning.

This is consistent with the current use of hooks as reusable completion and policy gates.

## 6. Generic CLI and MCP tools

Put reusable commands in `drm-copilot`, for example:

```text
dev.discovery.init
dev.discovery.validate
dev.discovery.coverage-report
dev.discovery.parity-report
dev.discovery.create-contract
dev.discovery.create-scenario
dev.discovery.link-evidence
dev.discovery.generate-acceptance-tests
```

The extension or MCP server should expose them consistently to Claude, Codex, and Copilot.

The repository already uses the extension and MCP bridge to expose shared workspace-facing automation across agent ecosystems.

## 7. Generic templates

Keep centrally maintained templates for:

* Feature contracts
* Runtime scenarios
* Coverage ledgers
* Parity records
* Product decisions
* Discovery summaries
* Acceptance scenarios

Templates can contain placeholders but no TaskMaster-specific examples in the canonical implementation.

## 8. Generic language and platform analyzers

Some analyzers are reusable enough to live in the extension:

* .NET solution inventory
* C# symbol and dependency inventory
* Event subscription extraction
* Configuration/settings extraction
* File-system and registry access detection
* COM interop usage extraction
* UI callback extraction
* Test-to-production-code mapping

A generic VSTO analyzer can also belong centrally because it may apply to other Office migrations.

That analyzer may understand:

* Ribbon XML
* Ribbon callback attributes
* Office interop references
* COM event subscriptions
* `ThisAddIn`
* Outlook item types
* MAPI property access

It should not understand TaskMaster feature semantics.

---

# What belongs in TaskMaster

TaskMaster is the authoritative source for the legacy system’s actual behavior.

## 1. The domain profile

TaskMaster should contain a repository-specific configuration file such as:

```text
docs/migration/discovery/domain-profile.yaml
```

It should define:

```yaml
system:
  name: TaskMaster
  role: legacy-source
  platform: outlook-vsto
  language: csharp
  framework: net-framework

inventory:
  solution_files:
    - TaskMaster.sln
  production_roots:
    - TaskMaster
    - GsyncCoding
  test_roots:
    - TaskMaster.Tests

required_modes:
  - outlook-online
  - outlook-cached-offline
  - reconnect
  - multi-store
  - shared-mailbox

domain_extensions:
  outlook_item_types:
    - mail
    - appointment
    - task
    - meeting
```

This file tells the generic tooling how to operate locally.

## 2. Legacy feature contracts

All discovered contracts belong in TaskMaster because TaskMaster is the evidence source:

```text
docs/migration/discovery/feature-contracts/
  TM-FILING-001.yaml
  TM-FILING-002.yaml
  TM-TRIAGE-001.yaml
  TM-TAGS-001.yaml
```

These should remain versioned alongside the legacy code so that any change in understanding can be reviewed against the source.

## 3. Legacy behavior evidence

Store all TaskMaster-specific evidence locally:

```text
docs/migration/discovery/evidence/
  static-analysis/
  runtime/
  screenshots/
  mailbox-snapshots/
  filesystem-snapshots/
  logs/
  interviews/
```

The existing framework already emphasizes repository-local, canonical evidence locations rather than global artifact directories.

## 4. Coverage ledger

TaskMaster should own the ledger proving that its legacy system was inspected:

```text
docs/migration/discovery/coverage-ledger.json
```

It should identify:

* Every project
* Every relevant class
* Every ribbon control
* Every event handler
* Every persistence mechanism
* Every public workflow
* Every test assembly
* Every external dependency

TMW should not own this ledger because it cannot prove the legacy application was inspected.

## 5. Characterization scenarios

Runtime scenarios for classic Outlook belong in TaskMaster:

```text
docs/migration/discovery/characterization/
  cached-mode/
  message-move/
  conversation-move/
  undo/
  classification/
  tags/
```

Any TaskMaster-specific recorder or fixture setup also belongs here unless it becomes reusable across multiple VSTO repositories.

## 6. The legacy test harness

Keep Outlook-hosted characterization tools with TaskMaster:

```text
tools/characterization/
  MailboxSnapshot/
  OutlookStateRecorder/
  ScenarioRunner/
  FixtureBuilder/
```

The extension may provide the orchestration command, but the code that knows how to instantiate TaskMaster, locate its settings, inspect its specific files, or interpret its domain state should remain local.

## 7. The unspecified-behavior log

TaskMaster should own the initial log because the ambiguity originates in legacy behavior:

```text
docs/migration/discovery/unspecified-behaviors.yaml
```

Examples:

* What happens when one message in a conversation cannot be moved?
* Does Undo Sort restore order, folder, category, and flag state?
* Is classifier confidence persisted?
* What happens when a destination folder disappears while offline?

## 8. Legacy architecture documentation

TaskMaster should contain:

* Current component diagram
* Current data-flow diagram
* Current state-machine descriptions
* COM/Outlook dependency map
* Settings and persistence map
* Legacy operational constraints

These are not reusable extension content.

---

# What belongs in TMW

TMW should describe the target implementation and its current parity status.

## 1. Target feature implementations

TMW should own the modern equivalents:

```text
docs/migration/discovery/feature-implementations/
  TM-FILING-001.yaml
  TM-TAGS-001.yaml
```

These records should reference the legacy contract ID and identify:

* Implemented behavior
* Deliberate deviations
* Target APIs
* Offline behavior
* Mobile behavior
* Remaining gaps
* Relevant tests

## 2. Target architecture decisions

Keep TMW-specific ADRs in TMW:

```text
docs/architecture/decisions/
  0001-local-first-storage.md
  0002-graph-sync-boundary.md
  0003-outlook-add-in-vs-pwa.md
  0004-conflict-resolution.md
```

The extension should provide an ADR template or validation skill, but not the decisions themselves.

## 3. TMW-local parity coverage

TMW should maintain implementation coverage against the TaskMaster contracts:

```text
docs/migration/parity/
  implementation-status.yaml
```

This is where TMW records:

* `not-started`
* `partial`
* `implemented`
* `verified-online`
* `verified-offline`
* `verified-mobile`
* `intentionally-changed`
* `retired`

## 4. Modern acceptance and contract tests

Executable target tests belong in TMW:

* Domain tests
* API tests
* Synchronization tests
* Offline queue tests
* Conflict-resolution tests
* PWA/mobile tests
* Outlook add-in tests

The Gherkin or behavioral scenario may originate in TaskMaster’s contract, but the executable adapters belong with the implementation.

## 5. TMW-specific analyzers and test fixtures

Examples:

```text
tools/testing/
  GraphEmulator/
  OfflineSyncHarness/
  ConflictScenarioBuilder/
  MobileViewportRunner/
```

These are implementation-specific and should not be pushed to every repository.

---

# Where the parity matrix should live

This is the one artifact that spans both repositories.

I recommend that the **authoritative parity matrix live in TMW**, with immutable references to TaskMaster contract IDs and commit SHAs.

Reason:

* TaskMaster defines what exists.
* TMW defines whether and how it has been replaced.
* The matrix changes primarily as TMW implementation progresses.
* Retiring TaskMaster later should not remove the migration history from the surviving repository.

Structure:

```yaml
legacy_repository:
  name: drmoisan/TaskMaster
  commit: abc123

target_repository:
  name: drmoisan/TMW
  commit: def456

entries:
  - feature_id: TM-FILING-001
    legacy_contract:
      repository: drmoisan/TaskMaster
      path: docs/migration/discovery/feature-contracts/TM-FILING-001.yaml
      commit: abc123
    target_status: partial
    online: verified
    offline: missing
    mobile: missing
```

TaskMaster can contain a generated snapshot or link, but TMW should be authoritative.

---

# Practical decision rule

Use this test for every proposed asset:

### Put it in `drm-copilot` when:

> Could this exact asset be pushed unchanged into an unrelated legacy application repository?

Examples:

* Agent role
* Workflow
* Schema
* Validator
* Hook
* Generic analyzer
* Generic CLI
* Template

### Put it in TaskMaster when:

> Does this asset describe, inspect, execute, or prove something about the legacy TaskMaster implementation?

Examples:

* Legacy contract
* Outlook scenario
* Runtime evidence
* Legacy fixture
* Coverage ledger
* Unspecified behavior

### Put it in TMW when:

> Does this asset describe or verify the modern implementation or migration status?

Examples:

* Target ADR
* Sync architecture
* Mobile behavior
* Offline queue tests
* Implementation status
* Cross-repository parity matrix

---

# Recommended repository layout

## `drm-copilot`

```text
.github/instructions/
  legacy-discovery.instructions.md

.claude/agents/
  legacy-parity-analyst.md
  runtime-characterization-analyst.md
  migration-coverage-reviewer.md

.claude/skills/
  inventory-legacy-system/
  extract-feature-contract/
  characterize-runtime-behavior/
  reconcile-parity-matrix/
  review-discovery-coverage/
  generate-acceptance-scenarios/

schemas/discovery/
  feature-contract.schema.json
  coverage-ledger.schema.json
  parity-matrix.schema.json
  runtime-scenario.schema.json
  product-decision.schema.json

scripts/dev_tools/discovery/
  init.py
  validate.py
  coverage.py
  parity.py
  evidence.py

scripts/dev_tools/analyzers/
  dotnet_solution.py
  csharp_events.py
  com_interop.py
  vsto_ribbons.py
```

## TaskMaster

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

tools/characterization/
  OutlookStateRecorder/
  MailboxSnapshot/
  TaskMasterFixtureBuilder/
```

## TMW

```text
docs/migration/
  source-contract-baseline.yaml
  parity-matrix.yaml
  implementation-status.yaml

docs/architecture/decisions/
docs/mobile/
docs/offline/

tests/
  parity/
  sync/
  offline/
  mobile/

tools/testing/
  GraphEmulator/
  OfflineSyncHarness/
```

# One important caution

Do not place the TaskMaster feature catalog or TaskMaster-specific prompts into the `drm-copilot` extension merely because multiple agents need them.

That would create two problems:

1. The extension would become coupled to one migration.
2. Pushing updates into unrelated repositories would carry irrelevant domain instructions and increase context noise.

The extension should distribute **capability**. The repositories should supply **context and truth**.

The best operating model is therefore:

```text
drm-copilot
    provides workflow + schemas + enforcement + generic analyzers
        ↓
TaskMaster
    provides legacy behavior + evidence + characterization
        ↓
TMW
    provides target architecture + implementation + parity verification
```

That separation will also make the discovery tooling reusable for your other modernization projects without forcing those projects to inherit TaskMaster-specific logic.
