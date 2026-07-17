## Conclusion

**The drm-copilot framework is sufficient as the orchestration and governance foundation, but it is not sufficient by itself to complete the TaskMaster discovery and parity-definition phase reliably.**

You do **not** need to build another general-purpose agentic framework. The repository already provides:

* Orchestrator-driven delegation
* Persistent checkpoints and resumability
* Research-only agents
* Structured feature folders
* Acceptance-criteria tracking
* Evidence-location enforcement
* Claude/Codex-compatible customizations
* Quality gates and completion validators

The missing layer is **domain-specific discovery tooling for reverse-engineering a large, behavior-rich VSTO application**. I would add a small set of TaskMaster-specific skills, schemas, validators, and executable inspection utilities rather than new general infrastructure.

## What drm-copilot already covers well

### 1. Work orchestration and state management

The orchestrator already distinguishes small and large changes, delegates investigation to `task-researcher`, persists progress after each step, and resumes from a machine-readable checkpoint. That is appropriate for a multi-week discovery program in which work will be broken into bounded investigations.

For your staffing model, this is particularly valuable. You can serve as product owner and adjudicator while Claude or Codex performs the systematic repository analysis.

### 2. Research discipline

The `task-researcher` is explicitly restricted to research, requires verified findings, calls for end-to-end reading of relevant modules, extracts behavior semantics and edge cases, maps requirements to design, and proposes testing implications.

The `research-issue` skill adds several useful requirements:

* Read relevant modules end-to-end
* Compare at least two approaches
* Gather authoritative external documentation
* Define success, failure, ordering, cancellation, and local-versus-CI semantics
* Map acceptance criteria into state models and internal API boundaries

That is a strong basis for architecture research and implementation planning.

### 3. Policy and tool isolation

The four-layer model—standing instructions, skills, specialist subagents, and hooks—is appropriate for preventing an agent from mixing discovery with premature implementation.

The research agent is allowed to write only into designated research locations, and a stop hook validates its output.  This reduces the risk that an exploratory agent silently changes legacy code while documenting it.

### 4. Acceptance-criteria governance

The acceptance-criteria skill has sound delivery controls: evidence before check-off, individual verification, preserved criterion text, explicit unmet criteria, and completion summaries.

This will be useful after parity requirements have been authored.

### 5. Claude and Codex interoperability

The repository deliberately maintains parallel Claude, Codex, and GitHub Copilot surfaces from a canonical policy set, and includes conversion and publishing tooling.

You therefore should not build separate TaskMaster discovery systems for Claude and Codex. Add the new discovery capabilities to the canonical source and publish them through the existing mechanism.

---

# Where the current framework is insufficient

The current research workflow is optimized for answering:

> “What is the best way to implement this defined feature?”

The TaskMaster discovery phase asks a different question:

> “What does this system actually do, including undocumented, stateful, host-dependent, and accidental behavior?”

Those are not equivalent.

## 1. No canonical feature-contract schema

The current research agent can write narrative findings, but the discovery milestone requires a **complete, mergeable, machine-checkable feature inventory**.

You need a structured record for every behavior, such as:

```yaml
feature_id: TM-FILING-014
name: Move entire conversation
surface:
  - outlook_explorer_ribbon
  - outlook_inspector_ribbon
trigger:
  control: quick_filer
  option: move_entire_conversation
preconditions:
  - selected_item_is_mail
inputs:
  - destination_folder
  - conversation_identity
observable_behavior:
  - all eligible conversation messages are moved
state_changes:
  mailbox:
    - message_parent_folder
  filesystem: []
  local_settings: []
offline_behavior: unknown
mobile_requirement: required
error_behavior:
  partial_failure: unknown
source_evidence:
  - path: TaskMaster/...
    symbol: ...
    lines: ...
confidence: medium
status: unspecified
```

Without such a schema, multiple research agents will produce inconsistent prose that is difficult to reconcile into a parity contract.

## 2. No coverage ledger

There is no visible mechanism that proves the agent inspected all relevant:

* Ribbon controls
* Event handlers
* Outlook item types
* Project assemblies
* Public controllers
* Settings
* Model persistence paths
* File-system writes
* Registry interactions
* Outlook custom properties
* Categories and flags
* Timers and background jobs
* COM event subscriptions
* Error dialogs
* Undo operations
* Import/export or training functions

“Read relevant modules end-to-end” is good guidance, but it does not prove completeness.

