# 2026-08-10-csharp-toolchain-gate-fidelity-512 (Plan)

- **Issue:** #512 (also closes #492, #509, #522)
- **Parent:** epic `build-ci-coverage-gate-fidelity` (Lane A, Wave 0)
- **Owner:** drmoisan
- **Work Mode:** full-bug (authoritative acceptance-criteria source is `spec.md`)
- **Last Updated:** 2026-08-10T14-08
- **Status:** Ready for preflight
- **Version:** 1.0

**Fail-closed evidence rule:** every evidence-producing task names its artifact path. A task whose
artifact is absent, or whose artifact omits `Timestamp:`, `Command:`, `EXIT_CODE:` or
`Output Summary:`, remains unchecked. Missing evidence yields BLOCKED or INCOMPLETE, never PASS.

**Evidence location invariant:** all evidence resolves under
`docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/<kind>/`. Paths under
`artifacts/` are forbidden for evidence.

## Notation and command conventions

`FEATURE` = `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512`.
`<TS>` = the ISO-8601 `yyyy-MM-ddTHH-mm` timestamp at the moment the artifact is written.
`MERGE_BASE` = the merge-base SHA resolved and recorded by task [P0-T3]. Never a hard-coded SHA.

All C# tooling runs through `pwsh -NoProfile -Command "..."` with absolute executable paths. The Bash
tool mangles MSBuild switches (`/m` becomes `M:/`) and must not be used for MSBuild or CSharpier.

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`
(not on PATH). `DOTNET` = `./.dotnet-sdk/dotnet.exe` (repo-pinned SDK 8.0.205).

Canonical command strings (defined in `spec.md` § "Canonical replacement strings"):

| Alias | Command |
|---|---|
| FORMAT-APPLY | `dotnet tool run csharpier format .` |
| FORMAT-VERIFY | `dotnet tool run csharpier check .` |
| ANALYZE | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` |
| TYPECHECK | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` |
| DOC-ANALYZE | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (the defective documented form, measured only) |
| DOC-TYPECHECK | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` (the defective documented form, measured only) |
| DEBT-PROBE | TYPECHECK plus `/p:Nullable=enable` (AC12 measurement only; expected EXIT 1) |

Every MSBuild invocation in this plan appends `/nologo /v:m /fl "/flp:logfile=coverage/<name>.log;verbosity=normal"`.
`coverage/*` and `*.log` are gitignored, so raw logs never enter the diff.

**Non-vacuity assertion (mandatory for every MSBuild step).** Count occurrences of the literal string
`Skipping target "CoreCompile"` in the `/fl` log and record the count in the artifact. Zero is the
pass condition for a genuine compile. Do **not** count `csc.exe` occurrences (zero at
`verbosity=normal` even for genuine compiles) and do **not** count `CoreCompile:` header lines (they
print even when the target is skipped). `spec.md` records this as a formal deviation from AC2's
parenthetical; every artifact for an MSBuild step must restate the deviation so the substitution is
auditable.

**Error counting.** Use MSBuild's own `N Error(s)` summary line, or count only node-prefixed
(for example `19>`) error lines. A naive `Select-String 'error CS'` double-counts (390 instead of 195).

**PoshQC analyze is RED at the merge base.** 16 PSScriptAnalyzer findings exist before any change,
three of them in `scripts/vscode/Invoke-VSBuild.ps1` itself (PSUseSingularNouns at lines 47 and 78,
PSAvoidUsingWriteHost at line 137). See
`FEATURE/evidence/baseline/baseline-powershell-toolchain.2026-08-10T15-40.md`. No task in this plan
may assert `EXIT_CODE: 0` for `mcp__drm-copilot__run_poshqc_analyze`; acceptance is **no new finding
relative to the Phase 0 baseline count**. Do not rename `Get-MSBuildBuildArguments` or
`Get-RequestedMSBuildProperties`, do not add a new plural-noun function, and do not add a new
`Write-Host`. The `Write-Warning` introduced by spec row 23 is a different rule and is permitted.

**C# scope limitation.** No C# source file changes in this feature. The C# toolchain steps execute as
verification that the documented commands work (AC7), not as change-validation. No C# coverage capture
and no MSTest/vstest run is planned; `Run MSTest suite with coverage` is failing on `main` for reasons
unrelated to this feature
(`FEATURE/evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md`). Task [P6-T9] records
that non-execution explicitly so it is a documented deviation rather than a silent skip. PowerShell is
the only language whose source changes, so `.claude/rules/powershell.md` coverage obligations apply
and the C# coverage obligations do not.

**Protected files.** `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` must not be touched. `AGENTS.md`, `.agents/**`,
`.github/instructions/**`, `.github/agents/**`, `.codex/**` (SD1) and `.github/workflows/ci.yml` (SD4)
must not be touched.

**Line numbers are advisory.** Every site is located by its exact current text per the per-site
replacement tables in `spec.md`; line numbers in this plan are navigation aids only.

### Phase 0 — Policy Reads, Environment Bootstrap, and Baseline Capture

- [ ] [P0-T1] Read the four core policy documents in the `policy-compliance-order` sequence — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, then the language rules `.claude/rules/csharp.md` and `.claude/rules/powershell.md` — and write `FEATURE/evidence/baseline/phase0-instructions-read.<TS>.md` recording `Timestamp:`, `Policy Order:` and the explicit list of files read.
  - Acceptance: the artifact exists and names all five files in the stated order, plus the skills read (`policy-compliance-order`, `atomic-plan-contract`, `evidence-and-timestamp-conventions`, `acceptance-criteria-tracking`).
