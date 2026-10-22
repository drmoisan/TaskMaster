---
name: execution-environment
description: 'Execution-environment brief for research, planning, preflight, execution, and orchestration. Use before planning or delegating work so tool routes, command-text restrictions, the batch budget, hook context, CI shape, contract tests, evidence rules, coverage and file-size gates, and test-isolation checks are known and are not restated in plans.'
---

# Execution-Environment Brief

## Purpose and Maintenance

This brief states the environment facts that every researcher, planner, preflight executor, executor, and orchestrator needs before work starts. Plans reference this brief instead of restating its content; a plan that restates a tool route, a launcher, or a command-text restriction duplicates text that goes stale when this brief changes.

Maintenance rule: this brief is updated in the same change that alters any fact it states. It carries no line numbers and no commit identifiers; it cites files, symbols, headings, job names, and test names. The same text is byte-identical in six locations: `.claude/skills/execution-environment/SKILL.md`, `.agents/skills/execution-environment/SKILL.md`, `.github/skills/execution-environment/SKILL.md`, and their bundled copies under `extensions/drm-copilot/resources/`. Where the Claude and Codex runtimes differ, both paths are named (`.claude/hooks/...` and `.codex/hooks/...`).

## Tool Routes by Agent

- `atomic-executor` has no PowerShell tool. Its `tools:` list is Read, Grep, Glob, Edit, Write, several `Bash(...)` entries, the four `mcp__drm-copilot__run_poshqc_*` tools, and `mcp__drm-copilot__validate_orchestration_artifacts`. A `(pattern)` suffix on a `tools:` entry admits the bare tool, so the executor has Bash, subject to the repository hooks and the runtime guard described under Command-Text Restrictions.
- `atomic-planner` has no Bash. It has Read, Grep, Glob, Edit, Write, and `mcp__drm-copilot__validate_orchestration_artifacts`, so it can validate a plan with the installed validator before handoff but cannot run any toolchain command.
- `task-researcher` has Read, Grep, Glob, WebFetch, and Write. It cannot run `git show` or `gh`; facts that need either are marked for the orchestrator to confirm.
- `orchestrator` and the main session running the `orchestrate` skill have Bash, the PoshQC and validation MCP tools, and the `Agent` tool. Only they can delegate, so any step that needs a further agent run is orchestrator-owned.
- PowerShell for the executor therefore runs through the `mcp__drm-copilot__run_poshqc_*` tools or through a `sh` launcher script stored under `SCRATCH` that invokes PowerShell 7 from inside the script file.

## Command-Text Restrictions

Two separate mechanisms restrict Bash command text. They are kept apart here because they have different causes and different fixes.

- **Worktree isolation guard (Claude Code runtime behavior, not a repository hook).** Inside an agent-isolated worktree, the Claude Code runtime refuses Bash command text it cannot show stays inside the worktree. Observed refusals include text containing `pwsh`, `bash`, or `wsl`, heredocs, `printf --`, a `cd <path> &&` prefix before `git`, `git -C <other worktree>`, embedded `jq` filter strings, an unquoted shell variable in an option position, and loops or other compound constructs around `sh`. Text that passes includes `sh <file>.sh` with a literal path, `npx`, `poetry run python <script>`, `gh`, worktree-scoped `git`, and `git commit -F <file>`. No file under `.claude/hooks/` implements this guard, and no settings file registers it.
- **Repository hooks.** `.claude/hooks/validate-bash.ps1` (`.codex/hooks/validate-bash.ps1` for Codex; functions `Get-BlockedBashPattern`, `Get-CdChainedReadCommandMatch`, `Get-BashBlockReason`) denies destructive patterns (`rm -rf`, forced pushes, `git reset --hard`, recursive forced removal) and a `cd` followed later in the same command by a file-reading command such as `grep`, `cat`, `head`, `tail`, `less`, `more`, `awk`, or `sed -n`. `.claude/hooks/enforce-parallel-worktree-removal-gate.ps1` denies the literal worktree-removal command anywhere in a command.
- A multi-line `poetry run python -c` program exits 0 without running. Put Python helpers in a file under `SCRATCH` and run the file.

## PowerShell Route and PoshQC MCP Limits

