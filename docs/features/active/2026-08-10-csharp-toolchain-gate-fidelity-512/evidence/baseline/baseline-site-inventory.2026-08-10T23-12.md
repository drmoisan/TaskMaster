# Baseline — pre-change divergent-site inventory ([P0-T17], the AC6 before-state)

Timestamp: 2026-08-10T23-12
Command: three `git grep` invocations, quoted exactly below
EXIT_CODE: 0 (all three)

## Exact grep commands

Pattern (a) — the CSharpier v0 bare-path form:

```
git grep -n -E 'csharpier[[:space:]]+\.' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
```

Pattern (b) — the same-line conjunction of `/t:Build` and `Nullable=enable`:

```
git grep -n -E '(/t:Build.*Nullable=enable|Nullable=enable.*/t:Build)' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
```

Pattern (c) — any occurrence of the bare token `Nullable=enable` on any line, regardless of what
else is on that line:

```
git grep -n -F 'Nullable=enable' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
```

`git grep` is used rather than a filesystem grep because it searches **tracked files only**.
Untracked build logs under `coverage/` contain the defective command lines verbatim and must not
enter the inventory.

## Exclusion list and its rationale

| Excluded path | Rationale |
|---|---|
| `docs/features/**` | Historical evidence, specs and plans. These **record a past measurement**; they do not document a command an agent is instructed to run. Correcting them would rewrite the audit trail. |
| `docs/research/**` | Same rationale: research artifacts quote the defective commands as findings. |
| `.claude/agent-memory/**` | Agent memory records what was measured at a point in time; it is owned by the agents that wrote it and is corrected by them, not here (`spec.md` § Rollout & Follow-up item 3). |
| `packages/**` | Restored third-party NuGet content; not repository source. |
| `.dotnet-sdk/**` | Bootstrapped toolchain payload; not repository source. |
| `**/bin/**`, `**/obj/**` | Build outputs. |

`packages/`, `.dotnet-sdk/`, `bin/` and `obj/` are additionally untracked, so `git grep` would not
reach them regardless; the pathspecs are retained for parity with [P5-T11].

---

## Pattern (a) — `csharpier\s+\.` — 16 hits

### In-scope sites (corrected by Phase 3 / Phase 4) — 6

| # | path:line | Corrected by |
|---|---|---|
| 1 | `CLAUDE.md:191` | [P3-T1] (Block R1) |
| 2 | `CLAUDE.md:192` | [P3-T1] (Block R1) |
| 3 | `CLAUDE.md:381` | [P3-T4] (row 6) |
| 4 | `CLAUDE.md:399` | [P3-T5] (row 9) |
| 5 | `.claude/rules/csharp.md:14` | [P4-T1] (row 12) |
| 6 | `.claude/skills/csharp-qa-gate/SKILL.md:30` | [P4-T5] (row 16) |

### SD1-excluded mirror sites — **10**

| # | path:line |
|---|---|
| 1 | `AGENTS.md:469` |
| 2 | `AGENTS.md:470` |
| 3 | `AGENTS.md:660` |
| 4 | `.agents/skills/csharp/SKILL.md:17` |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:32` |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:172` |
| 7 | `.github/agents/csharp-atomic-executor.agent.md:258` |
| 8 | `.github/instructions/csharp-code-change.instructions.md:32` |
| 9 | `.github/instructions/csharp-code-change.instructions.md:33` |
| 10 | `.github/instructions/csharp-unit-test.instructions.md:45` |

**SD1 hit count for pattern (a): 10.**

---

## Pattern (b) — same-line `/t:Build` AND `Nullable=enable` — 14 hits

### In-scope sites (corrected by Phase 3 / Phase 4) — 5

| # | path:line | Corrected by |
|---|---|---|
| 1 | `CLAUDE.md:206` | [P3-T3] (Block R3) |
| 2 | `CLAUDE.md:383` | [P3-T4] (row 8) |
| 3 | `CLAUDE.md:401` | [P3-T5] (row 11) |
| 4 | `.claude/rules/csharp.md:16` | [P4-T3] (row 14) |
| 5 | `.claude/skills/csharp-qa-gate/SKILL.md:32` | [P4-T5] (row 18) |

### SD1-excluded mirror sites — **9**

| # | path:line |
|---|---|
| 1 | `AGENTS.md:487` |
| 2 | `AGENTS.md:488` |
| 3 | `AGENTS.md:662` |
| 4 | `.agents/skills/csharp/SKILL.md:19` |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:34` |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:174` |
| 7 | `.github/instructions/csharp-code-change.instructions.md:50` |
| 8 | `.github/instructions/csharp-code-change.instructions.md:51` |
| 9 | `.github/instructions/csharp-unit-test.instructions.md:47` |

