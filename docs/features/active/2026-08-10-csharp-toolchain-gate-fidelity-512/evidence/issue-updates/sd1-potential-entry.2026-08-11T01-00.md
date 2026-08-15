# SD1 follow-up — potential-bug entry filed ([P7-T1])

Timestamp: 2026-08-11T01-00
Command: (none — analysis artifact; the entry was written by hand after both MCP routes were unavailable)
EXIT_CODE: (none — analysis artifact)

## Created file path

`docs/features/potential/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md`

## Route used, and why

`MCP_TOOL_UNAVAILABLE: new_potential_bug_entry`

The plan's preferred route is `mcp__drm-copilot__new_potential_bug_entry`. That function is **not in
the `atomic-executor` toolset**, which exposes only the four PoshQC functions
(`run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test`, `run_poshqc_analyze_autofix`). The
second route is escalation to the orchestrator, which does hold the promotion MCP tools; an executor
subagent cannot invoke the orchestrator mid-run, so that route is also unavailable from here.

**Route actually used: hand-written entry**, in the same shape as the existing entries under
`docs/features/potential/promoted/` (verified against
`docs/features/potential/promoted/2026-08-08-csharpier-documented-command-incompatible-with-pinned-version.md`,
which is the #509 entry from the same defect family), with the section headings the promotion tooling
maps into the GitHub bug-report template left unchanged.

**Escalation recorded for the orchestrator:** the orchestrator holds
`mcp__drm-copilot__potential_to_issue` and should promote the entry above to a GitHub issue, then
move the file under `docs/features/potential/promoted/`. See
`FEATURE/evidence/issue-updates/sd1-followup-issue.2026-08-11T01-02.md` ([P7-T2]).

## Acceptance checklist

| Requirement | Satisfied |
|---|---|
| Enumerates the eight mirror paths with line numbers from the SD1 table in `spec.md` | **yes** — `AGENTS.md` (466, 469, 470, 487, 488, 660, 662); `.github/instructions/csharp-code-change.instructions.md` (29, 32, 33, 50, 51); `.github/instructions/csharp-unit-test.instructions.md` (45, 47); `.agents/skills/csharp/SKILL.md` (17, 19); `.agents/skills/csharp-qa-gate/SKILL.md` (32, 34); `.github/agents/csharp-typed-engineer.agent.md` (172, 174); `.github/agents/csharp-atomic-executor.agent.md` (258, 260); `.codex/codex-web-setup.sh` (342) |
| Names `drm-copilot` as owner of `.agents/`, `.codex/` and `.github/agents/` | **yes** — § Suspected Cause / Notes, ground 3 |
| States that `.github/instructions/` needs its own authorization grant | **yes** — § Suspected Cause / Notes, ground 1, in bold |
| Records that the generator `scripts/dev-tools/sync-agents-from-instructions.ps1` named by `AGENTS.md` does not exist | **yes** — ground 2, with the observed contents of `scripts/dev-tools/` |
| Includes the `.csharpierignore` comment residual | **yes** — § Additional residuals |
| Includes the `TaskMaster/Ribbon/EngineCommandCatalog.cs:93` comment residual, noting it cites `/p:Nullable=enable` as the enforcing gate (false after this feature) and cannot be corrected here because the feature makes no `*.cs` change | **yes** — § Additional residuals |
| Includes the `.claude/rules/powershell.md:18` residual (cites `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, which does not exist in this checkout) | **yes** — § Additional residuals, with the verification commands |
| Folds in the `PREEXISTING_COVERAGE_SHORTFALL:` figure from [P0-T16] via [P6-T4] | **not applicable** — [P0-T16] measured 85.71% line coverage, at or above the 85% floor, so **no** `PREEXISTING_COVERAGE_SHORTFALL:` marker was recorded and [P6-T4] had nothing to fold in. The entry states this explicitly rather than omitting it silently. |
| Records which route was used | **yes** — above |

## Output Summary

The SD1 potential-bug entry was created at
`docs/features/potential/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md`.
`mcp__drm-copilot__new_potential_bug_entry` is not in the executor's toolset
(`MCP_TOOL_UNAVAILABLE`) and the orchestrator route is not reachable from a subagent, so the entry
was hand-written in the shape the promotion tooling expects. It enumerates all eight mirror paths
with line numbers, names `drm-copilot` as owner of `.agents/`, `.codex/` and `.github/agents/`,
states that `.github/instructions/` needs its own authorization grant, records the missing
`AGENTS.md` generator, and folds in the `.csharpierignore`, `EngineCommandCatalog.cs:93` and
`.claude/rules/powershell.md:18` residuals. No coverage shortfall existed to fold in.