- [ ] [P0-T2] Read the feature requirement inputs and the governance authorization — `FEATURE/spec.md`, `FEATURE/issue.md`, `FEATURE/research/toolchain-gate-fidelity.2026-08-10T14-40.md`, the seven artifacts under `FEATURE/evidence/`, and `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` § "Execution Authorization Required" — and record them in `FEATURE/evidence/baseline/phase0-feature-inputs-read.<TS>.md`.
  - Acceptance: the artifact quotes the epic's authorization sentence, names the three authorized edit targets (`CLAUDE.md`, `.claude/rules/csharp.md`, `.claude/skills/csharp-qa-gate/SKILL.md`), and names the protected/excluded files listed above.
- [ ] [P0-T3] Resolve and record the git baseline in `FEATURE/evidence/baseline/baseline-git-context.<TS>.md`: current branch, `git rev-parse HEAD`, the `MERGE_BASE` SHA resolved by the `pr-base-branch-merge-base` procedure (candidate branches enumerated with merge-base timestamps), `git status --porcelain`, and the SHA-256 of `CLAUDE.md`, `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`.
  - Acceptance: `MERGE_BASE` is a resolved SHA with its selected branch name and merge-base timestamp recorded; every later diff gate cites this artifact. Do not pin the HEAD SHA as an expectation elsewhere in the plan.
- [ ] [P0-T4] Run `pwsh ./scripts/vscode/Install-RepoDotNetSdk.ps1` and write `FEATURE/evidence/baseline/baseline-bootstrap-sdk.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records the installed SDK version and that `./.dotnet-sdk/dotnet.exe` exists.
- [ ] [P0-T5] Run `./.dotnet-sdk/dotnet.exe tool restore` and write `FEATURE/evidence/baseline/baseline-bootstrap-tool-restore.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records the restored CSharpier version and confirms it equals the version pinned in the repo-root manifest `./dotnet-tools.json` (expected 1.2.6).
- [ ] [P0-T6] Run `pwsh ./scripts/vscode/Invoke-Restore.ps1` and write `FEATURE/evidence/baseline/baseline-bootstrap-nuget-restore.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` states that all 18 projects restored.
- [ ] [P0-T7] Run the documented defective format command `./.dotnet-sdk/dotnet.exe tool run csharpier .` and write `FEATURE/evidence/baseline/baseline-csharpier-documented.<TS>.md`.
  - Acceptance: `EXIT_CODE: 1` with the `Required command was not provided.` / `Unrecognized command or argument '.'` rejection recorded verbatim. A `EXIT_CODE: 0` here contradicts the defect and halts the plan.
- [ ] [P0-T8] Run FORMAT-VERIFY (`./.dotnet-sdk/dotnet.exe tool run csharpier check .`) and write `FEATURE/evidence/baseline/baseline-csharpier-check.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records the `Checked N files` count, establishing that the tree is format-clean before any edit.
- [ ] [P0-T9] Run DOC-ANALYZE cold (immediately after the restore in [P0-T6]) with `/fl "/flp:logfile=coverage/baseline-doc-analyze-cold.log;verbosity=normal"` and write `FEATURE/evidence/baseline/baseline-doc-analyze-cold.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; the artifact records elapsed time and the `Skipping target "CoreCompile"` count from `coverage/baseline-doc-analyze-cold.log`.
- [ ] [P0-T10] Re-run DOC-ANALYZE immediately (warm) with `/fl "/flp:logfile=coverage/baseline-doc-analyze-warm.log;verbosity=normal"` and write `FEATURE/evidence/baseline/baseline-doc-analyze-warm.<TS>.md` proving Defect C.
  - Acceptance: `EXIT_CODE: 0` with a `Skipping target "CoreCompile"` count **greater than zero** (expected 18 of 18) and elapsed time under 5 s. A zero skip count contradicts the measured defect and halts the plan for re-scoping.
- [ ] [P0-T11] Run DOC-TYPECHECK warm with `/fl "/flp:logfile=coverage/baseline-doc-typecheck-warm.log;verbosity=normal"` and write `FEATURE/evidence/baseline/baseline-doc-typecheck-warm.<TS>.md` proving Defect A's false-pass mode.
  - Acceptance: `EXIT_CODE: 0` with a `Skipping target "CoreCompile"` count greater than zero (expected 18 of 18). This is the vacuous pass the feature removes.
