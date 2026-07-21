# Phase 0 — Instructions Read (Issue #283)

Timestamp: 2026-07-08T17-56

Policy Order: policy-compliance-order sequence — (1) CLAUDE.md, (2) general-code-change, (3) general-unit-test, (4) language rules (csharp, powershell), plus ci-workflows (CI pwsh step edited).

Files read (in order):
1. `CLAUDE.md` (project standing instructions — loaded in session context)
2. `.claude/rules/general-code-change.md` (cross-language code change policy — loaded in session context)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy — loaded in session context)
4. `.claude/rules/csharp.md` (C# code standards — loaded in session context)
5. `.claude/rules/powershell.md` (PowerShell code standards — loaded in session context)
6. `.claude/rules/ci-workflows.md` (CI workflow authoring; CI `pwsh` vstest step edited by P1-T6 — loaded in session context)

Supporting skill files read:
- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

Plan of record read:
- `docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/plan.2026-07-08T13-39.md`

Requirements source (sole, minor-audit):
- `docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/issue.md` (`## Acceptance Criteria`, AC1–AC7)

Notes:
- Work Mode: minor-audit (from `issue.md` metadata + plan). AC source is `issue.md` `## Acceptance Criteria` only. `spec.md`/`user-story.md`/`research.md` are NOT required and are absent; this is correct for minor-audit and is not a blocker.
- Evidence location invariant honored: all artifacts under `docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/evidence/<kind>/`.
