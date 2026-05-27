---
name: repo-root-is-source-of-truth-for-codex-bundle
description: When repo-root .codex/.agents/AGENTS.md/etc differ from the extension's bundled customizations, treat the repo root as authoritative and update the bundle to match — not the other way around.
type: feedback
---

When repo-root files such as `.codex/agents/*.toml`, `.codex/config.toml`, `.codex/hooks/*.ps1`, `.agents/skills/**`, and `AGENTS.md` differ from their bundled copies under `extensions/drm-copilot/resources/codex-and-agents-customizations/` and `extensions/drm-copilot/resources/claude-customizations/`, the repo-root version is the source of truth. Update the bundle to mirror the repo root.

**Why:** The user actively overwrites repo-root files (e.g., by running the codex-native-converter in apply mode) and expects the bundle to reflect those changes. Reverting repo-root files to match the bundle is the wrong direction. Confirmed 2026-05-02 after a converter apply run regenerated `.codex/`, `.agents/`, and `AGENTS.md`.

**How to apply:** When contract tests like `test_push_down_claude_resource_contracts`, `test_push_down_codex_and_agents_resource_contracts`, `test_codex_agent_wrapper_contracts`, `test_codex_full_migration_inventory`, `test_codex_handoff_contract_parity`, or `test_orchestration_guardrail_contracts` fail with diffs between repo and bundle, copy from repo → bundle. Never propose `git checkout HEAD --` on the repo-root file.
