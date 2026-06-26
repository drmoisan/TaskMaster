---
name: feature-promotion-lifecycle
description: Deterministic promotion workflow from potential feature/bug entry to issue, branch, active feature folder, and downstream spec/research handoffs. Agent sessions must use the drm-copilot MCP tool surface and record raw promotion receipts under the canonical checkpoint namespace.
---

# Feature Promotion Lifecycle

Canonical variable model and MCP-only command sequence for promoting potential feature/bug entries and initializing active feature delivery.

## When to Use This Skill

Use this skill when:
- A large-scope change requires feature/bug promotion workflow.
- A short-path workflow still requires promotion/folder initialization before delegated implementation.
- An orchestrator must create potential docs, promote to issue, branch, and active feature folder.
- Downstream research/spec agents depend on deterministic paths and identifiers.

## MCP Tool Availability Preflight

Before any promotion step starts, verify that the required `drm-copilot` MCP tools are available in the current agent session.

Required MCP tool set:
- feature potential entry: `mcp__drm-copilot__new_potential_entry` with `short_name=${short-name}`
- bug potential entry: `mcp__drm-copilot__new_potential_bug_entry` with `short_name=${short-name}`
- potential-to-issue promotion: `mcp__drm-copilot__potential_to_issue` with `potential_path=${relativeFile}`, `promotion_type=${promotion-type}`, `work_mode=${work-mode}`
- active feature folder creation: `mcp__drm-copilot__new_active_feature_folder` with `feature_name=${long-name}`, `type=${promotion-type}`, `issue_number=${issue-num}`, `work_mode=${work-mode}`

If the required MCP tools are unavailable, stop before potential-entry creation, issue promotion, or active-folder creation begins. Restore MCP connectivity first. Agent sessions do not have an approved non-MCP execution branch for promotion work.

## Agent-Session Promotion Execution Rule

Execute MCP-backed lifecycle operations only through the MCP tool forms listed
above. The MCP path is the sole authoritative execution path for potential
entry creation, issue promotion, and active feature folder creation in agent
sessions.

After each successful promotion operation, persist the raw MCP receipt payload under the matching checkpoint key in `artifacts/orchestration/orchestrator-state.json`:
- `delegation_receipts.promotion.potential_entry`
- `delegation_receipts.promotion.issue`
- `delegation_receipts.promotion.feature_folder`

Each `delegation_receipts.promotion.*` field stores the raw MCP receipt payload returned by the corresponding promotion operation without lossy normalization.

Branch creation and branch rename are required lifecycle sequencing evidence,
but they are not `surface: "mcp"` lifecycle operations unless a future MCP tool
exists for those branch operations. Record branch creation and branch rename as
branch/checkpoint evidence in `artifacts/orchestration/orchestrator-state.json`.

Note: VS Code command-palette commands may exist for interactive extension use, but this note is non-authoritative for agent sessions.

## Canonical Variables

- `${promotion-type}`: `feature` or `bug`
- `${short-name}`: lowercase slug, hyphen-separated
- `${relativeFile}`: workspace-relative path to created potential entry markdown
- `${long-name}`: `${relativeFile}` filename without `.md`
- `${issue-num}`: promoted GitHub issue number
- `${feature-folder}`: active feature folder path
- `${plan-path}`: single canonical plan file path reused across planning and preflight revisions
- `${work-mode}`: `minor-audit`, `full-feature`, or `full-bug` (legacy `full` is accepted only as an alias for `full-feature`)
- `${short-path-flag}`: `--work-mode minor-audit` (mandatory for short-path promotion/folder creation)
- `${pre-issue-branch}`: `${promotion-type}/${short-name}`, created or verified before issue creation
- `${final-branch}`: `${promotion-type}/${short-name}-${issue-num}`, created by branch rename after promotion returns a numeric issue number

`${relativeFile}` MUST resolve to a real potential markdown path before promotion begins. If the path is missing, invalid, or non-markdown, stop. Do not infer or synthesize the missing value.

`${issue-num}` MUST be numeric after promotion and before branch or folder creation. If promotion does not return a numeric issue number, stop. Do not infer or synthesize the missing value.

When orchestrator routing selects short path, promotion/folder initialization still occurs and MUST use `minor-audit` mode.