- [ ] [P0-T12] Run ANALYZE (the corrected form) with `/fl "/flp:logfile=coverage/baseline-analyze-rebuild.log;verbosity=normal"` and write `FEATURE/evidence/baseline/baseline-analyze-rebuild.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, `Skipping target "CoreCompile"` count **0**, elapsed time recorded. Establishes the corrected analyzer command is green before any edit.
- [ ] [P0-T13] Run DEBT-PROBE with `/fl "/flp:logfile=coverage/baseline-nullable-debt.log;verbosity=normal"` and write the AC12 measurement to `FEATURE/evidence/baseline/baseline-nullable-debt.<TS>.md`.
  - Acceptance: `EXIT_CODE: 1`; the artifact records MSBuild's `N Error(s)` figure (not a naive grep count), the per-diagnostic `CS86xx` breakdown, the owning `.csproj` attribution, the `CoreCompile` execution count, and the explicit statement that the figure is a **lower bound** because the build aborts before dependents compile. This build deletes every project's `bin`/`obj`; [P0-T14] restores them.
- [ ] [P0-T14] Run TYPECHECK (the corrected form) with `/fl "/flp:logfile=coverage/baseline-typecheck-rebuild.log;verbosity=normal"` and write `FEATURE/evidence/baseline/baseline-typecheck-rebuild.<TS>.md`. This is both the pre-change positive control and the mandatory build-output restoration after [P0-T13].
  - Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, `Skipping target "CoreCompile"` count **0**. If this returns non-zero, stop and report; a red positive control invalidates the design and must not be worked around.
- [ ] [P0-T15] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]` and write `FEATURE/evidence/baseline/baseline-poshqc-analyze.<TS>.md`.
  - Acceptance: the artifact records `EXIT_CODE:` as returned (expected 1) and the **exact finding count and the full rule/file/line table** at this HEAD. This count is the sole comparison basis for [P2-T8] and [P6-T2]; the expected value is 16 per the existing baseline artifact, and any divergence must be recorded rather than reconciled to 16.
- [ ] [P0-T16] Run `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["tests/scripts/vscode"]`, coverage enabled if the tool exposes a coverage parameter, and write `FEATURE/evidence/baseline/baseline-poshqc-test.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` with the numeric passed/failed counts in `Output Summary:`; plus **either** numeric `Line Coverage:` and `Branch Coverage:` values for `scripts/vscode/Invoke-VSBuild.ps1` **or**, if the tool exposes no coverage parameter, a `CoverageCapability:` section quoting the tool's full parameter list together with a function-to-`It` coverage enumeration for `Get-MSBuildBuildArguments` and `Get-RequestedMSBuildProperties`. Placeholders such as `UNVERIFIED` are not acceptable.
- [ ] [P0-T17] Capture the pre-change divergent-site inventory into `FEATURE/evidence/baseline/baseline-site-inventory.<TS>.md` by grepping tracked files for (a) the CSharpier v0 bare-path form `csharpier\s+\.` and (b) the same-line conjunction of `/t:Build` and `Nullable=enable`, excluding `docs/features/**`, `docs/research/**`, `.claude/agent-memory/**`, `packages/**`, `.dotnet-sdk/**`, `bin/**` and `obj/**`.
  - Acceptance: the artifact records the exact grep commands, the exclusion list with its rationale (historical evidence records a past measurement; it does not document a command), and a path:line table partitioned into in-scope sites and SD1-excluded mirror sites. This table is the before-state for the AC6 gate in [P5-T11].

### Phase 1 — Red Regression Tests for the Executable Carrier

- [ ] [P1-T1] [expect-fail] Add a new `It` to the existing `Describe 'Get-MSBuildBuildArguments'` block in `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` asserting that `-Target Rebuild` emits `'/t:Rebuild'` in the same array position that the default case emits `'/t:Build'` (spec.md executable-carrier row 27).
  - Acceptance: the new `It` exists; the existing `It` at lines 23-44 that asserts the default `'/t:Build'` argument array is **left byte-identical** (spec.md explicitly corrects research D4: the default target stays `Build` so the `build:` task is unchanged).
- [ ] [P1-T2] [expect-fail] Modify the `It 'maps nullable switches to the expected MSBuild properties'` in `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` to assert `@('TreatWarningsAsErrors=true')` only, and rename it to state that `-EnableNullable` emits no MSBuild property (spec.md executable-carrier row 28).
  - Acceptance: the assertion no longer contains the string `Nullable=enable`; the sibling analyzer-switch `It` is unchanged.
- [ ] [P1-T3] [expect-fail] Run `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["tests/scripts/vscode"]` and write the failing run to `FEATURE/evidence/regression-testing/red-pester-run.<TS>.md`.
  - Acceptance: `EXIT_CODE:` non-zero; **exactly two** failing `It` names are recorded, matching the tests added in [P1-T1] and modified in [P1-T2]; the default-target `It` and both `ConvertTo-MSBuildPropertyArgument` tests pass. The artifact quotes each failure message verbatim (expected: parameter-binding failure for `-Target`, and the `Nullable=enable` array mismatch).

### Phase 2 — Executable Carrier Fix and Green Regression Run

- [ ] [P2-T1] Add the `-Target` parameter to the `param(...)` block of `scripts/vscode/Invoke-VSBuild.ps1` as `[Parameter(Mandatory = $false)] [ValidateSet('Build', 'Rebuild')] [string]$Target = 'Build'` (spec.md executable-carrier row 21).
  - Acceptance: the parameter binds; an invalid value is rejected at bind time by `ValidateSet`; the default is `Build`.
- [ ] [P2-T2] Add the same `[ValidateSet('Build','Rebuild')] [string]$Target = 'Build'` parameter to `Get-MSBuildBuildArguments` in `scripts/vscode/Invoke-VSBuild.ps1` and replace the hardcoded `'/t:Build'` array element with `"/t:$Target"` (spec.md executable-carrier row 22).
  - Acceptance: the literal `'/t:Build'` no longer appears in the function body; the argument's array position is unchanged; no new function is introduced (avoids a seventeenth PSScriptAnalyzer finding).
