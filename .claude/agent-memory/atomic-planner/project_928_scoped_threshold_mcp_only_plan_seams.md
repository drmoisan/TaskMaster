---
name: project-928-scoped-threshold-mcp-only-plan-seams
description: "#928 (scoped coverage-runner threshold skip + formatting) plan seams: an isolated-worktree executor has MCP PoshQC only, so scripts/vscode line coverage is unmeasurable and needs a three-route task (bundled doc / direct pwsh / orchestrator work order); PoshQC format rewrote 0 of 46 on 2026-09-20 so the issue's 'would rewrite' claim is Invoke-Formatter-default drift; verbose-mode Rust regexes keep slash-bearing gates out of the blast-radius harvest"
metadata:
  type: project
---

Seams re-derived while authoring the #928 minimal-audit plan (2026-09-28, revision round 0) in an
isolated agent worktree whose Bash tool refuses pwsh.

**Why:** each one either made a caller-supplied constraint unsatisfiable as written or would have
put a harvestable non-Write-Set token into the plan.

**How to apply:** re-check before planning any PowerShell change that must be executed with the
MCP PoshQC tools alone, and before writing a slash-bearing regex or JSON argument into a plan.

1. **Numeric scripts/vscode coverage is unobtainable through `mcp__drm-copilot__run_poshqc_test`.**
   Its bundled JaCoCo document (artifacts/pester/powershell-coverage.xml) names only .claude and
   .codex packages (nine on 2026-09-09, LINE covered 0 / missed 6583). When Bash refuses pwsh the
   direct `Invoke-Pester` pairing is gone too. Plan a single coverage task with three exhaustively
   specified routes decided by observation (A: bundled doc names the changed file; C: the direct
   command launches; B: neither, so write a POSTING BLOCKED work order carrying the verbatim direct
   command and leave the coverage AC unchecked), and make the AC check-off task carry exactly two
   recorded outcomes. The direct command's JaCoCo output must go under the gitignored coverage/
   directory, never the feature folder (CLAUDE.md Committed Test Evidence Format forbids raw Pester
   JaCoCo and trx under evidence/).

2. **The issue's "PoshQC format would rewrite both scripts" premise was measured with bare
   `Invoke-Formatter` defaults.** The MCP formatter rewrote 0 of 46 files over scripts/vscode and
   tests/scripts/vscode on 2026-09-20 (#911 remediation baseline), including both runner scripts.
   Define the formatting AC against the MCP route, keep the possibly-untouched script in the Write
   Set as a superset, derive the drift set at run time, and prove formatter liveness in Phase 0 with a
   reverted perturbation (Edit tool prepends four spaces to a top-level statement; anchored numstat
   reads 1/1 before and empty after; `git checkout --` restores). `git hash-object --no-filters`
   over the Write Set files before/after a format run is the byte-identity observation for the
   idempotence clause and needs no shell.

3. **Test-tool exit semantics per artifact:** derive a test artifact's `EXIT_CODE:` from the bundled
   JUnit root (`failures` + `errors` > 0 -> 1) and record the MCP ok flag on its own line; an
   `[expect-fail]` MCP run has been observed returning `ok:false` (#565), so `ExpectedExitCode: 1`
   holds on that channel, unlike a direct `Invoke-Pester` run.

4. **Slash-bearing gate tokens are harvested as blast-radius paths** when backticked and
   whitespace-free: `["scripts/vscode"]`, `**/*.xml`, a branch name, a rule-file path, and a regex
   like `[\\/]Users[\\/]`. Write JSON arguments as `scan_folders = [...]` (multi-word span), glob
   patterns as `Glob pattern **/*.xml`, and regexes in Rust verbose mode with spaces:
   `(?x) (^|[^A-Za-z]) [A-Za-z] : [\\/]` (drive-letter path; the leading class stops `https://`
   from matching) and `(?xi) [\\/] Users [\\/]`. Neither self-matches the plan text. Scope any
   `workspace_root`-must-carry-`<repo-root>` check to the evidence/ subtree, because the plan's
   own command reference names the field without a value.

5. **Pre-fix RED must be for the right reason.** Dot-sourcing a not-yet-existing part file inside
   the entry point makes every entry-point test fail pre-fix with a file-not-found, including the
   intended controls. Order the tasks so the part file and the dot-source land in the same fix
   task after the expect-fail run, and guard the test file's own dot-source with `Test-Path` so the
   predicate tests fail as discrete command-not-found cases instead of one block-setup error.

6. **Three-production-file cap shaped the design:** the predicate went into a new part file
   dot-sourced from the entry point rather than from the helpers chain (Helpers.ps1 at 472 lines
   would have been a fourth production file). The shared entry-point test file
   Invoke-MSTest.RunSettings.Tests.ps1 sits at 499 lines, so a single new test file holds every new
   case; mocking the executable seam rather than the collection wrapper lets one BeforeEach serve
   the scoped, unscoped and non-zero-exit cases with the real wrapper running (Set-Content count 4).

7. **AC6's "scripts/vscode remains at or above 80 percent" may rest on a false premise:** the two
   most recent committed folder figures are 79.47 (2026-09-09, #815) and 76.96 (2026-09-13, #873,
   before its Phase 7 tests). Flag it to the orchestrator rather than amending the AC as planner.

Related: [[reference_poshqc_mcp_measurement_limits]], [[poshqc-mcp-and-msbuild-invocation-facts]],
[[powershell-gate-observables]], [[project_873_evidence_projection_plan_seams]],
[[project_839_createcancellationtoken_init_plan_seams]].
