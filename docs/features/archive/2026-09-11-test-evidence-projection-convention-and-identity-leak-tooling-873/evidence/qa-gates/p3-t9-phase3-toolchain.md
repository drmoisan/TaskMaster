# P3-T9 — Phase 3 PowerShell Toolchain

Timestamp: 2026-09-13T06-03
Task: [P3-T9]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders` to every
MCP invocation.

Toolchain order: format, then analyze, then test. Neither the analyze step nor the test step failed,
so the loop was not restarted from the format step. The format step rewrote two files this delivery
owns; per the plan's formatter restore rule that alone does not restart the loop, because the analyze
and the test steps that follow ran against the formatted tree.

---

## Step 1 — Format

PRE_FORMAT_TREE_STATE:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p3-t1-batch-open.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t2-collection-forwarding.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t3-entry-point-wiring.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t4-runsettings-call-site-repair.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t5-assembly-discovery-repair.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t6-results-directory-tests.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t7-retention-and-ordering-tests.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t8-projection-wiring-tests.md
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

The only paths under either script folder are `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
(modified by P3-T1 through P3-T3), `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
(repaired by P3-T4), `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`
(repaired by P3-T5) and `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`
(created by P3-T6 and extended by P3-T7 and P3-T8). All four were edited or created by this executor
in this phase, so no foreign path was present before the format invocation and the precondition on
the formatter restore rule held. The feature-folder prefix is elided to `<feature-folder>` in this
listing only, to keep the artifact readable; the full paths are the canonical evidence paths this plan
names.

Tool: mcp__drm-copilot__run_poshqc_format
Command: mcp__drm-copilot__run_poshqc_format with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC format against '<worktree>' with 2 selected scan folder(s).`

FIRST_POST_FORMAT_PORCELAIN_STATUS (limited to the two script folders):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

Restore clause: zero applicable paths. Every reported path is named as a backticked
repository-relative path in this plan, so `git checkout --` was run on no path. No path outside this
worktree was touched at any point in this step.

Observation beyond the exit code, recorded because the formatter exits zero both when it rewrites
nothing and when it repairs drift:

```
scripts/vscode/Invoke-MSTestWithCoverage.ps1 crlf=0 lfonly=438 bom=False lines=438
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 crlf=491 lfonly=0 bom=False lines=491
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 crlf=106 lfonly=0 bom=False lines=106
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 crlf=0 lfonly=268 bom=False lines=268
```

The formatter rewrote two of the four files: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lost its
carriage-return line-feed endings, and the new test file lost both its endings and its UTF-8
byte-order mark. This is the same behaviour Phase 1 recorded. Both were restored in place — the entry
point to carriage-return line-feed endings with no mark, which is how it is stored in this repository,
and the new test file to carriage-return line-feed endings with a mark, which the plan's new-file
convention requires — and the format step was then re-run:

```
scripts/vscode/Invoke-MSTestWithCoverage.ps1 crlf=438 lfonly=0 bom=False
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 crlf=268 lfonly=0 bom=True
```

Second format invocation: MCP_RESULT_OK_FLAG true, EXIT_CODE 0, same summary text.

SECOND_POST_FORMAT_PORCELAIN_STATUS (re-read after the restore clause completed with no `git
checkout --` action, and after the second format invocation):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

It lists no path this plan does not own, which is this step's acceptance.

Encoding state after the second format invocation, which is the state every later measurement reads:

```
scripts/vscode/Invoke-MSTestWithCoverage.ps1 crlf=438 lfonly=0 bom=False lines=438
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 crlf=491 lfonly=0 bom=False lines=491
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 crlf=106 lfonly=0 bom=False lines=106
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 crlf=268 lfonly=0 bom=True lines=268
```

Both properties survived the second pass, and no line count changed, so the second invocation
rewrote nothing.

---

## Step 2 — Analyze