Lifecycle guardrails:
- `${relativeFile}` MUST resolve to a real potential markdown path before promotion.
- `${issue-num}` MUST be numeric after promotion and before branch or folder creation.
- Do not infer or synthesize the missing value.
- If `${relativeFile}` or `${issue-num}` is missing, placeholder text, or unverified, stop before final branch rename, active-folder creation, or active-folder authoring.

1) Use the same MCP tool-availability preflight described above and continue only when the required promotion tools are available.

2) Verify route metadata readiness in `artifacts/orchestration/orchestrator-state.json`, including selected `route_id`, required route metadata, and `${work-mode}`.

3) Create or verify the pre-issue branch:
- `${pre-issue-branch}`

4) Create the potential entry through `mcp__drm-copilot__new_potential_entry` or `mcp__drm-copilot__new_potential_bug_entry`.

5) Promote the potential document through `mcp__drm-copilot__potential_to_issue` with the selected `${work-mode}` and capture the numeric `${issue-num}`.

6) Rename the branch to the final branch:
- `${final-branch}`

7) Create the active feature folder through `mcp__drm-copilot__new_active_feature_folder` with the selected `${work-mode}`.

7a) Verify minor-audit folder integrity before proceeding:
- `${feature-folder}/issue.md` exists and contains `- Work Mode: minor-audit`
- `${feature-folder}/issue.md` contains an explicit `## Acceptance Criteria` section
- `${feature-folder}/spec.md` does not exist
- `${feature-folder}/user-story.md` does not exist
- if any check fails, stop and remediate before planning

8) Delegate minimal-audit plan creation to `atomic_planner` with directive:
- `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED`

8a) Resolve and persist `${plan-path}` before delegation:
- reuse the earliest existing `plan*.md` in `${feature-folder}` when present
- otherwise create exactly one canonical plan file path and reuse it for all revisions

9) Require preflight validation via `atomic_executor` until:
- `PREFLIGHT: ALL CLEAR`

10) Execute plan Phase 0 only via executor and checkpoint evidence.

11) Branch:
- manual bootstrap: save state and stop ONLY when the initial user request explicitly opted into manual orchestration from the beginning,
- non-bootstrap: continue with constrained small-path development.

Automation rule:
- do not introduce manual bootstrap, human-operator validation, or any other manual handoff later in orchestration unless that initial explicit opt-in exists
- if automation cannot proceed, record blocked automated state instead of asking for manual intervention

12) Validate delivery via executor against `issue.md`, then run reduced audit/remediation loop until ready-to-merge.

## Required Outputs for Downstream Handoffs

Before delegating research/spec/planning, provide:
- `${feature-folder}/issue.md`
- `${feature-folder}/spec.md` (or expected target path)
- `${feature-folder}/user-story.md` (or explicit `NONE`)
- latest research artifact path(s)
- constraints/APIs/invariants to preserve

Mode-aware expectations:
- For `minor-audit`, the explicit `## Acceptance Criteria` section in `issue.md` is the primary acceptance-criteria source and `spec.md`/`user-story.md` may be intentionally absent by design.
- For `minor-audit`, do not infer acceptance criteria from other `issue.md` sections such as verification notes, next steps, or severity checklists.
- For `minor-audit`, `spec.md`/`user-story.md` must be treated as integrity failures when they appear unexpectedly in the active folder.
- For `full-feature`, `spec.md` and `user-story.md` are expected alongside `issue.md`.
- For `full-bug`, `spec.md` is expected alongside `issue.md`; `user-story.md` should be absent unless the requirements explicitly justify it.

Selected-mode persistence requirements:
- Producer outputs MUST persist exactly one marker in `issue.md` metadata above the first `##` heading:
	- `- Work Mode: minor-audit`
	- `- Work Mode: full-feature`
	- `- Work Mode: full-bug`
- Persisted marker MUST represent selected mode after eligibility checks, not requested mode.
- If a legacy requested `full` path is accepted, tooling MUST normalize it to `full-feature` before persistence.
- If a requested `minor-audit` path is rejected by eligibility checks, tooling MUST fail closed to `full-feature`, emit the downgrade reason, and persist `- Work Mode: full-feature`.