- PoshQC MCP results carry no output. The summary string is composed from a fixed template before the child process runs (`runPoshQcWorkflow` in `extensions/drm-copilot/src/repo-automation-service.ts`), and `RepoAutomationExecutionResult` has no exit-code or output field. An `ok: true` result proves only that the tool was invoked.
- Counts, percentages, and findings therefore come from files or direct runs: pass and fail counts from `artifacts/pester/pester-junit.xml`, line coverage from `artifacts/pester/powershell-coverage.xml`, and analyzer findings from a direct `Invoke-ScriptAnalyzer` run with `scripts/powershell/PoshQC/settings/pssa.settings.psd1`.
- The MCP test tool reads the installed extension's settings. Branch-current Pester runs import `scripts/powershell/PoshQC/PoshQC.psd1` and call `Invoke-PoshQCTest` from a `sh`-launched PowerShell 7. The coverage population is derived by `Invoke-PoshQCTest` from `config/poshqc-coverage.json`; there is no hand-maintained coverage path list.
- `Invoke-Pester` without `-EnableExit` (or `Run.Exit`) exits 0 when tests fail; read the JUnit file rather than the exit code of such a run.

## Batch Budget

- In direct mode the batch-budget hooks allow at most three distinct production files per kind in one session: the fourth distinct non-test PowerShell file is denied with `POWERSHELL_LARGE_PATH_REQUIRED` and the fourth distinct non-test Python file with `PYTHON_LARGE_PATH_REQUIRED` (`.claude/hooks/enforce-powershell-batch-budget.ps1` and `.claude/hooks/enforce-python-batch-budget.ps1`; Codex copies under `.codex/hooks/`). Test files never count.
- Exemption: the cap does not apply when `artifacts/orchestration/orchestrator-state.json` in the worktree that holds the hook script selects the `large`, `remediation`, or `preparation` route and is not terminal (`Test-BatchBudgetLargePathRoute` in `.claude/hooks/enforce-batch-budget-route.ps1`). An isolated subagent worktree has no checkpoint, because `artifacts/` is gitignored, so direct mode applies there.
- Bundle mirror copies under `extensions/drm-copilot/resources/` are production files and count against the cap.
- A scratch `.ps1` file counts when it resolves inside the worktree root. Files under `SCRATCH`, which is outside the worktree root, are discarded without consuming a slot.
- Only `Write` and `Edit` calls are seen (the hook matcher is `Write|Edit`). Files written by Bash, by a copy script, or by a generator are not seen.
- Budget state is kept per session under `.claude/state/`; there is no automatic reset.

## Hook Execution Context

- Hooks for an isolated subagent run inside the subagent's worktree with that worktree's hook scripts, which are `main`'s copies. A hook change on a branch is not exercised by isolated subagents until it merges.
- Hook behavior on a branch is therefore proven by Pester suites, not by a live subagent run. A live check of a branch hook needs a non-isolated delegation from the item worktree.
- Enforcement hooks do not invoke Python; bash is preferred and PowerShell is acceptable (`tests/scripts/claude-runtime/enforcement-hooks-no-python-invocation.Tests.ps1`).

## MCP Surfaces Run the Installed Extension

- Every `mcp__drm-copilot__*` tool runs the installed extension, not the branch. `validate_orchestration_artifacts` applies the installed rule set, so validator rules added on a branch apply through MCP only after the extension release that carries them is installed.
- Branch-current plan validation runs the repository's Python CLI `scripts/dev_tools/validate_orchestration_artifacts.py` with its `plan` subcommand, through Poetry in module form.
- A push-down served by the MCP tools writes the installed extension's bundled payload; a repository-side edit under `extensions/drm-copilot/resources/` changes that payload only after a rebuild and reinstall.

## CI Shape and Required Checks

