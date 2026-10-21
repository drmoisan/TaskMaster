# 2026-08-04-stale-fizzler-and-unsafe-binding-redirects (Plan)

- **Issue:** #953
- **Parent (optional):** none
- **Owner:** drmoisan
- **Branch:** bug/stale-fizzler-and-unsafe-binding-redirects-953
- **Last Updated:** 2026-10-02T03-20
- **Status:** Draft (awaiting validator and executor preflight; revision rounds 1 to 3 applied)
- **Version:** 1.3
- **Work Mode:** minor-audit (issue.md line 9)
- **Task Count:** 47 (Phase 0: 13, Phase 1: 17, Phase 2: 17), counted mechanically over lines matching the task pattern

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS. For this plan the coverage figure is read by the CI Pester job and not locally (Decision D8); the reduced-audit handoff must carry that deferral explicitly, and a handoff that omits it is INCOMPLETE.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

## 1. Objective and scope

Sweep the eleven stale Fizzler `bindingRedirect` entries to `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`, add a Pester-tested detector that reports a `bindingRedirect` whose `newVersion` equals no csproj `Reference` version for its assembly name, and add a repository-level ratchet test that pins the remaining known-debt set so the next drift fails the suite. The `System.Runtime.CompilerServices.Unsafe` half of the issue is already delivered (issue 929, PR 949) and is verified, not edited.

Requirements source: `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md`, section `## Acceptance Criteria`, items AC1 to AC6 (lines 84 to 89). There is no spec.md or user-story.md and none is created; the minor-audit mode fails closed if either appears (Phase 0 task P0-T3).

This plan is executed in a later run by `atomic-executor`. The planner executed nothing. Every repository-relative path below is relative to the execution worktree root, written in evidence as `<execution-worktree-root>`; absolute host paths are never transcribed into evidence.

## 2. Inputs read by the planner (2026-10-02)