You need a coverage manifest that starts from the repository structure and marks each discovered surface as:

* Inspected
* Out of scope
* Duplicate/delegated
* Blocked
* Behavior extracted
* Tests found
* Runtime validation required

## 3. No static behavior-extraction utilities

TaskMaster is large enough that manual agent reading alone will be inefficient and error-prone. The current tooling provides general context collection and development utilities, but the repository description does not show a dedicated VSTO reverse-engineering pipeline.

At minimum, automated tools should extract:

* Ribbon XML controls and callback names
* Callback-to-method mappings
* Project and assembly dependencies
* Outlook event subscriptions
* COM interface use
* Settings property definitions and defaults
* Files read and written
* Serialization formats
* Registry access
* Message property access, including MAPI property tags
* UI forms and launch points
* Logging and exception paths
* ML model creation, training, saving, and loading
* Public interfaces and concrete implementations
* Tests referencing each component

The output should be JSON or SQLite, not only Markdown.

## 4. No black-box characterization harness

Source-code inspection cannot fully define legacy behavior. TaskMaster contains Outlook-host interactions, cached-mode behavior, timing, COM object lifecycle, and user-interface semantics that may only be observable at runtime.

You need a characterization harness capable of recording:

* Initial mailbox and local-state fixture
* User action
* Outlook version and connectivity state
* Observable result
* Mailbox delta
* File-system delta
* Settings delta
* Logs and exceptions
* Screenshots where relevant
* Timing
* Repeatability
* Whether the behavior appears intentional or accidental

This is especially important for:

* Offline cached Exchange mode
* Message moves while disconnected
* Undo Sort
* Moving entire conversations
* Saving attachments and message copies
* Category and flag updates
* Multiple stores
* Shared mailboxes
* Search folders
* IMAP accounts
* Message-ID changes after moves
* Failure during partial multi-step operations
* Reconnection behavior

No agent prompt can substitute for this runtime evidence.

## 5. Acceptance criteria are delivery-oriented, not discovery-oriented

The existing acceptance-criteria protocol prohibits executors and reviewers from adding new criteria; those are expected to be authored earlier by planning or scoping agents.

During discovery, however, criteria are themselves the product. The agent needs a controlled mechanism to:

* Propose newly discovered criteria
* Link each criterion to evidence
* Mark it as confirmed, inferred, disputed, or unspecified
* Record your product decision
* Trace it into the future target architecture

That requires a separate **parity criteria lifecycle**, not reuse of the implementation check-off lifecycle unchanged.

## 6. No explicit unspecified-behavior workflow

The prior migration plan correctly called for an unspecified-behavior log. The current research principles prohibit recording assumptions, which is sound, but they do not define how unresolved behavior becomes a product decision.

You need statuses such as:

* `verified-current-behavior`
* `verified-defect`
* `inferred-behavior`
* `unobservable`
* `contradictory`
* `product-decision-required`
* `not-required-in-target`
* `required-parity`
* `intentional-change`

Every unresolved item should have an owner—probably you—and a disposition deadline.

## 7. No UX-specific discovery agent or template

The current roster includes research, planning, implementation, and review specialists, but no specialist focused on interaction inventory, workflow timing, and cross-form-factor reinterpretation.

TaskMaster’s migration is not simply API parity. Ribbon-based workflows must be translated into:

* Outlook web add-in commands
* Task panes
* Mobile add-in interactions
* Companion PWA workflows
* Offline queue states

You need a UX artifact that records:

* Current entry point
* Current number of clicks/keystrokes
* Keyboard behavior
* Required message context
* User decision points
* Feedback and error state
* Mobile relevance
* Offline relevance
* Proposed modern surface
* Any accepted interaction change

## 8. No cross-repository parity mapper

Discovery must compare TaskMaster and TMW continuously. The existing agent can inspect code, but you need a first-class mapping:

| Legacy capability | TaskMaster evidence | TMW evidence | Status  | Gap type    | Required decision      |
| ----------------- | ------------------- | ------------ | ------- | ----------- | ---------------------- |
| Folder search     | …                   | …            | Partial | Semantic    | Ranking parity?        |
| Move message      | …                   | …            | Partial | Reliability | Offline queue absent   |
| Save attachments  | …                   | …            | Partial | Scope       | Inline images excluded |
| Spam training     | …                   | None         | Missing | Feature     | Rebuild/retire         |
| Task tree         | …                   | None         | Missing | UX/domain   | PWA design             |

