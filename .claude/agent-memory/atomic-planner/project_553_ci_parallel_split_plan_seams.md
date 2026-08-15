---
name: project-553-ci-parallel-split-plan-seams
description: "#553 CI job-split plan seams: workflow-YAML-only scope (no C# toolchain), byte-identity via line-ending-normalized containment vs Phase 0 snapshot, ruleset PUT orchestrator-gated, QA loop before post-merge phases"
metadata:
  type: project
---

Plan seams for issue #553 (split `.github/workflows/ci.yml` monolith into 5 reusable callee workflows), plan at `docs/features/active/2026-08-14-ci-parallel-job-split-553/plan.2026-08-14T09-05.md`.

**Why:** Workflow-only features break several default planning habits: there is no local test harness, the authoritative gate is a live CI run, and the merge-policy PUT is outward-facing.

**How to apply:**
- **No C# toolchain for workflow-only diffs.** Put a binding "No-C#-Toolchain Statement" in the plan preamble plus a final `git diff --name-only <merge-base>..HEAD -- '*.cs' '*.csproj' ...` empty-check task; otherwise the executor attempts an unjustifiable csharpier/msbuild/vstest pass. Seeded probe commits that touch .cs are fine if each is reverted — the merge-base content diff nets to zero.
- **Byte-identity gates need a Phase 0 snapshot.** The source file is destroyed by the rewrite, so extract reference blocks (by verified line ranges with first-line sanity asserts) into `evidence/other/pre-split/` BEFORE editing, then verify containment with CRLF→LF normalization (`Get-Content -Raw` + `.Contains`). Works because callee `jobs.<id>.steps` sits at the same YAML depth as the monolith's.
- **Ruleset PUT task**: mark `ORCHESTRATOR CONFIRMATION REQUIRED — do not execute autonomously`; BLOCKED (not skipped) without recorded confirmation. Payload verification = 5 jq checks (read-only keys stripped, exactly N contexts, strict retained, diff-only-in-contexts via del()+`git diff --no-index`, every context verbatim in the captured-names artifact).
- **Phase ordering vs the final-QA-loop contract:** QA loop (actionlint = lint; formatting/type-check N/A for YAML; live green run = test) is the last code-verification phase; post-merge phases (ruleset migration, dispatch smoke) follow it with an explicit note that they modify no source files and an authorized `DEFERRED — awaiting merge` branch. Post-merge evidence lands via an orchestrator-owned follow-up commit since the PR is already merged.
- **Probe tasks** are `[expect-fail]` with dossiers in `evidence/regression-testing/`; wait for the probe run to finish before pushing the revert (`cancel-in-progress: true` would cancel it). Nullable probe must target a project without `TreatWarningsAsErrors` in its csproj so only the nullable gate reddens.
- **Check-run names captured, never assumed** (`gh api .../commits/<head>/check-runs`); names are SHA-independent so the capture artifact can be committed after the PUT without invalidating it.

**Preflight rev-1 findings (environment facts, reusable):**
- `jq` is NOT installed (git-bash or pwsh). Only `gh api --jq` works (filter compiled into gh). Plan JSON manipulation with `ConvertFrom-Json` / `ConvertTo-Json -Depth 20` — the default depth of 2 silently truncates nested objects (would corrupt a ruleset PUT payload). Same-serializer rule: both sides of a `git diff --no-index` JSON comparison must come from the identical `ConvertTo-Json -Depth 20` call form.
- `git diff --name-only $(git merge-base ...)..HEAD` is INVALID under pwsh — PowerShell does not concatenate the subexpression with the trailing `..HEAD` into one argument. Use two statements: `$base = git merge-base ...` then `"$base..HEAD"`.
- Git pathspec with no wildcard is anchored to repo root: `'packages.config'` matches 0 files; `'**/packages.config'` matches 18. Extension globs (`'*.cs'`) match at any depth and need no prefix.
- `gh pr create` is executor-BLOCKED by `enforce-pr-author-skill.ps1` unless (a) `artifacts/pr_context.summary.txt` from `collect_pr_context` (orchestrator-only MCP tool), (b) orchestrator-state passes `--require-pr-creation-ready` (orchestrator writes it), (c) `artifacts/pr_body_<N>.md` + `artifacts/pr_body_<N>.receipt.json` with fresh SHA-256. Therefore PR-creation tasks in executor plans need the ORCHESTRATOR CONFIRMATION REQUIRED marker.
- Every outward-facing task (dispatching workflows on main, preparing commits to main) needs the literal marker sentence, not gating prose; fold observable preconditions (e.g., `gh pr view --json state` == MERGED) into an existing task's command list to avoid task renumbering.
- `$env:TEMP` is shared with concurrent sibling-worktree agents — mandate the session scratchpad for tool downloads and temp files.
- Shell state does not persist between executor tool invocations: helper functions defined in plan prose must be written once to a `SCRATCH\helpers-<issue>.ps1` and dot-sourced in every invocation that calls them.
- Never hard-code the working branch (session worktree branch != feature branch); define `BRANCH` = `git rev-parse --abbrev-ref HEAD` captured in Phase 0.

**Execution rev-3 finding (tool flag, generalizable):**
- actionlint's `-color` is a BOOLEAN flag (force color on); `-no-color` suppresses color. `-color never` makes Go's flag parser treat `never` as a positional FILE argument → `could not read "never"`, exit 3. Never write `-color <value>` for actionlint; verified against 1.7.7's own `-h` output.

**Preflight rev-2 findings (introduced by rev-1, both generalizable):**
- When a convention enumerates the tasks that dot-source a helper file, the list must include the EARLIEST invocation — Phase 0 acceptance checks often call helpers before the implementation phases do. State that the helper file is created at first use.
- Identical `ConvertTo-Json -Depth 20` parameters do NOT imply identical key order. Any git-diff comparison of two serialized objects requires BOTH sides built from the same `[ordered]@{...}` literal (same keys, same order); a plain `@{}` hashtable emits keys in unspecified order and produces a spurious diff between semantically identical documents.

Related: [[plan-validator-phase-heading-constraint]], [[feedback-ac-checkoff-one-per-task]], [[evidence-path-normalization]].