- [ ] [P2-T3] Replace the `if ($EnableNullable) { $properties += 'Nullable=enable' }` body in `Get-RequestedMSBuildProperties` in `scripts/vscode/Invoke-VSBuild.ps1` with the deprecation `Write-Warning` text given verbatim in spec.md executable-carrier row 23, retaining the `[switch]$EnableNullable` parameter on both the script `param(...)` block and the function (spec.md row 20 and the SD3 note).
  - Acceptance: the string `Nullable=enable` no longer appears as an emitted property; the switch still binds; the added call is `Write-Warning`, not `Write-Host`.
- [ ] [P2-T4] Add `-Target $Target` to the `Get-MSBuildBuildArguments` call site in `scripts/vscode/Invoke-VSBuild.ps1` (spec.md executable-carrier row 24).
  - Acceptance: the script-level `-Target` value reaches the argument builder; `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -NoExecute` still returns without error.
- [ ] [P2-T5] Insert `"-Target", "Rebuild"` into the args array of the `lint: TaskMaster.sln (.NET analyzers)` task in `.vscode/tasks.json`, before the `-EnableNETAnalyzers` switch (spec.md executable-carrier row 25).
  - Acceptance: `.vscode/tasks.json` remains valid JSON; the task label is unchanged; the `build:` task args are untouched.
- [ ] [P2-T6] Replace `"-EnableNullable"` with `"-Target", "Rebuild"` in the args array of the `type-check: TaskMaster.sln (nullable warnings as errors)` task in `.vscode/tasks.json`, retaining `"-TreatWarningsAsErrors"` (spec.md executable-carrier row 26).
  - Acceptance: `.vscode/tasks.json` remains valid JSON; the string `-EnableNullable` no longer appears in the file; the task label is unchanged so external references by label still resolve.
- [ ] [P2-T7] Re-run `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["tests/scripts/vscode"]` and write the green run to `FEATURE/evidence/regression-testing/green-pester-run.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; the two `It` names recorded as failing in `FEATURE/evidence/regression-testing/red-pester-run.<TS>.md` now pass; total test count is the baseline count plus one.
- [ ] [P2-T8] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]` and write `FEATURE/evidence/qa-gates/poshqc-analyze-postfix.<TS>.md`.
  - Acceptance: the finding count and the rule/file/line table are **identical to the [P0-T15] baseline** (expected 16). Zero new findings, and specifically no new PSUseSingularNouns and no new PSAvoidUsingWriteHost. `EXIT_CODE: 0` is **not** the acceptance condition and must not be asserted.

### Phase 3 — Documentation Corrections in CLAUDE.md

- [ ] [P3-T1] Replace the CSharpier block in `CLAUDE.md` § C#1 item 1 (rows 1-2 of the Tier 1 replacement table; current text located by the strings `csharpier` is file-based and formats only `*.cs`, `- Approved commands:`, `dotnet tool run csharpier .`, and the global-install alternative) with Block R1 verbatim from `FEATURE/spec.md`.
  - Acceptance: the two retained-verbatim lines (the `dotnet format` warning and the formatter-output-wins line) are byte-identical to their merge-base text; the global-install alternative is gone; the `dotnet tool restore` prerequisite and the manifest-pinning sentence are present.
- [ ] [P3-T2] Replace the analyzer "Approved commands (PowerShell)" block in `CLAUDE.md` § C#1 item 2 (replacement-table row 3) with Block R2 verbatim from `FEATURE/spec.md`.
  - Acceptance: the block contains ANALYZE exactly, plus the `/t:Rebuild`-versus-`/t:Build` rationale sentence naming `.github/workflows/ci.yml` and the cold-runner reason (AC5 in-line rationale).
- [ ] [P3-T3] Replace the type-check block in `CLAUDE.md` § C#1 item 3 (replacement-table rows 4-5) with Block R3 verbatim from `FEATURE/spec.md`.
  - Acceptance: the block contains TYPECHECK exactly; the per-file opt-in sentence replaces the "fail builds on warnings for touched code paths" line; both "must not be restored" bullets are present; **no single line contains both `/t:Build` and `Nullable=enable`** (the AC6 gate in [P5-T11] is a same-line conjunction grep, so the R3 line breaks must be preserved).
- [ ] [P3-T4] Replace the three numbered toolchain commands in `CLAUDE.md` § CUT3 (replacement-table rows 6-8, located by the strings `1. `csharpier .``, `2. msbuild TaskMaster.sln /t:Build`, `3. msbuild TaskMaster.sln /t:Build`) with the row 6-8 replacements from `FEATURE/spec.md`.
  - Acceptance: item 1 names FORMAT-APPLY with FORMAT-VERIFY as the verify form; items 2 and 3 are ANALYZE and TYPECHECK character-for-character; step 4 (`vstest.console.exe`) is unchanged.
- [ ] [P3-T5] Replace the three commands in `CLAUDE.md` § "C# Toolchain (run in this exact order)" (replacement-table rows 9-11) with the row 9-11 replacements from `FEATURE/spec.md`.
  - Acceptance: item 1 carries the "always via `dotnet tool run`, never a global install" clause; items 2 and 3 are ANALYZE and TYPECHECK character-for-character; the restart-from-step-1 sentence below the list is unchanged.