Tool: mcp__drm-copilot__run_poshqc_analyze
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt: `Exception: PSScriptAnalyzer reported 16 issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

The exit code of 1 is not this gate; the diagnostic-set comparison below is. It is the legitimate
baseline shape, because `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` already carries an
unsuppressed plural-noun warning.

Paired direct run, unconditional:

Command: pwsh -NoProfile -Command '<Invoke-ScriptAnalyzer -Recurse over scripts/vscode and over tests/scripts/vscode, each diagnostic printed as severity, rule name, file leaf name and line number, ending with a labelled total>'
EXIT_CODE: 0

```
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 185
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 186
DIAG| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 139
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 147
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 52
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 87
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157
POWERSHELL_ANALYZER_TOTAL: 16
```

Comparison against the P0-T11 baseline set, by the tuple of rule name, file leaf name and severity:

- Total unchanged at 16. Composition unchanged at 13 Warning and 3 Information across 6 files.
- INTRODUCES_ENTRY_ABSENT_FROM_BASELINE: false. Every tuple present here is present in the baseline
  set, and the counts per tuple match.
- The only textual difference from the baseline remains the `PSUseSingularNouns` diagnostic on
  `Invoke-MSTestWithCoverage.Helpers.ps1` at line 139 rather than 138. That is the same pre-existing
  diagnostic displaced by the one dot-source line P1-T2 inserted above it. The comparison tuple is
  insensitive to the line number, so this is not a new entry.
- No diagnostic carries the file leaf name `Invoke-MSTestWithCoverage.ps1`,
  `Invoke-MSTest.RunSettings.Tests.ps1`, `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` or
  `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, which are the four files this phase touched.

---

## Step 3 — Test

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

Paired direct Pester run over the whole test folder, so the repaired call sites and the untouched
plain-path describes are both exercised:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Starting discovery in 15 files.
Discovery found 128 tests in 528ms.
[+] Install-RepoDotNetSdk.Tests.ps1
[+] Invoke-MSTest.AssemblyDiscovery.Tests.ps1
[+] Invoke-MSTest.Main.Tests.ps1
[+] Invoke-MSTest.RunSettings.Tests.ps1
[+] Invoke-MSTest.TrxSummary.Tests.ps1
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
[+] Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
[+] Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
[+] Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
[+] Invoke-MSTestWithCoverage.Helpers.Tests.ps1
[+] Invoke-MSTestWithCoverage.Merge.Tests.ps1
[+] Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
[+] Invoke-MSTestWithCoverage.Projection.Tests.ps1
[+] Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
[+] Invoke-MSTestWithCoverage.Threshold.Tests.ps1
[+] Invoke-VSBuild.Tests.ps1
Tests completed in 46.89s
Tests Passed: 128, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_FOLDER_COUNTS passed=128 failed=0 skipped=0
```

PASSED: 128
FAILED: 0
SKIPPED: 0

128 is the 119 recorded at the end of Phase 2 plus this phase's 9 new tests, with zero failed and
zero skipped, so no pre-existing test regressed. The three warnings are the broadened non-fatal
branch P3-T3 specifies, taken once per entry-point call in
`Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, which is the designed behaviour for a
document that parses but carries no result-summary node.

The pre-existing `Invoke-VSBuild.Tests.ps1` resolves an MSBuild path and runs the repository hint-path
synchroniser, which reported `All HintPaths are up to date`. It compiles nothing, so no build lock was
required and no project file was modified: the post-test porcelain status below lists no project file.

## Files created, written or deleted by the tests

None. No test in this phase created, wrote or deleted a file. Every fixture in the files this phase
added or repaired is a here-string assigned to a script-scoped variable, a search of
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` and of
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` for path-based fixture
loads and filesystem writes returned zero matches in each, and every filesystem command the coverage
entry point reaches — `Get-Content`, `Set-Content`, `New-Item`, `Remove-Item` and `Test-Path` — is
mocked in every test that invokes it. The post-test porcelain status across the whole worktree is
unchanged from the pre-format state except for the evidence artifacts this phase authored:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p3-t1-batch-open.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t2-collection-forwarding.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t3-entry-point-wiring.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t4-runsettings-call-site-repair.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t5-assembly-discovery-repair.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t6-results-directory-tests.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t7-retention-and-ordering-tests.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p3-t8-projection-wiring-tests.md
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

No coverage document, no test-result document, no summary and no projection appears anywhere in it,
which is the observable consequence of every write being mocked.

## Output Summary

Format: MCP ok true, exit 0, invoked twice; two porcelain listings recorded, the second listing no
path this plan does not own; the restore clause had zero applicable paths and `git checkout --` was
run on nothing. The first invocation rewrote the entry point's line endings and stripped the new test
file's endings and byte-order mark; both were restored and survived the second invocation with no
line-count change.
Analyze: MCP ok false, exit 1, 16 issues, which is the legitimate baseline shape; the direct
diagnostic set introduces no entry absent from the P0-T11 set when compared by rule name, file leaf
name and severity, and carries no entry for any of the four files this phase touched.
Test: MCP ok true, exit 0; direct whole-folder run 128 passed, 0 failed, 0 skipped, against the
Phase 2 floor of 119 plus this phase's 9 new tests.
FAILED: 0. The toolchain was not restarted.