CLAUDE.md; `.claude/rules/powershell.md`; `.claude/rules/plan-acceptance-gates.md`; `.claude/rules/tonality.md`; `.claude/skills/atomic-plan-contract/SKILL.md`; `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; `.claude/skills/acceptance-criteria-tracking/SKILL.md`; the feature issue.md and research artifact `research/2026-10-02T00-35-fizzler-redirect-remedy-and-redirect-gate-research.md`; `scripts/dependencies/PackageGraph.psm1`; `scripts/dependencies/ProjectConsistency.psm1`; `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`; `tests/scripts/dependencies/PackageGraph.Tests.ps1`; `.github/workflows/_pester.yml`; `.gitattributes`; `.gitignore`; `.claude/hooks/enforce-powershell-batch-budget.ps1`; the issue 929 evidence artifacts that recorded the PoshQC tools' success-case output and a redirect edit of the same class (`docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/`).

## 3. Design decisions (made by the orchestrator; recorded, not reopened)

- **D1 — Remedy is a sweep, not a removal.** The eleven stale blocks become `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`, the shape the two correct files (SVGControl, UtilitiesCS) already carry. No Fizzler block is removed. The two correct Fizzler files and all seventeen Unsafe redirects (every one at 6.0.3.0) are not edited.
- **D2 — Edit mechanism: the Edit tool with a two-line old_string.** The string `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` occurs 17 times across the 11 files: once on the Fizzler block in every one of them and once more on the System.ClientModel block in six of them (QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel). A single-line replacement is therefore wrong in six files and not unique for the Edit tool. The edit uses the `assemblyIdentity name="Fizzler"` line plus the following `bindingRedirect` line as one two-line old_string, which is unique in every file. `Invoke-BindingRedirectReconciliation` (ProjectConsistency.psm1 lines 271 to 375) was considered and rejected for the write: it is pure over text, so applying it requires a PowerShell read and write round trip in which `ReadAllText` discards the UTF-8 BOM and the write must re-add it with an explicit encoding, adding an encoding step whose correctness would itself need a gate, and a PowerShell-tool file write bypasses the PreToolUse hook chain the Edit tool passes through. If the Edit tool reports that the old_string was not found in a file, the executor records the tool message and stops (STOP: EDIT-MISMATCH); it does not rewrite the file by any other mechanism.
- **D3 — Encoding gates are worktree observations, not only git diffs.** `.gitattributes` line 4 sets `* text=auto`, so git normalises line terminators when it diffs the working tree; a rewrite that converted CRLF to LF could be invisible to `git diff`. Line-ending preservation is therefore observed with `git ls-files --eol`, whose `w/` field (the second whitespace-separated field of the output line; CMD-EOL) reports the working-tree file's own terminators (`w/crlf` expected; `w/lf` or `w/mixed` is a failure). BOM preservation is observed two ways: `git diff --numstat <BASE_SHA> -- <path>` reading exactly `1	1	<path>` (a lost BOM alters line 1 and would read `2	2`), and a byte read of the first three bytes (CMD-BOM, expected `239,187,191`), compared before and after the edit. ripgrep strips a UTF-8 BOM before matching, so the Grep tool cannot observe it; the planner's zero-match Grep for the BOM bytes is inconclusive by construction and the directive's BOM claim is confirmed at P0-T7 by CMD-BOM.
- **D4 — Gate design: new module `scripts/dependencies/BindingRedirectVerification.psm1`.** It imports `PackageGraph.psm1` (parser) and exports two pure functions: `ConvertTo-ReferenceVersionMap` (csproj texts to a name-to-versions map) and `Find-StaleBindingRedirect` (app.config text plus an injected `-DeployedVersionProvider` scriptblock to findings, an unverifiable-name list and an examined-entry count). `ConsistencyVerifier.psm1` is at 499 lines and cannot host it; `ProjectConsistency.psm1` (381 lines) documents a reconciliation-only ownership split in its header (lines 6 to 12). The provider seam is the injectable-delegate seam the PowerShell rule permits when a wrapper is insufficient; it lets every unit test run on in-memory fixtures with no temporary file and no `$TestDrive`.
- **D5 — Source of truth is the global set of csproj `Reference Include="Name, Version=..."` values.** Membership is by assembly name across every csproj, not per project: 535 of the 1176 redirect entries are for assemblies the config's own csproj does not reference, so a per-project check would report hundreds of false findings. packages.config versions are not used (Unsafe package 6.1.2 versus assembly 6.0.3.0; Svg 3.4.8 versus 3.4.0.0). Assembly metadata under packages/ is not usable because the CI Pester job (`.github/workflows/_pester.yml`) performs no NuGet restore and the worktree carries no packages tree. A name with no csproj Reference anywhere is unverifiable, listed separately, and not a finding.
- **D6 — Tests live in one new file, `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`.** `RepositoryTreeConsistency.Tests.ps1` is not modified: the session PowerShell file budget is shared and believed full, and the new file holds both the unit tests and the two repository-level tests. The existing SVGControl-only redirect test (RepositoryTreeConsistency.Tests.ps1 lines 92 to 110) keeps passing after the sweep because SVGControl is not edited.
- **D7 — Ratchet on (assembly name, newVersion) pairs; re-measure in Phase 0.** The repository-level test asserts that the sorted distinct set of `AssemblyName|NewVersion` findings equals exactly the fifteen recorded known-debt pairs (section 7) and that the sorted distinct unverifiable names equal exactly the recorded three. It fails on a new mismatch, on a stale known-debt entry and on any Fizzler regression. It does not key on per-config counts. P0-T8 re-measures every row by Grep; if any row, total or set differs from section 7 the executor records `KNOWN-DEBT-DRIFT:` with the differing rows and stops, because the table would then describe a tree the orchestrator has not measured; the executor never edits the table or the test literal on its own authority. Correcting the fifteen pairs is out of scope; P2-T16 records the follow-up for the coordinator to promote. The examined-count guard compares the detector's total to an independent count of `<bindingRedirect` elements over the same texts (1176 at planning time) and requires that count to exceed zero, so a parser that silently examined nothing cannot pass.
- **D8 — PowerShell toolchain through the PoshQC MCP tools; coverage read by CI, not locally.** Format, analyze and test run through `mcp__drm-copilot__run_poshqc_format`, `mcp__drm-copilot__run_poshqc_analyze` and `mcp__drm-copilot__run_poshqc_test` with `workspace_root` set to the worktree root and `scan_folders` set explicitly (no `config/poshqc-scan.json` exists). The tools return `{ok, tool, workspace_root, summary}` only; counts come from `artifacts/pester/pester-junit.xml` (CMD-JUNIT-READ). No raw `Invoke-Pester` or `Invoke-Formatter` command appears in this plan, and no `pwsh -Command` payload in this plan contains a double quote. Pester line coverage for `scripts/dependencies` is produced by the CI Pester job (`_pester.yml` line 45 measures the folder, line 71 fails below 80 percent; the repository rule is 85), and the MCP test route instruments none of `scripts/dependencies`, so no locally measured coverage percentage is asserted anywhere in this plan. AC6 as written states the same ("is not measured locally"). Each test artifact carries the literal line `COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure`. The atomic-plan-contract clause requiring numeric baseline coverage is discharged by that deferral under AC6; the reduced-audit handoff (P2-T17) carries `COVERAGE-SOURCE: CI` so the reviewer reads the CI job.
- **D9 — Budget.** `.claude/hooks/enforce-powershell-batch-budget.ps1` caps a session at 3 distinct production and 3 distinct test PowerShell paths and denies the write that would exceed a cap. The new .psm1 needs one production slot and the new .Tests.ps1 one test slot. If any Write or Edit to a PowerShell path is denied, the executor records `BUDGET-DENIED:` with the hook message and stops; it does not delete or edit the state file under `.claude/state/`, does not set `CLAUDE_POWERSHELL_BUDGET_PROD` or `CLAUDE_POWERSHELL_BUDGET_TEST`, and does not split the batch on its own authority. Fallback, applicable only after the orchestrator rules on the denial and only if `scripts/dependencies/ProjectConsistency.psm1` is already a counted path in the session (a repeat write to a counted path consumes no slot): append both functions to ProjectConsistency.psm1, update its header ownership statement, keep the module under 500 lines (381 plus about 120), and import ProjectConsistency.psm1 in the test file. Consequences: the Write Set replaces the new module with ProjectConsistency.psm1, the header's one-owner split is broken and must be documented, and the new test file is unchanged in name. The fallback is not the primary path and is not executed without the orchestrator's ruling.
- **D10 — No commits in this plan.** The orchestrator commits after the reduced audit. Every git gate is therefore a two-dot comparison of the working tree against the recorded BASE_SHA (`git diff ... <BASE_SHA> -- ...`) paired with `git status --porcelain --untracked-files=all`; neither can pass vacuously while the changes are uncommitted, and the pairing covers the new (untracked) files that a name-listing diff cannot see.
- **D11 — No C# toolchain.** No `*.cs`, `*.csproj`, `*.sln`, `*.props`, `*.targets` or packages.config file is touched. `.csharpierignore` excludes `**/app.config`. CSharpier, msbuild analyzer and nullable builds and the MSTest coverage run do not apply; P2-T7 proves the scope by pathspec.
- **D12 — Evidence locations.** Every artifact is written under `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/<kind>/` with kinds baseline, regression-testing, qa-gates and other. No `artifacts/` path is an evidence location; `artifacts/pester/` is a read source only (gitignored at `.gitignore` line 57) and `artifacts/orchestration/orchestrator-state.json` is untracked and outside the footprint.

## 4. Tree facts verified by the planner (re-derived 2026-10-02 in this worktree)

| Fact | Evidence | Used by |
|---|---|---|
| 17 app.config files at root level; 1176 `assemblyIdentity name=` entries and 1176 `<bindingRedirect` elements; 18 csproj at `*/*.csproj` | Glob and Grep count mode | P0-T8, test 14 |
| Fizzler redirect blocks: 13. Stale (`newVersion="1.3.0.0"`) at QuickFiler/app.config 50-51, QuickFiler.Test/app.config 46-47, SVGControl.Test/app.config 18-19, Tags/app.config 46-47, TaskMaster/app.config 50-51, TaskTree/app.config 46-47, TaskVisualization/app.config 46-47, TaskVisualization.Test/app.config 46-47, ToDoModel/app.config 51-52, ToDoModel.Test/app.config 46-47, UtilitiesCS.Test/app.config 46-47. Correct (1.3.1.0) at SVGControl/app.config 14-15 and UtilitiesCS/app.config 51-52 | Grep `name="Fizzler"` -A1 | D1, P0-T4, P1-T4 to P1-T14 |
| The string `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` occurs 17 times in 11 files: twice in QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel (Fizzler plus System.ClientModel at lines 103, 103, 115, 103, 103, 108 respectively) and once in the five test configs | Grep count mode; Grep `name="System.ClientModel"` -A1 | D2, P0-T5, P1-T16 |
| System.ClientModel redirects: 16 blocks (SVGControl/app.config carries none): 6 at 1.3.0.0 (the six above), 10 at 1.16.0.0; csproj Reference is 1.16.0.0 (e.g. UtilitiesCS/UtilitiesCS.csproj 305) | Grep | known-debt row, P0-T8 |
| Unsafe redirects: 17 blocks, every one `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"` | Grep `name="System.Runtime.CompilerServices.Unsafe"` -A1 | AC2, P0-T6, P1-T17 |
| Every app.config has a CR count equal to its line count (zero bare LF lines): QuickFiler 243, QuickFiler.Test 367, SVGControl.Test 227, Tags 239, TaskMaster 430, TaskTree 239, TaskVisualization 239, TaskVisualization.Test 363, ToDoModel 323, ToDoModel.Test 363, UtilitiesCS.Test 383 (others: VBFunctions.Test 351, SVGControl 23, TaskMaster.Test 359, Tags.Test 343, TaskTree.Test 343, UtilitiesCS 293) | Grep `\r$` count equals Grep `^` count per file | D3, P0-T7 |
| `.gitattributes` line 4: `* text=auto` | Read | D3 |
| `.gitignore` line 57 ignores `artifacts/`; line 150 `coverage/*` | Grep | D12 |
| Fizzler csproj Reference 1.3.1.0 at SVGControl/SVGControl.csproj 58 and UtilitiesCS/UtilitiesCS.csproj 65; no other csproj references Fizzler | Grep | D5 |
| Known-debt rows and unverifiable names of section 7 | multiline Grep per name (counts 6, 10, 12, 6, 6, 6, 6, 7, 7, 13, 13, 13, 13, 13, 6 = 137) and csproj Reference versions; no `Include="System.Linq.AsyncEnumerable`, `Include="Microsoft.IdentityModel.Clients.ActiveDirectory` or `Include="netstandard` in any csproj | Grep | D7, P0-T8, test 14 |
| `PackageGraph.psm1` 465 lines: `ConvertFrom-ProjectFileText` at 215 (Reference records carry `Kind` 'Reference' and `Value` equal to the Include attribute), `ConvertFrom-AppConfigText` at 285 (records with `Name`, `OldVersion`, `NewVersion`; empty `NewVersion` when a block has no bindingRedirect; throws when the text has no `<configuration` root, line 304-305); `Export-ModuleMember` at 457-465 | Read | module design |
| `ProjectConsistency.psm1` 381 lines; header lines 6-12 state reconciliation-only ownership; imports siblings without -Force at 39-44 with the reason at 33-38; `Invoke-BindingRedirectReconciliation` 271-375 | Read | D2, D9 |
| `ConsistencyVerifier.psm1` 499 lines; detectors return `Finding` plus `ExaminedCount` (lines 46-54) | Grep | result shape precedent |
| `RepositoryTreeConsistency.Tests.ps1` 152 lines: `$script:RepoRoot` from `$PSScriptRoot` at line 4; module import at 5; tree-reading tests use `[System.IO.File]::ReadAllText` at 80 and 94-95; discovery guard `Should -BeGreaterThan 9` at 74; examined guard at 87-89; SVGControl redirect test at 92-110 | Read | test conventions |
| `PackageGraph.Tests.ps1`: in-memory CRLF fixtures via a `ConvertTo-CrLf` helper at lines 10-13; app.config fixture shape at 41-69 | Read | fixture conventions |
| `_pester.yml`: Run.Path at line 41 includes `tests/scripts/dependencies`; CodeCoverage.Path at 45 includes `scripts/dependencies`; prints `PESTER Passed=...` at 51 and `COVERAGE LinePercent=...` at 64; exits 1 below 80 at 71 | Read | D8 |
| PoshQC tool payloads observed in issue 929 evidence: format `{"ok":true,...,"summary":"Ran bundled PoshQC format against '<root>' with 2 selected scan folder(s)."}`; analyze `...PoshQC analyze against '<root>' with 2 selected scan folder(s).`; test `...PoshQC test against '<root>' with 1 selected scan folder(s).`; a failing test run returns `{"ok": false, ..., "summary": "Command exited with code 4."}`; the JUnit document carries one `testsuite` per file (absolute-path `name`, `tests`, `failures`, `skipped`) and one `testcase` per It (`name` equal to Describe dot It, `status`, a `failure` child with `message`) | 929 evidence p0-t13, p0-t14, p0-t15, p1-t2 | CMD definitions |
| Issue 929 recorded a two-line Edit-tool change to SVGControl/app.config as `git diff --numstat` `2	2` with indentation unchanged | 929 evidence p1-t5 | D3 (one line edits read `1	1`) |
| `enforce-powershell-batch-budget.ps1` lines 8-15 (caps 3 and 3, state under `.claude/state/powershell-batch-budget.<session_id>.json`), 37-40 (override variables), 42-45 (deny) | Read | D9, P0-T13 |
| issue.md: `- Work Mode: minor-audit` at line 9; `## Acceptance Criteria` at 82; AC1 to AC6 at 84-89, all `- [ ]`; AC6 contains `is not measured locally` | Read | P0-T3, P2-T9 to P2-T14 |

## 5. Write Set (exhaustive)

1. `QuickFiler/app.config` (lines 50-51)
2. `QuickFiler.Test/app.config` (46-47)
3. `SVGControl.Test/app.config` (18-19)
4. `Tags/app.config` (46-47)
5. `TaskMaster/app.config` (50-51)
6. `TaskTree/app.config` (46-47)
7. `TaskVisualization/app.config` (46-47)
8. `TaskVisualization.Test/app.config` (46-47)
9. `ToDoModel/app.config` (51-52)
10. `ToDoModel.Test/app.config` (46-47)
11. `UtilitiesCS.Test/app.config` (46-47)
12. `scripts/dependencies/BindingRedirectVerification.psm1` (new)
13. `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` (new)
14. `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/` (issue.md AC check-offs, this plan's checkboxes, `evidence/`)

Nothing else is written. `artifacts/pester/*` is written by the MCP test tool and is gitignored; `artifacts/orchestration/orchestrator-state.json` is untracked and outside the footprint. `.claude/agent-memory/**` is tracked and written by agents during execution; every git gate in this plan subtracts that prefix (Convention C6).

## 6. Conventions and command definitions

- **C1 — Artifact fields.** Every command-bearing artifact carries `Timestamp: <yyyy-MM-ddTHH-mm>`, `Command: <exact invocation>`, `EXIT_CODE: <int>`, `Output Summary:` (1 to 20 lines). A task expected to observe a red state carries `ExpectedExitCode: 1`. One artifact carries one `EXIT_CODE:` row; additional exit values are named `Output Summary:` lines.
- **C2 — Artifact naming.** `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/<kind>/<task>-<slug>.<timestamp>.md`, for example `evidence/baseline/p0-t4-fizzler-census.2026-10-05T09-12.md`. Loop iterations add `.iter<N>` before the timestamp.
- **C3 — MCP exit derivation.** For an MCP PoshQC call, `EXIT_CODE:` is 0 when the payload's `ok` is true (and, for the test tool, when the JUnit root `failures` is 0) and 1 otherwise; the payload is recorded verbatim with the worktree root replaced by `<execution-worktree-root>`. The tool's own process exit is not observable and is not claimed.
- **C4 — Host paths.** No evidence line carries an absolute host path, machine name or account name; the worktree prefix is replaced by `<execution-worktree-root>`.
- **C5 — Tool routes.** Read-only tree observations use the Read, Grep and Glob tools or `git`. Grep patterns written in this plan are ripgrep patterns run through the Grep tool with the stated glob. Two PowerShell one-liners (CMD-BOM, CMD-JUNIT-DELETE) contain no double quote and no `$`, and each names its file operand by absolute path. No other PowerShell command is issued. Every git command in this plan, including every one written in task text without the option, is issued as `git -C <execution-worktree-root> ...`, so pathspecs and `.` resolve against the worktree root whatever the executor's current directory; every Grep and Glob call passes `<execution-worktree-root>` as its path, except a Grep over `artifacts/pester/pester-junit.xml`, which passes the absolute file path `<execution-worktree-root>/artifacts/pester/pester-junit.xml` as its path and no glob, because the Grep tool honours `.gitignore` when its path is a directory and `artifacts/` is ignored (`.gitignore` line 57), so a root path with a glob returns no match for that file (observed at preflight round 3); the Glob tool was observed in the same round to return that gitignored file with a worktree root as its path, so the Glob existence and absence checks of CMD-JUNIT-DELETE and CMD-JUNIT-READ keep the root path; every file operand of CMD-BOM and CMD-JUNIT-DELETE is absolute (`<execution-worktree-root>/<path>`). Evidence records the placeholder, never the host path (C4). If the Bash tool refuses a pwsh invocation, the executor uses the stated fallback and records `PWSH-REFUSED: <tool message>`; the fallback is not a deviation.
- **C6 — Git gate scoping.** Every `git diff` is anchored at `<BASE_SHA>` (P0-T2). Every footprint evaluation removes paths under `.claude/agent-memory/` and paths listed in the P0-T2 `INHERITED:` capture before comparing against the Write Set. Gitignored paths (`artifacts/`, `coverage/`) never appear in porcelain and need no subtraction.
- **C7 — Stop rule.** Where a task says STOP, the executor writes the task's artifact with the observed values, leaves the plan checkbox unchecked, and ends the run with a report to the orchestrator. No stop is a pass.
- **C8 — Check-off protocol.** Plan checkboxes are flipped only after the task's artifact exists with every C1 field. issue.md check-offs happen only in P2-T9 to P2-T14, one criterion per task, changing `- [ ]` to `- [x]` and nothing else on the line.

**CMD-POSHQC-FORMAT** — MCP `mcp__drm-copilot__run_poshqc_format` with `workspace_root` `<execution-worktree-root>` and `scan_folders` `["scripts/dependencies","tests/scripts/dependencies"]`. Success-case payload: `ok` true and a summary reading `Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s).` The tool reports no file list and exits the same whether or not it rewrote a file, so its observation is CMD-HASHSET taken immediately before and immediately after the call; identical hash sets mean no rewrite.

**CMD-HASHSET** — `git hash-object <path>` for every `.ps1` and `.psm1` file under `scripts/dependencies/` and `tests/scripts/dependencies/` (the files are enumerated with the Glob tool at run time; 14 at planning time (6 under scripts/dependencies, 8 under tests/scripts/dependencies), 16 once the two new files exist). `git hash-object` reads the working-tree file whether or not it is tracked, so an untracked new file is observed. The set is recorded as `HASH <path> <sha>` lines sorted by path.

**CMD-POSHQC-ANALYZE** — MCP `mcp__drm-copilot__run_poshqc_analyze` with the same `workspace_root` and `scan_folders`. Success-case payload: `ok` true and a summary reading `Ran bundled PoshQC analyze against '<execution-worktree-root>' with 2 selected scan folder(s).` The payload carries no count, file or rule; `ok` true is the lint result and is recorded with the line `GATE-SUBSTITUTION: PoshQC analyze ok flag stands in for a diagnostic count`. `ok` false is a failed step.

**CMD-JUNIT-DELETE** — `pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue'` followed by a Glob for `artifacts/pester/pester-junit.xml` with `<execution-worktree-root>` as its path that must return nothing (the Glob tool returns a gitignored file when one exists, observed at preflight round 3 with a worktree root as its path, so an empty result is an absence observation; C5). Run immediately before every CMD-POSHQC-TEST so the document read afterwards was written by that run. The pwsh payload contains no double quote. Fallback when pwsh is refused: `rm -f <execution-worktree-root>/artifacts/pester/pester-junit.xml` followed by the same Glob.

**CMD-POSHQC-TEST** — MCP `mcp__drm-copilot__run_poshqc_test` with `workspace_root` `<execution-worktree-root>` and `scan_folders` `["tests/scripts/dependencies"]`. Success-case payload: `ok` true and a summary reading `Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s).`; a failing run observed on this repository returned `ok` false with summary `Command exited with code 4.` The tool writes `artifacts/pester/pester-junit.xml` (and two coverage documents that instrument none of `scripts/dependencies` and are never read). Every test task records the payload verbatim (C3, C4), then runs CMD-JUNIT-READ, and carries the literal line `COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure`.

**CMD-JUNIT-READ** — Over `artifacts/pester/pester-junit.xml`, which must exist (Glob) after the run, every Grep below passing the absolute file path `<execution-worktree-root>/artifacts/pester/pester-junit.xml` as its path and no glob (C5 exception): (a) Grep pattern `<testsuites ` with `-n` → record `JUNIT-ROOT tests=<n> failures=<n> errors=<n> disabled=<n>` from that line's attributes; (b) Grep pattern `<testsuite ` with `-n` → one `JUNIT-SUITE <leaf name of the name attribute> tests=<n> failures=<n> skipped=<n>` line per match, the leaf taken after the last path separator so no host path is transcribed; (c) Grep pattern `status="Failed"` with `-n` and `-A 2` → one `JUNIT-NOTPASSED <name attribute>` line per matched testcase, then for each match the Read tool on the same file with `offset` equal to the matched line number and `limit` 3, from whose output the `JUNIT-MESSAGE <message attribute of the failure child>` line is transcribed, with any worktree prefix replaced per C4; when (c) returns no match and (a) matched exactly one line in the same task, the artifact states `JUNIT-NOTPASSED: none`; a no-match from (a) is a failed read, not an empty result. Long-line rule: the Grep tool replaces an output line longer than about 500 characters with `[Omitted long matching line]` or `[Omitted long context line]` (observed at preflight round 2 on four lines of version 1.1 of this plan, and re-observed by the planner on versions 1.2 and 1.3, where Grep pattern `^.{500,}` over this plan reports every line over 500 characters as omitted, among them the CMD-POSHQC-FORMAT, CMD-JUNIT-DELETE and CMD-JUNIT-READ definitions and section 9 tests 13 and 14); a `testsuite` line carries the absolute test-file path twice, so whether it is omitted depends on the execution worktree path length: at the `.claude/worktrees/agent-<id>` depth a dependencies testsuite line measured 403 to 439 characters at preflight round 3, below the 501-character omission threshold, and the Grep tool printed it in full; the test 13 `failure` message attribute is about 333 characters by construction (eleven `<directory>=1.3.0.0` entries) and is expected to print in full, and the test 14 `failure` line, which lists 16 pairs three times, is expected to exceed the threshold; neither failure line has been observed. Whenever a Grep in (a), (b) or (c) reports an omitted line, the executor reads that line number with the Read tool (`offset` equal to the line number, `limit` 1) and transcribes from the Read output; the Read tool returned section 9 test 14 of this plan (one line of 2,000 to 2,099 characters at version 1.3: Grep `^.{2000,}` matches it and `^.{2100,}` does not) in full at preflight round 2 and again to the planner in the round 2 and round 3 revision passes. Every task that transcribes JUNIT lines (P0-T12, P1-T3, P1-T15, P2-T3) follows this rule. Expected suite leaf names at planning time: AnalyzerItemRepair.Tests.ps1, ConsistencyVerifier.Tests.ps1, DependabotConfig.Tests.ps1, PackageCompatibility.Tests.ps1, PackageGraph.Tests.ps1, ProjectConsistency.Tests.ps1, Repair-PackageManifestConsistency.Tests.ps1, RepositoryTreeConsistency.Tests.ps1 and, from P1-T2, BindingRedirectVerification.Tests.ps1. Suite counts are recorded as observed at P0-T12 and compared against that record later; this plan pins no pre-existing suite's count.

**CMD-EOL `<path>`** — `git ls-files --eol -- <path>`. Output is one line of the form `i/<eol> w/<eol> attr/<attrs>` separated by runs of spaces, then one tab and `<path>` (observed: `i/lf    w/crlf  attr/text=auto` then a tab and the path). The gate reads the second whitespace-separated field, which describes the working-tree file's terminators: `w/crlf` passes; `w/lf`, `w/mixed`, `w/none` or `w/-text` fails. The first field is recorded and not gated.

**CMD-BOM `<path>`** — `pwsh -NoProfile -Command '(Get-Content -LiteralPath <execution-worktree-root>/<path> -AsByteStream -TotalCount 3) -join [char]44'`. A UTF-8 BOM prints `239,187,191`. The payload contains no double quote; `[char]44` is the comma separator. Recorded as `BOM-BYTES <path>=<output>`. Fallback when pwsh is refused: `od -A n -t u1 -N 3 <execution-worktree-root>/<path>`, whose success-case output is ` 239 187 191` (observed at preflight); recorded after whitespace normalisation as `BOM-BYTES <path>=239,187,191`, so the two routes yield the same recorded value.

**CMD-NUMSTAT `<path>`** — `git diff --numstat <BASE_SHA> -- <path>`. For a one-line replacement the output is exactly `1	1	<path>` (tab separated). Paired in the same task with `git diff <BASE_SHA> -- <path>`, whose content lines (those beginning `-` or `+` after the `---`/`+++` header pair) must be exactly one removed line and one added line.

**CMD-LINECOUNT `<path>`** — Grep tool, pattern `^`, output_mode count, path `<path>`; the returned count is the file's line count. Used for every line-count figure in this plan so figures are comparable.

**CMD-FOOTPRINT** — `git diff --name-only <BASE_SHA> -- .` together with `git status --porcelain --untracked-files=all`, both recorded verbatim (C4). The evaluated set is the union of the two captures' paths minus the P0-T2 `INHERITED:` list minus every path under `.claude/agent-memory/` (C6).

**EDIT-FIZZLER `<path>` `<identity line>` `<redirect line>`** — Edit tool on `<path>` with old_string equal to the two lines (identity line, then redirect line, each with its 8-space indent):

```text
        <assemblyIdentity name="Fizzler" publicKeyToken="4ebff4844e382110" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />
```

and new_string equal to the same two lines with the second replaced by:

```text
        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />
```

Acceptance for every EDIT-FIZZLER task (all six must hold): (1) CMD-NUMSTAT prints exactly `1	1	<path>`; (2) `git diff <BASE_SHA> -- <path>` shows exactly one `-` content line, whose text ignoring a trailing carriage return is `        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />`, and exactly one `+` content line, whose text ignoring a trailing carriage return is `        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />`; (3) CMD-EOL prints `w/crlf` in the second whitespace-separated field; (4) CMD-BOM prints a value equal to the `BOM-BYTES` value P0-T7 recorded for the same path (expected `239,187,191`); (5) Grep `name="Fizzler"` with `-A 1` on `<path>` shows the redirect line now reading `1.3.1.0` in both attributes; (6) Grep `name="System.ClientModel"` with `-A 1` on `<path>` shows the System.ClientModel redirect line unchanged from P0-T5 (1.3.0.0 in QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel; 1.16.0.0 in the five test configs). The task artifact goes to `evidence/qa-gates/` with `Command:` naming the Edit and the four observation commands and `EXIT_CODE: 0`. If the Edit tool reports no match, STOP: EDIT-MISMATCH (D2).

## 7. Known-debt set (orchestrator measurement 2026-10-02, re-derived by the planner 2026-10-02, re-measured by P0-T8)

Findings expected after the Fizzler sweep: exactly these 15 pairs, 137 redirect entries in all (assembly | config newVersion | only csproj Reference version | configs carrying it):

1. Azure.Core | 1.62.0.0 | 1.63.0.0 | 6
2. Microsoft.Bcl.Memory | 10.0.0.7 | 10.0.0.12 | 10
3. Microsoft.Bcl.Numerics | 10.0.0.5 | 10.0.0.12 | 12
4. Microsoft.Extensions.Diagnostics.Abstractions | 10.0.0.5 | 10.0.0.12 | 6
5. Microsoft.Identity.Client | 4.89.0.0 | 4.90.1.0 | 6
6. Microsoft.Identity.Client.Extensions.Msal | 4.89.0.0 | 4.90.1.0 | 6
7. Microsoft.IdentityModel.Abstractions | 8.22.0.0 | 8.23.0.0 | 6
8. Microsoft.IdentityModel.JsonWebTokens | 8.22.0.0 | 8.23.0.0 | 7
9. Microsoft.IdentityModel.Logging | 8.22.0.0 | 8.23.0.0 | 7
10. Microsoft.IdentityModel.Protocols | 8.22.0.0 | 8.23.0.0 | 13
11. Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.22.0.0 | 8.23.0.0 | 13
12. Microsoft.IdentityModel.Tokens | 8.22.0.0 | 8.23.0.0 | 13
13. Microsoft.IdentityModel.Validators | 8.22.0.0 | 8.23.0.0 | 13
14. System.IdentityModel.Tokens.Jwt | 8.22.0.0 | 8.23.0.0 | 13
15. System.ClientModel | 1.3.0.0 | 1.16.0.0 | 6

Unverifiable (no csproj `Reference Include="Name, Version=` anywhere): System.Linq.AsyncEnumerable (15 entries), Microsoft.IdentityModel.Clients.ActiveDirectory (13), netstandard (1) = 29 entries. Before the sweep the finding set additionally contains `Fizzler|1.3.0.0` (11 entries, 148 in all), which is why test 14 is red at P1-T3, before the config edits P1-T4 to P1-T14, and green at P1-T15.

Scope boundary: correcting these 15 pairs is out of scope for issue 953. P2-T16 records them as a follow-up note for the coordinator to promote through the potential-entry lifecycle; this plan creates no issue and no file outside the Write Set.

## 8. Module specification (`scripts/dependencies/BindingRedirectVerification.psm1`)

ASCII-only content (so `PSUseBOMForUnicodeEncodedFile` cannot fire), target 100 to 150 lines, hard ceiling 500. Comment-based help on both functions. Written as follows; the executor reproduces this content, adjusting only formatting that the PoshQC formatter requires.

Note on the empty-element path of `ConvertTo-ReferenceVersionMap`: a Mandatory `[string[]]` parameter rejects an empty-string element at binding unless it carries `[AllowEmptyString()]`, so the attribute is present and an empty or whitespace-only element reaches `ConvertFrom-ProjectFileText`, which throws for it (PackageGraph.psm1 lines 230-236). No section 9 test supplies such an element: the throw is the parser's own behaviour and PackageGraph.Tests.ps1 line 268 (`rejects whitespace-only project-file text`) already covers it. The attribute adds no executable line, so it has no coverage effect.

```powershell
<#
.SYNOPSIS
    Detects application-configuration binding redirects whose newVersion names an assembly
    version that no project file Reference declares.

.DESCRIPTION
    BindingRedirectVerification is a detection layer beside the dependency-consistency
    tooling for issue #911: PackageGraph parses, ProjectConsistency reconciles and
    ConsistencyVerifier detects manifest and project-file faults. This module carries the
    one rule issue #953 adds. A bindingRedirect newVersion must equal a version that some
    project file declares in a Reference Include for the same assembly name, because that is
    the assembly version the build copies to the output directory. Package versions are not
    consulted: a package version and its assembly version are different quantities.

    Both functions are pure over text. The deployed-version source is an injected
    scriptblock, so the detector runs in memory with no file dependency; the repository-level
    test supplies a provider built from every project file.

    Exported functions:
      - ConvertTo-ReferenceVersionMap
      - Find-StaleBindingRedirect
 #>

Set-StrictMode -Version Latest

<# Imported without -Force deliberately, as ProjectConsistency.psm1 does: a nested
   Import-Module -Force removes the module from the whole session before re-importing it,
   which would strip the parser from a caller that had already imported it. #>
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')

function ConvertTo-ReferenceVersionMap {
    <#
    .SYNOPSIS
        Builds a map from assembly name to the versions the supplied project files declare
        in Reference Include attributes.
    .DESCRIPTION
        Only an Include of the form "Name, Version=X.Y.Z.W, ..." contributes. A Reference
        without a Version is not evidence of a deployed version and is skipped. Versions are
        unioned across every supplied text, so an assembly two projects reference at two
        versions maps to both. An empty or whitespace-only element is accepted at parameter
        binding and then rejected by ConvertFrom-ProjectFileText, which throws for it.
    .PARAMETER ProjectText
        The project-file texts. An empty collection yields an empty map.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]]$ProjectText
    )

    $versions = @{}
    foreach ($text in $ProjectText) {
        $reference = @(ConvertFrom-ProjectFileText -Text $text | Where-Object { $_.Kind -eq 'Reference' })
        foreach ($record in $reference) {
            $match = [regex]::Match($record.Value, '^\s*(?<name>[^,]+?)\s*,\s*Version=(?<version>[^,\s]+)')
            if (-not $match.Success) { continue }
            $name = $match.Groups['name'].Value
            if (-not $versions.ContainsKey($name)) {
                $versions[$name] = [System.Collections.Generic.List[string]]::new()
            }
            $value = $match.Groups['version'].Value
            if (-not $versions[$name].Contains($value)) { $versions[$name].Add($value) }
        }
    }

    $map = @{}
    foreach ($key in $versions.Keys) { $map[$key] = [string[]]$versions[$key].ToArray() }
    return $map
}

function Find-StaleBindingRedirect {
    <#
    .SYNOPSIS
        Reports every bindingRedirect whose newVersion equals no deployed version of its
        assembly, with the examined-entry count and the unverifiable assembly names.
    .DESCRIPTION
        A dependentAssembly block with no bindingRedirect is not examined. An assembly for
        which the provider returns no version is listed as unverifiable and is not a finding,
        so a redirect for a transitively deployed assembly needs no allow list. Only
        newVersion is compared; the oldVersion range is a request filter, not a deployment
        claim. Empty or whitespace text examines zero entries. Text that is not an
        application configuration document is rejected by the parser.
    .PARAMETER AppConfigText
        The application configuration text.
    .PARAMETER DeployedVersionProvider
        A delegate taking an assembly name and returning its deployed version strings, or
        nothing when the name is unknown.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$AppConfigText,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DeployedVersionProvider
    )

    $finding = [System.Collections.Generic.List[pscustomobject]]::new()
    $unverifiable = [System.Collections.Generic.List[string]]::new()
    $examined = 0

    if (-not [string]::IsNullOrWhiteSpace($AppConfigText)) {
        foreach ($record in @(ConvertFrom-AppConfigText -Text $AppConfigText)) {
            if ([string]::IsNullOrEmpty($record.NewVersion)) { continue }
            $examined++
            $deployed = @(& $DeployedVersionProvider $record.Name |
                    Where-Object { -not [string]::IsNullOrEmpty([string]$_) } |
                    ForEach-Object { [string]$_ })
            if ($deployed.Count -eq 0) {
                if (-not $unverifiable.Contains($record.Name)) { $unverifiable.Add($record.Name) }
                continue
            }
            if ($deployed -contains $record.NewVersion) { continue }
            $finding.Add([pscustomobject]@{
                    PSTypeName       = 'BindingRedirectVerification.StaleRedirect'
                    AssemblyName     = $record.Name
                    NewVersion       = $record.NewVersion
                    DeployedVersions = [string[]]$deployed
                })
        }
    }

    return [pscustomobject]@{
        PSTypeName    = 'BindingRedirectVerification.DetectionResult'
        Finding       = $finding.ToArray()
        Unverifiable  = $unverifiable.ToArray()
        ExaminedCount = $examined
    }
}

Export-ModuleMember -Function @(
    'ConvertTo-ReferenceVersionMap',
    'Find-StaleBindingRedirect'
)
```

## 9. Test specification (`tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`)

ASCII-only, under 500 lines, `Set-StrictMode -Version Latest` at line 1, Arrange-Act-Assert comments in every It, one behaviour per It. `BeforeAll` resolves `$script:RepoRoot` from `$PSScriptRoot` exactly as RepositoryTreeConsistency.Tests.ps1 line 4 does, imports `scripts/dependencies/PackageGraph.psm1` with `-Force` and then `scripts/dependencies/BindingRedirectVerification.psm1` with `-Force` (the parser's functions are needed in test scope by test 13 and are not re-exported by the new module) using exactly these two lines, in this order: `Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/PackageGraph.psm1') -Force` and `Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/BindingRedirectVerification.psm1') -Force` (the RepositoryTreeConsistency.Tests.ps1 line 5 form). Every fixture and provider defined in BeforeAll is assigned to a `$script:` variable. BeforeAll also defines a `ConvertTo-CrLf` helper as PackageGraph.Tests.ps1 lines 10-13 do, and holds every fixture as an in-memory string: a configuration wrapper (`<?xml ...?><configuration><runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">...</assemblyBinding></runtime></configuration>`) around dependentAssembly blocks for Fizzler at 1.3.0.0 (`oldVersion="0.0.0.0-1.3.0.0"`), Fizzler at 1.3.1.0, Fizzler at 1.3.1.0 with `oldVersion="0.0.0.0-9.9.9.9"`, System.Runtime.CompilerServices.Unsafe at 6.0.3.0, an assembly named Contoso.Unknown at 1.0.0.0, and a block with an assemblyIdentity (Contoso.NoRedirect) and no bindingRedirect; a provider scriptblock returning `@('1.3.1.0')` for Fizzler, `@('6.0.3.0')` for System.Runtime.CompilerServices.Unsafe and `@()` otherwise; a second provider returning `@('1.3.0.0','1.3.1.0')` for Fizzler; and two project-file fixtures in the shape of PackageGraph.Tests.ps1 lines 71-85 (ProjectA with `<Reference Include="Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110">` and `<Reference Include="System.Xml" />`; ProjectB with `<Reference Include="Fizzler, Version=1.3.0.0, Culture=neutral">`). No `$TestDrive`, `New-Item`, `Set-Content`, `Out-File`, `Add-Content` or `New-TemporaryFile` appears anywhere in the file.

The 14 It names, verbatim, with the assertion each carries:

Describe 'Find-StaleBindingRedirect (in-memory fixtures)':

1. `reports one finding when the Fizzler redirect names 1.3.0.0 and the provider deploys 1.3.1.0` — negative control: config with Fizzler 1.3.0.0 plus Unsafe 6.0.3.0; `Finding.Count` 1; the finding's `AssemblyName` is `Fizzler`, `NewVersion` is `1.3.0.0`, `DeployedVersions` is `@('1.3.1.0')`; `ExaminedCount` 2.
2. `reports no finding when the Fizzler redirect names the deployed 1.3.1.0` — positive control: `Finding.Count` 0; `ExaminedCount` 2; `Unverifiable.Count` 0.
3. `compares newVersion only and ignores the oldVersion range` — Fizzler block with `oldVersion="0.0.0.0-9.9.9.9" newVersion="1.3.1.0"`: `Finding.Count` 0.
4. `lists an assembly the provider knows nothing about as unverifiable and not as a finding` — Contoso.Unknown block: `Unverifiable` equals `@('Contoso.Unknown')`; `Finding.Count` 0; `ExaminedCount` 1.
5. `skips a dependentAssembly block that carries no bindingRedirect` — Contoso.NoRedirect block plus Fizzler 1.3.1.0: `ExaminedCount` 1; `Finding.Count` 0; `Unverifiable.Count` 0.
6. `counts one examined entry per bindingRedirect-bearing block` — three redirect-bearing blocks (Fizzler 1.3.1.0, Unsafe 6.0.3.0, Contoso.Unknown 1.0.0.0): `ExaminedCount` 3.
7. `examines zero entries and reports nothing for empty text` — `-AppConfigText ''`: `ExaminedCount` 0, `Finding.Count` 0, `Unverifiable.Count` 0, no throw.
8. `accepts a newVersion equal to any one of several deployed versions` — Fizzler 1.3.0.0 with the second provider: `Finding.Count` 0.
9. `throws when the text is not an application configuration document` — `-AppConfigText '<packages />'` inside a scriptblock piped to `Should -Throw`.

Describe 'ConvertTo-ReferenceVersionMap (in-memory fixtures)':

10. `maps a Reference Include with a Version to its assembly name` — ProjectA: the map contains key `Fizzler` with value `@('1.3.1.0')`.
11. `omits a Reference Include that declares no Version` — ProjectA: the map does not contain key `System.Xml`; key count 1.
12. `unions the versions of one assembly across several project texts` — ProjectA and ProjectB: `@($map['Fizzler'] | Sort-Object)` equals `@('1.3.0.0','1.3.1.0')`.

Describe 'Repository binding redirects (issue 953)' (reads tracked files read-only through `[System.IO.File]::ReadAllText` on paths derived from `$script:RepoRoot`; writes nothing):

13. `names 1.3.1.0 in every Fizzler binding redirect across the repository app.config files` — Arrange: `$configPath` is every `app.config` directly under a root-level directory (`Get-ChildItem -LiteralPath $script:RepoRoot -Directory`, `Join-Path ... 'app.config'`, `Test-Path -LiteralPath`); `$configPath.Count | Should -BeGreaterThan 9`. Act: for each config, `ConvertFrom-AppConfigText` records whose `Name` is `Fizzler`, projected to `Config` (leaf directory name), `NewVersion`, `OldVersion`; `$stale` is those with `NewVersion -ne '1.3.1.0' -or OldVersion -ne '0.0.0.0-1.3.1.0'`. Assert: total Fizzler records `Should -Be 13 -Because 'thirteen configs carry a Fizzler redirect'`; `$stale.Count | Should -Be 0 -Because ('these configs redirect Fizzler to another version: ' + (($stale | ForEach-Object { $_.Config + '=' + $_.NewVersion }) -join '; '))`. This is the [expect-fail] regression test: at P1-T3, before the config edits, it fails with `Expected 0, because these configs redirect Fizzler to another version: ...=1.3.0.0; ..., but got 11.` naming the eleven directories; after P1-T14 it passes.
14. `reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference` — Arrange: `$configPath` as in test 13; `$projectPath` is every `*.csproj` directly under a root-level directory; both counts `Should -BeGreaterThan 9`; `$map = ConvertTo-ReferenceVersionMap -ProjectText @($projectPath | ForEach-Object { [System.IO.File]::ReadAllText($_) })`; `$provider = { param($Name) $map[$Name] }.GetNewClosure()`; `$expectedDebt` is the 15 strings `Name|NewVersion` from section 7; `$expectedUnverifiable` is `@('Microsoft.IdentityModel.Clients.ActiveDirectory','System.Linq.AsyncEnumerable','netstandard')`. Act: for each config text, add `[regex]::Matches($text, '<bindingRedirect\b').Count` to `$redirectElement`, run `Find-StaleBindingRedirect`, add `ExaminedCount` to `$examined`, collect `AssemblyName + '|' + NewVersion` per finding and each unverifiable name; `$actualDebt = @($pair | Sort-Object -Unique)`; `$actualUnverifiable = @($name | Sort-Object -Unique)`. Assert: `$redirectElement | Should -BeGreaterThan 0`; `$examined | Should -Be $redirectElement -Because 'every bindingRedirect element must be examined'`; `$actualDebt | Should -Be @($expectedDebt | Sort-Object -Unique) -Because ('the stale redirect set must equal the recorded known debt; observed: ' + ($actualDebt -join '; '))` (Pester renders a `Should -Be` failure as `Expected @(...), because <because text>, but got @(...).`, so the observed entries sit between `observed: ` and `, but got`, separated by `; `); `$actualUnverifiable | Should -Be @($expectedUnverifiable | Sort-Object -Unique)`; `@($actualDebt | Where-Object { $_ -like 'Fizzler|*' -or $_ -like 'System.Runtime.CompilerServices.Unsafe|*' }).Count | Should -Be 0`. At P1-T3, before the config edits, this test fails because `Fizzler|1.3.0.0` is a sixteenth pair, and the observed list in its failure message then carries exactly 16 entries (the 15 section 7 pairs plus `Fizzler|1.3.0.0`); after P1-T14 it passes. A new mismatch, a corrected known-debt entry or a new unverifiable name fails it.

## 10. Phases

### Phase 0 — Baseline capture and policy reads

- [x] [P0-T1] Read the policy and skill files in order and record the read: CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`, `.claude/rules/plan-acceptance-gates.md`, `.claude/rules/tonality.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, then this plan and issue.md. Artifact `evidence/baseline/phase0-instructions-read.<timestamp>.md` with `Timestamp:`, `Policy Order:` (the numbered order above) and the explicit list of files read. Acceptance: the artifact exists and lists all eleven files.
- [x] [P0-T2] Record the baseline git anchor for `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/`: run `git fetch origin main` (record `FETCH-EXIT:`; a non-zero value is recorded and the run continues against the local `origin/main` ref with the line `FETCH-STALE: local origin/main used`), then `git merge-base HEAD origin/main` → `BASE_SHA:`, `git rev-parse HEAD` → `P0-START:`, `git cat-file -t <BASE_SHA>`, `git status --porcelain --untracked-files=all` → `BASE-PORCELAIN:` (verbatim), `git diff --name-only <BASE_SHA> -- .` → `INHERITED:` (verbatim; this is the set every later footprint evaluation subtracts, C6). Acceptance: BASE_SHA is 40 hexadecimal characters and `git cat-file -t` prints `commit`; no path in BASE-PORCELAIN or INHERITED ends in `.cs`, `.csproj`, `packages.config` or `app.config`, and none lies under `scripts/dependencies/` or `tests/scripts/dependencies/` (the Write Set is clean at start); the feature folder paths and `.claude/agent-memory/` paths are expected and recorded, never asserted empty. Artifact `evidence/baseline/p0-t2-base-anchor.<timestamp>.md`.
- [x] [P0-T3] Verify the acceptance-criteria source and mode as a baseline precondition by reading `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md`: line 9 reads `- Work Mode: minor-audit`; exactly one `## Acceptance Criteria` heading (Grep count of `^## Acceptance Criteria$` is 1); Grep count of `^- \[ \] AC[1-6]:` is 6 and of `^- \[x\] AC[1-6]:` is 0; the AC6 line contains the literal `is not measured locally`; Glob for `spec.md` and `user-story.md` in the feature folder returns nothing. Any failed condition is STOP: AC-SOURCE. Artifact `evidence/baseline/p0-t3-ac-precondition.<timestamp>.md`.
- [x] [P0-T4] Baseline Fizzler census over `*/app.config`: Grep pattern `name="Fizzler"` with `-A 1`, glob `*/app.config`, content mode. Acceptance: 13 blocks; the following line reads `newVersion="1.3.0.0"` for exactly these 11 files at these lines — QuickFiler/app.config 51, QuickFiler.Test/app.config 47, SVGControl.Test/app.config 19, Tags/app.config 47, TaskMaster/app.config 51, TaskTree/app.config 47, TaskVisualization/app.config 47, TaskVisualization.Test/app.config 47, ToDoModel/app.config 52, ToDoModel.Test/app.config 47, UtilitiesCS.Test/app.config 47 — and reads `newVersion="1.3.1.0"` for SVGControl/app.config 15 and UtilitiesCS/app.config 52. A different member set or line is STOP: TREE-DRIFT (the Write Set line citations would be wrong). Artifact `evidence/baseline/p0-t4-fizzler-census.<timestamp>.md`.
- [x] [P0-T5] Baseline shared-string census over `*/app.config`: Grep pattern `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` glob `*/app.config` count mode → expected 17 occurrences across 11 files, 2 each in QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel and 1 each in QuickFiler.Test, SVGControl.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test; Grep pattern `name="System.ClientModel"` with `-A 1` → 16 blocks (SVGControl/app.config carries none), the following line at 1.3.0.0 for exactly Tags 103, TaskVisualization 103, TaskTree 103, QuickFiler 103, TaskMaster 115, ToDoModel 108 and at 1.16.0.0 for the other 10. Acceptance: both expectations hold (otherwise STOP: TREE-DRIFT). The artifact records the per-file System.ClientModel values as `CLIENTMODEL <file>=<newVersion>` lines for P1 comparison. Artifact `evidence/baseline/p0-t5-shared-string-census.<timestamp>.md`.
- [x] [P0-T6] Baseline Unsafe census over `*/app.config` (AC2 evidence): Grep pattern `name="System.Runtime.CompilerServices.Unsafe"` with `-A 1`, glob `*/app.config`. Acceptance: 17 blocks and every following line reads `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"`; the artifact lists the 17 files. Artifact `evidence/baseline/p0-t6-unsafe-census.<timestamp>.md`.
- [x] [P0-T7] Baseline encoding observation for the 11 Write Set configs (`QuickFiler/app.config` and the ten others of section 5): for each path run CMD-EOL (second whitespace-separated field must read `w/crlf`), CMD-BOM (record `BOM-BYTES <path>=<output>`; expected `239,187,191`; a different value is recorded, not a stop, and becomes the preservation reference for Phase 1), and CMD-LINECOUNT paired with Grep pattern `\r$` count mode on the same file (the two counts must be equal; expected values QuickFiler 243, QuickFiler.Test 367, SVGControl.Test 227, Tags 239, TaskMaster 430, TaskTree 239, TaskVisualization 239, TaskVisualization.Test 363, ToDoModel 323, ToDoModel.Test 363, UtilitiesCS.Test 383). Acceptance: 11 `w/crlf` lines, 11 BOM-BYTES lines, 11 equal count pairs. Artifact `evidence/baseline/p0-t7-encoding-baseline.<timestamp>.md`.
- [x] [P0-T8] Baseline re-measurement of the known-debt table over `*/app.config` and `*/*.csproj`: (a) Grep count mode, glob `*/app.config`, pattern `assemblyIdentity name=` → 1176 total across 17 files; pattern `<bindingRedirect` → 1176 across 17; Glob `*/*.csproj` → 18; (b) for each of the 15 rows of section 7, Grep multiline count mode, glob `*/app.config`, pattern `name="<escaped name>"[^\n]*\n[^\n]*newVersion="<escaped stale version>"` → the number of files reported equals the row's config count and every per-file count is 1, and Grep glob `*/*.csproj`, pattern `Include="<escaped name>, Version=[^,"]+` with `-o` → every printed match ends in `Version=<the row's Reference version>` and no other version is printed; (c) for each of the 3 unverifiable names, Grep glob `*/*.csproj`, pattern `Include="<escaped name>` → no match, and Grep glob `*/app.config`, pattern `name="<escaped name>"` count mode → 15, 13, 1 respectively. Acceptance: every figure equals section 7; any difference is recorded as `KNOWN-DEBT-DRIFT: <rows>` and the run stops (D7). Completeness beyond the 18 names is proven by test 14 at P1-T15, not here. Artifact `evidence/baseline/p0-t8-known-debt-remeasure.<timestamp>.md`.
- [x] [P0-T9] Baseline line counts (CMD-LINECOUNT) for `scripts/dependencies/PackageGraph.psm1` (465), `scripts/dependencies/ProjectConsistency.psm1` (381), `scripts/dependencies/ConsistencyVerifier.psm1` (499), `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (152), and Glob confirmation that `scripts/dependencies/BindingRedirectVerification.psm1` and `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` do not yet exist. Acceptance: the four counts equal the figures in parentheses (a difference is recorded as `LINECOUNT-DRIFT:` and the run continues, because none of the four is edited) and both Globs return nothing (a hit is STOP: PREEXISTING-FILE). Artifact `evidence/baseline/p0-t9-linecount-baseline.<timestamp>.md`.
- [x] [P0-T10] Baseline PowerShell format step over `scripts/dependencies` and `tests/scripts/dependencies`: CMD-HASHSET, then CMD-POSHQC-FORMAT, then CMD-HASHSET again and `git status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies`. Acceptance: `ok` true with the 2-folder summary literal; the two hash sets are identical and the porcelain over the two folders is empty (the Write Set is clean per P0-T2 and the new files do not exist yet). If a hash differs, the formatter rewrote pre-existing drift outside this item's change: record the paths as `FORMAT-DRIFT-REVERTED:`, restore each with `git checkout -- <path>`, re-run CMD-HASHSET to confirm equality with the first set, and continue. `EXIT_CODE:` per C3. Artifact `evidence/baseline/p0-t10-poshqc-format.<timestamp>.md`.
- [x] [P0-T11] Baseline PowerShell analyze step over `scripts/dependencies` and `tests/scripts/dependencies`: CMD-POSHQC-ANALYZE. Acceptance: `ok` true with the 2-folder summary literal (the folders were analyzer-clean on 2026-09-30 per issue 929 evidence); `ok` false is STOP: ANALYZE-BASELINE-RED, because pre-existing analyzer debt in the scan scope is outside this plan and would make P2-T2 unsatisfiable. Record `GATE-SUBSTITUTION: PoshQC analyze ok flag stands in for a diagnostic count`. Artifact `evidence/baseline/p0-t11-poshqc-analyze.<timestamp>.md`.
- [x] [P0-T12] Baseline PowerShell test step over `tests/scripts/dependencies`: CMD-JUNIT-DELETE, CMD-POSHQC-TEST, CMD-JUNIT-READ (JUNIT lines are transcribed under the CMD-JUNIT-READ long-line rule: any line the Grep tool reports as omitted is read by line number with the Read tool; whether a testsuite line is omitted depends on the execution worktree path length: at the .claude/worktrees/agent-<id> depth a dependencies testsuite line measured 403 to 439 characters at preflight round 3, below the 501-character omission threshold). Acceptance: `ok` true; JUNIT-ROOT `failures=0`; exactly 8 JUNIT-SUITE lines with the leaf names listed under CMD-JUNIT-READ (all but BindingRedirectVerification.Tests.ps1), each `failures=0 skipped=0`, counts recorded as `BASELINE-SUITE <leaf>=<tests>`; `JUNIT-NOTPASSED: none` (recorded only when CMD-JUNIT-READ (a) matched exactly one line in this task; a no-match from (a) is a failed read and the task is not complete); the `COVERAGE-MEASUREMENT:` literal line is present together with the line `COVERAGE-EXCEPTION: D8 (no local Pester coverage route instruments scripts/dependencies)`, which states the exception this plan takes to the atomic-plan-contract requirement for a numeric baseline coverage figure and its reason (the figure is read from the CI Pester job, D8). `EXIT_CODE: 0` per C3. Artifact `evidence/baseline/p0-t12-poshqc-test.<timestamp>.md`.
- [x] [P0-T13] Record the PowerShell budget baseline from `.claude/hooks/enforce-powershell-batch-budget.ps1`: quote lines 10-11 (caps), 14 (state file location) and 42-45 (deny behaviour); Glob `.claude/state/powershell-batch-budget.*.json` and record whether any state file exists (read-only; never opened for writing); restate the D9 stop rule in the artifact. Acceptance: the artifact quotes the three regions and carries the line `BUDGET-RULE: any denied PowerShell write stops the run; no state reset, no override variable`. Artifact `evidence/baseline/p0-t13-budget-baseline.<timestamp>.md`.

