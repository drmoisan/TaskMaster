# 2026-08-14-ci-parallel-job-split — Plan

- **Issue:** #553
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-14T09-05
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-feature

## Required References

- `CLAUDE.md` (all sections, including the C# Code Change Policy — read for scope confirmation only; see the No-C#-Toolchain Statement below)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/ci-workflows.md` (governs `pwsh` steps in workflows)
- `.claude/rules/benchmark-baselines.md` (governs the timing baseline/comparison)
- `.claude/rules/tonality.md`
- Design of record: `docs/features/active/2026-08-14-ci-parallel-job-split-553/spec.md`
- Acceptance-criteria source of record: `docs/features/active/2026-08-14-ci-parallel-job-split-553/issue.md` (Work Mode: full-feature)
- User story / AC mirror: `docs/features/active/2026-08-14-ci-parallel-job-split-553/user-story.md`
- Research: `docs/features/active/2026-08-14-ci-parallel-job-split-553/research/2026-08-14T13-30-ci-parallel-job-split-research.md` (Q8 = ruleset migration procedure; Q9 = `$LASTEXITCODE` review)
- Measured baseline: `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md` (444s)

**All work must comply with these policies; do not duplicate their content here.**

## Conventions Used in This Plan

- `FEATURE` = `docs/features/active/2026-08-14-ci-parallel-job-split-553` (relative to the repository root `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-14T09-01`).
- `<TS>` = execution-time ISO-8601 timestamp in the form `yyyy-MM-ddTHH-mm` per `evidence-and-timestamp-conventions`.
- All evidence artifacts go to `FEATURE/evidence/<kind>/` (canonical scheme). Every command-step evidence artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- All commands run from the repository root unless stated otherwise. Shell is PowerShell (`pwsh`); `gh` commands run identically from PowerShell. `jq` is NOT installed in this environment and must not be used; JSON manipulation uses `ConvertFrom-Json` / `ConvertTo-Json -Depth 20` (the default depth of 2 truncates nested objects). The only permitted `--jq` usage is the filter argument built into `gh api --jq`, which is compiled into `gh`.
- `BASELINE_SHA` = the commit recorded by P0-T2. Diff-scoped verifications reference `BASELINE_SHA`, never a live `HEAD` expectation.
- `BRANCH` = the branch name recorded by P0-T2 via `git rev-parse --abbrev-ref HEAD` (expected value: `feature/ci-parallel-job-split-553`). Tasks reference `BRANCH`; never hard-code a branch name into a command.
- `SCRATCH` = the session scratchpad directory `C:\Users\DANMOI~1\AppData\Local\Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-14T09-01\012c26d5-57f2-4f08-bc74-bf50a60b1e4e\scratchpad`. ALL temporary files (tool downloads, temp JSON projections, helper scripts) go under `SCRATCH`, never under `$env:TEMP` (shared with concurrent agents in sibling worktrees) and never under the feature folder.
- actionlint flag form: actionlint 1.7.7's `-color` is a BOOLEAN flag ("Always enable colorful output"); it takes no value. The correct way to suppress color is `-no-color`. Do not write `-color never` — Go's flag parser consumes `-color` as the boolean and treats `never` as a positional FILE argument, failing with `could not read "never"` (exit 3).
- Helper persistence: shell state does not persist between tool invocations in this environment. The executor MUST write the `Test-BlockContained` and `Test-CalleeContract` function definitions below once to a single file `SCRATCH\helpers-553.ps1` and dot-source that file (`. "<SCRATCH>\helpers-553.ps1"`) in every pwsh invocation that calls either helper (P0-T5, P1-T1 through P1-T6, P2-T1, P3-T5). `SCRATCH\helpers-553.ps1` is created at first use, in P0-T5 — the plan's earliest helper invocation is P0-T5's `Test-BlockContained` verification of the reference blocks.

## No-C#-Toolchain Statement (binding on the executor)

This feature modifies **no** `*.cs`, `*.csproj`, `*.props`, `*.targets`, or `packages.config` file in its final diff. Therefore:

- The executor MUST NOT run the C# toolchain (`csharpier`, `msbuild`, `vstest.console.exe`) as verification of this change. There is no C# code change to verify, and a local C# pass would assert nothing about workflow YAML.
- There is **no local test harness for GitHub Actions workflows**. The only local verification available is `actionlint` (which includes YAML parse validation). The authoritative verification is a real green run of the reworked pipeline on the branch head (post-push), per `modified-workflow-needs-green-run`.
- The seeded fault-isolation probes (Phase 4) temporarily commit C# edits and then revert them; the net branch diff over C# files is zero, verified by P5-T3. The probe commits are exercised by CI itself (the analyzer/format/nullable/test gates), not by a local toolchain pass.
- No language with mandatory coverage policy is modified, so no baseline/final-QC coverage capture tasks apply. The coverage-bearing artifact of this pipeline (the `test-results` upload) is preserved unchanged and its continued production is verified in P4-T5.

## Byte-Identity Verification Method (used by P1-T6 and referenced tasks)

Reference blocks are extracted from the pre-split `.github/workflows/ci.yml` in Phase 0 (before it is rewritten) into `FEATURE/evidence/other/pre-split/`. A transplanted block passes if the reference text appears as a contiguous substring of the callee file after line-ending normalization (CRLF → LF) — i.e., byte-identical modulo line endings, which git manages via working-tree conversion. Canonical check:

```powershell
function Test-BlockContained([string]$RefPath, [string]$TargetPath, [string]$Label) {
  $ref = (Get-Content -Raw $RefPath) -replace "`r`n", "`n"
  $tgt = (Get-Content -Raw $TargetPath) -replace "`r`n", "`n"
  if (-not $tgt.Contains($ref.TrimEnd("`n"))) { throw "NOT BYTE-IDENTICAL: $Label" }
  Write-Output "BYTE-IDENTICAL: $Label"
}
```

Callee files must therefore keep each transplanted step at the same indentation depth as in the monolith (`steps:` under `jobs.<id>`, step names at 6-space indent), which is structurally guaranteed because callee jobs sit at the same YAML nesting depth.

## Structural Callee Check (referenced by P1-T1 through P1-T5)

```powershell
function Test-CalleeContract([string]$Path, [int]$Timeout) {
  $raw = (Get-Content -Raw $Path) -replace "`r`n", "`n"
  if ($raw -notmatch '(?m)^\s+workflow_call:')     { throw "missing workflow_call: $Path" }
  if ($raw -notmatch '(?m)^\s+workflow_dispatch:') { throw "missing workflow_dispatch: $Path" }
  if ($raw -notmatch '(?m)^permissions:')          { throw "missing permissions: $Path" }
  if ($raw -notmatch "timeout-minutes:\s*$Timeout"){ throw "missing/incorrect timeout: $Path" }
  if ($raw -match '(?m)^\s*concurrency:')          { throw "forbidden concurrency block: $Path" }
  if ($raw -match '(?m)^\s*needs:')                { throw "forbidden needs edge: $Path" }
  Write-Output "CONTRACT OK: $Path"
}
```

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Reading

- [x] [P0-T1] Read the policy documents in the Required References order and write `FEATURE/evidence/other/phase0-instructions-read.md`
  - Contents: `Timestamp:`, `Policy Order:` (list in the order read), explicit list of files read (all eleven Required References), and the No-C#-Toolchain Statement restated (no C# source in scope; actionlint is the only local harness; authoritative verification is the green run).
  - Acceptance: artifact exists with all required fields; no code file has been modified yet.

- [x] [P0-T2] Record the git baseline in `FEATURE/evidence/baseline/git-baseline.<TS>.md`
  - Command: `git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git status --porcelain`
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including `BASELINE_SHA: <sha>`, `BRANCH: <name>` (expected: `feature/ci-parallel-job-split-553`), and the porcelain output (docs/evidence and `.claude/agent-memory` entries are expected and permitted; note them, do not gate on them).
  - Acceptance: artifact exists; `BASELINE_SHA` and `BRANCH` are recorded for use by later tasks (P3-T2, P3-T4 reference `BRANCH`; diff-scoped verifications reference `BASELINE_SHA`). This is a record, not an invariant that `HEAD` stays at this SHA.

- [x] [P0-T3] Obtain actionlint 1.7.7 (windows_amd64) into the session scratchpad and run the pre-change lint baseline; write `FEATURE/evidence/baseline/actionlint-baseline.<TS>.md`
  - Command:
    ```powershell
    $v = '1.7.7'; $dir = 'C:\Users\DANMOI~1\AppData\Local\Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-14T09-01\012c26d5-57f2-4f08-bc74-bf50a60b1e4e\scratchpad\actionlint-553'
    New-Item -ItemType Directory -Force $dir | Out-Null
    Invoke-WebRequest "https://github.com/rhysd/actionlint/releases/download/v${v}/actionlint_${v}_windows_amd64.zip" -OutFile "$dir\actionlint.zip"
    Expand-Archive "$dir\actionlint.zip" -DestinationPath $dir -Force
    & "$dir\actionlint.exe" -no-color
    ```
    (run from the repository root; actionlint auto-discovers `.github/workflows/`)
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (expected: exit 0, zero findings over `ci.yml` and `codex-web-setup-test.yml`).
  - Acceptance: artifact exists with `EXIT_CODE: 0`. If non-zero, halt and report — the pre-change tree must lint clean before decomposition.

- [x] [P0-T4] Verify the measured sequential baseline artifact exists and record the check in `FEATURE/evidence/baseline/sequential-baseline-check.<TS>.md`
  - Command: `Select-String -Path 'docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md' -Pattern '444s'`
  - Acceptance: artifact exists with `EXIT_CODE: 0` and `Output Summary:` quoting at least one matched `444s` line, confirming the comparison denominator for P4-T6.

- [x] [P0-T5] Snapshot the pre-split `ci.yml` and extract the seven reference blocks into `FEATURE/evidence/other/pre-split/`
  - Files created:
    - `ci.yml.pre-split.txt` — full copy of the current `.github/workflows/ci.yml` (160 lines).
    - `header.txt` — lines 1–15 (`name: CI` through `cancel-in-progress: true`).
    - `actionlint-steps.txt` — lines 23–36 (`    steps:` through `./actionlint`).
    - `format-step.txt` — lines 91–93 (`Verify formatting` step).
    - `analyzer-step.txt` — lines 95–101 (analyzer build step incl. `$LASTEXITCODE` guard).
    - `nullable-step.txt` — lines 103–116 (nullable build step incl. the full `/t:Rebuild` rationale comment and `$LASTEXITCODE` guard).
    - `vstest-step.txt` — lines 118–150 (MSTest step incl. vswhere discovery, `\bin\Debug\`/`\obj\`/`\ref\` filter, zero-assembly `throw`).
    - `upload-step.txt` — lines 152–160 (`test-results` upload step incl. `if: always()`).
  - Extraction command pattern (0-based `-Index`): `(Get-Content .github/workflows/ci.yml)[22..35] | Set-Content <out>` etc. Sanity-assert the first line of each extract (`header.txt` → `name: CI`; `actionlint-steps.txt` → `    steps:`; `format-step.txt` → `      - name: Verify formatting`; `analyzer-step.txt` → `      - name: Build with analyzers and code style enforcement`; `nullable-step.txt` → `      - name: Build with nullable warnings treated as errors`; `vstest-step.txt` → `      - name: Run MSTest suite with coverage`; `upload-step.txt` → `      - name: Upload test results`). If any first line mismatches, locate the block by its `- name:` header instead of the line range and correct the extract.
  - Verification: run `Test-BlockContained` for each of the seven reference files against the live `.github/workflows/ci.yml`; all seven report `BYTE-IDENTICAL`.
  - Acceptance: eight files exist under `FEATURE/evidence/other/pre-split/` and all seven containment checks pass against the pre-change `ci.yml`.

### Phase 1 — Author Callee Workflows

> Planner note: setup steps (checkout, setup-dotnet, setup-msbuild, setup-nuget, both caches, `nuget restore`, `dotnet tool restore`) are copied from `ci.yml.pre-split.txt` for consistency, but only the seven Phase 0 reference blocks are gated byte-identical (spec AC 5 covers the gate commands, the actionlint step, and the upload step). File size constraint: every file in this phase is well under the 500-line limit; P5-T4 audits this explicitly.

- [x] [P1-T1] Create `.github/workflows/_actionlint.yml`
  - Contents: `name: actionlint`; `on:` with `workflow_call:` and `workflow_dispatch:` (both bare); `permissions: contents: read`; single job `actionlint` (`name: actionlint`, `runs-on: ubuntu-latest`, `timeout-minutes: 10`); steps block transplanted verbatim from `actionlint-steps.txt` (checkout with `fetch-depth: 1` + download/run actionlint 1.7.7). No `env:`, no `concurrency:`, no `needs:`.
  - Verification: `Test-CalleeContract '.github/workflows/_actionlint.yml' 10` passes AND `Test-BlockContained 'FEATURE/evidence/other/pre-split/actionlint-steps.txt' '.github/workflows/_actionlint.yml' 'actionlint-steps'` passes.

- [x] [P1-T2] Create `.github/workflows/_format-check.yml`
  - Contents: `name: format-check`; `on: workflow_call: / workflow_dispatch:`; `permissions: contents: read`; single job `format-check` (`name: Verify formatting`, `runs-on: windows-latest`, `timeout-minutes: 10`); steps copied from the pre-split snapshot: checkout (`fetch-depth: 1`), `Setup .NET SDK` (`actions/setup-dotnet@v4`, `dotnet-version: 10.0.x`), `Cache dotnet tools` (`~/.nuget/packages`, key `dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}`), `Setup CSharpier` (`dotnet tool restore`), then the `Verify formatting` step transplanted verbatim from `format-step.txt`. Deliberately omits `setup-msbuild`, `setup-nuget`, the `packages` cache, and `nuget restore` (spec: CSharpier reads source text only — this is the unverified tailored-setup assumption; fallback is P3-T5). No `env:` block (no step consumes `SOLUTION_PATH`/`BUILD_CONFIGURATION`/`BUILD_PLATFORM`).
  - Verification: `Test-CalleeContract '.github/workflows/_format-check.yml' 10` passes AND `Test-BlockContained ... 'format-step.txt' ...` passes AND `Select-String -Path .github/workflows/_format-check.yml -Pattern 'nuget restore'` returns no match.

- [x] [P1-T3] Create `.github/workflows/_build-analyzers.yml`
  - Contents: `name: build-analyzers`; `on: workflow_call: / workflow_dispatch:`; `permissions: contents: read`; single job `build-analyzers` (`name: Build with analyzers and code style enforcement`, `runs-on: windows-latest`, `timeout-minutes: 30`); job-level `env:` replicating the monolith values `SOLUTION_PATH: TaskMaster.sln`, `BUILD_CONFIGURATION: Debug`, `BUILD_PLATFORM: Any CPU`; steps copied from the snapshot: checkout (`fetch-depth: 1`), `Setup MSBuild` (`microsoft/setup-msbuild@v2`), `Setup NuGet` (`nuget/setup-nuget@v2`, `nuget-version: latest`), `Cache NuGet packages` (`packages`, key `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`), `Restore solution` (`nuget restore $env:SOLUTION_PATH`), then the analyzer gate step transplanted verbatim from `analyzer-step.txt`. Deliberately omits `setup-dotnet`, the dotnet-tools cache, and `dotnet tool restore` (tailored-setup assumption; fallback P3-T5).
  - Verification: `Test-CalleeContract '.github/workflows/_build-analyzers.yml' 30` passes AND `Test-BlockContained ... 'analyzer-step.txt' ...` passes AND `Select-String -Path .github/workflows/_build-analyzers.yml -Pattern 'setup-dotnet'` returns no match.

- [x] [P1-T4] Create `.github/workflows/_build-nullable.yml`
  - Contents: identical shape to `_build-analyzers.yml` (`name: build-nullable`; job `build-nullable`, `name: Build with nullable warnings treated as errors`, `timeout-minutes: 30`, same `env:`, same setup steps), then the nullable gate step transplanted verbatim from `nullable-step.txt` — including the complete `/t:Rebuild` rationale comment and the `$LASTEXITCODE` guard.
  - Verification: `Test-CalleeContract '.github/workflows/_build-nullable.yml' 30` passes AND `Test-BlockContained ... 'nullable-step.txt' ...` passes AND `Select-String -Path .github/workflows/_build-nullable.yml -Pattern '/t:Rebuild'` matches (comment and command both present via the containment check).

- [x] [P1-T5] Create `.github/workflows/_mstest-coverage.yml`
  - Contents: `name: mstest-coverage`; `on: workflow_call: / workflow_dispatch:`; `permissions: contents: read`; single job `mstest-coverage` (`name: Run MSTest suite with coverage`, `runs-on: windows-latest`, `timeout-minutes: 30`); same `env:` and setup steps as `_build-analyzers.yml`; then a NEW plain build step (not transplanted):
    ```yaml
      - name: Build solution
        shell: pwsh
        run: |
          & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
              "/p:Platform=Any CPU"
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    ```
    (no analyzer or warning-promotion properties, per spec); then the MSTest step transplanted verbatim from `vstest-step.txt`; then the upload step transplanted verbatim from `upload-step.txt` (`test-results`, `if: always()`, same paths, `if-no-files-found: warn`).
  - Verification: `Test-CalleeContract '.github/workflows/_mstest-coverage.yml' 30` passes AND `Test-BlockContained ... 'vstest-step.txt' ...` passes AND `Test-BlockContained ... 'upload-step.txt' ...` passes AND the plain-build step contains neither `EnableNETAnalyzers` nor `TreatWarningsAsErrors` (`Select-String` on the file returns exactly zero matches for each of those two strings).

- [x] [P1-T6] Record the byte-identity evidence artifact `FEATURE/evidence/qa-gates/byte-identity.<TS>.md`
  - Command: run `Test-BlockContained` for all six gated blocks against their callee files (`actionlint-steps` → `_actionlint.yml`; `format-step` → `_format-check.yml`; `analyzer-step` → `_build-analyzers.yml`; `nullable-step` → `_build-nullable.yml`; `vstest-step` and `upload-step` → `_mstest-coverage.yml`) and capture the six `BYTE-IDENTICAL:` output lines.
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing all six `BYTE-IDENTICAL` results, plus the line-ending-normalization note from the method section.
  - Acceptance: artifact exists; all six checks pass; this is the evidence pointer for spec AC 5.

### Phase 2 — Orchestrator Rewrite, Workflows README, and Local Lint

- [x] [P2-T1] Rewrite `.github/workflows/ci.yml` as a pure orchestrator
  - Contents: the file begins with the 15 header lines byte-identical to `header.txt` (`name: CI`, unchanged `on:` triggers, `permissions: contents: read`, unchanged `concurrency` block with `cancel-in-progress: true`), followed by a `jobs:` block containing exactly five jobs and nothing else:
    ```yaml
    jobs:
      actionlint:
        name: actionlint
        uses: ./.github/workflows/_actionlint.yml
      format-check:
        name: format-check
        uses: ./.github/workflows/_format-check.yml
      build-analyzers:
        name: build-analyzers
        uses: ./.github/workflows/_build-analyzers.yml
      build-nullable:
        name: build-nullable
        uses: ./.github/workflows/_build-nullable.yml
      mstest-coverage:
        name: mstest-coverage
        uses: ./.github/workflows/_mstest-coverage.yml
    ```
    No inline `steps:`, no `needs:` edges, no job-level `env:`.
  - Verification (all four must hold):
    1. `Test-BlockContained 'FEATURE/evidence/other/pre-split/header.txt' '.github/workflows/ci.yml' 'header'` passes and the header is the start of the file.
    2. `Select-String -Path .github/workflows/ci.yml -Pattern 'steps:'` returns no match.
    3. `Select-String -Path .github/workflows/ci.yml -Pattern 'needs:'` returns no match.
    4. `(Select-String -Path .github/workflows/ci.yml -Pattern 'uses: \./\.github/workflows/_').Count` equals 5.

- [x] [P2-T2] Create `.github/workflows/README.md`
  - Contents (sections required):
    1. `## Pipeline overview` — orchestrator + five callees table (file, runner, gate, timeout), zero `needs:` edges, caller-owned concurrency group, callees declare none.
    2. `## Per-stage workflow_dispatch procedure` — how to re-run one gate standalone: `gh workflow run _<name>.yml --ref <branch>` (or the Actions UI), note that a standalone dispatch forms its own run outside the CI concurrency group.
    3. `## Branch-protection rename procedure` — the research Q8 sequence: green run on the PR head → capture exact check-run names via `gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'` → single atomic `PUT /repos/drmoisan/TaskMaster/rulesets/18572843` carrying the full writable object (`name`, `target`, `enforcement`, `bypass_actors`, `conditions`, `rules`) with `strict_required_status_checks_policy: true` retained and read-only GET fields stripped → merge immediately → verify by GET. Include the rollback (single PUT restoring the previous contexts set).
    4. `## Rules` — pointers to `.claude/rules/ci-workflows.md` (deliberately-failing nested command pattern) and `.claude/rules/benchmark-baselines.md`.
  - Verification: `Select-String -Path .github/workflows/README.md -Pattern 'workflow_dispatch procedure','Branch-protection rename procedure'` matches both headings; file under 500 lines.

