# 2026-09-02-ci-build-infra-debt-730 (Plan)

- **Issue:** #730
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T19-31
- **Status:** Draft
- **Version:** 0.3
- **Work Mode:** full-bug
- **AC Source:** `docs/features/active/2026-09-02-ci-build-infra-debt-730/spec.md` § `## Acceptance Criteria`

## Framing

- **AC count correction.** The delegation prompt for this plan stated `spec.md`'s `## Acceptance Criteria` section has 7 items. Direct re-read of `spec.md` in this pass shows the section spans lines 167–175 (heading at 167; 8 bulleted items, `AC1`–`AC8`, at lines 168–175). This plan and its AC-traceability section are built against the observed 8-item inventory, not the stated 7, per the Planner Adversarial Self-Review mandate to re-derive against current tree state rather than carry forward an uncited count.
- **No `[expect-fail]` regression-test phase.** Per CLAUDE.md's Bugfix Workflow and `spec.md`'s Repro & Evidence section, both findings are static configuration inspections with no executable failing state to capture (Finding 1: a cache fallback whose risk was investigated and refuted, not a functional defect; Finding 2: a missing suppression property, not a code defect). There is no application code path to red/green, so no synthetic failing test is authored. Phase 0's baseline msbuild rebuilds serve as the deterministic "before" state that Phase 2's rebuilds are diffed against, in place of a fail-before test.
- **No coverage-denominator impact.** Neither fix adds, removes, or modifies any `.cs` application source line. There is no new/changed-code coverage target and no repository-wide coverage delta to report; the Coverage Evidence Contract in `atomic-plan-contract` does not apply to this plan.
- **Evidence path.** All evidence in this plan resolves under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/<kind>/`, per the Non-Overridable Evidence Path Clause. No `artifacts/baseline*`, `artifacts/qa*`, or `artifacts/coverage*` path is used anywhere in this plan.
- **Scope boundary carried from the orchestrator's directive:** no edit under the Claude runtime tree at path .claude (and everything beneath it), the Codex mirror tree at path .codex (and everything beneath it), the dot-agents tree at path .agents (and everything beneath it), or the two published configuration files at paths config/blast-radius.json and config/orchestration-routing.json; no coverage threshold/Pester job/coverage gate change; no edit to the workflow file at path .github/workflows/ci.yml; no edit to any .csproj, packages.config, or Directory.Build.targets file; no `System.Reactive` `PackageReference` migration.

---

### Phase 0 — Policy Reads & Pre-Change Baseline Capture

- [ ] [P0-T1] Read, in the order defined by `policy-compliance-order`, the five policy files applicable to this change — CLAUDE.md, the general code-change rule at path .claude/rules/general-code-change.md, the general unit-test rule at path .claude/rules/general-unit-test.md, the C# rule at path .claude/rules/csharp.md (in scope because CLAUDE.md's C# Code Change Policy explicitly extends to `*.props` files, and Phase 1 creates `Directory.Build.props`), and the CI-workflows rule at path .claude/rules/ci-workflows.md (the rule scoped to the .github/workflows directory tree, in scope because Phase 1 edits three workflow files) — then write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (the five files listed in the order read), and an explicit itemized list of the five file paths. Acceptance: the artifact file exists and lists all five paths verbatim.

- [ ] [P0-T2] Confirm the pre-change absence of `Directory.Build.props` at the repository root: run `Test-Path Directory.Build.props` and record the result in `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/directory-build-props-absence.<timestamp>.md` with `Timestamp:`, `Command: Test-Path Directory.Build.props`, `EXIT_CODE: 0`, and `Output Summary:` recording the literal boolean printed. Acceptance: the artifact records the literal value `False`, establishing that Phase 0's baseline rebuilds (P0-T3, P0-T4) run before the file exists.

- [ ] [P0-T3] Execute, before any Phase 1 task has run, `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.log;verbosity=normal` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording three numeric values read from the log file: (a) `RxCheck_analyzers_pre` = the count of lines matching the literal single-line token System.Reactive.PackagesConfigCheck.targets (via `(Select-String -Path <logfile> -Pattern 'System.Reactive.PackagesConfigCheck.targets' -SimpleMatch).Count`), (b) `W_analyzers_pre` = the count of lines matching the literal token `: warning ` (`-SimpleMatch`), and (c) `E_analyzers_pre` = the count of lines matching the literal token `: error ` (`-SimpleMatch`). Acceptance: the artifact records all three values with `RxCheck_analyzers_pre` equal to exactly 5 (one occurrence per affected project: QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, and UtilitiesCS.Test, per research §2.1's confirmed exhaustive derivation) and `E_analyzers_pre` equal to 0.

- [ ] [P0-T4] Execute, before any Phase 1 task has run, `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.log;verbosity=normal` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording `RxCheck_nullable_pre`, `W_nullable_pre`, and `E_nullable_pre` using the same three `-SimpleMatch` counts as P0-T3, read from this command's log file. Acceptance: the artifact records all three values with `RxCheck_nullable_pre` equal to exactly 5 and `E_nullable_pre` equal to 0 (the System.Reactive.PackagesConfigCheck diagnostic is an MSBuild target-emitted `<Warning>`, not a compiler `CSxxxx`/`BCxxxx` diagnostic, so `/p:TreatWarningsAsErrors=true` — which is consumed by the Csc/Vbc compiler tasks, not by arbitrary target `<Warning>` calls — does not promote it to an error; this is consistent with `spec.md`'s Impact/Severity statement that neither finding causes a build failure today).

- [ ] [P0-T5] Record the pre-change git baseline: run `git rev-parse HEAD` and `git status --porcelain` on branch `bug/ci-build-infra-debt-730`, and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/git-state-pre.<timestamp>.md` with `Timestamp:`, the recorded HEAD SHA, and the `git status --porcelain` output. Acceptance: the artifact records a 40-character SHA and confirms `git status --porcelain` output contains exactly one line, `?? docs/features/active/2026-09-02-ci-build-infra-debt-730/` (the untracked feature folder holding `issue.md`, `spec.md`, `research/research.2026-09-02T09-15.md`, and this plan file, none of which are yet committed), and no other entries, before Phase 1 begins.