### Phase 1 — Implementation: detector module, regression tests, Fizzler sweep

- [x] [P1-T1] Author `scripts/dependencies/BindingRedirectVerification.psm1` with the content of section 8 (Write tool). Acceptance: the file exists; CMD-LINECOUNT is at least 90 and at most 200 (record the value; the 100-150 target is an observation); Grep pattern `^Export-ModuleMember` count 1 and Grep pattern `'ConvertTo-ReferenceVersionMap'|'Find-StaleBindingRedirect'` count 2 within the file; Grep pattern `Import-Module \(Join-Path \$PSScriptRoot 'PackageGraph.psm1'\)` count 1; Grep pattern `^Set-StrictMode -Version Latest` count 1; Grep pattern `[^\x00-\x7F]` count 0 (ASCII-only); Grep pattern `^Import-Module ` count 1 and Grep pattern `^Import-Module .*Force` count 0 (the import statement carries no -Force; the explanatory comment at the top of the module mentions -Force and is not gated). A denied Write is STOP: BUDGET-DENIED (D9). Artifact `evidence/qa-gates/p1-t1-module-authored.<timestamp>.md`.
- [x] [P1-T2] Author `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` per section 9 (Write tool). Acceptance: the file exists; Grep pattern `^\s*It '` count 14 and each of the 14 It names of section 9 is present verbatim (Grep with `-F` per name, count 1 each); CMD-LINECOUNT below 500; Grep pattern `\$TestDrive|New-Item|Set-Content|Out-File|Add-Content|New-TemporaryFile` count 0; Grep pattern `[^\x00-\x7F]` count 0; Grep pattern `^\s*Import-Module \(Join-Path \$script:RepoRoot 'scripts/dependencies/PackageGraph\.psm1'\) -Force\r?$` count 1 and Grep pattern `^\s*Import-Module \(Join-Path \$script:RepoRoot 'scripts/dependencies/BindingRedirectVerification\.psm1'\) -Force\r?$` count 1 (the two section 9 import lines; the `\r?` admits a carriage return before the line end, because ripgrep's `$` does not match before a carriage return and the terminator the Write tool and the PoshQC formatter leave on this new file is not pinned by this plan; the gate asserts the line content, not the terminator), Grep pattern `^\s*Import-Module ` count 2, the PackageGraph import on the earlier line. A denied Write is STOP: BUDGET-DENIED (D9). Artifact `evidence/regression-testing/p1-t2-tests-authored.<timestamp>.md`.
- [x] [P1-T3] [expect-fail] Run the suite before the config edits over `tests/scripts/dependencies`: CMD-JUNIT-DELETE, CMD-POSHQC-TEST, CMD-JUNIT-READ (the two JUNIT-MESSAGE lines below are transcribed from the Read tool under the CMD-JUNIT-READ long-line rule as CMD-JUNIT-READ (c) requires for every not-passed testcase, whatever the line length; the Grep `-n` output supplies the line numbers the Read tool is pointed at). Acceptance: JUNIT-SUITE `BindingRedirectVerification.Tests.ps1 tests=14 failures=2 skipped=0`; JUNIT-ROOT `failures=2`; exactly two JUNIT-NOTPASSED lines whose names end with test 13's name `names 1.3.1.0 in every Fizzler binding redirect across the repository app.config files` and test 14's name `reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference`; test 13's JUNIT-MESSAGE contains `but got 11` and `1.3.0.0` and each of the eleven directory names QuickFiler, QuickFiler.Test, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS.Test; test 14's JUNIT-MESSAGE contains `Fizzler|1.3.0.0`, and its text between `observed: ` and the following `, but got` lists exactly 16 `;`-separated entries: the 15 section 7 pairs plus `Fizzler|1.3.0.0`; any other entry is STOP: KNOWN-DEBT-DRIFT before P1-T4; every other suite line equals its P0-T12 `BASELINE-SUITE` count with `failures=0`. `EXIT_CODE: 1` with `ExpectedExitCode: 1` (C3: `ok` false or root failures above 0). A failure among tests 1 to 12, or a failure of test 13 or 14 whose JUNIT-MESSAGE does not begin `Expected ` and contain `, but got ` (an exception or StrictMode error inside the test body rather than the expected assertion failure), is a defect in a new file: fix the file concerned, record the correction, and re-run this task as `.iter<N>`; the acceptance above must hold on the final iteration. This artifact is the AC5 fail-before evidence. Artifact `evidence/regression-testing/p1-t3-fail-before.<timestamp>.md`.
- [x] [P1-T4] EDIT-FIZZLER `QuickFiler/app.config` lines 50-51; System.ClientModel at 102-103 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t4-quickfiler-redirect.<timestamp>.md`.
- [x] [P1-T5] EDIT-FIZZLER `QuickFiler.Test/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.16.0.0. Artifact `evidence/qa-gates/p1-t5-quickfiler-test-redirect.<timestamp>.md`.
- [x] [P1-T6] EDIT-FIZZLER `SVGControl.Test/app.config` lines 18-19; System.ClientModel at 46-47 stays 1.16.0.0. Artifact `evidence/qa-gates/p1-t6-svgcontrol-test-redirect.<timestamp>.md`.
- [x] [P1-T7] EDIT-FIZZLER `Tags/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t7-tags-redirect.<timestamp>.md`.
- [x] [P1-T8] EDIT-FIZZLER `TaskMaster/app.config` lines 50-51; System.ClientModel at 114-115 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t8-taskmaster-redirect.<timestamp>.md`.
- [x] [P1-T9] EDIT-FIZZLER `TaskTree/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t9-tasktree-redirect.<timestamp>.md`.
- [x] [P1-T10] EDIT-FIZZLER `TaskVisualization/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t10-taskvisualization-redirect.<timestamp>.md`.
- [x] [P1-T11] EDIT-FIZZLER `TaskVisualization.Test/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.16.0.0. Artifact `evidence/qa-gates/p1-t11-taskvisualization-test-redirect.<timestamp>.md`.
- [x] [P1-T12] EDIT-FIZZLER `ToDoModel/app.config` lines 51-52; System.ClientModel at 107-108 stays 1.3.0.0. Artifact `evidence/qa-gates/p1-t12-todomodel-redirect.<timestamp>.md`.
- [x] [P1-T13] EDIT-FIZZLER `ToDoModel.Test/app.config` lines 46-47; System.ClientModel at 102-103 stays 1.16.0.0. Artifact `evidence/qa-gates/p1-t13-todomodel-test-redirect.<timestamp>.md`.
- [x] [P1-T14] EDIT-FIZZLER `UtilitiesCS.Test/app.config` lines 46-47; System.ClientModel at 122-123 stays 1.16.0.0. Artifact `evidence/qa-gates/p1-t14-utilitiescs-test-redirect.<timestamp>.md`.
- [x] [P1-T15] Run the suite after the config edits over `tests/scripts/dependencies`: CMD-JUNIT-DELETE, CMD-POSHQC-TEST, CMD-JUNIT-READ (JUNIT lines transcribed under the CMD-JUNIT-READ long-line rule). Acceptance: `ok` true; JUNIT-ROOT `failures=0`; JUNIT-SUITE `BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0`; `RepositoryTreeConsistency.Tests.ps1` and every other suite equal their P0-T12 `BASELINE-SUITE` counts with `failures=0`; `JUNIT-NOTPASSED: none` (recorded only when CMD-JUNIT-READ (a) matched exactly one line in this task; a no-match from (a) is a failed read and the task is not complete); the `COVERAGE-MEASUREMENT:` literal line is present. `EXIT_CODE: 0`. This artifact is the AC5 pass-after evidence and, through test 14, the AC4 evidence; a red test 14 here means the known-debt set has a member P0-T8 did not measure: STOP: KNOWN-DEBT-DRIFT with the JUNIT-MESSAGE recorded. Artifact `evidence/regression-testing/p1-t15-pass-after.<timestamp>.md`.
- [x] [P1-T16] Sweep verification over `*/app.config` (AC1 evidence): Grep pattern `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"` glob `*/app.config` count mode → 13 occurrences across 13 files; Grep pattern `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` glob `*/app.config` count mode → exactly 6 occurrences across exactly QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel; Grep pattern `name="Fizzler"` with `-A 1` → 13 blocks all followed by `1.3.1.0` in both attributes; `git diff --name-only <BASE_SHA> -- '*/app.config'` lists exactly the 11 Write Set config paths, paired with `git status --porcelain -- '*/app.config'` showing exactly 11 ` M` lines for the same paths. Acceptance: all four observations hold. Artifact `evidence/qa-gates/p1-t16-sweep-verification.<timestamp>.md`.
- [x] [P1-T17] Unsafe re-verification after the edits over `*/app.config` (AC2 evidence): Grep pattern `name="System.Runtime.CompilerServices.Unsafe"` with `-A 1` → 17 blocks all `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"`; `git diff <BASE_SHA> -- '*/app.config'` contains no content line (beginning `-` or `+` after the header pairs) that contains `Unsafe`, and its 22 content lines are the 11 removed and 11 added Fizzler redirect lines. Acceptance: both hold. Artifact `evidence/qa-gates/p1-t17-unsafe-unchanged.<timestamp>.md`.

### Phase 2 — Final QC loop, footprint gate, acceptance check-off and reduced audit

The loop is P2-T1, P2-T2, P2-T3 in order. If P2-T1 rewrote a file, or P2-T2 returned `ok` false, or P2-T3 reported any failure, the executor fixes only Write Set PowerShell files (a rewrite of a file outside the Write Set is pre-existing drift: record it, restore with `git checkout -- <path>`, continue) and restarts at P2-T1 as iteration N+1 with `.iter<N+1>` artifacts. P2-T4 closes the loop only when one iteration shows no rewrite, `ok` true and zero failures together. No step may be recorded SKIPPED.

- [x] [P2-T1] Final QC format step (toolchain step 1) over `scripts/dependencies` and `tests/scripts/dependencies`: CMD-HASHSET, CMD-POSHQC-FORMAT, CMD-HASHSET. Acceptance on the terminal iteration: `ok` true with the 2-folder summary literal; the before and after hash sets are identical (16 entries, both new files present); the artifact records `REWRITE-COUNT: 0`. On a non-terminal iteration a differing Write Set hash is recorded as `REWRITE: <path>` and the loop restarts. Artifact `evidence/qa-gates/p2-t1-poshqc-format.iter<N>.<timestamp>.md`.
- [x] [P2-T2] Final QC analyze step (toolchain step 2) over `scripts/dependencies` and `tests/scripts/dependencies`: CMD-POSHQC-ANALYZE. Acceptance on the terminal iteration: `ok` true with the 2-folder summary literal and the `GATE-SUBSTITUTION:` line. `ok` false: fix the new files only and restart at P2-T1. Type checking is not applicable to PowerShell (`.claude/rules/powershell.md` line 17) and is recorded as such in the artifact. Artifact `evidence/qa-gates/p2-t2-poshqc-analyze.iter<N>.<timestamp>.md`.
- [x] [P2-T3] Final QC test step (toolchain step 4) over `tests/scripts/dependencies`: CMD-JUNIT-DELETE, CMD-POSHQC-TEST, CMD-JUNIT-READ (JUNIT lines transcribed under the CMD-JUNIT-READ long-line rule). Acceptance on the terminal iteration: `ok` true; JUNIT-ROOT `failures=0`; exactly 9 JUNIT-SUITE lines; `BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0`; every other suite equals its P0-T12 `BASELINE-SUITE` count with `failures=0 skipped=0`; `JUNIT-NOTPASSED: none` (recorded only when CMD-JUNIT-READ (a) matched exactly one line in this task; a no-match from (a) is a failed read and the task is not complete); the `COVERAGE-MEASUREMENT:` literal line is present and no numeric coverage percentage appears in the artifact. `EXIT_CODE: 0`. Artifact `evidence/qa-gates/p2-t3-poshqc-test.iter<N>.<timestamp>.md`.
- [x] [P2-T4] Loop closure record for the QC toolchain under `evidence/qa-gates/`: list every iteration run (P2-T1, P2-T2, P2-T3 artifact names) and name the terminal iteration N on which P2-T1 recorded `REWRITE-COUNT: 0`, P2-T2 `ok` true and P2-T3 `failures=0` together. Acceptance: the three terminal-iteration artifacts exist (Glob) and carry those values. Artifact `evidence/qa-gates/p2-t4-loop-closure.<timestamp>.md`.
- [x] [P2-T5] Per-function test enumeration for `scripts/dependencies/BindingRedirectVerification.psm1` (AC6 second clause): read the module's `Export-ModuleMember` list and `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`, and record for `Find-StaleBindingRedirect` the positive-path tests (2, 3, 5, 8, 13, 14), negative-path tests (1, 4, 9), edge-path tests (6, 7) and for `ConvertTo-ReferenceVersionMap` the positive (10), negative (11) and edge (12, one assembly at two versions across several project texts) tests, each by its verbatim It name. Acceptance: both exported names appear with at least one positive, one negative and one edge It each, and every cited It name is present in the test file (Grep `-F`, count 1). Artifact `evidence/qa-gates/p2-t5-function-test-map.<timestamp>.md`.
- [x] [P2-T6] File-size audit over `scripts/dependencies/` and `tests/scripts/dependencies/` after the terminal format pass: CMD-LINECOUNT for the two new files and the four P0-T9 neighbours. Acceptance: `BindingRedirectVerification.psm1` below 500 (record the value with `TARGET-BAND: 100-150` and whether it lies inside), `BindingRedirectVerification.Tests.ps1` below 500, and the four neighbour counts equal their P0-T9 values (they are not edited). Markdown under the feature folder is exempt from the 500-line cap per `.claude/rules/general-code-change.md`. Artifact `evidence/qa-gates/p2-t6-file-size-audit.<timestamp>.md`.
- [x] [P2-T7] No-C#-toolchain scope proof over the worktree: `git diff --name-only <BASE_SHA> -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '*packages.config'` prints nothing, paired with `git status --porcelain -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '*packages.config'` printing nothing (the leading `*` makes the pathspec match a packages.config in any project directory; a bare `packages.config` matches only the repository root). Acceptance: both empty; the artifact states `CSHARP-TOOLCHAIN: not applicable; no C# source, project, solution or manifest file changed (D11)`. Artifact `evidence/qa-gates/p2-t7-no-csharp-scope.<timestamp>.md`.
- [x] [P2-T8] Footprint assertion over the worktree (CMD-FOOTPRINT): the evaluated set (C6) must equal the union of the 11 Write Set configs, `scripts/dependencies/BindingRedirectVerification.psm1`, `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` and paths under `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/`. Acceptance: every evaluated path is in that union (any other path is STOP: FOOTPRINT, never reverted by this executor) and, as the positive control, all 13 named source paths are present in the evaluated set. Both captures are recorded verbatim per C4; the agent-memory subtraction is stated by composition, not by count. Artifact `evidence/qa-gates/p2-t8-footprint.<timestamp>.md`.
- [x] [P2-T9] Check off AC1 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 84, `- [ ] AC1:` to `- [x] AC1:`) citing P1-T4 to P1-T14 (one-line change, `w/crlf`, BOM preserved per file), P1-T16 (13 at 1.3.1.0, 11 changed paths) and P0-T7. Acceptance: Grep count of `^- \[x\] AC1:` in issue.md is 1 and the criterion text after the marker is unchanged. Artifact `evidence/qa-gates/p2-t9-ac1-checkoff.<timestamp>.md`.
- [x] [P2-T10] Check off AC2 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 85) citing P0-T6 and P1-T17. Acceptance: Grep count of `^- \[x\] AC2:` is 1 and the text is unchanged. Artifact `evidence/qa-gates/p2-t10-ac2-checkoff.<timestamp>.md`.
- [x] [P2-T11] Check off AC3 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 86) citing P1-T1, P1-T2 and the P2-T3 terminal pass of tests 1 (negative control), 2 (positive control) and 6 (examined count). Acceptance: Grep count of `^- \[x\] AC3:` is 1 and the text is unchanged. Artifact `evidence/qa-gates/p2-t11-ac3-checkoff.<timestamp>.md`.
- [x] [P2-T12] Check off AC4 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 87) citing P0-T8, P1-T15 and the P2-T3 terminal pass of test 14 (15 pairs, none Fizzler or Unsafe, 3 unverifiable names). Acceptance: Grep count of `^- \[x\] AC4:` is 1 and the text is unchanged. Artifact `evidence/qa-gates/p2-t12-ac4-checkoff.<timestamp>.md`.
- [x] [P2-T13] Check off AC5 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 88) citing P1-T3 (fail-before, `but got 11`) and P1-T15 (pass-after). Acceptance: Grep count of `^- \[x\] AC5:` is 1 and the text is unchanged. Artifact `evidence/qa-gates/p2-t13-ac5-checkoff.<timestamp>.md`.
- [x] [P2-T14] Check off AC6 in `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md` (line 89) citing P2-T1 to P2-T5 and quoting the AC6 literal `is not measured locally` as the authority for the CI coverage deferral (D8). Acceptance: Grep count of `^- \[x\] AC6:` is 1 and the text is unchanged; the artifact carries `COVERAGE-SOURCE: CI`. Artifact `evidence/qa-gates/p2-t14-ac6-checkoff.<timestamp>.md`.
- [x] [P2-T15] Acceptance-criteria status summary and plan reconciliation for `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/`: Grep count of `^- \[x\] AC[1-6]:` in issue.md is 6 and of `^- \[ \] AC[1-6]:` is 0; the artifact carries the `### Acceptance Criteria Status` block (Source, Total AC items: 6, Checked off: 6, Remaining: 0). Acceptance: both counts hold. Artifact `evidence/qa-gates/p2-t15-ac-status.<timestamp>.md`.
- [x] [P2-T16] Follow-up note for the coordinator under `evidence/other/`: the 15 known-debt pairs of section 7 with their config counts and Reference versions, the 3 unverifiable names, the statement that correcting them is out of scope for issue 953, and the statement that this plan created no issue and no potential entry (the coordinator promotes). Acceptance: the artifact lists all 15 pairs and 3 names and carries `FOLLOW-UP: known-debt redirect correction, 15 pairs, 137 entries; promotion by the coordinator`. Artifact `evidence/other/p2-t16-known-debt-followup.<timestamp>.md`.
- [x] [P2-T17] Reduced-audit handoff index under `evidence/other/` for the QC and test evidence: `BASE_SHA:`, the Write Set, `COVERAGE-SOURCE: CI`, every `GATE-SUBSTITUTION:` line used, the budget outcome (no denial, or the denial record), the terminal loop iteration N, the final suite counts, and the path of every artifact written by P0-T1 through P2-T16 (bounded there; this artifact names itself only as the index). The index cites D8 as this plan's exception to the atomic-plan-contract Coverage Evidence Contract: no numeric baseline or post-change coverage figure is recorded locally because no local Pester coverage route instruments `scripts/dependencies`, and the figure is read from the CI Pester job. Acceptance: every artifact path named by P0-T1 through P2-T16 exists (Glob) and is listed; the index carries the literals `AUDIT-MODE: reduced (minor-audit)` and `COVERAGE-EXCEPTION: D8 (no local Pester coverage route instruments scripts/dependencies)`. Artifact `evidence/other/p2-t17-reduced-audit-handoff.<timestamp>.md`.

