# AC6 repository-wide site-inventory reconciliation ([P5-T11])

Timestamp: 2026-08-11T00-18
Command: the three `git grep` invocations from [P0-T17], reproduced verbatim below
EXIT_CODE: 0 (all three)

Before-state: `FEATURE/evidence/baseline/baseline-site-inventory.2026-08-10T23-12.md`.

`SD1FollowUpIssue: #535 — https://github.com/drmoisan/TaskMaster/issues/535` (back-filled in place by [P7-T8]; was `pending` when this artifact was written at [P5-T11], because [P7-T2] had not yet run)

## The three commands (identical patterns and exclusions to [P0-T17])

```
git grep -n -E 'csharpier[[:space:]]+\.' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
git grep -n -E '(/t:Build.*Nullable=enable|Nullable=enable.*/t:Build)' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
git grep -n -F 'Nullable=enable' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory' ':!packages' ':!.dotnet-sdk' ':!**/bin/**' ':!**/obj/**'
```

`git grep` restricts the search to **tracked files**, exactly as in [P0-T17]. Untracked build logs
under `coverage/` contain the defective command lines verbatim and are correctly excluded.

---

## Pattern (a) — `csharpier\s+\.` — 10 hits, **all inside the SD1 allowlist**

**No hit outside the SD1 allowlist.** All 6 in-scope sites are corrected.

| # | path:line | In [P0-T17] SD1 list at the same line? |
|---|---|---|
| 1 | `AGENTS.md:469` | yes |
| 2 | `AGENTS.md:470` | yes |
| 3 | `AGENTS.md:660` | yes |
| 4 | `.agents/skills/csharp/SKILL.md:17` | yes |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:32` | yes |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:172` | yes |
| 7 | `.github/agents/csharp-atomic-executor.agent.md:258` | yes |
| 8 | `.github/instructions/csharp-code-change.instructions.md:32` | yes |
| 9 | `.github/instructions/csharp-code-change.instructions.md:33` | yes |
| 10 | `.github/instructions/csharp-unit-test.instructions.md:45` | yes |

| | Before ([P0-T17]) | After (this run) | Identical |
|---|---|---|---|
| SD1 hit count | **10** | **10** | **YES** |

Corrected and now absent: `CLAUDE.md:191`, `CLAUDE.md:192`, `CLAUDE.md:381`, `CLAUDE.md:399`,
`.claude/rules/csharp.md:14`, `.claude/skills/csharp-qa-gate/SKILL.md:30`.

---

## Pattern (b) — same-line `/t:Build` AND `Nullable=enable` — 9 hits, **all inside the SD1 allowlist**

**No hit outside the SD1 allowlist.** All 5 in-scope sites are corrected.

| # | path:line | In [P0-T17] SD1 list at the same line? |
|---|---|---|
| 1 | `AGENTS.md:487` | yes |
| 2 | `AGENTS.md:488` | yes |
| 3 | `AGENTS.md:662` | yes |
| 4 | `.agents/skills/csharp/SKILL.md:19` | yes |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:34` | yes |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:174` | yes |
| 7 | `.github/instructions/csharp-code-change.instructions.md:50` | yes |
| 8 | `.github/instructions/csharp-code-change.instructions.md:51` | yes |
| 9 | `.github/instructions/csharp-unit-test.instructions.md:47` | yes |

| | Before ([P0-T17]) | After (this run) | Identical |
|---|---|---|---|
| SD1 hit count | **9** | **9** | **YES** |

Corrected and now absent: `CLAUDE.md:206`, `CLAUDE.md:383`, `CLAUDE.md:401`,
`.claude/rules/csharp.md:16`, `.claude/skills/csharp-qa-gate/SKILL.md:32`.

The R3 and R5 prohibition prose was deliberately line-wrapped so the `/p:Nullable=enable` clause and
the `/t:Build` clause never share a physical line, keeping this same-line gate satisfiable without
weakening it.

---

## Pattern (c) — bare `Nullable=enable` — 14 hits: 10 SD1 + 4 permitted residuals

### SD1-excluded mirror sites — 10

| # | path:line | In [P0-T17] SD1 list at the same line? |
|---|---|---|
| 1 | `AGENTS.md:487` | yes |
| 2 | `AGENTS.md:488` | yes |
| 3 | `AGENTS.md:662` | yes |
| 4 | `.agents/skills/csharp/SKILL.md:19` | yes |
| 5 | `.agents/skills/csharp-qa-gate/SKILL.md:34` | yes |
| 6 | `.github/agents/csharp-typed-engineer.agent.md:174` | yes |
| 7 | `.github/agents/csharp-atomic-executor.agent.md:260` | yes |
| 8 | `.github/instructions/csharp-code-change.instructions.md:50` | yes |
| 9 | `.github/instructions/csharp-code-change.instructions.md:51` | yes |
| 10 | `.github/instructions/csharp-unit-test.instructions.md:47` | yes |

| | Before ([P0-T17]) | After (this run) | Identical |
|---|---|---|---|
| SD1 hit count | **10** | **10** | **YES** |

