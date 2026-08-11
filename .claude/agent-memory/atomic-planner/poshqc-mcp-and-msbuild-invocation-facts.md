---
name: poshqc-mcp-and-msbuild-invocation-facts
description: "PoshQC MCP test tool takes only workspace_root/scan_folders (no runsettings path), Pester discovery misses hidden-parent test dirs, and msbuild must be vswhere-resolved because Invoke-VSBuild.ps1 hard-codes /t:Build"
metadata:
  type: reference
---

Command-shape facts that make PowerShell/C# plan tasks executable in this repo.

- **`mcp__drm-copilot__run_poshqc_test` accepts only `workspace_root` and `scan_folders`.** It supplies its
  own bundled Pester settings. `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` is a *bundled
  extension resource* cited by `.claude/rules/powershell.md:18`; `scripts/powershell/` does not exist in this
  repository, so naming that path as a config argument in a plan task is a defect.
- **"Available" is not "working."** `run_poshqc_test` can terminate with exit `-1` and no per-test detail while
  remaining callable. A fallback authorization worded "if the MCP tool is *unavailable*" therefore leaves a
  final-QC task demanding `EXIT_CODE: 0` with no non-SKIPPED completion path. Word every MCP fallback trigger as
  "unavailable **or** returns a non-zero/negative exit code without per-test diagnostic detail", require both the
  MCP attempt and the fallback to be recorded with their exit codes, and bind the `EXIT_CODE: 0` requirement to
  *the route that produced the reported figures*.
- **Test-file discovery is not guaranteed.** `config/poshqc-scan.json` does not exist here and every existing
  Pester file lives under `tests/scripts/vscode/`. A new test file under a hidden-parent directory
  (e.g. `tests/.claude/hooks/`) may never be collected, so a green suite proves nothing. Any suite-run task
  must require the artifact to *enumerate executed test files* with a non-zero test count each, and to record
  which discovery route was used (default scan set vs explicit `scan_folders: tests`).
- **`msbuild` is not on `PATH`.** `scripts/vscode/Invoke-VSBuild.ps1` hard-codes `/t:Build` at line 64 and
  exposes no target parameter, so it cannot deliver `/t:Rebuild`. Resolve MSBuild the way that wrapper does at
  lines 127-134: `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires
  Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1`. A rebuild task
  should also require a non-zero `CoreCompile` project count so an up-to-date no-op cannot pass as a build.
- **`mcp__drm-copilot__potential_to_issue` requires `potential_path`** pointing at an existing
  `docs/features/potential/*.md`. A plan that files follow-up issues must include a task that *authors* those
  potential entries first, or the promotion call has no input. See [[feature-promotion-lifecycle]] usage in
  [[project_preexisting_issue_breaks_promotion_receipt]].