## 11. Traceability

| AC | Implementation | Tests | Evidence |
|---|---|---|---|
| AC1 | P1-T4 to P1-T14 (eleven EDIT-FIZZLER edits) | test 13; P1-T16 Grep counts | P0-T4, P0-T5, P0-T7, P1-T4 to P1-T14, P1-T16, P2-T9 |
| AC2 | none (verified, not edited) | RepositoryTreeConsistency.Tests.ps1 lines 92-110 (SVGControl Unsafe) | P0-T6, P1-T17, P2-T10 |
| AC3 | P1-T1 (`Find-StaleBindingRedirect`) | tests 1, 2, 6 (and 3 to 9) | P1-T1, P1-T2, P2-T3, P2-T11 |
| AC4 | P1-T1 (`ConvertTo-ReferenceVersionMap`, detector) | test 14 | P0-T8, P1-T15, P2-T3, P2-T12 |
| AC5 | P1-T3 before, P1-T15 after | test 13 | P1-T3, P1-T15, P2-T13 |
| AC6 | P2-T1 to P2-T5 | all 14 tests; P2-T5 map | P2-T1, P2-T2, P2-T3, P2-T4, P2-T5, P2-T14 |

## 12. Residual risks

1. **Edit tool line-ending handling.** The Edit tool was used on a CRLF, BOM-carrying app.config in issue 929 (P1-T5 evidence: numstat `2	2`, indentation unchanged). If a future tool version normalises terminators, CMD-EOL reads `w/lf` or `w/mixed` and the EDIT-FIZZLER task fails; the recovery is `git checkout -- <path>` and STOP: EDIT-MISMATCH, not a different write mechanism.
2. **Budget denial.** A spent production or test slot stops Phase 1 at P1-T1 or P1-T2 (D9). The fallback is not executed without the orchestrator's ruling.
3. **Dependabot churn.** A bot merge that bumps a csproj Reference without its redirect, or corrects a known-debt redirect, fails test 14 by design; P0-T8 detects such drift before any edit and stops the run.
4. **Coverage figure.** No local Pester line-coverage figure exists for `scripts/dependencies` through the MCP route. The CI Pester job supplies it on the pushed head; the handoff carries `COVERAGE-SOURCE: CI`. A reviewer who requires a local figure must raise it against AC6's wording, not against this plan.
5. **MCP test tool exit.** The tool's payload `ok` has been observed false with `Command exited with code 4.` on a red suite and true on a green one in this repository; its process exit code is not observable and is not claimed (C3).
6. **Hook-required preflight line.** The planner's output carries a `PREFLIGHT: REVISIONS REQUIRED` line because the SubagentStop hook bounds the internal-review record with it; the line records that executor preflight is outstanding and is not a discovered defect or a self-approval.
7. **pwsh refusal under worktree isolation.** The round 1 preflight reviewer observed the Bash tool refuse a pwsh one-liner in this worktree while `od`, `rm -f` and `git -C` ran. CMD-BOM and CMD-JUNIT-DELETE carry those fallbacks (C5); a run that uses them records `PWSH-REFUSED:` with the tool message and is not a deviation.