This should be generated from structured records rather than maintained manually in narrative form.

---

# Recommended additions

## A. Add one new discovery agent

Create:

```text
.claude/agents/legacy-parity-analyst.md
```

Its responsibility should be narrower than `task-researcher`:

* Extract legacy behavior without proposing implementation
* Maintain the feature catalog and coverage ledger
* Trace every assertion to code, test, documentation, or runtime evidence
* Identify contradictions and unspecified behavior
* Never decide whether behavior should be preserved
* Never produce target architecture recommendations

I would **not** add separate agents for every feature area initially. One parity analyst can be delegated bounded slices such as QuickFiler, tags, triage, task visualization, and operational behavior.

Use the existing `task-researcher` for external platform research and target-architecture questions.

## B. Add four discovery-specific skills

### 1. `inventory-legacy-surfaces`

Produces:

* Repository/project inventory
* Ribbon and UI control inventory
* Event-handler inventory
* Settings and persistence inventory
* External dependency inventory
* Coverage ledger

### 2. `extract-feature-contract`

Takes one bounded capability and creates a structured feature contract containing:

* Trigger
* Preconditions
* Inputs
* State transitions
* Outputs
* Errors
* Side effects
* Ordering
* Offline behavior
* Mobile relevance
* Evidence
* Confidence
* Open questions

### 3. `characterize-legacy-behavior`

Guides an agent and you through runtime experiments against classic Outlook.

It should generate a reproducible scenario file and evidence package for each test.

### 4. `reconcile-parity-matrix`

Joins:

* TaskMaster contracts
* TMW contracts
* Runtime characterization
* Product decisions

It should calculate statuses such as `parity`, `partial`, `missing`, `changed-intentionally`, and `retired`.

## C. Add machine-readable schemas

I recommend these files:

```text
docs/migration/discovery/
  scope.yaml
  component-inventory.json
  coverage-ledger.json
  feature-contracts/
    TM-FILING-001.yaml
    TM-TRIAGE-001.yaml
  parity-matrix.yaml
  unspecified-behaviors.yaml
  product-decisions.yaml
  acceptance-tests/
    TM-FILING-001.feature
```

JSON Schema files should live under something like:

```text
schemas/discovery/
  feature-contract.schema.json
  coverage-ledger.schema.json
  parity-matrix.schema.json
  unspecified-behavior.schema.json
  characterization-scenario.schema.json
```

## D. Add validators and hooks

The existing framework relies heavily on fail-closed hooks and subagent-output validation, so the new discovery workflow should follow that model. The repository already uses hooks to validate completion gates and enforce restricted workflows.

Add:

* `validate-feature-contract.ps1`
* `validate-coverage-ledger.ps1`
* `validate-parity-matrix.ps1`
* `validate-unspecified-behaviors.ps1`
* `validate-discovery-completion.ps1`

The completion validator should fail unless:

1. Every in-scope project/component has a coverage disposition.
2. Every discovered user-facing control maps to at least one feature contract.
3. Every feature contract has evidence.
4. Every `unknown` or contradictory behavior appears in the unspecified-behavior log.
5. Every required-parity behavior has at least one acceptance test.
6. Every TaskMaster capability has a TMW mapping status.
7. Offline and mobile applicability are explicitly classified—not left blank.

## E. Add static extraction scripts

A small Python/.NET toolset should generate initial inventories automatically.

Recommended commands:

```text
dev.extract-solution-inventory
dev.extract-ribbon-callbacks
dev.extract-outlook-event-bindings
dev.extract-settings-and-persistence
dev.extract-com-and-mapi-usage
dev.extract-ui-entrypoints
dev.build-legacy-callgraph
dev.validate-discovery-artifacts
```

These utilities do not need sophisticated semantic understanding. Their value is ensuring that the agents do not overlook large portions of the codebase.

## F. Add runtime characterization support

For TaskMaster specifically, I would build a lightweight Windows test recorder rather than a full automated Outlook UI test suite at the outset.

It should capture before-and-after snapshots of:

* Folder IDs and paths
* Message identifiers
* Subject, categories, flags, and parent folder
* Conversation members
* Relevant custom/MAPI properties
* Local application settings
* Created or modified files
* Logs
* Network status
* Outlook cached-mode status

A scenario definition could look like:

