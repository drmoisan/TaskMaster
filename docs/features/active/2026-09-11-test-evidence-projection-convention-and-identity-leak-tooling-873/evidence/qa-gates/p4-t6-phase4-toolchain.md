# P4-T6 — Phase 4 PowerShell Toolchain

Timestamp: 2026-09-13T06-18
Task: [P4-T6]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders` to every
MCP invocation.

Toolchain order: format, then analyze, then test. The loop ran twice. On the first pass the analyze
step introduced one diagnostic absent from the P0-T11 baseline set, so the loop was restarted from the
format step after the cause was removed. The second pass completed with no step failing.

Absolute paths are elided to `<worktree>` in every quoted output below, because this delivery's own
rule prohibits carrying an absolute host path into an artifact.

---

## Pass 1 — the restart and its cause

PRE_FORMAT_TREE_STATE (pass 1):

```
 M docs/features/active/<feature-folder>/plan.2026-09-12T10-26.md
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t1-batch-open.md
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

Every path under either script folder was edited or created by this executor in this phase:
`scripts/vscode/Invoke-MSTest.ps1` by P4-T1 and P4-T2,
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` by P4-T3,
`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` by P4-T4 and
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` by P4-T5. No foreign path was present
before the format invocation, so the precondition on the formatter restore rule held and `git checkout
--` was run on no path in either pass.

Format (pass 1): MCP_RESULT_OK_FLAG true, EXIT_CODE 0.

FIRST_POST_FORMAT_PORCELAIN_STATUS (pass 1, limited to the two script folders):

```
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

Restore clause: zero applicable paths.

Analyze (pass 1): MCP_RESULT_OK_FLAG false, EXIT_CODE 1, stderr excerpt
`Exception: PSScriptAnalyzer reported 17 issue(s).` The paired direct run reported
`POWERSHELL_ANALYZER_TOTAL: 17` and identified the delta as

```
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 250
```

a third `PSAvoidUsingWriteHost` occurrence on `scripts/vscode/Invoke-MSTest.ps1` where the P0-T11
baseline records exactly two. It was introduced by the `Write-Host` call P4-T2 added to report the
summary path. Because the baseline comparison is a per-tuple count comparison, a third occurrence of
an existing tuple is an entry absent from the baseline set and fails this gate. The call was changed
to `Write-Output`, which is what the coverage entry point already uses for the same purpose, and the
loop was restarted from the format step. The two pre-existing `Write-Host` calls were left unchanged:
they predate the rule and are outside this delivery's remit.

---

## Pass 2 — Step 1, Format

PRE_FORMAT_TREE_STATE (pass 2):

```
 M docs/features/active/<feature-folder>/plan.2026-09-12T10-26.md
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t1-batch-open.md
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

The same four script-folder paths, all owned by this phase.

Tool: mcp__drm-copilot__run_poshqc_format
Command: mcp__drm-copilot__run_poshqc_format with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC format against '<worktree>' with 2 selected scan folder(s).`

SECOND_POST_FORMAT_PORCELAIN_STATUS (limited to the two script folders):

```
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

It lists no path this plan does not own, which is this step's acceptance. Restore clause: zero
applicable paths, so `git checkout --` was run on nothing and no path outside this worktree was
touched at any point in this step.

Observation beyond the exit code, recorded because the formatter exits zero both when it rewrites
nothing and when it repairs drift:

```
scripts/vscode/Invoke-MSTest.ps1 lines=262 crlf=262 lfonly=0 bom=False
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 lines=498 crlf=498 lfonly=0 bom=False
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 lines=146 crlf=146 lfonly=0 bom=False
tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1 lines=119 crlf=119 lfonly=0 bom=True
```

Carriage-return line-feed endings survived on all four files and the UTF-8 byte-order mark survived on
the one file this phase created, which is the file the plan's new-file convention applies to. The
three pre-existing files carry no byte-order mark and carried none at the base anchor either: the
anchored diff of `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` reports a single hunk at the
exact-array assertion and no change to its first line, so the formatter stripped nothing this pass.
No restore of stored encoding was needed, unlike Phases 1 and 3.

---

## Pass 2 — Step 2, Analyze

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
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 210
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 211
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
  set, and the count per tuple matches.