## 13. Planner internal review

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler/app.config | lines 50-51 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.3.0.0
CITATION: QuickFiler.Test/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.16.0.0
CITATION: SVGControl.Test/app.config | lines 18-19 Fizzler 1.3.0.0; 46-47 System.ClientModel 1.16.0.0
CITATION: Tags/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.3.0.0
CITATION: TaskMaster/app.config | lines 50-51 Fizzler 1.3.0.0; 114-115 System.ClientModel 1.3.0.0
CITATION: TaskTree/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.3.0.0
CITATION: TaskVisualization/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.3.0.0
CITATION: TaskVisualization.Test/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.16.0.0
CITATION: ToDoModel/app.config | lines 51-52 Fizzler 1.3.0.0; 107-108 System.ClientModel 1.3.0.0
CITATION: ToDoModel.Test/app.config | lines 46-47 Fizzler 1.3.0.0; 102-103 System.ClientModel 1.16.0.0
CITATION: UtilitiesCS.Test/app.config | lines 46-47 Fizzler 1.3.0.0; 122-123 System.ClientModel 1.16.0.0
CITATION: SVGControl/app.config | lines 14-15 Fizzler 1.3.1.0 (not edited); no System.ClientModel block
CITATION: UtilitiesCS/app.config | lines 51-52 Fizzler 1.3.1.0 (not edited); 107-108 System.ClientModel 1.16.0.0
CITATION: VBFunctions.Test/app.config | lines 98-99 System.ClientModel 1.16.0.0 (with TaskMaster.Test 98-99, TaskTree.Test 222-223, Tags.Test 222-223, UtilitiesCS 107-108 and the five Write Set test configs: 10 blocks at 1.16.0.0, 16 ClientModel blocks in all)
CITATION: scripts/dependencies | 6 .ps1/.psm1 files by Glob; with the 8 under tests/scripts/dependencies, CMD-HASHSET enumerates 14 at planning time
CITATION: SVGControl/SVGControl.csproj | line 58 Reference Fizzler, Version=1.3.1.0
CITATION: UtilitiesCS/UtilitiesCS.csproj | line 65 Reference Fizzler, Version=1.3.1.0; line 305 Reference System.ClientModel, Version=1.16.0.0
CITATION: scripts/dependencies/PackageGraph.psm1 | ConvertFrom-ProjectFileText line 215, AllowEmptyString line 230, empty-text throw lines 234-236; ConvertFrom-AppConfigText line 285; root check lines 304-305; Export-ModuleMember lines 457-465; 465 lines
CITATION: scripts/dependencies/ProjectConsistency.psm1 | header ownership lines 6-12; import without -Force lines 33-44; Invoke-BindingRedirectReconciliation lines 271-375; 381 lines
CITATION: scripts/dependencies/ConsistencyVerifier.psm1 | 499 lines; DetectionResult shape lines 46-54
CITATION: tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 | RepoRoot line 4; import line 5 in the form Import-Module (Join-Path $script:RepoRoot '<module path>') -Force; discovery guard line 74; examined guard lines 87-89; SVGControl redirect test lines 92-110; 152 lines
CITATION: tests/scripts/dependencies/PackageGraph.Tests.ps1 | ConvertTo-CrLf lines 10-13; app.config fixture lines 41-69; project fixture lines 71-85; whitespace-only project text rejection test line 268
CITATION: .github/workflows/_pester.yml | Run.Path line 41; CodeCoverage.Path line 45; PESTER line 51; COVERAGE line 64; floor line 71
CITATION: .claude/rules/powershell.md | toolchain lines 15-20; change budget lines 37-41; seams lines 43-52
CITATION: .claude/hooks/enforce-powershell-batch-budget.ps1 | caps lines 10-11; state file line 14; override variables lines 37-40; deny lines 42-45
CITATION: docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md | Work Mode line 9; Acceptance Criteria heading line 82; AC1-AC6 lines 84-89
CITATION: docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t5-redirects-fixed.2026-09-28T20-01.md | Edit-tool redirect change observed as numstat 2 2
CITATION: docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t15-poshqc-test-mcp.2026-09-28T20-01.md | run_poshqc_test payload literal and JUnit suite lines
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T4..P1-T14 EDIT-FIZZLER | TESTS: test 13 names 1.3.1.0 in every Fizzler binding redirect; P1-T16 greps | EVIDENCE: P0-T4, P0-T5, P0-T7, P1-T4..P1-T14, P1-T16, P2-T9
AC-MAPPING: AC2 | IMPLEMENTATION: none, verified not edited | TESTS: RepositoryTreeConsistency.Tests.ps1 lines 92-110 | EVIDENCE: P0-T6, P1-T17, P2-T10
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T1 Find-StaleBindingRedirect | TESTS: tests 1, 2, 6 plus 3-9 | EVIDENCE: P1-T1, P1-T2, P2-T3, P2-T11
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1 ConvertTo-ReferenceVersionMap and detector | TESTS: test 14 known-debt ratchet | EVIDENCE: P0-T8, P1-T15, P2-T3, P2-T12
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T3 fail-before and P1-T15 pass-after | TESTS: test 13 | EVIDENCE: P1-T3, P1-T15, P2-T13
AC-MAPPING: AC6 | IMPLEMENTATION: P2-T1..P2-T5 QC loop and function-test map | TESTS: all 14 tests through run_poshqc_test | EVIDENCE: P2-T1, P2-T2, P2-T3, P2-T4, P2-T5, P2-T14
UNRESOLVED-GAPS: NONE
PREFLIGHT: REVISIONS REQUIRED

