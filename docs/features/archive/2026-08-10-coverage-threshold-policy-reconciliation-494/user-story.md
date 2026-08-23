# `2026-08-10-coverage-threshold-policy-reconciliation-494` — User Story

- Issue: #494
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/494
- Epic: `build-ci-coverage-gate-fidelity` (wave 2)
- Owner: drmoisan
- Work Mode: `full-bug`
- Status: Specified
- Last Updated: 2026-08-10T16-10

> **Why this document exists.** The work mode for issue #494 is `full-bug`, for which a
> `user-story.md` is normally absent; this document exists because the epic preparation route
> requires it as a deliverable. **For this remediation, only the `## Acceptance Criteria` section
> in `spec.md` is the sole acceptance-criteria source.** This document is narrative context only
> and must not be used for acceptance-criteria check-off. The "Outcomes" section below is
> narrative context, not a check-off list.

## Operative Remediation Scope Correction

The existing upstream prompt at
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the local
TaskMaster deliverable. No upstream receipt, release, publication, validation, or external
execution is required. `CLAUDE.md`, all non-memory `.claude/**` paths (including rules, hooks,
skills, agents, settings, and generated runtime assets), `.agents/skills/**`, and external
repositories are prohibited. Active scope remains limited to the existing TaskMaster coverage
runner and Pester work already present in the repository; this remediation does not reopen,
implement, test, re-evaluate, or plan that work.

The following pre-existing `.claude/agent-memory/**` records are immutable and are permitted
solely for protected-path classification:

- `.claude/agent-memory/atomic-executor/MEMORY.md`
- `.claude/agent-memory/atomic-executor/project_511_is_a_testhost_crash_not_n_failing_tests.md`
- `.claude/agent-memory/atomic-executor/project_pester5_result_shape_container_tests_and_ci_codecoverage.md`
- `.claude/agent-memory/atomic-planner/MEMORY.md`
- `.claude/agent-memory/atomic-planner/poshqc-mcp-and-msbuild-invocation-facts.md`
- `.claude/agent-memory/atomic-planner/project_494_threshold_reconciliation_plan_seams.md`

Do not edit, create, delete, rename, stage, or otherwise modify these records, including their
content or history.

## Story Statement

- **As an agent executing any code change in this repository**, I want exactly one document to
  state the coverage thresholds, the coverage denominator, and the exclusion policy, so that I can
  apply a coverage gate by rule instead of improvising a resolution between two always-loaded
  policy documents that contradict each other — or halting, as `CLAUDE.md` currently instructs me
  to do on every change that touches coverage.

- **As a feature-review agent**, I want the gate I enforce to consume a coverage artifact with a
  committed producer, to fail closed when that artifact is missing, and to state the same numbers
  in its documentation and in its code, so that a PASS verdict I issue means the coverage was
  measured and met the floor rather than that the input was absent.

- **As the repository maintainer**, I want the coverage policy I recorded to be the coverage policy
  the repository states, so that a governance-bundle sync cannot silently replace a decision I made
  with a number imported from a different codebase, and so that a future divergence is resolved by
  a written rule rather than by a precedent carried in agent memory.

- **As a contributor or agent reading policy through a different toolchain surface** — Codex via
  `.agents/`, Copilot via `.github/instructions/` — I want every surface to point at one authority
  rather than restate its own number, so that the verdict on my change does not depend on which
  assistant reviewed it.

## Problem / Why

`CLAUDE.md` § UT2 states a >= 80% repository-wide line floor with a >= 90% bar for new units,
applied to a testable denominator that excludes COM/VSTO/WinForms code.
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a >= 85% line and
>= 75% branch floor and forbid excluding any production file from the denominator. Both surfaces
are always loaded. Neither defers to the other, and the exclusion clauses are contradictory
definitions of the same quantity rather than two wordings of one rule.

`CLAUDE.md` instructs agents to halt and notify the user on any conflicting instruction. A conflict
embedded in the policy documents themselves therefore makes nearly every code change unresolvable
as written. In practice agents have not halted; they have improvised. Issue #424 invented a
disposition (change-scoped gates blocking, repository-wide figures reported non-blocking), issue
#230 applied it by analogy, and the rule now lives in prior-plan archaeology and committed agent
memory. That is invisible to reviewers, unenforced by tooling, and it drifts.

The enforcement side is weaker than the documentation side. The only numeric gate in the repository
is a review hook whose documentation says 80 and whose code says 85 and 75; it reads an artifact
format nothing in the repository emits, from a path nothing in the repository produces; and when the
artifact is absent it skips its numeric checks entirely and passes. A gate that cannot fail is not
a gate.

## Personas and Scenarios

### Persona — Agent executing a code change

- Reads `CLAUDE.md` and every `.claude/rules/` file whose path scope matches the files being
  changed. Both coverage positions are in context on every change.
- Cares about: knowing which number applies before writing the plan's coverage gate tasks; not
  being blocked by a contradiction it has no authority to resolve.
- Constraints: `CLAUDE.md`'s halt directive; no authority to change policy documents outside an
  explicit per-issue authorization.
- Current friction: must choose a camp, improvise a justification, and record it in a plan that the
  next agent will find only by archaeology.

### Persona — Feature-review agent

- Issues PASS/FAIL verdicts on coverage rows in the policy audit, and is itself policed by
  `.claude/hooks/validate-feature-review-coverage.ps1`.
- Cares about: a gate whose enforced numbers match its documented numbers; an artifact contract it
  can actually satisfy.