- [x] [P2-T3] Run actionlint over the post-change workflow set and write `FEATURE/evidence/qa-gates/actionlint-postchange.<TS>.md`
  - Command: `& "<SCRATCH>\actionlint-553\actionlint.exe" -no-color` from the repository root (lints all seven files: `ci.yml`, five `_*.yml`, `codex-web-setup-test.yml`; README is not lintable YAML).
  - Loop rule: if actionlint reports findings, fix the workflow files and re-run from this command until exit 0; re-run P1-T6 and P2-T1 verifications after any fix that touches a gated block.
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  - Acceptance: artifact exists with `EXIT_CODE: 0` on the final pass.

### Phase 3 — Commit, Pull Request, and First Green Run

- [x] [P3-T1] Commit the workflow change set and Phase 0–2 evidence
  - Files staged (explicit): `.github/workflows/ci.yml`, `.github/workflows/_actionlint.yml`, `.github/workflows/_format-check.yml`, `.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`, `.github/workflows/_mstest-coverage.yml`, `.github/workflows/README.md`, `FEATURE/evidence/**` (new artifacts), `FEATURE/plan.2026-08-14T09-05.md` (checkbox progress).
  - Verification: `git status --porcelain` shows no unstaged modifications to `.github/**` or `FEATURE/**` after the commit; `git log -1 --stat` lists the seven workflow-tree files.