- [ ] [P3-T6] Verify the protected section is untouched: extract the text between `### UT2. Coverage and Scenarios` and `### UT3. Test Structure and Diagnostics` from both `git show <MERGE_BASE>:CLAUDE.md` and the working-tree `CLAUDE.md`, and record the comparison in `FEATURE/evidence/qa-gates/claudemd-ut2-guard.<TS>.md`.
  - Acceptance: the two extracts are byte-identical (zero differing lines). Any difference halts the plan and is reverted before Phase 4 begins.

### Phase 4 — Documentation Corrections in the Rules File and the QA-Gate Skill

- [ ] [P4-T1] Replace the formatting bullet in `.claude/rules/csharp.md` § Toolchain (replacement-table row 12, located by the string `Command: `dotnet tool run csharpier .` or `csharpier .``) with the row 12 replacement text from `spec.md`.
  - Acceptance: the bullet names both FORMAT-APPLY and FORMAT-VERIFY, states the `dotnet tool run` pinning rule, and no longer contains the bare-path form.
- [ ] [P4-T2] Replace the linting bullet in `.claude/rules/csharp.md` § Toolchain (replacement-table row 13) with the ANALYZE placeholder form (`<solution>.sln`) plus rationale sentence R4 from `spec.md`.
  - Acceptance: the command uses `/t:Rebuild /m` and `"/p:Platform=Any CPU"`; R4 is present verbatim and states the deliberate difference from `.github/workflows/ci.yml` (AC5).
- [ ] [P4-T3] Replace the type-checking bullet in `.claude/rules/csharp.md` § Toolchain (replacement-table row 14) with the per-file-opt-in sentence, the TYPECHECK placeholder form, and rationale sentences R4 and R5 from `spec.md`.
  - Acceptance: the string `Nullable=enable` appears only inside R5's prohibition sentence; no single line contains both `/t:Build` and `Nullable=enable`; the four-step order line at the end of the section is unchanged.
- [ ] [P4-T4] Replace only the embedded command string in the severity-first ordering invariant at `.claude/rules/csharp.md` § "Severity-first ordering invariant" (replacement-table row 15), changing `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` to `msbuild ... /t:Rebuild /m ... /p:TreatWarningsAsErrors=true`.
  - Acceptance: every other word of the invariant is byte-identical to the merge-base text, verified by `git diff <MERGE_BASE> -- .claude/rules/csharp.md` showing a single changed line in that section; the SecurityCodeScan / CS8032 paragraph is untouched.
- [ ] [P4-T5] Replace steps 1-3 of § "Toolchain Execution Sequence" in `.claude/skills/csharp-qa-gate/SKILL.md` (replacement-table rows 16-18) with the FORMAT-APPLY/FORMAT-VERIFY pair, ANALYZE and TYPECHECK in their placeholder forms.
  - Acceptance: step 4 (`vstest.console.exe`) and the restart-from-step-1 sentence are unchanged; no `/t:Build` and no `Nullable=enable` remain in the numbered list.
- [ ] [P4-T6] Append bullet R6 verbatim from `spec.md` to § "Evidence Storage" in `.claude/skills/csharp-qa-gate/SKILL.md` (replacement-table row 19).
  - Acceptance: the appended bullet requires an `/fl` file log and a **zero** `Skipping target "CoreCompile"` count for steps 2 and 3, and states that a non-zero skip count means unverified, not passed; the existing canonical evidence-path bullets are unchanged.

### Phase 5 — Verification of the Corrected Commands and Acceptance Evidence