---

### Phase 1 — Implementation (Comment-Only Workflow Edits + New `Directory.Build.props`)

- [ ] [P1-T1] In `.github/workflows/_build-analyzers.yml`, insert the following 16 comment lines immediately above the `restore-keys:` line (currently line 40, re-derived directly against the current tree in this planning pass), at the same 10-space indentation as the sibling `key:`/`restore-keys:` lines, with no other change to the file:
  ```
            # The bare-prefix fallback below is safe against stale package
            # versions: `nuget restore` (next step) always runs unconditionally
            # and is idempotent per package for packages.config-style restores —
            # each package is materialized under a version-qualified directory
            # (packages/{id}.{version}/, matching every HintPath in this repo's
            # .csproj files). A fallback cache populated under an older
            # packages.config hash can therefore only ever contribute either
            # (a) version-folders that still match the current packages.config
            # (a legitimate, desired reuse) or (b) inert orphaned version-
            # folders for packages no longer referenced by any HintPath. Either
            # way, `nuget restore` fetches exactly the delta implied by the
            # current packages.config from the network before the build step
            # runs, so a fallback hit can never cause the build to compile
            # against a package version other than the one packages.config
            # names. See docs/features/active/2026-09-02-ci-build-infra-debt-730/
            # research/ for the full analysis (issue #730).
  ```
  Acceptance: the file's `key:`, `path:`, `uses:` (`actions/cache@v4`), the `restore-keys:` key and its value (`nuget-${{ runner.os }}-`), and the subsequent `Restore solution` step are byte-identical to before; the only change is these 16 inserted comment lines.

