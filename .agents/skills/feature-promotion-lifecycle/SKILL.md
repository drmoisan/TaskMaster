---
name: feature-promotion-lifecycle
description: 'Deterministic promotion workflow from potential item to issue, branch, and active feature folder. Use when orchestration must initialize delivery state in Codex without duplicating host-specific automation logic.'
---

# Feature Promotion Lifecycle

Canonical variable model and promotion sequence for initializing active feature delivery.

## Required Shared Skill

Always use:
- `repo-automation-adapter`

## Canonical Variables

- `${promotion-type}`: `feature` or `bug`
- `${short-name}`: lowercase hyphenated slug
- `${relativeFile}`: workspace-relative path to the potential entry
- `${long-name}`: `${relativeFile}` filename without `.md`
- `${issue-num}`: promoted GitHub issue number
- `${feature-folder}`: active feature folder path
- `${plan-path}`: canonical plan file path
- `${work-mode}`: `minor-audit`, `full-feature`, or `full-bug`

## Workflow

1. Create the potential entry.
2. Promote the potential entry to an issue.
3. Create the branch.
4. Create the active feature folder.
5. Resolve and persist the canonical `plan-path`.
6. Delegate planning to `atomic-planner`.
7. Require `atomic-executor` preflight before execution.

## Codex Execution Rule

Do not encode host-specific implementation details here.

For each lifecycle step:
- use `repo-automation-adapter` to choose the direct repo automation path,
- use a deterministic local fallback only when explicitly supported,
- otherwise stop and report the missing automation dependency.

## Mode-Aware Expectations

- `minor-audit`: `issue.md` is authoritative and `spec.md` / `user-story.md` are intentionally absent
- `full-feature`: `issue.md`, `spec.md`, and `user-story.md` are expected
- `full-bug`: `issue.md` and `spec.md` are expected
