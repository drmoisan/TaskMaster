# P5-T3 — PoshQC analyze autofix (Final QA Loop, iteration 1)

Timestamp: 2026-09-02T23-03

## Trigger evaluation

P5-T2 iteration 1 reported 4 diagnostics across this plan's 13 write-set files:

| Rule | Severity | File | Line | SuggestedCorrections |
|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 137 | 1 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 145 | 0 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 146 | 0 |
| PSUseOutputTypeCorrectly | Information | scripts/vscode/Invoke-MSTest.ps1 | 100 | resolved by hand in P5-T2 |

`PSUseSingularNouns` carries a non-empty `SuggestedCorrections` collection, which is the mechanism
`Invoke-ScriptAnalyzer -Fix` consumes. The task's trigger condition was therefore treated as
possibly met, and the autofix tool was run rather than argued away.

## Command

Command: `mcp__drm-copilot__run_poshqc_analyze_autofix` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

EXIT_CODE: 1

MCP payload:

```
ok: false
tool: run_poshqc_analyze_autofix
summary: Command exited with code 1.
stderr_excerpt: run-poshqc-analyze-autofix.ps1: Cannot bind parameter because parameter
'ScanFolders' is specified more than once. To provide multiple values to parameters that can
accept multiple values, use the array syntax. For example, "-parameter value1,value2,value3".
```

This is a defect in the bundled autofix runner's parameter binding when more than one scan folder
is supplied. `mcp__drm-copilot__run_poshqc_format` and `mcp__drm-copilot__run_poshqc_analyze`
accept the identical two-element `scan_folders` value without error, so the defect is specific to
the autofix runner. It is recorded here as an observation about the tooling; it is outside this
plan's write set and was not modified.

The run was therefore repeated once per folder.

Command: `mcp__drm-copilot__run_poshqc_analyze_autofix`, `scan_folders` = `["scripts/vscode"]`.
EXIT_CODE: 1
MCP payload: `ok: false`, `summary: Command exited with code 1.`,
`stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).`
The exit code reflects the post-fix diagnostic count in that folder, not a failure to run: the
tool did rewrite files, as the hash comparison below shows.

Command: `mcp__drm-copilot__run_poshqc_analyze_autofix`, `scan_folders` = `["tests/scripts/vscode"]`.
EXIT_CODE: not emitted; `ok: true`,
`summary: Ran bundled PoshQC analyze autofix with 1 selected scan folder(s).`
No test file was rewritten.

## Rewrite detection

SHA-256 hashes of all 21 files under both scan folders were captured immediately before and
immediately after the autofix runs. Exactly two files changed:

| File | Hash before | Hash after | In write set |
|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | BB84C28E577EB3CB | 87CC819DA3261220 | yes |
| scripts/vscode/Invoke-VSBuild.ps1 | B4F0D74E691B4CCB | C32E3F340705AE1F | no |

## What the autofix actually changed, and why it was reverted

The autofix applied the `PSUseSingularNouns` suggested correction by renaming each flagged
function's **definition only**, leaving every call site bound to the old name. It also inserted a
UTF-8 BOM at the head of each file it touched.

In `scripts/vscode/Invoke-VSBuild.ps1` (out of this plan's write set) it renamed:

- `function Get-MSBuildBuildArguments` to `function Get-MSBuildBuildArgument`
- `function Get-RequestedMSBuildProperties` to `function Get-RequestedMSBuildProperty`

while lines 157 and 158 of that same file still call `Get-RequestedMSBuildProperties` and
`Get-MSBuildBuildArguments`. The script was left non-functional.

In `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` it renamed:

- `function Get-CoberturaLineConditionCoverageParts` to
  `function Get-CoberturaLineConditionCoveragePart` at line 137

while the two call sites at lines 202 and 322, and the doc-comment reference at line 171, still
name `Get-CoberturaLineConditionCoverageParts`. That file was likewise left non-functional.

Both rewrites were reverted:

- `scripts/vscode/Invoke-VSBuild.ps1` — reverted with
  `git checkout -- scripts/vscode/Invoke-VSBuild.ps1`. Required by the Conventions rule that any
  rewritten path outside this plan's write set is reverted and the reversion recorded. The file
  was clean before the autofix run, so the checkout restores it exactly.
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — the BOM was stripped and the single
  definition rename was reverted by byte-level rewrite, leaving every other line untouched. This
  file is in the write set, but the `PSUseSingularNouns` diagnostic on
  `Get-CoberturaLineConditionCoverageParts` is pre-existing: it is recorded in the P0-T6 baseline
  as "pre-existing, is not one of the seven findings, and is out of this plan's scope to change",
  and it is separately ratified as an accepted finding in
  `docs/features/epics/build-ci-coverage-gate-fidelity/feature-audit.2026-08-15T05-11.md` line 66.
  Applying a rename that breaks the file is not an acceptable resolution of it.

Post-revert verification: all 21 file hashes under both scan folders are byte-identical to their
pre-autofix values, including `Invoke-MSTestWithCoverage.Helpers.ps1` back at BB84C28E577EB3CB and
`Invoke-VSBuild.ps1` back at B4F0D74E691B4CCB.

## Output Summary

- Autofix was run (not skipped). It produced no net change to the tree: everything it rewrote was
  breaking and was reverted, so the file set is byte-identical to its pre-autofix state.
- The `PSUseSingularNouns` correction the tool offers is definition-only and leaves call sites
  dangling, so this rule is not usable as an autofix on either affected file.
- No autofixable-and-safe diagnostic remains in this plan's write set.
- The Final QA Loop does restart at iteration 2, but because P5-T2 changed
  `scripts/vscode/Invoke-MSTest.ps1` to resolve the newly introduced `PSUseOutputTypeCorrectly`
  diagnostic, not because of this task.
