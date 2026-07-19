# Atomic Plan — utilitiescs-nullable-ci-capstone (Issue #376)

- Feature folder: `docs/features/active/utilitiescs-nullable-ci-capstone/`
- Work Mode: full-feature (AC source: `spec.md` + `user-story.md`, both present)
- Requirements source precedence: `issue.md` AC1–AC7, elaborated in `spec.md` and `user-story.md`
- Research: `docs/features/active/utilitiescs-nullable-ci-capstone/research/2026-07-19T00-30-ci-capstone-research.md`
- Scope: this plan authors and preflight-clears atomic execution steps; atomic execution itself is
  performed later by epic-orchestrator. No implementation is performed by this planning pass.
- Evidence root (canonical, non-overridable): `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/`

## Notes for the executing agent

- The gate-step edit target is `.github/workflows/ci.yml`, step "Build with nullable warnings
  treated as errors" (lines 103–115 as of research date 2026-07-19; re-confirm line numbers before
  editing since intervening commits may shift them).
- AC2's opted-in/non-opted-in candidate files MUST be re-selected at execution time via a fresh
  `#nullable enable` grep (Phase 2, P2-T1); the research's `PercentageFormatter.cs` /
  `ActionButton.cs` candidates are illustrative defaults only.
- No `.claude/rules/*` file is edited by any task in this plan (AC4). No csproj `<Nullable>`
  element is added by any task in this plan (AC5). Both are enforced as explicit verification
  tasks in Phase 3.
- `modified-workflow-needs-green-run` is recorded, not satisfied, by this plan (Phase 4) — the
  green CI run against the branch head is an execution/merge-time obligation for epic-orchestrator.

### Phase 0 — Baseline Capture

- [ ] [P0-T1] Read `docs/features/active/utilitiescs-nullable-ci-capstone/issue.md` in full and confirm AC1–AC7 are present and unchanged from the versions quoted in this plan.
  - Acceptance: read completed; AC1–AC7 text confirmed present; no discrepancy noted (or discrepancy recorded and plan halted for reconciliation).
- [ ] [P0-T2] Read `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md` in full, including the "Maintainer Decision Summary", "Rules-vs-convention conflict detail (AC4)", and "Optional project-level flip detail (AC5)" sections.
  - Acceptance: read completed; all three named sections confirmed present.
- [ ] [P0-T3] Read `docs/features/active/utilitiescs-nullable-ci-capstone/user-story.md` in full.
  - Acceptance: read completed; Acceptance Criteria and Non-Goals sections confirmed present.
- [ ] [P0-T4] Read `docs/features/active/utilitiescs-nullable-ci-capstone/research/2026-07-19T00-30-ci-capstone-research.md` in full, including sections (a) gate edit, (b) verification method/candidates, (c) rules conflict, (d) optional flip, (e) maintainer-decision inventory, (f) CI-workflow rule applicability.
  - Acceptance: read completed; all six lettered sections confirmed present.
- [ ] [P0-T5] Read policy files in compliance order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/rules/benchmark-baselines.md` — and write `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/phase0-instructions-read.md`.
  - Acceptance: artifact exists with `Timestamp:`, `Policy Order:` (the six files listed in the order read), and an explicit list of files read. Task remains unchecked if the artifact is absent or any field is missing.
- [ ] [P0-T6] Run `dotnet csharpier check .` from the repository root and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-csharpier.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of unformatted files, expected zero since no `.cs` file is in this feature's default scope).
- [ ] [P0-T7] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-analyzers-build.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE:`, `Output Summary:` stating the analyzer/code-style error count (expected zero on the unmodified integration-branch head).
- [ ] [P0-T8] Run the pragma-driven nullable gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-nullable-gate.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE: 0`, `Output Summary:` confirming a clean full rebuild with no CS86xx or other build errors. This is the gate under test (this plan's Phase 1 edit changes only the CI workflow YAML that invokes an equivalent command; this local baseline confirms the command itself is currently clean).
- [ ] [P0-T9] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll and SVGControl.Test.dll paths> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed test counts and the numeric coverage headline (line-rate and branch-rate from the emitted coverage report). This is the pre-edit coverage reference for the AC7 no-regression comparison in Phase 5.

### Phase 1 — Gate-Step Edit (AC1)

- [ ] [P1-T1] Edit `.github/workflows/ci.yml`, step "Build with nullable warnings treated as errors", removing `/p:Nullable=enable` from the `msbuild` invocation and replacing the explanatory comment with the pragma-driven rationale, producing exactly:
  ```yaml
        - name: Build with nullable warnings treated as errors
          shell: pwsh
          run: |
            # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
            # recompile. Enforcement now relies entirely on each file's own #nullable
            # enable pragma (the repo's per-file opt-in convention; UtilitiesCS.csproj and
            # SVGControl.csproj carry no project-level <Nullable> element) plus
            # /p:TreatWarningsAsErrors=true. MSBuild's incremental up-to-date check does
            # not invalidate on this command-line property change alone, so a plain
            # /t:Build would silently skip recompilation and never enforce this gate.
            & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
                "/p:Platform=Any CPU" `
                /p:TreatWarningsAsErrors=true
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  ```
  - Acceptance: `git diff .github/workflows/ci.yml` shows only this step's `run:` block changed (comment text and the one `msbuild` line with `/p:Nullable=enable` removed); no other line in the file differs from the pre-edit state.
