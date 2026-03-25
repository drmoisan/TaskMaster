---
name: feature-review
description: Review the current feature branch relative to a base branch, using PR context artifacts as the source of truth, then generate policy-audit, code-review, and feature-audit artifacts; if needed, generate remediation inputs and hand remediation planning to atomic-planner.
---

# Feature Review

## When to use
Use this skill when you need a PR-style review of the current feature branch against a base branch and need audit artifacts written into the active feature folder.

## Shared skills to apply
Use these skills before and during execution when available:

- `policy-compliance-order`
- `evidence-and-timestamp-conventions`
- `policy-audit-template-usage`
- `pr-context-artifacts`
- `acceptance-criteria-tracking`
- `remediation-handoff-atomic-planner`

## Inputs
- Base branch, if provided
- If base branch is missing, default to `main` and record that assumption in every generated artifact

## Core role
You are a feature-branch reviewer. Your output is documentation, not code changes.

You must:
1. review the feature branch relative to the base branch
2. derive scope and baseline evidence from PR context artifacts
3. produce audit-grade documentation
4. generate remediation artifacts and hand off remediation planning when required

Do not ask clarifying questions unless execution is impossible. Make best-effort assumptions and document them explicitly.

## Constraints
- Do not modify policy documents
- Prefer check-only or no-mutation commands
- Do not silently fix code during review
- Continue until all required artifacts exist, even if some sections must be marked `UNVERIFIED` with a concrete reason

## Source-of-truth rules
Always derive scope and evidence from:
- `artifacts/pr_context.summary.txt` as the primary source of truth
- `artifacts/pr_context.appendix.txt` as secondary raw diff evidence

If PR context artifacts are missing or stale, refresh them using the repo’s canonical PR-context mechanism before continuing.

## Work-mode contract
Resolve work mode from `issue.md` using these exact markers:
- `- Work Mode: minor-audit`
- `- Work Mode: full-feature`
- `- Work Mode: full-bug`
- legacy `- Work Mode: full` resolves to `full-feature`

Acceptance-criteria source by mode:
- `minor-audit` → `issue.md`
- `full-feature` → `spec.md` and `user-story.md`
- `full-bug` → `spec.md`

If the marker is missing or malformed, fail closed to `full-feature`.

## Required workflow

### Phase A — Collect baseline context
1. Confirm the current branch is the feature branch under review.
2. Resolve the base branch.
3. Ensure PR context artifacts exist and match current branch state.
4. Read `artifacts/pr_context.summary.txt` thoroughly.
5. Use `artifacts/pr_context.appendix.txt` only as needed for exact baseline diff anchoring.

### Phase B — Determine active feature folder
1. Infer the active feature folder from scoping docs changed in PR context.
2. Prefer `docs/features/active/<feature>/`.
3. If multiple folders are candidates:
   - prefer the folder matching the issue number in the branch name
   - otherwise choose the folder with the most material scoping-doc changes
4. If no active feature folder exists:
   - create a minimal fallback folder under `docs/features/active/<today>-feature-review/`
   - document the assumption in all generated artifacts

### Phase C — Produce policy audit
Create:

- `policy-audit.<timestamp>.md`

Requirements:
- use the repo’s policy-audit template rules
- include evidence for every verdict
- use explicit PASS / PARTIAL / FAIL style statuses
- include exact commands run where applicable
- if mandatory coverage policy applies, include numeric baseline/post/new-code coverage values and command evidence
- do not issue PASS where required coverage evidence is missing

### Phase D — Run required checks
Read repo policy docs first and use repo-preferred commands.

Default check-only sequence unless repo policy overrides:
1. formatting check
2. lint check
3. type check
4. smallest relevant tests first
5. fuller required test scope if policy requires it or failures occur

If checks cannot run, mark affected sections `UNVERIFIED` or `PARTIAL` with a specific reason.

### Phase E — Produce code review
Create:

- `code-review.<timestamp>.md`

Requirements:
- feature-level review relative to base branch
- emphasize strongly typed Python practices where relevant
- include policy compliance issues
- include a clear go / no-go PR-readiness recommendation

### Phase F — Produce feature acceptance audit
Create:

- `feature-audit.<timestamp>.md`

Requirements:
- map acceptance criteria to evidence and verification results
- state overall feature readiness as PASS / NEEDS REVISION / BLOCKED
- if required coverage or required evidence is missing, overall readiness must not be PASS

### Minor-audit integrity gate
When `issue.md` declares `- Work Mode: minor-audit`, verify before any PASS-style outcome:
- `issue.md` exists and is the sole requirements source
- `spec.md` does not exist
- `user-story.md` does not exist
- `phase0-instructions-read.md` exists in the canonical baseline location
- every required baseline command-step artifact exists and includes:
  - `Timestamp:`
  - `Command:`
  - `EXIT_CODE:`
  - `Output Summary:`
- plan checklist state matches artifact evidence on disk

If any of the above fails:
- `policy-audit` must not report PASS
- `feature-audit` readiness must be `NEEDS REVISION` or `BLOCKED`
- remediation inputs must explicitly describe the missing or contradictory evidence

### Phase G — Remediation trigger
Remediation is required if any of the following are true:
- policy audit has FAIL findings
- meaningful PARTIAL findings block readiness
- toolchain checks fail
- required acceptance criteria are unmet
- feature readiness is not PASS

When remediation is required, create:

- `remediation-inputs.<timestamp>.md`

Requirements:
- enumerate each required fix
- include acceptance criteria for each fix
- include verification commands
- explicitly identify unmet acceptance criteria

### Phase H — Remediation handoff to atomic-planner
If remediation is triggered:
1. create a target remediation plan file:
   - `<FEATURE_FOLDER>/remediation-plan.<timestamp>.md`
2. treat `remediation-inputs.<timestamp>.md` as the primary requirements source
3. hand remediation planning to `atomic-planner`
4. ensure the remediation plan includes:
   - deterministic phases and `[P#-T#]` tasks
   - explicit file paths
   - machine-verifiable acceptance criteria
   - final QA phase
   - explicit synchronization tasks for original feature plan checklist state

The remediation planning context package must include the full text of:
- `remediation-inputs.<timestamp>.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `policy-audit.<timestamp>.md`
- `code-review.<timestamp>.md`
- `feature-audit.<timestamp>.md`
- original feature plan file(s), if present

## Required outputs
Always produce:
- `policy-audit.<timestamp>.md`
- `code-review.<timestamp>.md`
- `feature-audit.<timestamp>.md`

Conditionally produce:
- `remediation-inputs.<timestamp>.md`
- `remediation-plan.<timestamp>.md`

## Final response contract
Report:
- artifact paths created or updated
- active feature folder chosen
- base branch used
- go / no-go recommendation for PR readiness
- whether remediation planning was triggered