- [x] [P3-T2] Push the branch to origin
  - Command: `git push -u origin <BRANCH>` where `<BRANCH>` is the value recorded by P0-T2 (expected: `feature/ci-parallel-job-split-553`). `-u` is correct: the remote branch does not yet exist. Do not hard-code any other branch name.
  - Acceptance: push succeeds (exit 0). Note: `ci.yml` does not trigger on push to this branch (`on: push` is main/development only); the pipeline runs on the `pull_request` event after P3-T3.

- [ ] [P3-T3] **ORCHESTRATOR CONFIRMATION REQUIRED — do not execute autonomously.** Create the pull request to `main` following the `pr-author` skill
  - Gate: the hook `.claude/hooks/enforce-pr-author-skill.ps1` blocks `gh pr create --body-file` unless ALL THREE preconditions hold, and two of them are outside the executor's capability:
    1. `artifacts/pr_context.summary.txt` exists, written by `mcp__drm-copilot__collect_pr_context` — this MCP tool is in the orchestrator's tool surface, not the executor's.
    2. `artifacts/orchestration/orchestrator-state.json` passes validation with `--require-pr-creation-ready` — the orchestrator writes this checkpoint; the executor never does.
    3. `artifacts/pr_body_553.md` AND `artifacts/pr_body_553.receipt.json` exist at exactly those canonical paths, with a matching, non-stale SHA-256 receipt per `.claude/skills/pr-author/SKILL.md`.
    The executor MUST halt at this task and hand off to the orchestrator to satisfy preconditions 1 and 2 and confirm execution; record the confirmation in the P3-T4 artifact. Without recorded confirmation this task is BLOCKED, not skipped.
  - Procedure (after confirmation and preconditions): produce `artifacts/pr_body_553.md` plus `artifacts/pr_body_553.receipt.json` per the `pr-author` skill, then `gh pr create --base main --title "CI: split quality-gates into five parallel reusable-workflow jobs (#553)" --body-file artifacts/pr_body_553.md`.
  - **Execution status (2026-08-14T10-35): DEFERRED to the orchestrator.** No pull request exists and the executor must not create one. The `modified-workflow-needs-green-run` obligation that the PR run would have satisfied is instead satisfied by the green `workflow_dispatch` run 31809697953 against head `0b016c81` (recorded in `FEATURE/evidence/qa-gates/first-run.<TS>.md`), which `remediation-inputs.2026-08-14T10-21.md` finding B1 explicitly accepts as an alternative. This task remains unchecked.
  - Acceptance: PR exists targeting `main`; `gh pr view --json url,headRefOid` returns the PR URL and head SHA; the PR's own run executes the NEW pipeline (head-ref workflow files run for `pull_request` events, research Q8 fact 1). Expected and acceptable: the PR is blocked by the still-required old context `Format, build, analyze, and test` until Phase 6 (fail-closed over-blocking, never under-gating).