- [ ] [P5-T1] Run FORMAT-APPLY (`./.dotnet-sdk/dotnet.exe tool run csharpier format .`) and write `FEATURE/evidence/qa-gates/csharpier-format.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `git status --porcelain` after the run shows **no tracked file modified** beyond those this plan already edits (the tree was format-clean at [P0-T8], and no `*.cs` file is edited by this feature). Any unexpected reformat is recorded and reverted before proceeding.
- [ ] [P5-T2] Run FORMAT-VERIFY (`./.dotnet-sdk/dotnet.exe tool run csharpier check .`) and write `FEATURE/evidence/qa-gates/csharpier-check.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` with the `Checked N files` count recorded, matching the [P0-T8] baseline count. Satisfies AC1's execute-and-record requirement for the verify form.
- [ ] [P5-T3] Run ANALYZE with `/fl "/flp:logfile=coverage/qa-analyze.log;verbosity=normal"` and write `FEATURE/evidence/qa-gates/analyze-rebuild.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, `Skipping target "CoreCompile"` count **0**, elapsed time recorded, and the AC2 counting-mechanism deviation restated in the artifact.
- [ ] [P5-T4] Run TYPECHECK with `/fl "/flp:logfile=coverage/qa-typecheck-positive.log;verbosity=normal"` on the unperturbed tree and write `FEATURE/evidence/qa-gates/typecheck-positive-control.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, MSBuild summary `0 Error(s)`, `Skipping target "CoreCompile"` count **0**, elapsed time recorded. If non-zero, stop and report; do not work around a red positive control. Satisfies AC3.
- [ ] [P5-T5] [expect-fail] Append the measured negative-control probe to the `QueueExtensions` class body in `UtilitiesCS/Extensions/QueueExtensions.cs` (after the closing brace of `DequeueChunk`, before the class closing brace), preserving the file's existing line endings, then run TYPECHECK with `/fl "/flp:logfile=coverage/qa-typecheck-negative.log;verbosity=normal"` and write `FEATURE/evidence/qa-gates/typecheck-negative-control.<TS>.md`.
  - Probe (the form validated by `FEATURE/evidence/regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md`): a `public static string` method whose body assigns `null` to a `string?` local and returns it. `spec.md` § "Negative-path proof design" prescribes the equivalent one-line `=> null;` form; the local-variable form is adopted because it is the measured one.
  - Acceptance: `EXIT_CODE: 1`; at least one diagnostic line matching `error CS8603` attributed to `UtilitiesCS\Extensions\QueueExtensions.cs` and to `UtilitiesCS.csproj`, quoted verbatim; `Skipping target "CoreCompile"` count **0**; the perturbed file's project appears among the compiled projects (asserted from the log, not assumed). The artifact records the file, the exact perturbation text, the command, the diagnostic, and the exit code. Satisfies AC4 except for the revert confirmation added by [P5-T6].
- [ ] [P5-T6] Revert the perturbation with `git checkout -- UtilitiesCS/Extensions/QueueExtensions.cs` and append the revert confirmation to `FEATURE/evidence/qa-gates/typecheck-negative-control.<TS>.md`.
  - Acceptance: `git status --porcelain UtilitiesCS/Extensions/QueueExtensions.cs` is empty; a grep for the probe method name in that file returns zero hits; the file's line count equals its merge-base line count. The perturbation is never committed.
- [ ] [P5-T7] Re-run TYPECHECK with `/fl "/flp:logfile=coverage/qa-typecheck-restore.log;verbosity=normal"` and write `FEATURE/evidence/qa-gates/typecheck-restore.<TS>.md`. This is the mandatory build-output restoration: the failed `/t:Rebuild` in [P5-T5] cleaned every project's `bin`/`obj`.
  - Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, `Skipping target "CoreCompile"` count **0**, and confirmation that `UtilitiesCS/bin/Debug` contains a rebuilt assembly.
- [ ] [P5-T8] Execute the corrected `lint:` task surface by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild` with the console transcript captured to `coverage/task-lint.log`, and write `FEATURE/evidence/qa-gates/vscode-task-lint.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; the transcript contains **zero** occurrences of `Skipping target "CoreCompile"`; the argument list is identical to the `lint:` task args in `.vscode/tasks.json` after [P2-T5].
- [ ] [P5-T9] Execute the corrected `type-check:` task surface by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -TreatWarningsAsErrors` with the transcript captured to `coverage/task-typecheck.log`, and write `FEATURE/evidence/qa-gates/vscode-task-typecheck.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; zero `Skipping target "CoreCompile"` occurrences in the transcript; the argument list is identical to the `type-check:` task args in `.vscode/tasks.json` after [P2-T6].
- [ ] [P5-T10] Prove the deprecated switch is inert by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -EnableNullable -TreatWarningsAsErrors` with the transcript captured to `coverage/task-deprecated-switch.log`, and write `FEATURE/evidence/qa-gates/enablenullable-noop-proof.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` **and** the deprecation warning text present in the transcript. Exit 0 is the discriminating signal: if `Nullable=enable` were still emitted, this run would fail with the `CS86xx` population measured in [P0-T13]. Records the tail of `FEATURE/evidence/baseline/baseline-nullable-debt.<TS>.md` as the contrast.
- [ ] [P5-T11] Run the AC6 repository-wide reconciliation grep with the same patterns and exclusions as [P0-T17] and write `FEATURE/evidence/qa-gates/site-inventory-reconciled.<TS>.md`.
  - Acceptance: zero hits for `csharpier\s+\.` and zero hits for the same-line conjunction of `/t:Build` and `Nullable=enable` outside the SD1 allowlist (`AGENTS.md`, `.github/instructions/**`, `.agents/**`, `.github/agents/**`, `.codex/**`); the residual allowlist hits are enumerated path:line with the SD1 rationale and a pointer to the follow-up issue from [P7-T2]. If a corrected site's rationale prose places both tokens on one line, re-wrap the prose rather than weakening the gate.
- [ ] [P5-T12] Record the AC10 rationale-prose resolution in `FEATURE/evidence/qa-gates/rationale-prose-resolution.<TS>.md`.
  - Acceptance: the artifact states that the `CLAUDE.md` "formats only `*.cs` without touching project files" claim is corrected by Block R1, cites the measured `csharpier check` probes of `QuickFiler/packages.config` and a `*.xml` file from `FEATURE/evidence/baseline/baseline-csharpier-replacement-forms.2026-08-10T14-45.md`, and separately records as reviewed-and-verified-correct: the `dotnet format` warning line, the formatter-output-wins line, `CLAUDE.md` "Keep nullable reference types enabled", and `.claude/rules/csharp.md` § Coding Standards null-safety bullet, the severity-first invariant, and the CS8032 paragraph. `.csharpierignore` is recorded as a known residual folded into the SD1 follow-up.
- [ ] [P5-T13] Verify the protected files against the merge base and write `FEATURE/evidence/qa-gates/protected-files-zero-diff.<TS>.md`.
  - Acceptance: `git diff <MERGE_BASE> -- .claude/rules/general-unit-test.md .claude/rules/quality-tiers.md` produces **zero** output lines; the `CLAUDE.md` § UT2 extract comparison from [P3-T6] is re-run against the final working tree and is byte-identical; `git diff <MERGE_BASE> --name-only` contains none of `AGENTS.md`, `.agents/`, `.github/instructions/`, `.github/agents/`, `.codex/`, `.github/workflows/ci.yml`. Satisfies AC9.
