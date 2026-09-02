---
name: research-issue
description: Investigate the best implementation approach for a feature or bug by analyzing the codebase and external references, then writing structured findings to the resolved research path under docs/features/<feature>/research/ (feature-associated) or docs/research/ (one-off).
allowed-tools:
  - Read
  - Grep
  - Glob
  - WebFetch
---

# Research Issue Skill

Research the best implementation approach for a feature or bug described in the provided issue and spec documents. Output is a structured research file, not implementation code.

## Inputs

Accept one or more feature documents as context:

- `docs/features/active/<feature>/issue.md` (primary)
- `docs/features/active/<feature>/user-story.md` (when present)
- `docs/features/active/<feature>/spec.md` (when present)

## Output

Create or update a single research file at one of the two tracked research roots:

- Feature-associated research: `docs/features/<feature>/research/<timestamp>-<short-name>-research.md` (for example `docs/features/active/<feature>/research/<timestamp>-<short-name>-research.md`).
- One-off research not tied to a feature: `docs/research/<timestamp>-<short-name>-research.md`.
- Routing rule: write to the feature research root when an active feature folder is in scope (the orchestrator supplies the resolved path from `feature-folder` in `orchestrator-state.json`); otherwise write to `docs/research/`. The filename convention `<timestamp>-<short-name>-research.md` is unchanged.
- Use the Task Researcher template from the repository exactly.
- Place rejected-alternatives summaries inside `## Recommended Approach`, not as a separate top-level header.

## Investigation Areas

### 1. Current State Analysis

- Identify implementation targets from the feature docs.
- Read relevant modules end-to-end.
- Document current behavior, key abstractions, extension points, and toolchain constraints.

### 2. Candidate Implementation Approaches

- Research and compare at least two viable approaches.
- Select one final recommendation with justification.
- For any new dependency, confirm whether it already exists in the repo and justify adding it.
- Gather authoritative external documentation to support the recommendation.

### 3. Behavior Semantics and Edge Cases

- Extract intended behavior from feature docs.
- Define success/failure conditions, ordering rules, cancellation behavior, and CI vs. local expectations.
- Research comparable tools that implement similar semantics.

### 4. Requirements Mapping to Design

- Map acceptance criteria into a concrete design.
- Propose state model, transitions, internal API boundaries, and required file changes.
- For every numeric count, enumeration, or population proposed for an approved `spec.md` acceptance criterion, add complete `## Numeric Derivation Evidence`. Each record must identify `Complete Family`, `Exhaustive Search Scope`, `Inclusion Rules`, `Exclusion Rules`, `Primary Search Strategy or Query Expression`, `Primary Member Set`, `Primary Count`, `Cross-check Search Strategy or Query Expression`, `Cross-check Member Set`, `Cross-check Count`, and `Member-set Comparison`.
- Both derivations must be non-empty and independently constructed. They must use distinct search strategies or query expressions, independently enumerate the member sets, and explicitly compare normalized member sets. The scope must cover the entire declared family, including all relevant overloads and members; reject a single grep, a narrow named-pattern search, or a query that covers only one family member even when totals and member sets appear equal. Withhold the numeric assertion if the records are incomplete, duplicated, non-exhaustive, narrow, or disagree.

### 5. Testing Implications

- Propose a test strategy consistent with repository policy (unit tests for pure logic, integration seams).
- Do not write test code; describe the strategy only.

## Constraints

- Research only. Do not implement changes.
- Ground all findings in verified evidence from the codebase and authoritative external sources.
- Keep discussion of non-selected approaches brief.
- Do not claim or perform nested worker delegation.
- Omit numeric acceptance-criterion facts when the numeric derivation record is absent, incomplete, or disagrees.