- [x] [P3-T4] Observe the first run of the split pipeline to completion and record `FEATURE/evidence/qa-gates/first-run.<TS>.md`
  - Commands:
    ```powershell
    gh run list --branch <BRANCH> --workflow ci.yml --limit 1 --json databaseId,headSha,status,conclusion
    gh run watch <run-id> --exit-status
    gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/jobs --jq '.jobs[] | {name, conclusion, started_at, completed_at}'
    ```
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, run id, head SHA, per-job names and conclusions, `Output Summary:`.
  - Branching (explicit): (a) all five jobs succeed → record `GREEN` and proceed to P3-T5's NOT-REQUIRED branch; (b) a job fails with a symptom attributable to a trimmed setup step (e.g., missing .NET SDK / `dotnet` not found in an msbuild job, missing restored packages in the format job) → proceed to P3-T5's REQUIRED branch; (c) a job fails for any other cause → halt and report to the orchestrator with the run URL; do not improvise fixes outside this plan.
  - Acceptance: artifact exists and records one of the three branch outcomes explicitly.

- [x] [P3-T5] Execute the tailored-setup fallback if and only if P3-T4 took branch (b); record `FEATURE/evidence/qa-gates/tailored-setup-fallback.<TS>.md`
  - REQUIRED branch: restore into the affected callee(s) only the specific setup steps the failure implicates, copied verbatim from `FEATURE/evidence/other/pre-split/ci.yml.pre-split.txt` (msbuild callees: `Setup .NET SDK`; format callee: `Setup NuGet` + `Cache NuGet packages` + `Restore solution`). Re-run P1-T6 containment checks and P2-T3 actionlint, commit (`fix(ci): restore <steps> to <callee> — tailored-setup assumption failed`), push, and repeat P3-T4 observation until branch (a) or (c). Record which steps were restored, to which files, and the final green run id. Spec authorizes this fallback at an estimated ~56s/job cost.
  - NOT-REQUIRED branch (explicitly authorized skip): if P3-T4 recorded `GREEN`, write the artifact with `Result: NOT REQUIRED — tailored-setup assumption held` and the green run id. This is the only permitted non-executing outcome for this task.
  - Acceptance: artifact exists recording exactly one branch; the pipeline is green on the current head at task completion.

### Phase 4 — Seeded Fault-Isolation Probes and Post-Split Timing

