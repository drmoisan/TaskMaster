---
name: feature-promotion-lifecycle
description: Deterministic promotion workflow from potential feature/bug entry to issue, branch, active feature folder, and downstream spec/research handoffs.
---

# Feature Promotion Lifecycle

Canonical variable model and command sequence for promoting potential feature/bug entries and initializing active feature delivery.

## When to Use This Skill

Use this skill when:
- A large-scope change requires feature/bug promotion workflow.
- A short-path workflow still requires promotion/folder initialization before delegated implementation.
- An orchestrator must create potential docs, promote to issue, branch, and active feature folder.
- Downstream research/spec agents depend on deterministic paths and identifiers.

## Canonical Variables

- `${promotion-type}`: `feature` or `bug`
- `${short-name}`: lowercase slug, hyphen-separated
- `${relativeFile}`: workspace-relative path to created potential entry markdown
- `${long-name}`: `${relativeFile}` filename without `.md`
- `${issue-num}`: promoted GitHub issue number
- `${feature-folder}`: active feature folder path
- `${work-mode}`: `full` or `minor-audit`
- `${short-path-flag}`: `--work-mode minor-audit` (mandatory for short-path promotion/folder creation)

## Canonical Command Sequence

1) Create potential entry by type:
- feature: `VS Code command: `drm-copilot: New Potential Entry (PowerShell Placeholder)` (command ID: `drmCopilotExtension.newPotentialEntryPsPlaceholder`) -ShortName ${short-name}`
- bug: `VS Code command: `drm-copilot: New Potential Bug Entry (Python Placeholder)` (command ID: `drmCopilotExtension.newPotentialBugEntryPyPlaceholder`) --short-name ${short-name}`

2) Promote potential doc:
- `VS Code command: `drm-copilot: Potential To Issue (Placeholder)` (command ID: `drmCopilotExtension.potentialToIssuePlaceholder`) --potential-path ${relativeFile} --promotion-type ${promotion-type} --work-mode ${work-mode}`

3) Create branch:
- `${promotion-type}/${short-name}-${issue-num}`

4) Create active feature folder:
- `VS Code command: `drm-copilot: New Active Feature Folder (Placeholder)` (command ID: `drmCopilotExtension.newActiveFeatureFolderPlaceholder`) --feature-name ${long-name} --type ${promotion-type} --issue-number ${issue-num} --work-mode ${work-mode}`

## Canonical Short-Path Sequence (Minor Audit Mode)

When orchestrator routing selects short path, promotion/folder initialization still occurs and MUST use `minor-audit` mode.

1) Promote potential doc with short-path flag:
- `VS Code command: `drm-copilot: Potential To Issue (Placeholder)` (command ID: `drmCopilotExtension.potentialToIssuePlaceholder`) --potential-path ${relativeFile} --promotion-type ${promotion-type} --work-mode minor-audit`

2) Create branch:
- `${promotion-type}/${short-name}-${issue-num}`

3) Create active feature folder with short-path flag:
- `VS Code command: `drm-copilot: New Active Feature Folder (Placeholder)` (command ID: `drmCopilotExtension.newActiveFeatureFolderPlaceholder`) --feature-name ${long-name} --type ${promotion-type} --issue-number ${issue-num} --work-mode minor-audit`

4) Delegate minimal-audit plan creation to `atomic_planner` with directive:
- `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED`

5) Require preflight validation via `atomic_executor` until:
- `PREFLIGHT: ALL CLEAR`

6) Execute plan Phase 0 only via executor and checkpoint evidence.

7) Branch:
- manual bootstrap: save state and stop,
- non-bootstrap: continue with constrained small-path development.

8) Validate delivery via executor against `issue.md`, then run reduced audit/remediation loop until ready-to-merge.

## Required Outputs for Downstream Handoffs

Before delegating research/spec/planning, provide:
- `${feature-folder}/issue.md`
- `${feature-folder}/spec.md` (or expected target path)
- `${feature-folder}/user-story.md` (or explicit `NONE`)
- latest research artifact path(s)
- constraints/APIs/invariants to preserve

Mode-aware expectations:
- For `minor-audit`, `issue.md` is the primary acceptance-criteria source and `spec.md`/`user-story.md` may be intentionally absent by design.
- For `full`, `spec.md` and `user-story.md` are expected alongside `issue.md`.

Selected-mode persistence requirements:
- Producer outputs MUST persist exactly one marker in `issue.md` metadata above the first `##` heading:
	- `- Work Mode: minor-audit`
	- `- Work Mode: full`
- Persisted marker MUST represent selected mode after eligibility checks, not requested mode.
- If a requested `minor-audit` path is rejected by eligibility checks, tooling MUST fail closed to `full`, emit fallback reason, and persist `- Work Mode: full`.