- Two textual differences from the baseline, neither a new entry. The `PSUseSingularNouns` diagnostic
  on `Invoke-MSTestWithCoverage.Helpers.ps1` remains at line 139 rather than the baseline's 138, the
  pre-existing diagnostic displaced by the dot-source line P1-T2 inserted above it. The two
  `PSAvoidUsingWriteHost` diagnostics on `Invoke-MSTest.ps1` moved from lines 185 and 186 to 210 and
  211, the same pre-existing pair displaced by the parameter, dot-source and path-resolution lines
  P4-T2 inserted above them. The comparison tuple is insensitive to the line number, and the count
  for that tuple is 2 in both sets.
- No diagnostic carries the file leaf name `Invoke-MSTest.RunSettings.Tests.ps1`,
  `Invoke-MSTest.Main.Tests.ps1` or `Invoke-MSTest.ResultsDirectory.Tests.ps1`.

---

## Pass 2 — Step 3, Test

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

Paired direct Pester run over the whole test folder, so the repaired call sites and every untouched
describe block are both exercised:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Starting discovery in 16 files.
Discovery found 131 tests in 468ms.
[+] Install-RepoDotNetSdk.Tests.ps1
[+] Invoke-MSTest.AssemblyDiscovery.Tests.ps1
WARNING: Test-result summary was not written: Cannot find path '<repo>\coverage\test-results\mstest-run.trx' because it does not exist.
[+] Invoke-MSTest.Main.Tests.ps1
[+] Invoke-MSTest.ResultsDirectory.Tests.ps1
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
Tests completed in 45.34s
Tests Passed: 131, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_FOLDER_COUNTS passed=131 failed=0 skipped=0
```

PASSED: 131
FAILED: 0
SKIPPED: 0

131 is the 128 recorded at the end of Phase 3 plus this phase's 3 new tests, with zero failed and zero
skipped, so no pre-existing test regressed. The four warnings are all the designed non-fatal branch.
The first is the plain entry point in `Invoke-MSTest.Main.Tests.ps1`, which mocks the path-existence
test to true and mocks no content reader, so its test-result read fails against a path that does not
exist — exactly the case P4-T2 and P4-T4 describe, and the reason that file needed no reader mock. The
other three are the coverage entry point in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, whose unfiltered reader
mock answers with a document that parses but carries no result-summary node, taken once per
entry-point call. Where a warning was taken, neither the summary write nor the discard ran.

The pre-existing `Invoke-VSBuild.Tests.ps1` resolves an MSBuild path and runs the repository hint-path
synchroniser, which reported `All HintPaths are up to date`. It compiles nothing, so no build lock was
required and no project file was modified: the post-test porcelain status below lists no project file.

## Files created, written or deleted by the tests

None. Every filesystem command the plain entry point reaches — `Get-Content`, `Set-Content`,
`Remove-Item` and `Test-Path` — is mocked in every test that invokes it, and every fixture in the file
this phase created is either an in-memory literal or the parsed abstract syntax tree of a production
file that is read but never written. The non-fatal test in
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` asserts `Set-Content` and
`Remove-Item` are each invoked exactly zero times. The post-test porcelain status across the whole
worktree is unchanged from the pass-2 pre-format state:

```
 M docs/features/active/<feature-folder>/plan.2026-09-12T10-26.md
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t1-batch-open.md
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

No test-result document, no summary, no coverage document and no projection appears anywhere in it,
which is the observable consequence of every write being mocked.

## Output Summary

Format: MCP ok true, exit 0, invoked once per pass; two porcelain listings per pass, the second
listing no path this plan does not own; the restore clause had zero applicable paths in both passes
and `git checkout --` was run on nothing. Carriage-return line-feed endings and the new file's UTF-8
byte-order mark both survived, and no restore of stored encoding was needed.
Analyze: pass 1 reported 17 issues, one entry absent from the P0-T11 baseline, introduced by this
phase's own `Write-Host` call; the call was changed to `Write-Output` and the loop restarted from
format. Pass 2: MCP ok false, exit 1, 16 issues, which is the legitimate baseline shape; the direct
diagnostic set introduces no entry absent from the P0-T11 set when compared by rule name, file leaf
name and severity, and carries no entry for any test file this phase touched.
Test: MCP ok true, exit 0; direct whole-folder run 131 passed, 0 failed, 0 skipped, against the
Phase 3 floor of 128 plus this phase's 3 new tests.
FAILED: 0 in the final pass.
