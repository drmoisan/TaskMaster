# claude-md-cites-ciyml-for-moved-toolchain-commands (Spec)

- **Issue:** #564
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T08-58
- **Status:** Draft
- **Version:** 0.1

## Write Set

- `CLAUDE.md`

## Context
- `CLAUDE.md` cites .github/workflows/ci.yml as the source of three C# toolchain commands (CSharpier pinned-version parity, the analyzer `/t:Build /m` step, and the nullable `TreatWarningsAsErrors` step). PR #556 (issue #553, the CI parallel job split) moved the underlying steps out of ci.yml into five reusable `workflow_call` files; ci.yml is now a five-job dispatcher with no `msbuild` or `csharpier` invocation of its own.
- Observed environment(s): repository documentation (`CLAUDE.md`), not runtime.
- Customer impact and severity (who is affected, how often, how bad): every agent session loads `CLAUDE.md` first in the policy precedence order; any agent that follows a citation to verify a command finds nothing at that file, and the third citation names a step title that exists only inside _build-nullable.yml. Low severity — the cited commands themselves are still accurate; only the file attribution is wrong.
- First observed date and version(s) impacted: filed 2026-08-15 as issue #564, found during the `build-ci-coverage-gate-fidelity` epic fan-in review; the underlying split landed via PR #556.

## Repro & Evidence
- Steps to reproduce: open `CLAUDE.md`, read the C# toolchain section (`## C# Code Change Policy` / `### C#1. Tooling & Baseline for C#`), and open .github/workflows/ci.yml to verify the cited command exists there.
- Expected vs actual behavior: expected — the cited file contains the command described; actual — ci.yml contains only `uses:` dispatch entries to _actionlint.yml, _format-check.yml, _build-analyzers.yml, _build-nullable.yml, and _mstest-coverage.yml.
- Logs/screenshots/error snippets: not applicable; verified directly by reading `CLAUDE.md` lines 185-213 and the five workflow files under .github/workflows/ on 2026-09-02.
- Frequency / determinism: deterministic — the stale citation is present on every read of `CLAUDE.md` on the current `main` branch.

## Scope & Non-Goals
- In scope: correcting the three file citations in `CLAUDE.md` (around lines 194, 202, and 210-211) to point at the reusable workflow file that actually contains the cited step.
- Out of scope / non-goals: changing any command text, changing any workflow file's behavior, changing .claude/rules/csharp.md (already correct — verified 2026-09-02, no citation to ci.yml or any reusable workflow file name is present there), and any change under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json (those are published from the upstream `drm-copilot` repository with zero templating; any apparent need to edit one of those paths is recorded as out-of-scope, not implemented here).
- Explicitly excluded systems, integrations, or datasets: none; this repository has no extensions/ tree, no scripts/dev_tools/ tree, and no Python toolchain, so no plan step targets those paths.