**SD1 hit count for pattern (b): 9.**

---

## Pattern (c) — bare token `Nullable=enable`, any line — 19 hits

### In-scope sites corrected by Phase 1 / 2 / 3 / 4 — 8

| # | path:line | Corrected by |
|---|---|---|
| 1 | `CLAUDE.md:206` | [P3-T3] (Block R3) |
| 2 | `CLAUDE.md:383` | [P3-T4] (row 8) |
| 3 | `CLAUDE.md:401` | [P3-T5] (row 11) |
| 4 | `.claude/rules/csharp.md:16` | [P4-T3] (row 14) |
| 5 | `.claude/rules/csharp.md:83` | [P4-T4] (row 15) |
| 6 | `.claude/skills/csharp-qa-gate/SKILL.md:32` | [P4-T5] (row 18) |
| 7 | `scripts/vscode/Invoke-VSBuild.ps1:107` | [P2-T3] (row 23) |
| 8 | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1:60` | [P1-T2] (row 28) |

### SD1-excluded mirror sites — **10**

| # | path:line |
|---|---|
| 1 | `AGENTS.md:487` |
| 2 | `AGENTS.md:488` |
| 3 | `AGENTS.md:662` |
| 4 | `.agents/skills/csharp/SKILL.md:19` |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:34` |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:174` |
| 7 | `.github/agents/csharp-atomic-executor.agent.md:260` |
| 8 | `.github/instructions/csharp-code-change.instructions.md:50` |
| 9 | `.github/instructions/csharp-code-change.instructions.md:51` |
| 10 | `.github/instructions/csharp-unit-test.instructions.md:47` |

**SD1 hit count for pattern (c): 10.**

Note that `.github/agents/csharp-atomic-executor.agent.md:260` appears in pattern (c) but **not** in
pattern (b), because it spells the flag as `dotnet build -p:Nullable=enable` with no `/t:Build`.

### Permitted residuals after the fix — three classes

**Class (i) — R3 / R5 prohibition prose introduced by this feature.** Text of the form
"Do not add `/p:Nullable=enable`", introduced at `CLAUDE.md` § C#1 item 3 (Block R3) and
`.claude/rules/csharp.md` item 3 (sentence R5). These lines name the flag **in order to prohibit
it**. They did not exist at [P0-T17] time and will be enumerated at [P5-T11] time.

**Class (ii) — the [P2-T3] deprecation `Write-Warning` text** in `scripts/vscode/Invoke-VSBuild.ps1`.
Its message names the flag in order to explain why the switch is inert. It did not exist at
[P0-T17] time.

**Class (iii) — the pre-existing code comment at `TaskMaster/Ribbon/EngineCommandCatalog.cs:93`.**
Present **before** this feature. Exact text as read at [P0-T17] time:

```
                // returns false. This keeps the file clean under /p:Nullable=enable.
```

`git grep` line: `TaskMaster/Ribbon/EngineCommandCatalog.cs:93:                // returns false. This keeps the file clean under /p:Nullable=enable.`

This is a `*.cs` file, which this feature's scope limitation forbids editing, and the comment
documents a null-forgiving annotation rather than a toolchain command. Recording its exact text here
proves at [P5-T11] time that this feature neither introduced nor modified it. It is folded into the
[P7-T1] follow-up entry.

---

## Before-state counts — the comparison basis for the [P5-T11] gate

| Pattern | Total hits | In-scope (to be corrected) | SD1-excluded (must be unchanged) | Pre-existing residual |
|---|---|---|---|---|
| (a) `csharpier\s+\.` | 16 | 6 | **10** | 0 |
| (b) same-line `/t:Build` + `Nullable=enable` | 14 | 5 | **9** | 0 |
| (c) bare `Nullable=enable` | 19 | 8 | **10** | 1 (`EngineCommandCatalog.cs:93`) |

At [P5-T11] the three SD1 counts must be **identical** (10 / 9 / 10), proving this feature neither
corrected nor introduced a mirror site. Any hit outside the SD1 allowlist, for any of the three
patterns, that is not a permitted class (i), (ii) or (iii) residual is a half-corrected site
(`spec.md` option (b), rejected) and is a gate failure, not a residual.

## Output Summary

Three tracked-file greps recorded 16 / 14 / 19 hits for patterns (a) / (b) / (c). Partitioned: 6 / 5
/ 8 in-scope sites that Phases 1-4 correct; **10 / 9 / 10** SD1-excluded mirror sites that must be
byte-unchanged; and one pre-existing pattern-(c) residual at
`TaskMaster/Ribbon/EngineCommandCatalog.cs:93`, whose exact text is recorded above so its
pre-existence is provable at [P5-T11]. These counts are the AC6 before-state.