- [ ] [P1-T2] In `.github/workflows/_build-nullable.yml`, insert the identical 16 comment lines quoted in P1-T1 immediately above the `restore-keys:` line (currently line 40, re-derived directly against the current tree in this planning pass), at the same 10-space indentation, with no other change to the file. Acceptance: same as P1-T1, applied to `.github/workflows/_build-nullable.yml`.

- [ ] [P1-T3] In `.github/workflows/_mstest-coverage.yml`, insert the identical 16 comment lines quoted in P1-T1 immediately above the `restore-keys:` line (currently line 40, re-derived directly against the current tree in this planning pass), at the same 10-space indentation, with no other change to the file. Acceptance: same as P1-T1, applied to `.github/workflows/_mstest-coverage.yml`.

- [ ] [P1-T4] Create a new file `Directory.Build.props` at the repository root (confirmed absent pre-change in P0-T2) with exactly this content:
  ```xml
  <Project>
    <!--
      System.Reactive 7.0.0+ refuses to build cleanly against packages.config
      projects (see System.Reactive.PackagesConfigCheck.targets) and instead
      emits an "unsupported scenario" warning on every build of every project
      that references it. This repository intentionally keeps its legacy
      non-SDK VSTO / .NET Framework 4.8.1 projects on packages.config (see
      .claude/rules/csharp.md) rather than migrating to PackageReference, so
      the warning is accepted here as a known, deliberate trade-off rather than
      fixed by migration. RxUseUnsupportedPackagesConfig=true is the package's
      own documented suppression switch for this exact scenario. See issue #730
      and docs/features/active/2026-09-02-ci-build-infra-debt-730/ for the
      accepted-trade-off rationale.
    -->
    <PropertyGroup>
      <RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig>
    </PropertyGroup>
  </Project>
  ```
  Acceptance: the file exists at the repository root and its content is byte-identical to the block quoted above.

---

### Phase 2 — Verification & Final QC

- [ ] [P2-T1] Stage exactly the four changed files: `git add Directory.Build.props .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml`, then run `git status --porcelain` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/git-stage-scope.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the full porcelain output. Acceptance: the porcelain output lists exactly these four staged paths — `A  Directory.Build.props`, `M  .github/workflows/_build-analyzers.yml`, `M  .github/workflows/_build-nullable.yml`, `M  .github/workflows/_mstest-coverage.yml` — plus the single pre-existing untracked-directory line `?? docs/features/active/2026-09-02-ci-build-infra-debt-730/` recorded in P0-T5, and no other entries.

- [ ] [P2-T2] Run `git diff --cached origin/main --name-status` (anchored to the `origin/main` ref, paired with the `git status --porcelain` companion captured in P2-T1) and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/git-scope-boundary.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the full name-status output and an explicit statement of zero occurrences of each of these substrings in that output: .csproj, packages.config, Directory.Build.targets, .claude/, .codex/, .agents/, config/blast-radius.json, config/orchestration-routing.json. Acceptance: the artifact records the name-status output matching the four-line set from P2-T1 exactly and confirms none of the listed substrings appear.

- [ ] [P2-T3] Run `git diff --cached origin/main -- .github/workflows/_build-analyzers.yml` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/diff-build-analyzers.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the count of added lines (`+`-prefixed, excluding the `+++` file-header line) and the count of removed lines (`-`-prefixed, excluding the `---` file-header line), plus confirmation each added line matches one of the 16 comment lines quoted in P1-T1. Acceptance: the artifact records exactly 16 added lines and 0 removed lines.

- [ ] [P2-T4] Run `git diff --cached origin/main -- .github/workflows/_build-nullable.yml` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/diff-build-nullable.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the count of added lines (`+`-prefixed, excluding the `+++` file-header line) and the count of removed lines (`-`-prefixed, excluding the `---` file-header line), plus confirmation each added line matches one of the 16 comment lines quoted in P1-T1. Acceptance: the artifact records exactly 16 added lines and 0 removed lines.