- `.github/workflows/ci.yml` triggers on `push` to `main` and `development`, on `pull_request` to `main`, `development`, and `epic/**`, on `workflow_dispatch`, and on `merge_group` (`types: [checks_requested]`, added by issue #952), with no path filters. When the merge queue on `main` is enabled, every required check is produced by a `merge_group` run of `ci.yml` as well as by the pull-request run; `.github/workflows/README.md` (`## Triggers`) records the trigger set.
- Jobs in `ci.yml` and their check names: `Change scope` (job `changes`, the change-scope decision below); `CI full run` (job `full-run-marker`, which runs only on a full run, marks that run as a valid base for later decisions, and is not a required check); `quality-checks7 / Code Quality & Tests (<python-version>)` on Python 3.10, 3.11, 3.12, and 3.13 (`_quality-checks.yml`: Black check, Ruff, Pyright, the Codex generator `--check`, the tier classification, Pytest with line and branch coverage, and the coverage-threshold check on the JSON report totals); `security-scan / Security Scanning` (its `safety check` step carries `continue-on-error: true` and does not fail the check); `docs-validation / Documentation Validation`; `build-check / Build Package`; `shell-coverage / Shell Coverage (Bats + kcov)`; `drm-copilot-extension-tests / drm-copilot Extension Tests (<os>)` on `ubuntu-latest` and `windows-latest`; `root-typescript-tests / Root TypeScript Tests (<os>)` on `ubuntu-latest` and `windows-latest`; `NPM Audit Gate / npm audit (<manifest>)` (the calling job carries the name `NPM Audit Gate`); and the `poshqc` group below.
- PowerShell CI (`_poshqc.yml`): `poshqc / PowerShell QC static` (format and analyze on `windows-latest`); the shard matrix `poshqc / PowerShell QC shard 1`, `shard 2`, and `shard 3` (`Invoke-PoshQCTest -ShardIndex -ShardCount` over the scan folders of `config/poshqc-scan.json`, with coverage, on `windows-latest`); the aggregator `poshqc / PowerShell QC`, which requires the static and shard jobs to succeed, verifies shard completeness against a Pester discovery count, and merges shard coverage; and `poshqc / PowerShell hook suites (Linux)`, which runs only `tests/scripts/claude-hooks` and `tests/scripts/codex-hooks` on `ubuntu-latest` with coverage disabled. A new test file under a scanned folder is assigned to a shard automatically and is counted by the completeness check.
- Change-scope decision (issue #962): the `Change scope` job runs `scripts/ci/Invoke-CiChangeScope.ps1` and publishes the job output `code`. The decision is `code=false` (docs-only) only on a `pull_request` event from this repository, through one of two comparisons. Green-run base: every file changed since the newest successful `ci.yml` run on the pull-request branch whose `CI full run` job succeeded, and whose head commit is present and an ancestor of the current head, is on the allowlist `config/ci-docs-only-paths.json`; the reason is `DOCS_ONLY_SINCE_<green head sha>`. Merge-base fallback (issue #973): when the green-run step ends with no earlier full green run (`NO_FULL_GREEN_RUN`), a missing base (`BASE_NOT_PRESENT`), or a non-ancestor base (`BASE_NOT_ANCESTOR`), and the pull request's base ref is exactly `main`, the script computes `git merge-base` of the payload base SHA (`BASE_SHA`, from `github.event.pull_request.base.sha`; the base ref arrives as `BASE_REF`) and the head, lists the files changed since that merge base with the same diff arguments, and decides `DOCS_ONLY_SINCE_MERGE_BASE_<merge base sha>` when every changed file is allowlisted. Both comparisons run the diff with rename detection off, so both the old and the new path of a rename are classified. A path is allowlisted when it starts with `docs/features/active/`, `docs/features/archive/`, `docs/features/completed/`, `docs/features/epics/`, `docs/features/parallel/`, `docs/features/potential/`, or `docs/research/`, does not start with `docs/features/templates/` or `docs/features/potential/template.md`, and ends in `.md`, `.txt`, `.log`, `.json`, or `.xml`; the allowlist file itself is not allowlisted. Every other case is a full run (`code=true`): `push`, `merge_group`, and `workflow_dispatch` events, a fork pull request, unreadable run or job data, no earlier full green run or a missing or non-ancestor base when the base ref is not `main`, a merge base that cannot be resolved (`MERGE_BASE_UNRESOLVED`, for example a blank base SHA or a base commit missing from the checkout), a failed ancestry check, an empty diff, any path outside the allowlist, and any error. The decision step carries `continue-on-error: true` and the callers compare the output with `!= 'false'`, so a failed or missing decision also gives a full run.
- Docs-only behavior (`code=false`): `ci.yml` passes `run-heavy: false` to `_quality-checks.yml`, `_security-scan.yml`, `_build-check.yml`, `_poshqc.yml`, `_shell-coverage.yml`, and `_drm-copilot-extension-tests.yml`. Each required job still starts under its full-run check name with every step skipped, so every required context reports `success` without running its suite. In `_poshqc.yml` the aggregator `poshqc / PowerShell QC` runs with every step gated, and `poshqc / PowerShell QC static`, the shard jobs, and `poshqc / PowerShell hook suites (Linux)` are skipped at job level. `docs-validation / Documentation Validation` runs in full. `root-typescript-tests`, `NPM Audit Gate`, and `CI full run` are skipped at job level. A `success` on a docs-only commit is therefore not evidence that the heavy suites pass for that commit; that evidence is the newest run whose `CI full run` job succeeded.
- A commit that changes any path outside the allowlist (any code, workflow, configuration, script, test, or customization file) runs the full set of jobs, and its `CI full run` job marks it as a base for later docs-only decisions. A standalone `workflow_dispatch` of a reusable workflow, or a caller that omits `run-heavy`, also runs in full. `.github/workflows/README.md` (`## Change-Scope Decision (Issue #962)`) records the reason codes and the per-context behavior table.
- Required contexts of ruleset 15241672, confirmed by the orchestrator through `gh api` on 2026-10-16: `drm-copilot-extension-tests / drm-copilot Extension Tests (ubuntu-latest)`, `drm-copilot-extension-tests / drm-copilot Extension Tests (windows-latest)`, `docs-validation / Documentation Validation`, `poshqc / PowerShell QC`, `shell-coverage / Shell Coverage (Bats + kcov)`, `security-scan / Security Scanning`, `quality-checks7 / Code Quality & Tests (3.10)`, `quality-checks7 / Code Quality & Tests (3.11)`, `quality-checks7 / Code Quality & Tests (3.12)`, `quality-checks7 / Code Quality & Tests (3.13)`, `build-check / Build Package`.
- Refresh rule: a change that renames a job, adds or removes a matrix entry, or changes the ruleset refreshes this list in the same change, and the orchestrator re-confirms the contexts with a read-only `gh api repos/drmoisan/drm-copilot/rulesets/15241672` call before relying on them.
- Skipped-check semantics: a job skipped by an `if:` condition reports success to a required check; a workflow that does not run at all reports nothing, which leaves the required check pending.
- A pull request into a branch other than `main`, `development`, or `epic/**` gets no CI run; dispatch `ci.yml` manually for it.

## CI-Only Failure Classes

- A test that reads gitignored `artifacts/` state passes only on a worktree that holds that state; CI has none.
- A fixture rooted at a drive letter or a `C:/workspace` path fails `path.isAbsolute` and path joins on Linux.
- `_shell-coverage.yml` checks out at depth one, so `origin/main` does not exist in that job; no test may diff against it.
- The Linux hook-suite job runs `tests/scripts/claude-hooks` and `tests/scripts/codex-hooks` on a case-sensitive file system: use `Join-Path`, forward-slash relative paths, exact file-name case, and no drive letters or backslash literals.
- The `.gitattributes` rule `* text=auto eol=lf` normalizes line endings on checkout, so a CRLF fixture committed as text is not CRLF in CI.
- Shard and single-process Pester runs expose module-scope import visibility and leaked state that per-suite local runs hide.

## Contract Tests That Pin Mirrored Files

Run these after editing any mirrored or bundled file; each is under `tests/scripts/dev_tools/` unless named otherwise.

- `test_push_down_claude_resource_contracts.py` and `test_push_down_codex_and_agents_resource_contracts.py`: every distributable `.claude`, `.agents`, and `.codex` file exists in the bundle with equal text.
- `test_push_down_claude_pack_manifest_completeness.py` and `test_push_down_codex_and_agents_pack_manifest_completeness.py`: every bundled file is listed in a pack manifest.
- `test_push_down_pack_manifest_dependency_closure.py`: every `$PSScriptRoot` dependency of a manifested PowerShell file, and every hook script named by `.claude/settings.json`, manifested agent frontmatter, or `.codex/config.toml`, is in core plus the owning pack.
- The Codex generator check: `scripts/dev_tools/generate_codex_agent_variants.py` run in module form with `--check` (base agents are hand-edited; variants, bundle copies, and the Codex `core.json` are generated).
- `test_codex_agent_wrapper_contracts.py` and `test_codex_handoff_contract_parity.py`: pinned Codex agent fragments and handoff parity.
- `test_claude_rules_frontmatter.py`: rule `paths:` scopes and that every preloaded skill exists.
- `parallel_orchestrator_surface_expectations.py` (`PINNED_FROZEN_SURFACE_HASHES`): byte hashes of frozen orchestration surfaces, including `.claude/skills/epic-orchestrate/SKILL.md` and `.claude/agents/epic-orchestrator.md`.
- `test_quality_checks_workflow_contracts.py`: workflow step conditions.
- `test_push_down_issue_824_follow_ups.py`: pushed rules and skills name no consuming product.
- `test_typescript_toolchain_instruction_contracts.py`: no reference to another test framework, and every backticked npm script token in the pinned mirrors exists in the root `package.json`.
- `tests/scripts/claude-runtime/claude-architecture-doc.Tests.ps1` and `tests/scripts/claude-runtime/hook-dependency-guard-completeness.Tests.ps1`: agent tool names in the architecture document and the guarded dot-source pattern in hooks.

## Preflight Defect Classes the Validator Cannot See

These pass every validator rule and are found only by reading the plan against the tree:

- `git grep` against a file the plan creates (untracked, so invisible to `git grep`); a search pattern beginning with `-` that is not passed through `-e`.
- A checkbox diff-pair assertion against a file the plan adds, and a CRLF fixture under the `eol=lf` attribute rule.
- A new TypeScript production file without its own `coverageThreshold` entry in `extensions/drm-copilot/jest.config.cjs`, whose threshold map has per-file entries only and no `global` key.
- A per-file coverage floor asserted for code that the tests replace with blanket mocks.
- Batch-budget exhaustion across a session, and an acceptance condition that depends on a branch hook being exercised by an isolated subagent.
- `origin/main` advancing during planning: scope diffs anchor on the merge base of `origin/main` and `HEAD`, never on `origin/main` itself.
- `Measure-Object -Line` counts non-empty lines only; physical line counts use `wc -l` or `@(Get-Content -LiteralPath <path>).Count`.
- A coverage argument written as a filesystem path measures nothing; the dotted module form is required, and branch figures come from the JSON report, not the combined terminal column.
- A task that changes the observable behavior of an existing function, script, fixture, or test stub (signature, argv, output, exit status, or written files) while the tests that assert that behavior are outside the task's file set: those tests fail once the change lands, and the plan's own acceptance cannot be met. Preflight searches the test tree for the changed symbol, the file name, and the asserted output, and confirms that the task names each matching test with its disposition (unchanged because unaffected, or updated with the reason).
- A plan that alters a fact this brief states (it adds or renames a CI job, changes a required status context, a tool route, or a batch-budget rule) with no task that updates the brief in the same change, in all six byte-identical copies: the plan then breaks the maintenance rule in `## Purpose and Maintenance`. Preflight compares each task's changes against the facts this brief states and confirms that a brief-update task exists and that the plan's `Mirror parity` Pre-Mortem bullet names it.

## Evidence and Documentation Rules

- Evidence lives under `<FEATURE>/evidence/<kind>/` with kinds `baseline`, `regression-testing`, `qa-gates`, `other`, `issue-updates`, and `remediation-baseline` (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; enforced by `.claude/hooks/enforce-evidence-locations.ps1`).
- Evidence filenames carry the UTC time of the write, read from the clock, in the `yyyy-MM-ddTHH-mm` shape; a consumer selects the lexicographically greatest `<name>.*.md`. Plan files are stamped by the feature-folder tooling in America/New_York.
- Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. No artifact records an absolute path, a host name, or a session scratchpad path; scratch locations are recorded as the token `SCRATCH`.
- `SCRATCH` is `<session scratchpad>/issue-<issue-num>/`, and scratch file names start with `issue-<issue-num>-`.
- `.github/workflows/_docs-validation.yml` checks only that `README.md` is present and non-empty and that `LICENSE` exists, and it warns when two instruction files are missing. There is no Markdown lint or link check in CI.

## Coverage and File-Size Gates

- Line coverage of at least 85% and branch coverage of at least 75% apply per changed production file in Python and TypeScript. PowerShell has a line threshold of 85% and no branch gate, because Pester does not measure branch coverage. No production file may be excluded from measurement.
- Python: measure a module with `--cov=<dotted.module>`, `--cov-branch`, `--cov-report=term-missing`, and `--cov-report=json:<path>`; read branch figures from the JSON report.
- TypeScript: a new production file needs its own entry in the `coverageThreshold` map of `extensions/drm-copilot/jest.config.cjs`; Jest runs through `node run-jest.cjs` in `extensions/drm-copilot/`.
- PowerShell: per-file line coverage is read from `artifacts/pester/powershell-coverage.xml` written by the self-hosted PoshQC module.
- No production, test, or reusable script file exceeds 500 physical lines; Markdown is exempt. Files at or near the limit are not extended; new logic goes into a new module and a new test file.

## Test Isolation Checks

- Tests create no temporary files; file and transcript reads go through injected seams with in-memory content.
- Before review, run each isolation check and record it under `<FEATURE>/evidence/regression-testing/`:
  - a seeded shuffled Jest run (`node run-jest.cjs --randomize --seed=<n>` in `extensions/drm-copilot/`) with the seed recorded;
  - a reversed-order run of the new or changed Python test modules in one process;
  - a seeded shuffled file-order run of the touched Pester suites in one process, with at least three seeds and each seed and its file order recorded;
  - a fresh single-process Pester run over `tests/scripts`, which is the CI shape.
- An order dependence found by any run is fixed on both sides: the suite that leaves state behind (globals, functions, environment variables, mocks, imported modules, script-scope state) and the suite that assumes prior state. Every seed is then re-run.