The PREFLIGHT line above is the hook-required terminator of the bounded record. It records that executor preflight has not yet run; it is not a discovered defect and not a self-approval. The orchestrator runs the validator and the atomic-executor preflight.

Revision round 1 (2026-10-02T02-10), applied against the round 1 preflight report and re-derived against the tree in the same pass: section 4 row 4 and P0-T5 (ClientModel 16 blocks, 6 plus 10); CMD-HASHSET and P2-T1 (14 and 16 hash entries); P1-T1 (Import-Module gates replace the `-Force` count); P0-T8(b) (`-o` pattern reads the version; multiline count read per file); C5, CMD-BOM, CMD-JUNIT-DELETE and residual risk 7 (`git -C`, absolute operands, pwsh-refused fallbacks); CMD-EOL, D3, P0-T7 and EDIT-FIZZLER item (3) (space-padded fields, one tab before the path); P2-T7 (`*packages.config`); section 9 and P1-T2 (exact import lines, `$script:` fixtures); P1-T3 and section 9 test 14 (16-entry observed list); P2-T5 (tests 10, 11, 12); section 8 (`[AllowEmptyString()]` and the corrected help sentence, with the note before the fence); P0-T12 and P2-T17 (D8 named as the Coverage Evidence Contract exception). Task count unchanged at 47.