## Root Cause Analysis
- Confirmed root cause: PR #556 relocated the CSharpier-check step into .github/workflows/_format-check.yml, the analyzer build step into .github/workflows/_build-analyzers.yml, and the nullable build step (named "Build with nullable warnings treated as errors") into .github/workflows/_build-nullable.yml, but `CLAUDE.md`'s three citations were not updated to match. The parallel fix in .claude/rules/csharp.md (merged at `fb8eff9b`) never carried a citation to ci.yml, so it required no equivalent correction.
- Signals/evidence supporting it: direct read of .github/workflows/ci.yml (five `uses:` jobs, no `msbuild`/`csharpier` command), .github/workflows/_build-analyzers.yml (`msbuild ... /t:Build /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), .github/workflows/_format-check.yml (`dotnet tool restore` then a CSharpier check step), and .github/workflows/_build-nullable.yml (job step named "Build with nullable warnings treated as errors" running `msbuild ... /t:Rebuild /m ... /p:TreatWarningsAsErrors=true`, matching `CLAUDE.md`'s cited command character-for-character).
- Affected components/modules: `CLAUDE.md` only.

## Proposed Fix

### Design summary (what changes where):
Three inline-text edits to `CLAUDE.md`:
1. Line ~194 (end of the CSharpier bullet): replace the .github/workflows/ci.yml citation with .github/workflows/_format-check.yml.
2. Line ~202 (end of the analyzer bullet): replace the .github/workflows/ci.yml citation with .github/workflows/_build-analyzers.yml.
3. Line ~210 (start of the nullable-command explanatory bullet): replace the .github/workflows/ci.yml citation with .github/workflows/_build-nullable.yml; the parenthetical step name "Build with nullable warnings treated as errors" is retained unchanged because it correctly names the step inside _build-nullable.yml.

### Boundaries and invariants to preserve:
- Do not alter any command text (`dotnet tool run csharpier check .`, the `msbuild ... /t:Build /m ...` invocation, or the `msbuild ... /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` invocation).
- Do not alter .claude/rules/csharp.md — verified already correct.
- Do not alter any file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json.

### Dependencies or blocked work:
None. This is a documentation-only correction with no code or workflow behavior change.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `CLAUDE.md`

#### Functions/classes/CLI commands impacted:
None — no command text changes, only file-attribution text.

#### Data flow and validation changes:
None.

#### Error handling and logging updates:
None.

#### Rollback/feature-flag considerations (if applicable):
None; a single-file text revert is sufficient if needed.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
Markdown prose edits only; no schema or interface change.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
None; documentation citation only.

#### Performance constraints (latency/throughput/memory):
Not applicable.

## Assumptions, Constraints, Dependencies
- Assumptions: .github/workflows/_build-analyzers.yml, .github/workflows/_format-check.yml, and .github/workflows/_build-nullable.yml remain the current homes of the three cited commands as of 2026-09-02 (verified by direct read).
- Constraints: documentation-only change; no .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json edits; no extensions/ or scripts/dev_tools/ targets (neither tree exists in this repository).
- External dependencies: none.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
- Regression tests to add or update: none — this is a documentation citation fix with no executable behavior; a Markdown-text change has no unit-test surface, so no MSTest/Pester/pytest additions apply.
- Unit tests: not applicable (no source code changed).
- Edge cases and negative scenarios: verify no other stale ci.yml citation for a moved command remains in `CLAUDE.md` after the edit.
- Error handling and logging verification: not applicable.
- Coverage impact and targets for changed lines/modules: not applicable; `CLAUDE.md` is documentation, outside the coverage-measured source tree.
- Toolchain commands to run: none of the C# toolchain commands apply to a Markdown-only change; verification is a targeted text search over `CLAUDE.md` confirming the three citations now name the correct reusable workflow files and confirming no other citation to .github/workflows/ci.yml remains for a relocated command.
- Manual validation steps: read the corrected `CLAUDE.md` lines 194, 202, and 210-211 alongside the corresponding .github/workflows/_format-check.yml, .github/workflows/_build-analyzers.yml, and .github/workflows/_build-nullable.yml files to confirm each citation now names the file that actually contains the cited step.

## Acceptance Criteria
- [ ] `CLAUDE.md` line ~194 cites .github/workflows/_format-check.yml (not ci.yml) for the CSharpier pinned-version claim.
- [ ] `CLAUDE.md` line ~202 cites .github/workflows/_build-analyzers.yml (not ci.yml) for the analyzer `/t:Build /m` claim.
- [ ] `CLAUDE.md` line ~210-211 cites .github/workflows/_build-nullable.yml (not ci.yml) for the nullable `TreatWarningsAsErrors` claim, retaining the step-name parenthetical "Build with nullable warnings treated as errors".
- [ ] No remaining citation to .github/workflows/ci.yml exists in `CLAUDE.md` for any of the three relocated commands.
- [ ] .claude/rules/csharp.md is unchanged.
- [ ] No file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json is changed.
- [ ] The command text in all three cited bullets is unchanged (only the file citation is edited).

## Risks & Mitigations
- Technical or operational risks: low — a text-only Markdown edit with no execution path; the only risk is citing the wrong reusable workflow file name, mitigated by the direct verification performed in Root Cause Analysis above.
- Mitigations and rollbacks: a single-file `git revert`/diff rollback of `CLAUDE.md` is sufficient if a citation is later found incorrect.

## Rollout & Follow-up
- Release/rollout steps: merge via the standard small-scope bug PR; no deployment or feature-flag steps.
- Post-fix monitoring or clean-up tasks: none identified.
- Links: issue #564 (https://github.com/drmoisan/TaskMaster/issues/564); PR #556 / issue #553 (the CI parallel job split that moved the cited commands).