- [ ] [P5-T14] Review the full change diff for policy relaxation and write `FEATURE/evidence/qa-gates/no-relaxation-review.<TS>.md`.
  - Acceptance: `git diff <MERGE_BASE> -- CLAUDE.md .claude/rules/csharp.md .claude/skills/csharp-qa-gate/SKILL.md scripts/vscode/Invoke-VSBuild.ps1 .vscode/tasks.json tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` is reviewed hunk by hunk and the artifact records: no numeric threshold reduced, no mandatory toolchain step removed (the four-step order and the restart rule survive at every site), and no added `#pragma warning disable`, `[Ignore]`, `SuppressMessage`, `NoWarn` or `WarningsNotAsErrors` token. The "strengthening, not a relaxation" argument from `spec.md` is summarized for the PR body. Satisfies AC8.
- [ ] [P5-T15] Record the CI-parity comparison in `FEATURE/evidence/qa-gates/ci-parity.<TS>.md`.
  - Acceptance: the artifact places each corrected documented command beside the corresponding step in `.github/workflows/ci.yml` (`Verify formatting`, `Build with analyzers and code style enforcement`, `Build with nullable warnings treated as errors`), confirms the type-check command is character-for-character identical modulo the solution token, and states the one deliberate difference (documented ANALYZE uses `/t:Rebuild` where CI uses `/t:Build`) together with the location of its in-line rationale at each edited site. Satisfies AC5.
- [ ] [P5-T16] Consolidate the AC12 nullable-debt record into `FEATURE/evidence/qa-gates/nullable-debt-record.<TS>.md`, sourced from `FEATURE/evidence/baseline/baseline-nullable-debt.<TS>.md`.
  - Acceptance: the artifact records the measured error count from MSBuild's `N Error(s)` line, the per-diagnostic `CS86xx` breakdown, the owning project attribution, the explicit lower-bound qualification (the build aborts before dependents compile, so the solution-wide figure is `>= N` and unmeasured), and an explicit statement that the diagnostics are **not** fixed in this feature.

### Phase 6 — Final QC Toolchain Loop

- [ ] [P6-T1] Run `mcp__drm-copilot__run_poshqc_format` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]` and write `FEATURE/evidence/qa-gates/final-poshqc-format.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; `git status --porcelain` recorded before and after. If the formatter changes any file, the loop restarts at [P6-T1] after the change is reviewed.
- [ ] [P6-T2] Run `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]` and write `FEATURE/evidence/qa-gates/final-poshqc-analyze.<TS>.md`.
  - Acceptance: the finding count and the rule/file/line table are identical to the [P0-T15] baseline (expected 16); zero new findings. `EXIT_CODE: 0` is not asserted and is not the acceptance condition.
- [ ] [P6-T3] Run `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["tests/scripts/vscode"]`, coverage enabled if the tool exposes a coverage parameter, and write `FEATURE/evidence/qa-gates/final-poshqc-test.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, zero failures, numeric passed count equal to the [P0-T16] baseline plus one; plus numeric `Line Coverage:` and `Branch Coverage:` for `scripts/vscode/Invoke-VSBuild.ps1`, or the same `CoverageCapability:` + function-to-`It` enumeration fallback defined in [P0-T16]. No placeholders.
- [ ] [P6-T4] Write the PowerShell coverage comparison to `FEATURE/evidence/qa-gates/powershell-coverage-delta.<TS>.md`.
  - Acceptance: the artifact reports baseline coverage (from `FEATURE/evidence/baseline/baseline-poshqc-test.<TS>.md`), post-change coverage (from `FEATURE/evidence/qa-gates/final-poshqc-test.<TS>.md`), and changed-line coverage for the four edited regions of `scripts/vscode/Invoke-VSBuild.ps1`; asserts line coverage `>= 85%` and branch coverage `>= 75%` where numeric values are available, and asserts no regression against baseline on the changed lines. If numeric coverage is unavailable from the runner, the artifact states that explicitly with the tool's parameter list and reports the outcome as remediation-required rather than PASS.
- [ ] [P6-T5] Run FORMAT-APPLY (`./.dotnet-sdk/dotnet.exe tool run csharpier format .`) as C# toolchain step 1 and write `FEATURE/evidence/qa-gates/final-csharpier-format.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` and zero tracked files modified by the run. If any file changes, restart the C# loop at [P6-T5].
- [ ] [P6-T6] Run FORMAT-VERIFY (`./.dotnet-sdk/dotnet.exe tool run csharpier check .`) and write `FEATURE/evidence/qa-gates/final-csharpier-check.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` with the `Checked N files` count matching the [P0-T8] baseline.
- [ ] [P6-T7] Run ANALYZE as C# toolchain step 2 with `/fl "/flp:logfile=coverage/final-analyze.log;verbosity=normal"` and write `FEATURE/evidence/qa-gates/final-analyze.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, `Skipping target "CoreCompile"` count **0**, elapsed time recorded, AC2 deviation restated.
- [ ] [P6-T8] Run TYPECHECK as C# toolchain step 3 with `/fl "/flp:logfile=coverage/final-typecheck.log;verbosity=normal"` and write `FEATURE/evidence/qa-gates/final-typecheck.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, `Skipping target "CoreCompile"` count **0**.
- [ ] [P6-T9] Record the scope-limited treatment of C# toolchain step 4 in `FEATURE/evidence/qa-gates/csharp-test-step-scope.<TS>.md`.
  - Acceptance: the artifact states that no `*.cs`, `*.csproj`, `*.props` or `*.targets` file is modified by this feature (verified by `git diff <MERGE_BASE> --name-only`), that `vstest.console.exe` and C# coverage capture are therefore not run, and cites `FEATURE/evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` for the pre-existing unrelated failure of `Run MSTest suite with coverage` on `main`. This is a recorded deviation, not a skipped planned command.