Revision round 2 (2026-10-02T02-45), applied against the round 2 preflight report and re-derived against the tree in the same pass: CMD-JUNIT-READ (`-n` on every Grep, the Read-tool transcription of the `failure` message, and the long-line rule for any line the Grep tool omits; the planner re-observed the omission on version 1.2 of this plan with Grep pattern `^.{500,}` and read section 9 test 14 in full with the Read tool), with P0-T12, P1-T3, P1-T15 and P2-T3 stating that their JUNIT lines follow the rule; P1-T3 (a test 13 or 14 failure whose message does not begin `Expected ` and contain `, but got ` is a defect in a new file and re-runs as `.iter<N>`); section 7 and section 9 tests 13 and 14 (`before Phase 1` replaced by the P1-T3 position, because tests 13 and 14 are authored at P1-T2 inside Phase 1; no other occurrence of the phrase exists in the plan, checked by Grep); the section 13 CITATION for VBFunctions.Test/app.config (UtilitiesCS 107-108 added to the 1.16.0.0 list; all sixteen System.ClientModel blocks re-derived by Grep `name="System.ClientModel"` with `-A 1`: 1.3.0.0 at QuickFiler 102-103, Tags 102-103, TaskMaster 114-115, TaskTree 102-103, TaskVisualization 102-103, ToDoModel 107-108; 1.16.0.0 at QuickFiler.Test 102-103, SVGControl.Test 46-47, TaskVisualization.Test 102-103, ToDoModel.Test 102-103, UtilitiesCS.Test 122-123, TaskMaster.Test 98-99, TaskTree.Test 222-223, Tags.Test 222-223, UtilitiesCS 107-108, VBFunctions.Test 98-99); P1-T2 (`-Force$` becomes `-Force\r?$` on both import-line gates, because ripgrep's `$` does not match before a carriage return; the only other `$`-anchored gate over a file the executor writes is P0-T3's `^## Acceptance Criteria$` on issue.md, which the planner left unchanged after observing that issue.md carries zero carriage returns and that the pattern matches once; the `\r$` patterns of P0-T7 and section 4 match the carriage return deliberately). Task count unchanged at 47.