### Permitted residuals — 4, each matched against the [P0-T17] before-state

**Class (i) — R3 / R5 prohibition prose introduced by this feature (2 hits).** These name the flag in
order to prohibit it.

| path:line | Text | Introduced by |
|---|---|---|
| `CLAUDE.md:211` | ``- **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element ...`` | [P3-T3] (Block R3) |
| `.claude/rules/csharp.md:19` | ``- This is `ci.yml`'s command verbatim. Do not add `/p:Nullable=enable` (no project carries a`` | [P4-T3] (sentence R5) |

Neither existed at [P0-T17] time (the before-state lists no in-scope hit at either line), so both are
new residuals of class (i), as [P0-T17] anticipated.

**Class (ii) — the [P2-T3] deprecation warning text (1 hit).**

| path:line | Text |
|---|---|
| `scripts/vscode/Invoke-VSBuild.ps1:117` | ``Write-Warning 'The -EnableNullable switch is deprecated and has no effect. ... /p:Nullable=enable is deliberately absent from CI and makes the gate unpassable. See CLAUDE.md C#1 item 3.'`` |

The message names the flag in order to explain why the switch is inert. It did not exist at
[P0-T17] time; the before-state records `scripts/vscode/Invoke-VSBuild.ps1:107` as an **in-scope
site** (`$properties += 'Nullable=enable'`), which is now corrected. [P5-T10] proves the flag is not
emitted (`nullable+` occurrences in the `csc.exe` command lines: **0**).

**Class (iii) — the pre-existing comment at `TaskMaster/Ribbon/EngineCommandCatalog.cs:93` (1 hit).**

| Check | Result |
|---|---|
| Text at working-tree line 93 | `                // returns false. This keeps the file clean under /p:Nullable=enable.` |
| Text at `<MERGE_BASE>` line 93 (`git show <MERGE_BASE>:TaskMaster/Ribbon/EngineCommandCatalog.cs \| sed -n '93p'`) | `                // returns false. This keeps the file clean under /p:Nullable=enable.` |
| `git diff --stat <MERGE_BASE> -- TaskMaster/Ribbon/EngineCommandCatalog.cs` | **(empty)** |
| Same text recorded at [P0-T17] time? | **yes**, verbatim |

**Byte-identical to its merge-base text.** This feature neither introduced nor modified it. It is a
`*.cs` file, which this feature's scope limitation forbids editing, and the comment documents a
null-forgiving annotation rather than a toolchain command. It is folded into the [P7-T1] follow-up
entry.

### No half-corrected sites

Every pattern-(c) hit outside the SD1 allowlist belongs to residual class (i), (ii) or (iii). There
is **no** hit that is neither an SD1 mirror nor a permitted residual, so there is no half-corrected
site (`spec.md` option (b), rejected) and no gate failure.

---

## Corroboration — the complete changed-file set

`git diff --name-only <MERGE_BASE>`:

```
.claude/rules/csharp.md
.claude/skills/csharp-qa-gate/SKILL.md
.vscode/tasks.json
CLAUDE.md
docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md
scripts/vscode/Invoke-VSBuild.ps1
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
```

No SD1-allowlisted path appears, which independently confirms this feature neither corrected nor
introduced a mirror site. (Untracked evidence artifacts under the feature folder are not listed by
`--name-only` against the merge base; they are new files.)

## SD1 residual allowlist with rationale

The mirror tree is deliberately left unchanged under SD1, which AC6's final sentence permits provided
the sites are enumerated with rationale:

1. `.github/instructions/**` sits under the **unsuspended** `policy-compliance-order` hard constraint
   ("Do NOT modify policy documents under `.claude/rules/` or `.github/instructions/`"); the epic's
   authorization names only `CLAUDE.md` and `.claude/rules/csharp.md` for this child.
2. `AGENTS.md` declares itself generated from seventeen `.github/**` sources, forbids manual editing,
   and names a regeneration script (`scripts/dev-tools/sync-agents-from-instructions.ps1`) that does
   not exist in this checkout.
3. `.agents/**` and `.codex/**` are inbound Codex push-down artifacts owned by `drm-copilot`; an edit
   here is overwritten by the next push-down.
4. `.github/agents/**` is excluded on ground 3 only.

`SD1FollowUpIssue: #535 — https://github.com/drmoisan/TaskMaster/issues/535` (back-filled in place by [P7-T8]; was `pending` when this artifact was written at [P5-T11], because [P7-T2] had not yet run)

## Output Summary

Pattern (a): **10** hits, all SD1, SD1 count identical to the before-state (10), **zero** hits
outside the allowlist. Pattern (b): **9** hits, all SD1, SD1 count identical to the before-state (9),
**zero** hits outside the allowlist. Pattern (c): **14** hits — 10 SD1 (count identical to the
before-state) plus exactly the four anticipated residuals: two class-(i) prohibition-prose lines
introduced by R3/R5, one class-(ii) deprecation-warning line introduced by [P2-T3], and one
class-(iii) pre-existing `*.cs` comment proven byte-identical to its merge-base text. No
half-corrected site exists. AC6 is satisfied.
