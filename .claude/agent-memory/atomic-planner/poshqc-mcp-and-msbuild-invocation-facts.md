---
name: poshqc-mcp-and-msbuild-invocation-facts
description: "PoshQC MCP tools return no counts/coverage/severities (pair unconditionally with direct runs, never a 'fallback'); Invoke-VSBuild.ps1 DOES support -Target Rebuild; hidden-parent test dirs may never be collected"
metadata:
  type: reference
---

Command-shape facts that make PowerShell/C# plan tasks executable in this repo.

- **`mcp__drm-copilot__run_poshqc_test` accepts only `workspace_root` and `scan_folders`** and its payload is
  exactly `{ok, tool, workspace_root, summary}` — no exit code, no pass/fail counts, no per-test names, no
  executed-file inventory, no coverage figure. It returns `ok: true` even with no `config/poshqc-scan.json`
  present. `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` is a *bundled extension resource*
  cited by `.claude/rules/powershell.md:17` (not 18); `scripts/powershell/` does not exist in this repository.
- **Never write an MCP "fallback" trigger — write an unconditional pairing.** A fallback gated on
  "unavailable or non-zero exit without diagnostics" is unreachable: the tool returns `ok: true` with no data,
  so the trigger never fires while every required number is missing (#494 preflight blocking finding B3). The
  correct plan shape: run the MCP tool unconditionally for the `.claude/rules/powershell.md` step 4 policy
  record (EXIT_CODE 0/1 from `ok`), and unconditionally pair it with a direct `Invoke-Pester` run
  (`New-PesterConfiguration`, `Run.PassThru`, `CodeCoverage.OutputFormat = "JaCoCo"`) that supplies the numbers
  and a `FILE=`/`TESTS=` executed-file inventory via `$r.Containers`. Prefix with
  `New-Item -ItemType Directory -Force -Path artifacts/pester | Out-Null` — Pester's JaCoCo writer does not
  create parent directories.
- **Issue #536: the MCP-written `artifacts/pester/powershell-coverage.xml` reports zero covered lines
  repository-wide.** Any figure read from it is false, and a "no regression" claim computed 0% → 0% from it is
  vacuous. The direct run overwrites that same path with truthful figures; coverage-delta tasks must name the
  direct-route artifacts as the comparison source.
- **`mcp__drm-copilot__run_poshqc_analyze` reports only a total issue count** — no rule names, files, or
  severities — and exits 1 on any Warning. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` carries a
  pre-existing unsuppressed `PSUseSingularNouns` warning, so baseline `EXIT_CODE: 1` is legitimate. Severity /
  rule-by-rule gates need a paired direct `Invoke-ScriptAnalyzer -Path . -Recurse` run (exclude
  `\.claude\worktrees\` paths) and the gate must be the diagnostic-set diff, not the exit code.
- **`PSUseSingularNouns` is active** — a new plural-named function (e.g. `Get-CoberturaCoverageRates`) emits a
  new diagnostic and fails a zero-new-diagnostics final gate. Plan the suppression up front using the pattern
  at `.claude/hooks/enforce-pr-author-skill.ps1:78`, and have the authoring task record it so final lint can
  classify it as authored. New `.ps1` files also need a UTF-8 BOM (`PSUseBOMForUnicodeEncodedFile`).
- **Test-file discovery is not guaranteed.** `config/poshqc-scan.json` does not exist here and pre-existing
  Pester files live under `tests/scripts/vscode/` — there are **five** (`Install-RepoDotNetSdk`,
  `Invoke-MSTest.RunSettings`, `Invoke-MSTestWithCoverage.ClosureFilter`, `Invoke-MSTestWithCoverage.Helpers`,
  `Invoke-VSBuild`), not four; spec prose that says "four" undercounts. A new test file under a hidden-parent
  directory (e.g. `tests/.claude/hooks/`) may never be collected, so suite tasks must require the artifact to
  enumerate executed files by name with non-zero test counts.
- **`msbuild` is not on `PATH`, but `scripts/vscode/Invoke-VSBuild.ps1` DOES support rebuild** — script-level
  `[string]$Target = 'Build'` with `ValidateSet('Build','Rebuild')` at line 13, interpolated `"/t:$Target"` at
  line 73, passed through at 158, vswhere resolution at 137-142. (An earlier version of this memory claimed it
  hard-coded `/t:Build`; that claim was refuted by the #494 preflight — verify wrapper capabilities before
  asserting absence.) Prefer the wrapper (`-Target Rebuild`) over hand-rolled vswhere resolution per
  `policy-compliance-order`'s "prefer repo-defined tasks/commands". Rebuild tasks should require a non-zero
  `CoreCompile` project count so an up-to-date no-op cannot pass.
- **`mcp__drm-copilot__potential_to_issue` requires `potential_path`** pointing at an existing
  `docs/features/potential/*.md`. A plan that files follow-up issues must include a task that *authors* those
  potential entries first, or the promotion call has no input. See [[feature-promotion-lifecycle]] usage in
  [[project_preexisting_issue_breaks_promotion_receipt]].