- [ ] [P1-T2] Confirm the exit-code handling line `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` is present verbatim (unchanged) in the edited step (AC3).
  - Acceptance: grep of the edited step shows the exact line unchanged from the pre-edit version; no new deliberately-failing nested command is introduced in this `run:` block, so `.claude/rules/ci-workflows.md`'s reset/exit-0 requirement is not triggered by this edit.
- [ ] [P1-T3] Confirm no other line in the step — step name, `shell: pwsh` directive, or any line outside the `run:` block's comment and `msbuild` invocation — differs from the pre-edit state.
  - Acceptance: line-by-line diff review confirms only the comment block and the single `msbuild` command line changed; step name and `shell: pwsh` are byte-identical to the pre-edit version.

### Phase 2 — Genuine-Enforcement Verification (AC2)

- [ ] [P2-T1] Re-grep `#nullable enable` (and bare `#nullable`) across `UtilitiesCS/**/*.cs` and `SVGControl/**/*.cs` at current execution time, and select one still-opted-in candidate file and one still-non-opted-in candidate file, re-confirming (not assuming) `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` and `UtilitiesCS/Dialogs/ActionButton.cs` are still representative or substituting alternates if either has changed opt-in status. Record the full current opted-in file count and both selected candidates to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/nullable-opt-in-regrep.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the full re-grep file count and file list, and the two selected candidate file paths with an explicit statement of each candidate's opt-in status as of this grep.
- [ ] [P2-T2] Run the pragma nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) with no defect present and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-1-clean-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming a clean rebuild immediately before defect introduction.
- [ ] [P2-T3] [expect-fail] Introduce a deliberate CS8602-class null-dereference defect in the selected opted-in candidate file (e.g. `string? maybeNull = null; int len = maybeNull.Length;` inside a method body), run the pragma nullable gate, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-2-optedin-defect-fail.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` including the literal CS86xx diagnostic line from the MSBuild error output, confirming the gate fails on the opted-in file's defect.
- [ ] [P2-T4] Revert the opted-in-candidate defect introduced in P2-T3 and confirm `git diff <opted-in candidate file path>` is empty.
  - Acceptance: `git diff` for the specific file returns no output; the file is byte-identical to its pre-defect state.
- [ ] [P2-T5] Introduce the same defect class (a null-literal assignment dereferenced through a non-nullable local, e.g. `string local = null; Console.WriteLine(local.Length);`) in the selected non-opted-in candidate file, run the pragma nullable gate, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-3-nonoptedin-defect-pass.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming the gate does not fail and no CS86xx diagnostic is emitted for the non-opted-in file (non-cross-block evidence).
- [ ] [P2-T6] Revert the non-opted-in-candidate defect introduced in P2-T5 and confirm `git diff <non-opted-in candidate file path>` is empty.
  - Acceptance: `git diff` for the specific file returns no output; the file is byte-identical to its pre-defect state.
- [ ] [P2-T7] Run the pragma nullable gate once more on the fully-reverted tree and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-4-restored-clean.<timestamp>.md`, then confirm `git status --porcelain` is empty.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming a clean restored-state rebuild; `git status --porcelain` output is empty, confirming no defect or failing-gate state remains on the branch (AC2 completion requirement).

### Phase 3 — Documentation Verification (AC4, AC5, AC6)