- [ ] [P2-T5] Run `git diff --cached origin/main -- .github/workflows/_mstest-coverage.yml` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/diff-mstest-coverage.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the count of added lines (`+`-prefixed, excluding the `+++` file-header line) and the count of removed lines (`-`-prefixed, excluding the `---` file-header line), plus confirmation each added line matches one of the 16 comment lines quoted in P1-T1. Acceptance: the artifact records exactly 16 added lines and 0 removed lines.

- [ ] [P2-T6] After Phase 1 is complete, execute `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-analyzers-post.log;verbosity=normal` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-analyzers-post.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording, using the same three `-SimpleMatch` counts defined in P0-T3: (a) `RxCheck_analyzers_post`, (b) `W_analyzers_post`, and (c) `E_analyzers_post`, read from this command's log file. Acceptance: the artifact records `RxCheck_analyzers_post == 0` (down from the `RxCheck_analyzers_pre == 5` baseline recorded in P0-T3), `W_analyzers_post == W_analyzers_pre - 5` (baseline `W_analyzers_pre` from P0-T3), and `E_analyzers_post == E_analyzers_pre` (both baseline and post-change error counts recorded, expected 0 in both).

- [ ] [P2-T7] After Phase 1 is complete, execute `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-nullable-post.log;verbosity=normal` and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-nullable-post.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording `RxCheck_nullable_post`, `W_nullable_post`, and `E_nullable_post` using the same three `-SimpleMatch` counts defined in P0-T3, read from this command's log file. Acceptance: the artifact records `RxCheck_nullable_post == 0` (down from the `RxCheck_nullable_pre == 5` baseline recorded in P0-T4), `W_nullable_post == W_nullable_pre - 5` (baseline `W_nullable_pre` from P0-T4), and `E_nullable_post == E_nullable_pre` (both expected 0).

