# Upstream Claude Coverage-Policy Reconciliation Prompt

Timestamp: 2026-08-11T12-41

Command: Not applicable; this artifact records a user-directed scope change for upstream execution.

EXIT_CODE: 0

## Usage boundary

Use this prompt in the upstream customization source repository that owns the generated
`CLAUDE.md` and `.claude/**` assets consumed by TaskMaster.

Do not apply the requested `CLAUDE.md` or `.claude/**` changes directly in the TaskMaster
repository. TaskMaster permits repository-specific memory updates under
`.claude/agent-memory/**`; that exception does not authorize local edits to rules, hooks,
skills, agents, settings, or other Claude customization files.

## Objective

Resolve the coverage-policy conflict documented by TaskMaster issue #494 at the upstream
source of truth. Ensure the generated Claude policy and feature-review enforcement surfaces
state and apply one internally consistent coverage contract.

Use these TaskMaster inputs as requirements evidence:

- `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`
- `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`
- The revised issue #494 atomic plan after its user-directed scope correction

The existing plan has unresolved preflight findings. Do not treat its current replacement
text as approved merely because it is present in the plan.

## Required upstream work

1. Locate the canonical upstream sources that generate or distribute the affected
   `CLAUDE.md` coverage section and `.claude/**` customizations.
2. Reconcile the conflicting repository-wide, new-code, branch-coverage, denominator, and
   exclusion rules described by issue #494. Record one authoritative policy source and make
   the remaining generated surfaces refer to it without conflicting restatements.
3. Remove or correct claims that require absent `quality-tiers.yml`, `tier-classification`, or
   `docs/ci.research.md` artifacts, according to the approved issue #494 disposition.
4. Update the upstream source for the Claude feature-review coverage hook so its documented
   behavior, artifact format, missing-input behavior, line-coverage decision, and
   branch-coverage disposition agree with the approved policy.
5. Add or update deterministic upstream tests for every changed rule generator, hook, or
   customization template. Include negative-path coverage proving that a below-threshold
   input produces the required failure signal.
6. Regenerate or package the upstream customization output and report the release or
   publication mechanism required for downstream adoption. Do not push generated Claude
   files into TaskMaster as part of issue #494.

## Acceptance criteria

- The upstream source contains no contradictory coverage thresholds or exclusion rules across
  the affected generated surfaces.
- The upstream hook behavior matches the approved policy and fails closed when required input
  is absent or invalid.
- Upstream tests cover allowed, denied, missing-input, malformed-input, and boundary cases.
- The change identifies every generated TaskMaster path that would be affected by a future
  supported publication, without directly editing those paths in TaskMaster.
- The final upstream response includes changed source paths, exact validation commands,
  results, release or publication instructions, and any remaining downstream action.

## Non-goals and constraints

- Do not directly edit TaskMaster `CLAUDE.md`.
- Do not directly edit TaskMaster `.claude/**`, except repository-specific memory files when a
  separate task explicitly requires them.
- Do not change the C# toolchain command contract owned by issue #512.
- Do not silently choose or lower a coverage threshold to match a measured result. Preserve
  the issue #494 decision and evidence process.
- Do not modify unrelated Claude customizations.