> Probe rules: each probe is one temporary commit on the PR branch, exercised by CI, then reverted with `git revert --no-edit`. Wait for the probe run to complete BEFORE pushing the revert (`cancel-in-progress: true` would otherwise cancel the probe run). Each probe must fail exactly the targeted gate; if a probe reddens more than one gate, adjust the probe edit and repeat before recording. Net C# diff after reverts is zero (verified in P5-T3).

- [x] [P4-T1] [expect-fail] Exercise the formatting-violation probe and record `FEATURE/evidence/regression-testing/probe-format.<TS>.md`
  - Probe edit: a formatting-only change in one `*.cs` file that `csharpier check` rejects (e.g., broken indentation inside one method body); it must introduce no compiler diagnostic. Commit message: `probe(553): formatting violation — to be reverted`.
  - Sequence: commit → push → `gh run watch <run-id>` → `gh api .../runs/<run-id>/jobs` → assert `format-check` job conclusion `failure` and the other four jobs `success` → `git revert --no-edit <probe-sha>` → push.
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, probe commit SHA, revert commit SHA, run URL, per-job conclusion table, `Output Summary: exactly one red gate (format-check)`.
  - Acceptance: artifact shows exactly the format gate red; revert commit exists on the branch. Update spec.md seeded-condition checkbox 3 to `[x]` with this artifact as the evidence pointer.

- [x] [P4-T2] [expect-fail] Exercise the nullable-violation probe and record `FEATURE/evidence/regression-testing/probe-nullable.<TS>.md`
  - Probe edit: in a production `*.cs` file that carries `#nullable enable`, in a project whose `.csproj` does NOT set `TreatWarningsAsErrors` (verify with `Select-String` on the csproj before committing), add a correctly-formatted statement producing a nullable-flow warning (e.g., `string probeValue = null;` assigned to a non-nullable local). This fails only the nullable gate (`/p:TreatWarningsAsErrors=true`); the analyzer gate and the MSTest job's plain build treat it as a warning. Commit message: `probe(553): nullable violation — to be reverted`.
  - Sequence and contents: same shape as P4-T1; assert `build-nullable` conclusion `failure`, other four `success`; revert and push.
  - Acceptance: artifact shows exactly the nullable gate red; revert commit exists. Update spec.md seeded-condition checkbox 4 with this artifact as evidence.

- [x] [P4-T3] [expect-fail] Exercise the test-failure probe and record `FEATURE/evidence/regression-testing/probe-mstest.<TS>.md`
  - Probe edit: invert one assertion in one existing fast MSTest test (not `TestCategory=LiveOutlook`), keeping the file csharpier-clean and free of new compiler diagnostics. Commit message: `probe(553): deliberate test failure — to be reverted`.
  - Sequence and contents: same shape as P4-T1; assert `mstest-coverage` conclusion `failure`, other four `success`; revert and push.
  - Acceptance: artifact shows exactly the MSTest gate red; revert commit exists. Update spec.md seeded-condition checkbox 5 with this artifact as evidence.

- [x] [P4-T4] Confirm a green run on the post-revert head and record `FEATURE/evidence/qa-gates/post-probe-green-run.<TS>.md`
  - Commands: `gh run watch <run-id> --exit-status` on the run triggered by the final revert push; `gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/jobs --jq '.jobs[] | {name, conclusion}'`.
  - Acceptance: artifact records run id, head SHA, and all five job conclusions `success`.

- [x] [P4-T5] Verify the `test-results` artifact on the green run and record `FEATURE/evidence/qa-gates/test-results-artifact.<TS>.md`
  - Command: `gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/artifacts --jq '.artifacts[] | {name, size_in_bytes}'` (run id from P4-T4).
  - Acceptance: an artifact named exactly `test-results` exists with non-zero size. Update spec.md seeded-condition checkbox 6 with this artifact as evidence.

