---
name: translate-claude-to-codex
description: Translate the native Claude Code runtime (CLAUDE.md, .claude/rules/*.md, .claude/skills/<name>/SKILL.md, .claude/agents/*.md, .claude/hooks/*.ps1, .claude/settings.json) into the native Codex ecosystem (AGENTS.md, .codex/config.toml, .codex/agents/*.toml, .agents/skills/<name>/SKILL.md, .codex/hooks.json). Classify each Claude surface into its Codex-native equivalent, preserve mechanical enforceability of every hardening element, diff against existing Codex state, produce a translation plan with an enforceability-preservation ledger for user approval, then apply.
---

# Translate Claude to Codex

Deterministic translation workflow that ports the `.claude/` runtime into the Codex-native customization surface. The primary objective is preservation of mechanical enforceability: every Claude hardening element that blocks an action at the process or OS level must map to a Codex mechanism that blocks at the same level, or be explicitly flagged as a degraded gate with a compensating control.

This skill is the inverse of `translate-copilot-to-claude`. It does not modify or delete any `.claude/` source file. Translation is a copy-forward operation.

## When to Use This Skill

Use this skill when:

- One or more `.claude/` runtime files must be mirrored into the Codex ecosystem.
- A Claude subagent persona needs to be re-expressed as a Codex agent definition.
- A batch of `.claude/rules/*.md` path-scoped rules must be re-grounded as Codex instruction or sandbox surfaces.
- A Claude `PreToolUse` or `SubagentStop` hook must be re-expressed with equivalent Codex blocking semantics.
- The user wants to know which hardening elements lose mechanical enforceability under Codex before any change is made.

## Authoritative Inputs

This skill consumes the Codex ecosystem mapping recorded in `artifacts/research/codex-native-ecosystem.2026-06-16T13-32.md`. That artifact is the source of truth for Codex primitive behavior, discovery order, and enforceability. Do not restate Codex documentation from memory; cite the research artifact.

Required source paths under `.claude/` (one or more). Accepted types:

- `CLAUDE.md` (repo root standing instructions)
- `.claude/rules/<name>.md` (path-scoped rules with `paths:` frontmatter)
- `.claude/skills/<name>/SKILL.md`
- `.claude/agents/<name>.md`
- `.claude/hooks/<name>.ps1`
- `.claude/settings.json` (`permissions`, `hooks` blocks)

Optional inputs:

- `mode=plan-only` — emit the translation plan and the enforceability ledger, then stop. This is the default.
- `mode=apply` — apply the plan after it is produced and approved in the same turn.
- `target-scope=<instruction|skill|agent|hook|permission|all>` — restrict surfaces considered.
- `timestamp-override=<ISO-8601>` — override the auto-generated evidence-folder timestamp.

Apply requires explicit confirmation, either via `mode=apply` or a plain-text approval in the same turn after the plan is shown.

## Codex Discovery Locations (Targets)

| Codex surface | Path | Trust precondition |
|---|---|---|
| Repo standing instructions | `AGENTS.md` at repo root | none (prompt-level) |
| Directory-scoped instructions | `<subdir>/AGENTS.md` | none (prompt-level) |
| Project configuration | `.codex/config.toml` | project `trust_level = "trusted"` |
| Permissions profiles | `[permissions.<name>]` in `.codex/config.toml` | trusted |
| Hooks | `[hooks]` in `.codex/config.toml` or `.codex/hooks.json` | trusted; first-run hook trust review |
| Agents | `.codex/agents/<name>.toml` | trusted |
| Skills | `.agents/skills/<name>/SKILL.md` | none (repo-wide discovery) |
| MCP servers | `[mcp_servers.<id>]` in `.codex/config.toml` | trusted |
| Enterprise managed hooks | `requirements.toml` (`managed_dir`) | admin file placement |

An untrusted project skips all `.codex/` layers (config, hooks, agents). Record the trust dependency for every target that requires it; the plan must state that `projects."<abs-path>".trust_level = "trusted"` is set in user-level `~/.codex/config.toml`.

## Phase 1 — Intake and Surface Extraction

For each input file:

1. Read the full file with `Read`.
2. For Markdown sources, parse YAML frontmatter (`name`, `description`, `paths`, `allowed-tools`, `context`, `agent`, `tools`, `model`, `skills`, `memory`, `hooks`).
3. For `.claude/settings.json`, parse the `permissions.allow`, `permissions.deny`, `permissions.additionalDirectories`, `hooks.PreToolUse`, and `hooks.SubagentStop` arrays.
4. For `.claude/hooks/*.ps1`, identify the blocking semantics: non-zero `exit`, `Write-Error`, and any emitted `{"decision":"block"}` JSON. Record the matcher that registers the hook in `settings.json`.
5. Record each surface element as `{source_path, element, kind, enforceability_class, raw_content}` where `enforceability_class` is one of `os-enforced`, `process-enforced`, `prompt-level`.

Never modify or delete source files.

## Phase 2 — Classification Taxonomy

Apply these rules in order. Use the first rule that matches.

### 2.1 Mechanical gate (process- or OS-enforced) -> Codex hooks, sandbox, or permissions

A Claude element is a mechanical gate when it blocks an action independent of model cooperation:

- `.claude/hooks/*.ps1` registered under `hooks.PreToolUse` that exit non-zero or emit `{"decision":"block"}`.
- `hooks.SubagentStop` validators that exit non-zero to reject subagent completion.
- `permissions.deny` entries (secret-path read/write blocks).
- `permissions.allow` tool and path allowlists.

Targets, by sub-kind:

- **PreToolUse blocker** -> `[hooks] PreToolUse` entry in `.codex/config.toml` (or `.codex/hooks.json`) with a `matcher` on `tool_name`. This is a direct mechanical equivalent. The hook script must block via **exit code `2`** or stdout `{"permissionDecision":"deny","stopReason":"..."}`. Translate the Claude convention (`exit 1` / `{"decision":"block"}`) to the Codex convention (`exit 2` / `permissionDecision: deny`). Preserve the script logic; only the exit/decision contract changes.
- **SubagentStop hard rejection** -> two targets are required because Codex `SubagentStop` with `{"decision":"block"}` forces continuation rather than rejecting output. (1) A `[hooks] SubagentStop` entry that re-prompts the agent to produce the missing artifact, and (2) a required GitHub Actions status check (`openai/codex-action` job or a `codex exec` wrapper) that runs the same validation and fails the merge if the artifact is absent. The CI check is the hard gate; the in-process hook is best-effort. Flag this as a **degraded gate** in the ledger.
- **`permissions.deny` path block** -> `[permissions.<name>.filesystem]` deny rule in `.codex/config.toml`, enforced by the OS sandbox. Secret-path denies map to filesystem deny globs and a `read-only` or `workspace-write` sandbox boundary.
- **`permissions.allow` tool allowlist** -> there is no per-tool allowlist in Codex equivalent to Claude `Skill(...)`/`Agent(...)`/`Bash(...)` allow entries. Re-ground in: `sandbox_mode`, `[permissions.<name>]` filesystem/network rules, MCP `enabled_tools`/`disabled_tools`, and where a built-in tool must be blocked outright, a `PreToolUse` hook with a `matcher` that denies it. Flag any allowlist element that cannot be fully reproduced as a **degraded gate**.

Hooks generated by this skill are validation-only unless the user explicitly authorizes state mutation.

### 2.2 Path-scoped declarative rules -> AGENTS.md (with enforceability caveat)

`.claude/rules/<name>.md` files carry `paths:` file-extension globs (for example `**/*.py`). Codex has no frontmatter `paths:` glob. Targets:

- **Cross-cutting rule** (no narrow path scope, or repo-wide) -> append to repo-root `AGENTS.md`.
- **Directory-scoped rule** (scope maps cleanly to a directory) -> a nested `<subdir>/AGENTS.md`.
- **File-extension-scoped rule** (for example Python-only, scattered across directories) -> there is no mechanical Codex equivalent. Re-express as repo-root `AGENTS.md` guidance and, where the rule encodes an enforceable constraint (file-size limit, banned API, test-purity), pair it with the corresponding `PreToolUse` hook from section 2.1 rather than relying on the prose. Flag the loss of automatic glob scoping in the ledger.

A `.claude/rules` element that is enforced today only by prose (not by a hook) remains prompt-level in both runtimes; record it as `prompt-level` with no degradation.

### 2.3 Reusable procedures -> Codex skills

`.claude/skills/<name>/SKILL.md` maps near-1:1 to `.agents/skills/<name>/SKILL.md`. Reconcile the frontmatter schema:

- Keep `name` and `description`.
- `allowed-tools` has **no** Codex frontmatter equivalent. Do not silently drop it. Re-ground the intended restriction in the agent's `sandbox_mode`, in `[permissions.<name>]`, in MCP `disabled_tools`, or in a `PreToolUse` hook, and record the re-grounding target in the ledger.
- `context: fork` with `agent:` routing has no Codex equivalent. Re-express the routing as an explicit instruction in the skill body that the orchestrator spawns the named `.codex/agents/<name>.toml` agent.
- Optional Codex `agents/openai.yaml` may carry UI metadata and tool-dependency hints; create it only if the user requests UI metadata.

### 2.4 Agent personas -> Codex agent definitions

`.claude/agents/<name>.md` maps to `.codex/agents/<name>.toml`. Field mapping:

- `name` -> `name`; `description` -> `description`; persona body -> `developer_instructions`.
- `model` -> `model`; reasoning settings -> `model_reasoning_effort`.
- `tools:` allowlist -> **no direct equivalent.** Approximate via `sandbox_mode`, per-agent `mcp_servers`, and `skills.config` enable/disable toggles. Any tool that the Claude persona forbids and that cannot be removed from the Codex agent's reach via sandbox or MCP scoping must be blocked by a session-level `PreToolUse` hook. Flag every such element as a **degraded gate**.
- Preloaded `skills:` -> `skills.config` toggles enabling the corresponding `.agents/skills/<name>` packages.
- `hooks:` on the persona -> Codex hooks are session-level, not agent-definition-level. Re-express as `[hooks]` entries whose `matcher` filters the relevant `tool_name`; note that per-agent hook scoping is not mechanically available.

Normalize underscores to hyphens in target filenames (`python_typed_engineer` -> `python-typed-engineer.toml`).

### 2.5 Standing instructions and tone -> AGENTS.md

`CLAUDE.md` (tone policy, policy-reading order, architecture overview) maps to repo-root `AGENTS.md`. Concatenation precedence is positional (closest-to-`$CWD` wins). Keep the tone policy near the top so it is not truncated by `project_doc_max_bytes` (default 32 KiB). Cross-reference rather than restate content already placed in nested `AGENTS.md` files.

### 2.6 Out-of-scope content

Not translated by this skill:

- `.github/` Copilot or Actions surfaces (handled by `translate-copilot-to-claude` in the other direction).
- `.claude/agent-memory/**` runtime memory state.
- `artifacts/**` and `docs/**` evidence and feature artifacts.

List skipped inputs explicitly in the plan.

## Phase 3 — Target Resolution

For each classified element, compute a concrete Codex target path:

1. **PreToolUse hook**: a script under `.codex/hooks/<verb-noun>.ps1` (or reuse the existing `.claude/hooks/*.ps1` logic copied forward) plus a `[hooks] PreToolUse` registration with a `matcher`. Translate exit semantics to Codex (`exit 2` / `permissionDecision: deny`).
2. **SubagentStop gate**: a `[hooks] SubagentStop` registration plus a required CI workflow job under `.github/workflows/` invoking `openai/codex-action` or a `codex exec` wrapper with a post-validation step.
3. **Permissions**: `[permissions.<name>]` profile in `.codex/config.toml` with `filesystem`, `network`, and `extends` keys; sandbox boundary via `sandbox_mode`.
4. **Instruction**: repo-root `AGENTS.md` or `<subdir>/AGENTS.md`.
5. **Skill**: `.agents/skills/<kebab-case-name>/SKILL.md`.
6. **Agent**: `.codex/agents/<kebab-case-name>.toml`.
7. **Project config**: `.codex/config.toml` (`model`, `sandbox_mode`, `approval_policy`, `[agents]`, `[mcp_servers.*]`, `[features]`).

Record target paths in the plan. Do not write anything yet.

## Phase 4 — Existing Codex State Diff

For every target path:

1. `Read` the existing file when present.
2. Compute a per-element delta: `add`, `replace`, `merge`, `skip`, or `conflict` (source and target disagree on substantive content; requires user decision).
3. For `.codex/config.toml` tables, compute add-only updates to `[hooks]`, `[permissions.*]`, and `[mcp_servers.*]`. Never remove an existing permission or hook without explicit instruction.
4. For `AGENTS.md`, prefer `merge` (append a delimited section) over `replace` to preserve hand-authored content and respect positional precedence.

Flag every `conflict` row. Conflicts stop Phase 6 for that row until resolved.

## Phase 5 — Translation Plan and Enforceability Ledger

Write the plan to `artifacts/translation/<timestamp>/plan.md`. The timestamp is ISO-8601 UTC per `evidence-and-timestamp-conventions`.

Plan structure:

```markdown
# Translation Plan: Claude -> Codex (<source basename(s)>)

Generated: <timestamp>
Mode: <plan-only | apply>
Research basis: artifacts/research/codex-native-ecosystem.2026-06-16T13-32.md

## Inputs
- <source path 1>
- <source path 2>

## Mapping Table
| Source Element | Kind | Codex Target | Action | Trust Required |
|---|---|---|---|---|
| .claude/hooks/validate-bash.ps1 | PreToolUse | .codex/hooks/validate-bash.ps1 + [hooks].PreToolUse | add | yes |
| .claude/settings.json deny secrets | permission | [permissions.base.filesystem] deny | add | yes |
| .claude/rules/python.md | rule | AGENTS.md + PreToolUse hook | merge | partial |
| .claude/agents/python-typed-engineer.md | agent | .codex/agents/python-typed-engineer.toml | add | yes |
| .claude/skills/python-qa-gate/SKILL.md | skill | .agents/skills/python-qa-gate/SKILL.md | add | no |

## Enforceability Preservation Ledger
| Hardening element | Claude class | Codex class | Status | Compensating control |
|---|---|---|---|---|
| PreToolUse Bash validator | process-enforced | process-enforced | PRESERVED | exit 2 / permissionDecision deny |
| SubagentStop artifact gate | process-enforced | process-enforced (CI) | DEGRADED | required GitHub Actions status check |
| Skill allowed-tools | process-enforced | re-grounded | DEGRADED | sandbox_mode + MCP disabled_tools |
| rules paths: glob scoping | prompt-level | prompt-level | PRESERVED | per-directory AGENTS.md |
| Agent tools allowlist | process-enforced | re-grounded | DEGRADED | sandbox_mode + skills.config + PreToolUse hook |
| permissions.deny secrets | os-enforced | os-enforced | PRESERVED | filesystem deny glob + sandbox |

## Conflicts (require user decision)
<one row per conflict, or "none">

## New Files
<list>

## Updated Files
<list>

## Config Delta (.codex/config.toml)
- Hooks added: <list of matchers>
- Permissions profiles added: <list>
- MCP servers added: <list>
- Trust precondition: projects."<abs-path>".trust_level = "trusted"

## CI Backstops (for DEGRADED gates)
- <workflow path : job name : gate enforced>

## Evidence Paths
- artifacts/translation/<timestamp>/plan.md
- artifacts/translation/<timestamp>/diff.md (populated after apply)
```

The Enforceability Preservation Ledger is mandatory. Every element classified `os-enforced` or `process-enforced` in Phase 1 must appear with a `PRESERVED`, `DEGRADED`, or `LOST` status. A `LOST` status (no mechanical Codex equivalent and no compensating control) blocks apply until the user accepts the risk or supplies a control. Always produce the plan artifact, even in `mode=apply`.

## Phase 6 — Apply (only after explicit approval)

Execute only when the user invoked `mode=apply` or sent a plain-text approval ("proceed", "apply", "execute the plan") in the same turn after the plan was shown.

Apply order (to minimize breakage):

1. **AGENTS.md** — add or merge repo-root and nested instruction files first so downstream agents read current standing instructions.
2. **Skills** — write `.agents/skills/<name>/SKILL.md` packages.
3. **Hook scripts** — copy hook logic into `.codex/hooks/<name>.ps1`, converting block semantics to `exit 2` / `permissionDecision: deny`. Parse-check each with `pwsh -NoProfile -Command "[System.Management.Automation.Language.Parser]::ParseFile(..., [ref]$null, [ref]$errors)"`.
4. **Agents** — write `.codex/agents/<name>.toml`.
5. **`.codex/config.toml`** — append `[hooks]`, `[permissions.*]`, `[mcp_servers.*]`, `sandbox_mode`, `approval_policy`, and `[agents]` tables with targeted edits. Never rewrite the whole file.
6. **CI backstops** — for every `DEGRADED` SubagentStop gate, add the required GitHub Actions job that runs the equivalent validation and fails the merge.
7. **Evidence** — write `artifacts/translation/<timestamp>/diff.md` and copy every new or modified target file under `artifacts/translation/<timestamp>/snapshots/`.

After apply, run a verification sweep:

- TOML-parse `.codex/config.toml` to confirm validity.
- Parse-check every `.codex/hooks/*.ps1`.
- Confirm each new skill `SKILL.md` has valid `name` and `description` frontmatter by reading back the first five lines.
- Confirm each `.codex/agents/*.toml` carries `name`, `description`, and `developer_instructions`.
- Re-read the Enforceability Preservation Ledger and confirm no row remains `LOST`.
- Report the final mapping table with `done` / `skipped` / `conflict-unresolved` statuses.

State that hook trust must be granted once via the Codex `/hooks` command, or bypassed in CI with `--dangerously-bypass-hook-trust`. State that the project must be trusted in user-level `~/.codex/config.toml` for `.codex/` layers to load.

## Phase 7 — Reporting

Every completion response must include:

1. **Inputs** — source files.
2. **Mapping summary** — counts by Codex target kind (instruction, skill, agent, hook, permission, MCP).
3. **Action summary** — counts by action (add, replace, merge, skip, conflict).
4. **Enforceability summary** — counts by ledger status (PRESERVED, DEGRADED, LOST) with the compensating control for each non-preserved row.
5. **Files changed** — explicit created and modified paths.
6. **Config delta** — hooks, permissions, MCP servers added; trust precondition.
7. **CI backstops** — workflow jobs added for degraded gates.
8. **Evidence paths** — artifact locations.
9. **Conflicts** — unresolved conflict rows, or "none".

## Guarantees and Prohibitions

- **Enforceability accounting**: no mechanically-enforced Claude gate is dropped without an explicit ledger row and a stated status. A `LOST` status blocks apply.
- **Idempotency**: running the skill twice on the same input produces no new changes beyond the regenerated plan artifact.
- **No source deletion**: `.claude/` files are never modified or removed.
- **No silent overwrites**: existing Codex content is merged by default. Replace requires user instruction or a resolved `conflict` row.
- **No hook-based state mutation**: generated hooks are validation-only unless the user authorizes otherwise.
- **No `config.toml` rewrites**: only targeted edits that append tables.
- **No `notify` as a gate**: the Codex `notify` program is fire-and-forget and cannot block. Use `PreToolUse` / `PermissionRequest` hooks for blocking, never `notify`.
- **Trust dependency disclosed**: every target requiring `trust_level = "trusted"` is recorded; the skill never assumes silent trust.

## Classification Quick Reference

| Claude source | Default Codex target | Enforceability note |
|---|---|---|
| `CLAUDE.md` | repo-root `AGENTS.md` | prompt-level both sides; positional precedence |
| `.claude/rules/*.md` (`paths:` glob) | `AGENTS.md` (+ per-directory) | no glob equivalent; pair enforceable rules with hooks |
| `.claude/skills/<name>/SKILL.md` | `.agents/skills/<name>/SKILL.md` | drop `allowed-tools`/`context`; re-ground in sandbox/MCP/hook |
| `.claude/agents/<name>.md` | `.codex/agents/<name>.toml` | model + sandbox OS-enforced; tools allowlist re-grounded |
| `.claude/settings.json` `permissions` | `[permissions.<name>]` in `config.toml` | OS-enforced via sandbox; more expressive than Claude |
| `.claude/hooks/*.ps1` (PreToolUse) | `[hooks] PreToolUse` + script | direct equivalent; exit 2 / permissionDecision deny |
| `.claude/hooks/*.ps1` (SubagentStop, exit 1) | `[hooks] SubagentStop` + required CI check | degraded; CI status check is the hard gate |
| `permissions.deny` (secrets) | `[permissions.<name>.filesystem]` deny | OS-enforced |

## Invocation Examples

- Translate the settings enforcement surface, plan only:
  `translate-claude-to-codex .claude/settings.json`
- Translate a hook with its registration and apply:
  `translate-claude-to-codex .claude/hooks/validate-bash.ps1 .claude/settings.json mode=apply`
- Translate an agent persona bundle (persona + its rules + its hooks):
  `translate-claude-to-codex .claude/agents/python-typed-engineer.md .claude/rules/python.md .claude/hooks/enforce-python-batch-budget.ps1 target-scope=all`
- Translate the full runtime:
  `translate-claude-to-codex CLAUDE.md .claude/rules .claude/skills .claude/agents .claude/hooks .claude/settings.json mode=plan-only`