```yaml
scenario_id: CHAR-FILING-007
feature_id: TM-FILING-014
environment:
  outlook: classic
  account_type: exchange
  cached_mode: true
  network: disconnected
fixture:
  selected_messages: 1
  conversation_messages: 4
action:
  command: quick_file
  option_move_entire_conversation: true
  destination: Projects/Test
expected: unspecified
observations: []
```

Initially, you can perform the UI action manually while the harness records state changes. That will provide much higher confidence than asking an agent to infer behavior from COM code alone.

---

# Recommended discovery workflow for you plus agents

## Phase 1: Establish scope and inventory

The orchestrator should delegate repository inventory for both TaskMaster and TMW.

Outputs:

* Complete solution/component inventory
* UI and command inventory
* Settings/persistence inventory
* Dependency inventory
* Coverage ledger initialized to zero

**Gate:** No feature analysis begins until the inventory validator confirms all projects and primary UI entry points are represented.

## Phase 2: Extract static feature contracts

Work capability-by-capability:

1. Filing and folder search
2. Attachment/message export
3. Undo and conversation behavior
4. Folder classifier
5. SpamBayes
6. Triage
7. Tags
8. Task tree and visualization
9. Settings and store management
10. Diagnostics and resilience

Each agent invocation should cover a bounded subsystem and update structured contracts rather than create independent narrative reports.

**Gate:** Every contract must contain evidence, confidence, and unresolved questions.

## Phase 3: Build the TaskMaster-to-TMW parity matrix

Map each contract to:

* Existing TMW implementation
* Partial implementation
* Missing implementation
* Incompatible architecture
* Candidate retirement

This distinguishes true migration work from functionality already prototyped in TMW.

## Phase 4: Runtime characterization

Prioritize behavior that cannot be trusted from static inspection:

* Offline cached mode
* Failure and retry
* Moves and undo
* Multi-store behavior
* Conversation semantics
* Persistence and model loading
* Outlook lifecycle events
* Timing-sensitive operations

You conduct the user action; the agent prepares fixtures, reads evidence, and updates the contracts.

## Phase 5: Product adjudication

Your primary role is resolving the unspecified-behavior queue.

For each item, decide:

* Preserve exactly
* Preserve user outcome but change mechanics
* Correct a legacy defect
* Remove
* Defer
* Require further experiment

No agent should silently make these product decisions.

## Phase 6: Generate executable acceptance tests

Convert confirmed feature contracts into:

* Gherkin-style behavioral scenarios
* API contract tests where relevant
* Domain-level characterization tests
* Manual host-validation scenarios for Outlook-specific behavior
* Offline/mobile scenario matrices

At this point, the existing acceptance-criteria tracking framework becomes directly useful.

---

# Staffing implications

With only you plus agentic tooling, the original **four-week estimate is plausible only if “discovery” means high-value capability discovery**, not exhaustive behavioral characterization of every TaskMaster path.

A defensible four-week outcome would be:

### Week 1

* Install drm-copilot customizations into TaskMaster and TMW
* Add discovery schemas, agent, skills, and validators
* Generate repository and UI inventories
* Define scope boundaries

### Week 2

* Extract contracts for filing, folder search, exports, undo, and settings
* Build the first parity-matrix sections
* Begin runtime characterization for offline filing

### Week 3

* Extract triage, spam, classifiers, tags, and task behavior
* Run prioritized runtime scenarios
* Populate unspecified-behavior decisions

### Week 4

* Complete parity matrix
* Produce acceptance-test catalog
* Validate coverage
* Freeze the initial feature contract
* Identify deferred low-value or uncharacterized behavior

A fully exhaustive catalog may require longer because the bottleneck will not be agent coding speed. It will be **runtime observation and your product adjudication**.

# Final recommendation

Use drm-copilot as the foundation. Do not replace or substantially redesign its orchestration model.

Before beginning TaskMaster discovery, add:

1. One `legacy-parity-analyst` agent.
2. Four discovery-specific skills.
3. Five structured artifact schemas.
4. Static VSTO inventory extractors.
5. Discovery completion validators and hooks.
6. A lightweight Outlook characterization recorder.
7. A formal product-decision queue for unspecified behaviors.

The essential distinction is:

* **drm-copilot already controls how the agents work.**
* **You still need to define what constitutes complete legacy-system discovery.**

Without the additional artifacts and validation gates, the agents are likely to produce strong research documents but not a demonstrably complete parity contract.
