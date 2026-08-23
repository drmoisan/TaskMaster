# Baseline — PowerShell toolchain state for the executable-carrier change (D4)

Timestamp: 2026-08-10T15-40

Captured because the proposed correction to `scripts/vscode/Invoke-VSBuild.ps1` brings
`.claude/rules/powershell.md` obligations into play (PoshQC format, PSScriptAnalyzer, Pester, and the
coverage floors). A plan that assumes a clean starting point would fail its own gate.

## PSScriptAnalyzer baseline

Command: `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders = ["scripts/vscode"]`
EXIT_CODE: 1
Output Summary: `PSScriptAnalyzer reported 16 issue(s).`

The same 16 are reported for `["scripts/vscode", "tests/scripts/vscode"]`, so all 16 originate in
`scripts/vscode` and none in the test folder.

Command: `Invoke-ScriptAnalyzer -Path './scripts/vscode' -Recurse`
EXIT_CODE: 0

| Severity | Rule | File | Line |
|---|---|---|---|
| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26 |
| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36 |
| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39 |
| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59 |
| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79 |
| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106 |
| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 119 |
| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 120 |
| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 146 |
| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32 |
| **Warning** | **PSUseSingularNouns** | **Invoke-VSBuild.ps1** | **47** |
| **Warning** | **PSUseSingularNouns** | **Invoke-VSBuild.ps1** | **78** |
| **Warning** | **PSAvoidUsingWriteHost** | **Invoke-VSBuild.ps1** | **137** |
| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150 |
| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154 |
| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157 |

**Three of the sixteen are in the file the change would modify.** Lines 47 and 78 are
`Get-MSBuildBuildArguments` and `Get-RequestedMSBuildProperties`; PSUseSingularNouns objects to the
plural nouns "Arguments" and "Properties". Line 137 is a `Write-Host` call.

## Pester baseline

Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["tests/scripts/vscode"]`
EXIT_CODE: 0
Output Summary: `Ran bundled PoshQC test ... with 1 selected scan folder(s).` The existing suite is
green at the merge base, so any failure introduced during execution is attributable to the change.

## Consequence for the plan

The PoshQC analyze step is **red at the merge base** and will remain red after any correct change to
this file. The plan must therefore:

1. Capture this 16-finding baseline as a Phase 0 artifact and express the Phase 2 acceptance as
   **"no new findings relative to the recorded baseline"**, not as "exit 0". A plan whose acceptance
   is `EXIT_CODE: 0` for PoshQC analyze is unsatisfiable and would fail preflight.
2. **Not** rename `Get-MSBuildBuildArguments` or `Get-RequestedMSBuildProperties` to satisfy
   PSUseSingularNouns. Both are referenced by `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`, and
   renaming is out of scope for all four issues.
3. Avoid introducing a new plural-noun function. If a helper is added for target selection, name it
   with a singular noun so PSUseSingularNouns does not fire a seventeenth finding.
4. Not add new `Write-Host` calls.

## Additional finding — `.claude/rules/powershell.md` cites a path that does not exist

Found by `atomic-planner` during planning and verified by the orchestrator on 2026-08-10T16-09.

`.claude/rules/powershell.md:18` states that the Pester step should "use repo config at
`scripts/powershell/PoshQC/settings/pester.runsettings.psd1`".

Commands run to verify:

- `ls scripts/` -> `dev-tools/`, `temp-extract-coverage.ps1`, `vscode/`. **There is no
  `scripts/powershell/` tree.**
- `find . -name "pester.runsettings*" -not -path "./.git/*"` -> **no matches.**
- `find . -type d -name "PoshQC" -not -path "./.git/*"` -> **no matches.**

PoshQC is entirely MCP-server-side in this repository; the cited settings file is not present in the
checkout. This means the documented PowerShell test command, like the documented C# format and
type-check commands, references something that does not exist locally — the same class of defect this
feature exists to correct, in a different rule file.

**It is out of scope here.** The epic charter's "Execution Authorization Required" section suspends
the `policy-compliance-order` hard constraint for `CLAUDE.md` and `.claude/rules/csharp.md` only.
`.claude/rules/powershell.md` is not covered by that suspension, and none of issues #492, #509, #512
or #522 enumerates it.

**Consequence for this feature's own gates.** Whether `mcp__drm-copilot__run_poshqc_test` can emit a
numeric coverage figure at all is unverified. The plan therefore does not assert an unsatisfiable
numeric coverage gate; it accepts either numeric coverage or a recorded capability statement, and
reports remediation-required rather than PASS if numeric values prove unavailable. Preflight is
settling this empirically.

**Required follow-up.** Fold this into the same follow-up issue as the SD1 mirror sites, or file it
separately. Recording it only as prose in this feature folder is not sufficient, because the folder is
archived at merge.

## Relevant obligations from `.claude/rules/powershell.md`

- Formatting via `mcp__drm-copilot__run_poshqc_format`; linting via `run_poshqc_analyze`; testing via
  `run_poshqc_test` with `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`.
- Pester v5.x; tests mirror code structure. The target file already exists at
  `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`, so **no new test file is required** and the
  test-file-location rule is satisfied by construction.
- Line coverage >= 85%, branch coverage >= 75%.
- PowerShell 7+ compatibility.

## Ready-made regression-test seam

`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` dot-sources the script with `-NoExecute` and tests the
pure functions directly. Line 36 asserts `'/t:Build'` in the expected argument list and line 60
asserts `'Nullable=enable'` in the expected property list. **These two assertions currently pin the
defect as correct behavior.** Inverting them is a genuine red-before-green regression test requiring
no new seam, which satisfies the `full-bug` workflow's "failing regression test first" requirement.