- [x] [P4-T6] Capture post-split per-job timings and write the baseline comparison `FEATURE/evidence/qa-gates/ci-split-timing-comparison.<TS>.md`
  - Command: `gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/jobs` (same method as the baseline capture; run id from P4-T4).
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`; a per-job table (name, started, completed, duration); measured pipeline wall clock (latest `completed_at` minus earliest `started_at` across the five jobs); comparison row against the measured 444s baseline from `ci-sequential-baseline.2026-08-14T13-05.md` with absolute and percentage delta; summed billed `windows-latest` seconds vs the ~444s baseline; a runner-environment-parity statement (both measurements GitHub-hosted `windows-latest`, satisfying `.claude/rules/benchmark-baselines.md`); a note that the spec's ~277s/~333s figures were estimates and this artifact is the measurement of record.
  - Acceptance: artifact exists with the comparison table populated from live API data (no placeholder values). Update spec.md seeded-condition checkbox 7 with this artifact as evidence.

### Phase 5 — Final QA Loop, Acceptance Reconciliation, and Pre-Migration Commit

> QA-loop note: the applicable language is GitHub Actions YAML. The loop is: (1) formatting — no repo-mandated YAML formatter exists; not applicable; (2) linting — actionlint (P5-T1); (3) type checking — not applicable to YAML; (4) testing — the live green run (P4-T4, re-confirmed on the final head in P5-T15). If any task in this phase changes a workflow file, re-run from P5-T1 and re-run the P1-T6 containment checks. Per the No-C#-Toolchain Statement, csharpier/msbuild/vstest are not part of this loop and must not be run.

- [x] [P5-T1] Run the final actionlint pass and record `FEATURE/evidence/qa-gates/actionlint-final.<TS>.md`
  - Command: `& "<SCRATCH>\actionlint-553\actionlint.exe" -no-color` from the repository root (re-download per P0-T3 if the scratchpad was cleared).
  - Acceptance: `EXIT_CODE: 0` over all seven workflow files. Update spec.md seeded-condition checkbox 1 with this artifact as evidence.

- [x] [P5-T2] Verify `$LASTEXITCODE` hygiene across the pwsh-bearing workflow files and record `FEATURE/evidence/qa-gates/lastexitcode-review.<TS>.md`
  - Scope: six of the seven workflow files are enumerated — the five callees plus `ci.yml` (the orchestrator has no steps). `.github/workflows/codex-web-setup-test.yml` is explicitly EXCLUDED from enumeration because it declares no `shell: pwsh` or `shell: powershell` step, so the `.claude/rules/ci-workflows.md` pattern cannot apply to it; the artifact must state this exclusion and reason so the recorded enumeration is consistent with its scope statement.
  - Method: enumerate every `shell: pwsh` step in the five callees and `ci.yml` (the orchestrator has none); confirm (a) no step intentionally invokes a failing nested command (the `.claude/rules/ci-workflows.md` pattern is therefore not triggered — matches research Q9), and (b) both msbuild gate guards `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` and the vstest `throw` are present, via `Select-String -Path .github/workflows/_build-analyzers.yml,.github/workflows/_build-nullable.yml -Pattern 'if \(\$LASTEXITCODE -ne 0\) \{ exit \$LASTEXITCODE \}'` (2 matches) and `Select-String -Path .github/workflows/_mstest-coverage.yml -Pattern 'throw "MSTest execution failed'` (1 match) and `Select-String -Path .github/workflows/_mstest-coverage.yml -Pattern 'throw "No test assemblies found'` (1 match).
  - Acceptance: artifact records the step-by-step table and the four match counts. Update spec.md seeded-condition checkbox 8 with this artifact as evidence.

- [x] [P5-T3] Verify the branch contains zero C#/project-file changes and record `FEATURE/evidence/qa-gates/no-csharp-diff.<TS>.md`
  - Command (two statements — PowerShell does not concatenate a subexpression with a trailing `..HEAD` into one argument):
    ```powershell
    $base = git merge-base origin/main HEAD
    git diff --name-only "$base..HEAD" -- '*.cs' '*.csproj' '*.props' '*.targets' '**/packages.config' '**/app.config'
    ```
    (the `**/` prefix is required on `packages.config` and `app.config`: a git pathspec with no wildcard is anchored to the repo root and would match zero files; `*.cs` and the other extension globs already match at any depth)
  - Acceptance: output is empty (probe commits are fully cancelled by their reverts). The artifact restates the No-C#-Toolchain Statement as the justification for the absence of a C# toolchain pass. If output is non-empty, halt and report — an unreverted probe or scope drift exists.

- [x] [P5-T4] Audit file sizes and record `FEATURE/evidence/qa-gates/file-size-audit.<TS>.md`
  - Command: `Get-ChildItem .github/workflows/*.yml, .github/workflows/README.md | ForEach-Object { "{0}`t{1}" -f $_.Name, (Get-Content $_.FullName).Count }`
  - Acceptance: every listed file is under 500 lines (expected: each callee < 100, `ci.yml` ~32, README < 150).

- [x] [P5-T5] Link the new workflows README from the feature folder
  - Edit: append to `FEATURE/issue.md` a `## References` entry: `- Workflows README: `.github/workflows/README.md` (created by #553)`.
  - Acceptance: `Select-String -Path FEATURE/issue.md -Pattern 'workflows/README.md'` matches (satisfies the spec DoD "created and linked from the feature folder" clause, completed by P7-T6).

- [x] [P5-T6] Check off spec.md acceptance criterion 1 (four gates as separate jobs, zero `needs:` edges) and its mirrors (issue.md AC 1, user-story.md AC 1)
  - Evidence pointers: `.github/workflows/ci.yml` (P2-T1 verification 3), `FEATURE/evidence/qa-gates/post-probe-green-run.<TS>.md`.
  - Acceptance: all three checkboxes `[x]`, each citing the evidence paths.

- [x] [P5-T7] Check off spec.md acceptance criterion 2 (five callee workflows with the reusable-workflow contract) and its mirrors (issue.md AC 2, user-story.md AC 2)
  - Evidence pointers: the five P1 task verifications (`Test-CalleeContract` results), `FEATURE/evidence/qa-gates/actionlint-final.<TS>.md`.
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [x] [P5-T8] Check off spec.md acceptance criterion 3 (`ci.yml` orchestrator, no inline `steps:`) and its mirrors (issue.md AC 3, user-story.md AC 3)
  - Evidence pointers: P2-T1 verification outputs (no `steps:` match, five `uses:` references).
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [x] [P5-T9] Check off spec.md acceptance criterion 4 (no cross-job file sharing; `test-results` upload preserved) and its mirrors (issue.md AC 4, user-story.md AC 4)
  - Evidence pointers: `FEATURE/evidence/qa-gates/byte-identity.<TS>.md` (upload block), `FEATURE/evidence/qa-gates/test-results-artifact.<TS>.md`.
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [x] [P5-T10] Check off spec.md acceptance criterion 5 (gate commands and actionlint step byte-identical, incl. `/t:Rebuild` comment, `$LASTEXITCODE` guards, zero-assembly `throw`)
  - Evidence pointers: `FEATURE/evidence/qa-gates/byte-identity.<TS>.md`, `FEATURE/evidence/qa-gates/lastexitcode-review.<TS>.md`.
  - Acceptance: spec checkbox `[x]` with evidence paths (no issue/user-story mirror carries this criterion standalone; their AC 8 equivalents are handled in P7-T3).

- [x] [P5-T11] Check off spec.md acceptance criterion 7 (README documents dispatch + rename procedures) and its mirrors (issue.md AC 6, user-story.md AC 6)
  - Evidence pointers: `.github/workflows/README.md` (P2-T2 heading verification).
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [x] [P5-T12] Check off spec.md acceptance criterion 8 (green run against the branch head, `modified-workflow-needs-green-run`) and its mirrors (issue.md AC 7, user-story.md AC 7)
  - Evidence pointers: `FEATURE/evidence/qa-gates/post-probe-green-run.<TS>.md` (superseded by P5-T15's final-head confirmation if additional commits landed in this phase).
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [x] [P5-T13] Check off spec.md acceptance criterion 10 (post-split wall clock measured with the baseline's collection method and recorded)
  - Evidence pointers: `FEATURE/evidence/qa-gates/ci-split-timing-comparison.<TS>.md`.
  - Acceptance: spec checkbox `[x]` with evidence path.

- [x] [P5-T14] Commit and push the Phase 3–5 evidence and document updates
  - Files staged (explicit): `FEATURE/evidence/**` (new artifacts from Phases 3–5), `FEATURE/spec.md`, `FEATURE/issue.md`, `FEATURE/user-story.md`, `FEATURE/plan.2026-08-14T09-05.md`.
  - Verification: `git status --porcelain` scoped to `FEATURE/**` and `.github/**` is empty after commit; push succeeds.

- [x] [P5-T15] Confirm a green run on the final pre-migration head and record `FEATURE/evidence/qa-gates/pre-migration-green.<TS>.md`
  - Commands: `gh run watch <run-id> --exit-status` on the run triggered by P5-T14's push; `gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/jobs --jq '.jobs[] | {name, conclusion}'`; `gh pr view --json headRefOid`.
  - Acceptance: all five jobs `success` on the current PR head SHA; artifact records run id and head SHA. This head is the reference state for the ruleset migration.

- [x] [P5-T16] Capture the exact check-run context names from the final head SHA into `FEATURE/evidence/other/check-run-names.<TS>.md`
  - Command: `gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'` (head SHA from P5-T15).
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, the verbatim name list, and the selected five required-context strings (the five CI contexts of the form `<caller job name> / <callee job name>`, plus/including the relocated actionlint context — captured, never assumed; research Q8 fact 3).
  - Acceptance: artifact exists with exactly five selected context strings, each copied verbatim from the API output. This file is committed in P7-T9 (committing it now would advance the head; the context names are derived from workflow/job names and do not vary by SHA).

### Phase 6 — Required-Status-Check Ruleset Migration (Orchestrator-Gated)

> Sequencing note: this phase runs only after P5-T15 (green run on the final head) and P5-T16 (names captured from that head). It mutates the repository's merge policy on `main` and is outward-facing. Phases 6–7 modify no source files; the Phase 5 QA loop therefore remains the final code-verification pass. If any workflow file changes after Phase 5, return to P5-T1.

- [ ] [P6-T1] Capture the pre-PUT ruleset and record `FEATURE/evidence/other/ruleset-pre-put.<TS>.json` plus `FEATURE/evidence/other/ruleset-pre-put.<TS>.md`
  - Command: `gh api repos/drmoisan/TaskMaster/rulesets/18572843 > <FEATURE>/evidence/other/ruleset-pre-put.<TS>.json`
  - Contents (md record): `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming the current required contexts are exactly `actionlint` and `Format, build, analyze, and test` and `strict_required_status_checks_policy` is `true`.
  - Acceptance: both files exist; the JSON is the unmodified GET response.

- [ ] [P6-T2] Construct the atomic PUT payload `FEATURE/evidence/other/ruleset-put-payload.<TS>.json` and verify it, recording checks in `FEATURE/evidence/other/ruleset-payload-verification.<TS>.md`
  - Tooling note: `jq` is not installed in this environment; all JSON manipulation in this task uses `ConvertFrom-Json` / `ConvertTo-Json`, and every `ConvertTo-Json` call MUST state `-Depth 20` explicitly — the default depth of 2 truncates the nested ruleset object and would silently corrupt the PUT payload.
  - Construction (PowerShell):
    ```powershell
    $rs = Get-Content -Raw '<FEATURE>/evidence/other/ruleset-pre-put.<TS>.json' | ConvertFrom-Json
    $payload = [ordered]@{
      name = $rs.name; target = $rs.target; enforcement = $rs.enforcement
      bypass_actors = $rs.bypass_actors; conditions = $rs.conditions; rules = $rs.rules
    }
    $rule = $payload.rules | Where-Object { $_.type -eq 'required_status_checks' }
    $rule.parameters.required_status_checks = @(<five @{ context = '<name>' } entries from P5-T16>)
    # strict_required_status_checks_policy is carried over unchanged from the GET (must remain $true)
    $payload | ConvertTo-Json -Depth 20 | Set-Content '<FEATURE>/evidence/other/ruleset-put-payload.<TS>.json'
    ```
    Never a partial patch; never a remove-then-add two-step.
  - Verification commands (all must pass, recorded in the md artifact; `$p = Get-Content -Raw '<payload>' | ConvertFrom-Json`):
    1. Read-only fields stripped: `$p.PSObject.Properties.Name` contains none of `id`, `node_id`, `created_at`, `updated_at`, `_links`, `source`, `source_type`, `current_user_can_bypass`.
    2. Exactly five contexts: `(($p.rules | Where-Object { $_.type -eq 'required_status_checks' }).parameters.required_status_checks).Count` equals `5`.
    3. Strict policy retained: `($p.rules | Where-Object { $_.type -eq 'required_status_checks' }).parameters.strict_required_status_checks_policy` is `$true`.
    4. Diff-only-in-contexts: load the pre-PUT JSON and the payload as objects; build the pre-PUT projection with the identical ordered literal used in the construction block — `[ordered]@{ name = $rs.name; target = $rs.target; enforcement = $rs.enforcement; bypass_actors = $rs.bypass_actors; conditions = $rs.conditions; rules = $rs.rules }` — same keys, same order; a plain `@{}` hashtable is PROHIBITED here because its key order is unspecified and `ConvertTo-Json` would emit the six keys in arbitrary order, producing a spurious diff between semantically identical documents; on BOTH objects set the `required_status_checks` rule's `parameters.required_status_checks` to `$null`; serialize BOTH with the same `ConvertTo-Json -Depth 20` call form (identical parameters, so `git diff --no-index` compares canonically) to `SCRATCH\proj-pre.json` and `SCRATCH\proj-new.json` (the session scratchpad, NOT the feature folder); `git diff --no-index SCRATCH\proj-pre.json SCRATCH\proj-new.json` must be empty (exit 0).
    5. Every context string in the payload appears verbatim in `check-run-names.<TS>.md`.
  - Acceptance: payload file and verification artifact exist; all five checks pass.

- [ ] [P6-T3] **ORCHESTRATOR CONFIRMATION REQUIRED — do not execute autonomously.** Apply the single atomic ruleset PUT
  - Gate: this task mutates the `main` merge policy. The executor MUST halt at this task and obtain explicit confirmation from the orchestrator before running the command. Record the confirmation (who/when/what was confirmed) in the P6-T4 artifact. Without recorded confirmation this task is BLOCKED, not skipped.
  - Command: `gh api --method PUT repos/drmoisan/TaskMaster/rulesets/18572843 --input <FEATURE>/evidence/other/ruleset-put-payload.<TS>.json`
  - Acceptance: command exits 0. The old context `Format, build, analyze, and test` is replaced by the five captured contexts in one request with no under-gating window (research Q8 fact 4).

- [ ] [P6-T4] Verify the post-PUT ruleset by GET and record `FEATURE/evidence/other/ruleset-post-put.<TS>.json` plus `FEATURE/evidence/other/ruleset-post-put.<TS>.md`
  - Command: `gh api repos/drmoisan/TaskMaster/rulesets/18572843 > <FEATURE>/evidence/other/ruleset-post-put.<TS>.json`
  - Verification (PowerShell; `jq` is not installed): `$post = Get-Content -Raw '<FEATURE>/evidence/other/ruleset-post-put.<TS>.json' | ConvertFrom-Json; $req = ($post.rules | Where-Object { $_.type -eq 'required_status_checks' }).parameters; $req.required_status_checks.context` returns exactly the five intended context strings (set equality with P5-T16's selection) and `$req.strict_required_status_checks_policy` is `$true`. Any re-serialization performed while recording this check MUST use `ConvertTo-Json -Depth 20`.
  - Contents (md record): `Timestamp:`, `Command:`, `EXIT_CODE:`, the orchestrator confirmation record from P6-T3, the five-context verification result, and the rollback procedure (single PUT restoring `ruleset-pre-put.<TS>.json`'s writable projection).
  - Acceptance: both files exist; context set matches exactly.

- [ ] [P6-T5] Record the merge-readiness handoff in `FEATURE/evidence/other/migration-handoff.<TS>.md`
  - Contents: statement that the PUT has landed and every other open PR now over-blocks until this PR merges (fail-closed, research Q8); instruction that the orchestrator must merge the split PR immediately, updating the branch first if `strict_required_status_checks_policy` requires it; note that the merge itself is orchestrator-owned and outside executor scope; pointers to all Phase 6 evidence files.
  - Acceptance: artifact exists; the orchestrator is notified in the executor's status output that the merge is the immediate next action.

### Phase 7 — Post-Merge Verification and Evidence Wrap-Up (Orchestrator-Gated)

> Timing gate: execute this phase only after the orchestrator has merged the split PR into `main`. If the session ends before the merge, record each remaining task's artifact with `Result: DEFERRED — awaiting merge` and the reason (this deferral branch is explicitly authorized for Phase 7 only). Evidence produced in this phase lands on `main` via a follow-up commit (P7-T9) because the PR is already merged.

- [ ] [P7-T1] **ORCHESTRATOR CONFIRMATION REQUIRED — do not execute autonomously.** Run the standalone `workflow_dispatch` smoke of each callee on `main` and record `FEATURE/evidence/qa-gates/dispatch-smoke.<TS>.md`
  - Merge precondition (recorded before the first dispatch): `gh pr view <pr-number> --json state,mergedAt,mergeCommit` must return `state == MERGED`; record the output in the artifact. If not merged, this task is in the DEFERRED branch (phase header), not executable.
  - Commands (for each of the five callees, after the precondition and recorded confirmation): `gh workflow run _<name>.yml --ref main`, then `gh run list --workflow _<name>.yml --limit 1 --json databaseId,status,conclusion` and `gh run watch <id> --exit-status`.
  - Contents: `Timestamp:`, `Command:`, `EXIT_CODE:`, the merge-precondition output, a five-row table (callee, run id, conclusion).
  - Acceptance: all five standalone dispatches conclude `success`. Update spec.md seeded-condition checkbox 2 with this artifact as evidence.

- [ ] [P7-T2] Check off spec.md acceptance criterion 6 (atomic ruleset PUT with captured contexts and recorded evidence) and its mirrors (issue.md AC 5, user-story.md AC 5)
  - Evidence pointers: `FEATURE/evidence/other/ruleset-pre-put.<TS>.json`, `ruleset-put-payload.<TS>.json`, `ruleset-post-put.<TS>.md`, `check-run-names.<TS>.md`.
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [ ] [P7-T3] Check off spec.md acceptance criterion 9 (no gate dropped, weakened, or made non-required) and its mirrors (issue.md AC 8, user-story.md AC 8)
  - Evidence pointers: `FEATURE/evidence/qa-gates/byte-identity.<TS>.md` (commands unweakened), `FEATURE/evidence/other/ruleset-post-put.<TS>.md` (all five contexts required).
  - Acceptance: all three checkboxes `[x]` with evidence paths.

- [ ] [P7-T4] Check off spec.md Definition of Done item 1 (acceptance criteria delivered and individually verified)
  - Evidence pointers: P5-T6 through P5-T13, P7-T2, P7-T3 (all ten spec ACs `[x]`).
  - Acceptance: DoD checkbox 1 `[x]`; every spec AC checkbox is `[x]` with an evidence pointer.

- [ ] [P7-T5] Check off spec.md Definition of Done item 2 (seeded test conditions exercised and outcomes recorded)
  - Evidence pointers: seeded checkboxes 1–8 updated by P5-T1, P7-T1, P4-T1, P4-T2, P4-T3, P4-T5, P4-T6, P5-T2 respectively.
  - Acceptance: DoD checkbox 2 `[x]`; all eight seeded-condition checkboxes are `[x]`.

- [ ] [P7-T6] Check off spec.md Definition of Done item 3 (README created and linked from the feature folder)
  - Evidence pointers: `.github/workflows/README.md` (P2-T2), `FEATURE/issue.md` References entry (P5-T5).
  - Acceptance: DoD checkbox 3 `[x]`.

- [ ] [P7-T7] Check off spec.md Definition of Done item 4 (evidence committed under the feature evidence tree)
  - Evidence pointers: `FEATURE/evidence/other/` (ruleset before/payload/after, check-run names, migration handoff), `FEATURE/evidence/qa-gates/` (green-run references, timing comparison), all staged for P7-T9.
  - Acceptance: DoD checkbox 4 `[x]`; every evidence file named by this plan exists on disk.

- [ ] [P7-T8] Check off spec.md Definition of Done item 5 (no C# toolchain pass required)
  - Evidence pointers: `FEATURE/evidence/qa-gates/no-csharp-diff.<TS>.md` (P5-T3).
  - Acceptance: DoD checkbox 5 `[x]`.

- [ ] [P7-T9] **ORCHESTRATOR CONFIRMATION REQUIRED — do not execute autonomously.** Commit residual evidence and write the final status summary `FEATURE/evidence/other/final-status.<TS>.md`
  - Files staged: `FEATURE/evidence/other/check-run-names.<TS>.md`, all Phase 6 evidence files, all Phase 7 evidence files, updated `FEATURE/spec.md` / `issue.md` / `user-story.md` / `plan.2026-08-14T09-05.md`.
  - Contents of the summary: pointers to every evidence artifact produced by this plan, the measured wall-clock delta vs the 444s baseline, the final ruleset state, and any deferred Phase 7 items with reasons.
  - Delivery: because the PR is merged, this commit lands via the orchestrator's follow-up mechanism (direct commit to `main` or a small follow-up PR, per orchestrator policy). The executor prepares the commit and reports; the push/PR decision is orchestrator-owned.
  - Acceptance: `git status --porcelain` scoped to `FEATURE/**` is clean after the commit; the summary artifact exists.

## Test Plan

- **Unit:** not applicable — no C# source is modified (see the No-C#-Toolchain Statement). No coverage baseline/final capture applies because no coverage-bearing language is in scope.
- **Static verification (local):** actionlint 1.7.7 over all seven workflow files (P0-T3 baseline, P2-T3 post-change, P5-T1 final); byte-identity containment checks over the six transplanted gate blocks (P1-T6); orchestrator structural checks (P2-T1).
- **Integration (authoritative):** the live pipeline itself — first green run (P3-T4, fallback P3-T5), three seeded fault-isolation probes with exactly-one-red-gate assertions (P4-T1..T3), post-revert green run (P4-T4), artifact continuity (P4-T5), standalone dispatch smoke of each callee post-merge (P7-T1).
- **Performance evidence:** post-split per-job timings vs the measured 444s baseline, same collection method, runner-environment parity (P4-T6).
- **Merge-policy migration:** pre-PUT GET, payload verification (five checks), orchestrator-confirmed atomic PUT, post-PUT GET set-equality verification (P6-T1..T4).

## Open Questions / Notes

- **Tailored-setup assumption is unverified** until the first green run (spec risk 2). P3-T4/P3-T5 handle both outcomes explicitly; the fallback restores only the implicated steps, verbatim from the pre-split snapshot.
- **Context-name capture is mandatory, never assumed** (highest-likelihood failure, research Q8/Q10 risk 1). The plan's caller/callee job names in P2-T1 are authoring inputs, not migration inputs; only P5-T16's captured strings enter the PUT payload.
- **Measured timings will differ from estimates** (~277s target / ~333s worst case are estimates); P4-T6 records the measurement of record without gating on a numeric threshold.
- **Cross-run account-level runner contention** can erode realized speedup and is not determinable from repository data (research Q7); note it in P4-T6 if the measured wall clock is anomalous.
- **Validator status:** this planner's tool surface has no Bash or MCP access; `mcp__drm-copilot__validate_orchestration_artifacts` was NOT run. A structural self-check was performed (phase headings `### Phase N — <Title>`, digit-only sequential task IDs per phase, checkbox task format). The orchestrator must run the validator and the `atomic-executor` preflight (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`) against this exact file path before execution, and all revisions must be made to this same file.