- Current friction: its own agent definition states 85/75 at one point and 90/80/80 fourteen lines
  later, in the same numbered procedure. Its policing hook enforces numbers that match neither its
  own documentation nor `CLAUDE.md`, and silently passes when the coverage artifact is withheld —
  a bypass that committed agent memory records as an accepted tactic.

### Persona — Repository maintainer (drmoisan)

- Set the coverage policy at the #178 governance sync under the directive "keep current policy,
  adapt mechanism": 80% line, 90% new-module, line-only with no branch gate, with the COM/VSTO
  exemption retained and the 85/75 tier model explicitly rejected as reference-repo leakage.
- Cares about: the recorded decision remaining the operative decision; the exemption boundary
  staying minimal and honest; not being asked to re-decide something already decided.
- Current friction: the rejected model is present in the tree today, and the document that carries
  the maintainer's decision was not touched by the change that reintroduced it.

### Persona — Contributor reading policy through Codex or Copilot

- Reads `.agents/skills/**` or `.github/instructions/**` rather than `.claude/**`.
- Cares about: the same change receiving the same verdict regardless of which assistant reviewed
  it.
- Current friction: three `.agents/` files state the opposite camp from their `.claude/`
  counterparts, so a Codex session and a Claude session reach different coverage verdicts on the
  same PowerShell change and on the same feature review today.

### Scenario — Planning a C# bug fix, after this feature lands — Historical, non-executable

An agent plans a change to `QuickFiler`. It reads `CLAUDE.md` § UT2 and finds one set of numbers,
one denominator rule, and an explicit statement that this section is authoritative over
`.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.github/instructions/`, `.agents/`, and
`AGENTS.md`. `.claude/rules/general-unit-test.md` cites that section and states no number of its
own. The agent writes change-scoped blocking gates — no regression on changed lines, >= 90% on new
units — and records the repository-wide figure as a reported, tracked value with the written reason
it is not yet blocking. No halt, no improvisation, no archaeology.

### Scenario — A coverage regression reaches the gate — Historical, non-executable

A change lowers coverage below the floor. The gate reads the coverage artifact produced by a
committed script, compares against the floor from the authority, and returns non-zero with a
message naming the figure and the floor. A second attempt omits the coverage artifact entirely; the
gate returns non-zero for a missing input rather than skipping the check. Both outcomes are proven
by unit tests before the gate is trusted, and the proof is captured as acceptance evidence.

### Scenario — A future divergence appears — Historical, non-executable

Someone syncs a governance bundle that reintroduces a different number into
`.claude/rules/general-unit-test.md`. An agent reading both surfaces applies the written
conflict-resolution rule: the authority governs, the other document is defective, and the
divergence is filed as an issue. The agent does not halt and does not invent a disposition. An
authority-consistency test in the Pester suite detects the reintroduced numeral independently.

## Outcomes This Feature Is Expected to Produce

Narrative only. For this remediation, only the `## Acceptance Criteria` section in `spec.md` is
the acceptance-criteria source; this document is not a check-off list.

- One set of coverage numbers exists in one document; every other document cites it and states no
  numeral of its own.
- The COM/VSTO/WinForms testable-denominator exemption and the "no production file may be excluded"
  clause are resolved into a single denominator rule, written once, expressed in terms of which
  production lines leave the denominator by any mechanism rather than in terms of configuration
  glob entries.
- A written conflict-resolution rule replaces the halt directive for this specific class of
  conflict, so a future divergence is resolvable by rule.
- The #424/#230 precedent becomes written policy, split by scope: change-scoped gates blocking now,
  the repository-wide floor reported and tracked with a stated reproducibility tolerance and a named
  condition under which it becomes blocking.
- A gate exists that consumes a committed artifact, fails on a below-threshold figure, and fails on
  a missing artifact — proven by both cases before it is trusted.
- `.claude/rules/quality-tiers.md` no longer asserts a mapping file, a CI stage, and a
  source-of-truth document that do not exist.
- Every remaining threshold-stating site in the repository has a recorded disposition, including the
  three sites the issue's own inventory missed.

## Non-Goals

- **Deciding whether the COM/VSTO/WinForms exemption should survive in the long term.** This
  feature retains an already-ratified exemption, which requires no new ratification. Narrowing or
  revoking it is a maintainer decision reserved by `CLAUDE.md`'s own text and is deferred.
- **Raising or lowering the bar.** Reconciling to the recorded numbers restores a standing decision
  and removes un-reconciled import leakage. No threshold is re-tuned to accommodate a measurement,
  and no policy is relaxed to make a gate pass.
- **Fixing coverage measurement non-determinism.** The repository-wide figure has a documented
  run-to-run spread that neither #441/#478 nor #457 addresses. This feature names the tolerance and
  the exit condition; it does not fix the collector.
- **Bringing coverage up to the floor.** Assemblies below the governing floor are enumerated with a
  remediation path; raising them is separate work.
- **Editing the C# toolchain command blocks, `.claude/rules/csharp.md`, or
  `.claude/skills/csharp-qa-gate/SKILL.md`.** These are owned by sibling feature
  `csharp-toolchain-gate-fidelity-512` and are out of bounds.
- **Aligning `AGENTS.md`, the `.agents/` bundle, `.github/instructions/`, the `*-qa-gate` skills,
  and `.claude/agents/feature-review.md` in this change.** They are enumerated with dispositions and
  deferred to a named follow-up issue filed before merge.
- **Authoring `quality-tiers.yml`, `docs/ci.research.md`, or a `tier-classification` CI stage.**