- [ ] [P6-T10] Write the toolchain loop-closure attestation to `FEATURE/evidence/qa-gates/final-toolchain-attestation.<TS>.md`.
  - Acceptance: the artifact lists every command executed in [P6-T1] through [P6-T8] with its `EXIT_CODE:` and confirms that no step in the final pass modified a tracked file; if any step did, it records the restart and the results of the subsequent clean pass. It also confirms the working tree contains no residue from [P5-T5].

### Phase 7 — Follow-Up Promotion, Acceptance Check-Off, and Toolchain Attestation

- [ ] [P7-T1] File the SD1 potential-bug entry with `mcp__drm-copilot__new_potential_bug_entry` covering the Codex/Copilot instruction mirrors, and record the created file path in `FEATURE/evidence/issue-updates/sd1-potential-entry.<TS>.md`.
  - Acceptance: the entry enumerates the eight mirror paths with line numbers from the SD1 table in `spec.md`, names `drm-copilot` as owner of `.agents/`, `.codex/` and `.github/agents/`, states that `.github/instructions/` needs its own authorization grant, records that the generator `scripts/dev-tools/sync-agents-from-instructions.ps1` named by `AGENTS.md` does not exist, and includes the `.csharpierignore` comment residual.
- [ ] [P7-T2] Promote the entry with `mcp__drm-copilot__potential_to_issue` and mirror the result to `FEATURE/evidence/issue-updates/sd1-followup-issue.<TS>.md`.
  - Acceptance: the artifact records the created issue number and URL, `PostedAs: body`, and the promoted file path. Prose in the feature folder is not sufficient; the issue must exist.
- [ ] [P7-T3] Check off AC1 in `FEATURE/spec.md` § Acceptance Criteria with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/csharpier-format.<TS>.md` and `FEATURE/evidence/qa-gates/csharpier-check.<TS>.md`, both recording `EXIT_CODE: 0`.
- [ ] [P7-T4] Check off AC2 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/final-analyze.<TS>.md` and `FEATURE/evidence/qa-gates/final-typecheck.<TS>.md`, and the check-off note restates the recorded deviation from AC2's `csc.exe` parenthetical.
- [ ] [P7-T5] Check off AC3 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/typecheck-positive-control.<TS>.md` with `EXIT_CODE: 0` and a zero skip count.
- [ ] [P7-T6] Check off AC4 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/typecheck-negative-control.<TS>.md` including the revert confirmation from [P5-T6] and the restoration run `FEATURE/evidence/qa-gates/typecheck-restore.<TS>.md`.
- [ ] [P7-T7] Check off AC5 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/ci-parity.<TS>.md`.
- [ ] [P7-T8] Check off AC6 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/site-inventory-reconciled.<TS>.md` and the SD1 follow-up issue number from [P7-T2].
- [ ] [P7-T9] Check off AC7 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer enumerates the ordered verification artifacts under `FEATURE/evidence/qa-gates/` for all three documented commands.
- [ ] [P7-T10] Check off AC8 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/no-relaxation-review.<TS>.md`.
- [ ] [P7-T11] Check off AC9 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/protected-files-zero-diff.<TS>.md`.
- [ ] [P7-T12] Check off AC10 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/rationale-prose-resolution.<TS>.md`.
- [ ] [P7-T13] Check off AC11 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names the directory listings of `FEATURE/evidence/baseline/` and `FEATURE/evidence/qa-gates/` and asserts that every artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.
- [ ] [P7-T14] Check off AC12 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `FEATURE/evidence/qa-gates/nullable-debt-record.<TS>.md` with the measured figure, its attribution, and its lower-bound qualification.
- [ ] [P7-T15] Check off AC13 in `FEATURE/spec.md` with its evidence pointer.
  - Acceptance: exactly one checkbox changes; the pointer names `spec.md` § SD2 (the recorded decision and widening) and the corrected analyzer sites edited in [P3-T2], [P4-T2] and [P4-T5].
- [ ] [P7-T16] Update the status metadata in `FEATURE/spec.md` and this plan file `FEATURE/plan.2026-08-10T14-08.md`.
  - Acceptance: `spec.md` `Status:` and `Last Updated:` reflect completion; every task checkbox in this plan whose evidence artifact exists is checked, and any task without its artifact remains unchecked.
- [ ] [P7-T17] Write the closing attestation to `FEATURE/evidence/qa-gates/completion-attestation.<TS>.md`.
  - Acceptance: the artifact lists the final-pass toolchain results for both languages (CSharpier format and check, MSBuild analyze, MSBuild type-check, PoshQC format, PoshQC analyze, PoshQC test with coverage), states which commands were run and that each passed in the final pass, records the C# step-4 scope deviation from [P6-T9], confirms `git status --porcelain` contains no unintended residue, and lists the acceptance-criteria status summary for AC1 through AC13.