Revision round 3 (2026-10-02T03-20), applied against the round 3 preflight report and re-derived against the tree in the same pass: C5 and CMD-JUNIT-READ (every Grep over `artifacts/pester/pester-junit.xml` passes the absolute file path and no glob, because the Grep tool honours `.gitignore` when its path is a directory and `artifacts/` is ignored at `.gitignore` line 57; the planner reproduced both outcomes in a sibling worktree at the same `.claude/worktrees/agent-<id>` depth, where Grep pattern `testsuites` with the worktree root as path and the glob `artifacts/pester/pester-junit.xml` returned no match and the same pattern with the absolute file path returned the `<testsuites ` line; the Glob tool with the same worktree root as path returned that gitignored file, so the Glob existence check of CMD-JUNIT-READ and the absence check of CMD-JUNIT-DELETE keep the root path and CMD-JUNIT-DELETE now states it); CMD-JUNIT-READ (`JUNIT-NOTPASSED: none` requires (a) to have matched exactly one line in the same task; a no-match from (a) is a failed read), with the same condition stated in P0-T12, P1-T15 and P2-T3, the three tasks that record the none outcome (P1-T3 expects two not-passed testcases and does not record it); CMD-JUNIT-READ and P0-T12 (the testsuite-line omission prediction replaced by the round 3 measurement: 403 to 439 characters for a dependencies testsuite line at the agent-worktree depth, below the omission threshold; the planner measured a sibling worktree's `tests/scripts/vscode` testsuite line at 390 to 439 characters with Grep patterns `^.{390,}` and `^.{440,}`, printed in full; P1-T15 and P2-T3 carried no occurrence of the prediction, checked by Grep for `test-file path twice`); CMD-JUNIT-READ (the length of section 9 test 14 restated as 2,000 to 2,099 characters at version 1.3 from Grep `^.{2000,}` matching and `^.{2100,}` not matching that line, re-measured after this round's edits; the long-line rule names the omitted lines by definition and test number and quotes no plan line numbers, so no line-number restatement was needed; the failure-line estimates of about 550 and 1,700 characters replaced by the constructed test 13 message length of about 333 characters and an unobserved expectation for test 14); P1-T3 (the Read tool is used for the JUNIT-MESSAGE lines because CMD-JUNIT-READ (c) requires it for every not-passed testcase, whatever the line length). The planner also confirmed in this pass that Grep patterns beginning with `<` (`<testsuites `, `<testsuite name=`, `<bindingRedirect`) match through the Grep tool when the pattern is passed as written (`<bindingRedirect` over `*/app.config` re-derived at 1176 across 17 files). Task count unchanged at 47.