- [ ] [P2-T8] Run `dotnet tool run csharpier check .` (read-only verify command; not a write-mode command, so no G7 rewrite-literal is required) and write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/csharpier-check.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the literal console output observed at run time. Acceptance: `EXIT_CODE: 0` is recorded. (Per the .csharpierignore file, `*.props` and `*.yml` are outside CSharpier's checked-extension set — the .csharpierignore file excludes `*.props` explicitly and CSharpier does not process `.yml` at all — so the new `Directory.Build.props` and the three comment-only workflow edits are not expected to introduce any formatting drift; this task executes the command and records its actual exit code rather than assuming the result.)

- [ ] [P2-T9] Locate `vstest.console.exe` via `vswhere.exe` using the same resolution the repository's own `.github/workflows/_mstest-coverage.yml:60-67` uses (`Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'`, then `-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`), locate UtilitiesCS.Test.dll and QuickFiler.Test.dll under their respective bin\Debug\ output directories using the same filter `.github/workflows/_mstest-coverage.yml:70-76` uses (path matches bin\Debug\, excludes obj\ and ref\), then run `& $vstestPath UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`, then write `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/vstest-regression.<timestamp>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the literal Passed/Failed/Total counts printed in the console output. Acceptance: `EXIT_CODE: 0` and `Failed: 0` are both recorded, confirming the Rx-dependent MSTest suites pass unchanged after the `Directory.Build.props` addition.

---

### Phase 3 — AC Traceability, Acceptance-Criteria Check-off, and Commit

- [ ] [P3-T1] In `docs/features/active/2026-09-02-ci-build-infra-debt-730/spec.md`, check off AC1 (line 168, the comment-block-present criterion) from `- [ ]` to `- [x]`, citing evidence: implementation P1-T1/P1-T2/P1-T3 and verification P2-T3/P2-T4/P2-T5 (16-line comment-only diff confirmed in all three workflow files). Acceptance: the AC1 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T2] In `spec.md`, check off AC2 (line 169, the `Directory.Build.props` existence/content criterion) from `- [ ]` to `- [x]`, citing evidence: P1-T4 (file creation, content byte-identical to the quoted block). Acceptance: the AC2 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T3] In `spec.md`, check off AC3 (line 170, the zero-warnings-post-rebuild criterion) from `- [ ]` to `- [x]`, citing evidence: baseline P0-T3/P0-T4 (`RxCheck_*_pre == 5`) and post-change P2-T6/P2-T7 (`RxCheck_*_post == 0`, `W_*_post == W_*_pre - 5`, `E_*_post == E_*_pre`). Acceptance: the AC3 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T4] In `spec.md`, check off AC4 (line 171, the no-.csproj/packages.config/Directory.Build.targets/application-source-modified criterion) from `- [ ]` to `- [x]`, citing evidence: P2-T1/P2-T2 (exactly four changed files, none matching the excluded path substrings). Acceptance: the AC4 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T5] In `spec.md`, check off AC5 (line 172, the no-coverage-threshold/Pester-job/coverage-gate-change criterion) from `- [ ]` to `- [x]`, citing evidence: P2-T2's exhaustive four-file change enumeration (no coverage/Pester/gate-related file present) together with the literal diff content quoted in P1-T1 through P1-T4 (comment text and MSBuild property only, no coverage/threshold token present). Acceptance: the AC5 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T6] In `spec.md`, check off AC6 (line 173, the Rx-dependent MSTest-suites-pass criterion) from `- [ ]` to `- [x]`, citing evidence: P2-T9 (`EXIT_CODE: 0`, `Failed: 0` for UtilitiesCS.Test and QuickFiler.Test). Acceptance: the AC6 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T7] In `spec.md`, check off AC7 (line 174, the full-toolchain-pass criterion) from `- [ ]` to `- [x]`, citing evidence: P2-T8 (`csharpier check`, `EXIT_CODE: 0`), P2-T6/P2-T7 (both `msbuild /t:Rebuild` commands, `EXIT_CODE: 0`), and P2-T9 (`vstest.console.exe` regression re-run, `EXIT_CODE: 0`). Acceptance: the AC7 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task.

- [ ] [P3-T8] In `spec.md`, check off AC8 (line 175, the evidence-capture-location criterion) from `- [ ]` to `- [x]`, citing evidence: all baseline artifacts under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/` (P0-T1 through P0-T5) and all final-QC artifacts under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/` (P2-T6 through P2-T9). Acceptance: the AC8 checkbox in `spec.md` reads `- [x]` and no other line in the Acceptance Criteria section is altered by this task; confirm via `Get-ChildItem` that both `evidence/baseline/` and `evidence/qa-gates/` directories are non-empty.

- [ ] [P3-T9] Stage and commit all changes and evidence artifacts: run `git add Directory.Build.props .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml docs/features/active/2026-09-02-ci-build-infra-debt-730/spec.md docs/features/active/2026-09-02-ci-build-infra-debt-730/issue.md docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md docs/features/active/2026-09-02-ci-build-infra-debt-730/plan.2026-09-02T08-57.md` (the four files from Phase 1, the updated `spec.md`, and the remaining pre-existing feature-folder files not yet tracked by git), then run `git add -f docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/` (force-add is required because the repository root `.gitignore:84`'s blanket `*.log` rule would otherwise silently exclude the four MSBuild log artifacts `msbuild-analyzers-pre.log`, `msbuild-nullable-pre.log`, `msbuild-analyzers-post.log`, and `msbuild-nullable-post.log` from a plain `git add`), then run `git status --porcelain --ignored=no` before committing and confirm each of these four literal paths appears with an `A  ` status: `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.log`, `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.log`, `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-analyzers-post.log`, and `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-nullable-post.log`, then commit, then run `git status --porcelain`. Acceptance: the pre-commit `git status --porcelain --ignored=no` check confirms all four named `.log` files staged with `A  ` status, and the post-commit `git status --porcelain` output is empty (clean worktree).