- [ ] [P3-T1] Verify AC4: confirm `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md`'s "Rules-vs-convention conflict detail (AC4)" section quotes `.claude/rules/csharp.md` lines 16 and 81–83 verbatim and presents both maintainer options (accept-as-documentation-debt vs. explicit ratified exception) without choosing between them.
  - Acceptance: section confirmed present with both verbatim quotations and both options listed; no resolution/choice is asserted in the section text.
- [ ] [P3-T2] Verify AC5: confirm `spec.md`'s "Optional project-level flip detail (AC5)" section documents the project-level `<Nullable>enable</Nullable>` flip for `UtilitiesCS.csproj` and `SVGControl.csproj` as a separately-gated OPTIONAL step with an explicit maintainer decision gate, and confirm neither csproj currently contains a `<Nullable>` element (grep both files for `Nullable`, expect zero matches).
  - Acceptance: section confirmed present describing the flip as optional/not-performed-by-default; grep of both `UtilitiesCS/UtilitiesCS.csproj` and `SVGControl/SVGControl.csproj` returns zero `<Nullable>` matches.
- [ ] [P3-T3] Verify AC6: confirm `spec.md`'s "Maintainer Decision Summary" table includes all required rows — `Interfaces/**` (~62 files) exclusion, `Resources.Designer.cs`/`Settings.Designer.cs` exclusion, `PeopleScoDictionaryNewBackup.cs` exclude/delete decision, 6 `OlFolderTools` Designer files, the three pre-existing >500-line files, `MSDemoConv.cs`, `To Depricate/*`, and `MailResolution_ToRemove`.
  - Acceptance: all eight named rows (or their equivalent single-consolidated-list entries) are confirmed present in the table with their source citations.
- [ ] [P3-T4] Confirm no `.claude/rules/*` file appears in `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD` for this feature branch.
  - Acceptance: the diff name-only listing contains zero paths under `.claude/rules/`, confirming AC4's "no `.claude/rules/*` file is edited" constraint holds across the entire branch, not only the files touched by this plan.

### Phase 4 — Modified-Workflow Green-Run Recording

- [ ] [P4-T1] Record that `.github/workflows/ci.yml` is modified by this feature, which triggers the `modified-workflow-needs-green-run` policy rule (`.claude/skills/feature-review-workflow/SKILL.md`), and that a green CI run against the branch head is a required, unsatisfied-by-this-plan, execution/merge-time obligation carried out by epic-orchestrator's fan-in. Write this record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/green-run-requirement-recorded.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the exact modified-workflow path (`.github/workflows/ci.yml`), the rule citation, and an explicit `NOT SATISFIED BY THIS PLAN` statement naming epic-orchestrator as the responsible party for capturing the green run before merge.

### Phase 5 — Final QC

- [ ] [P5-T1] Run `dotnet csharpier format .` from the repository root and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-csharpier.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero files reformatted. If this step reformats any file, restart the Final QC loop from P5-T1.
- [ ] [P5-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-analyzers-build.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming zero analyzer/code-style errors. If this step fails or changes files, restart the Final QC loop from P5-T1.
- [ ] [P5-T3] Run the pragma nullable gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) on the fully-reverted, fully-edited tree and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-nullable-gate.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE: 0`, `Output Summary:` confirming a clean full rebuild with no CS86xx or other build errors, matching the Phase 0 baseline. If this step fails or changes files, restart the Final QC loop from P5-T1.
- [ ] [P5-T4] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll and SVGControl.Test.dll paths> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed test counts and the numeric post-change coverage headline (line-rate and branch-rate). If this step fails or changes files, restart the Final QC loop from P5-T1.
- [ ] [P5-T5] Compare the P0-T9 baseline coverage figures against the P5-T4 post-change coverage figures and record the delta verification to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-coverage-delta.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the baseline line-rate/branch-rate numbers, the post-change line-rate/branch-rate numbers, and an explicit statement that post-change coverage is not lower than baseline coverage (AC7 no-regression). If a reduction is found, this plan's outcome is remediation-required, not PASS.
- [ ] [P5-T6] Confirm `git status --porcelain` is empty at the repository root, repo-wide, after all Phase 5 steps complete.
  - Acceptance: `git status --porcelain` returns no output, confirming no deliberately-introduced defect, no stray reformat, and no untracked file remains on the branch at hand-off.
