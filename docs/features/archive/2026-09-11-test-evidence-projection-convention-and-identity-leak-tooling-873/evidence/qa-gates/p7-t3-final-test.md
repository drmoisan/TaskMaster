# P7-T3 — Final PowerShell Test Step

Timestamp: 2026-09-13T06-27
Task: [P7-T3]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders`. The
direct run covers exactly the test folder `tests/scripts/vscode`, which is the same population the
P0-T12 baseline measured, so the passed-count comparison below reads a like-for-like set.

## PRE_PESTER_PORCELAIN

Captured immediately before the Pester step, because
`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` executes the repository hint-path synchroniser during
a Pester run and a whole-folder run can therefore rewrite project files.

Command: git status --porcelain -uall

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-batch-open.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-final-format.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t2-final-analyze.md
```

No `.csproj` path is reported. Every reported path is a Phase 7 evidence artifact or this plan file.

## Step 1 — MCP test invocation

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders
set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

The MCP tool returns no counts, so it is paired unconditionally with the direct run below.

## Step 2 — Paired direct Pester run over the whole test folder

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; Import-Module Pester -MinimumVersion 5.0; $cfg = New-PesterConfiguration; $cfg.Run.Path = "tests/scripts/vscode"; $cfg.Run.PassThru = $true; $cfg.Output.Verbosity = "None"; $r = Invoke-Pester -Configuration $cfg; "PESTER_VERSION: " + (Get-Module Pester).Version; "PESTER_COUNTS: passed=" + $r.PassedCount + " failed=" + $r.FailedCount + " skipped=" + $r.SkippedCount; foreach ($c in $r.Containers) { $n = $c.Item; if ($n -is [System.IO.FileInfo]) { "CONTAINER| " + $n.Name } else { "CONTAINER| " + [string]$n } }; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'

EXIT_CODE: 0

The explicit exit statement is present and mandatory: Pester ignores `Run.Exit` by default, so a
direct run sets no process exit code of its own.

PESTER_VERSION: 5.6.1

### Counts line, verbatim

```
PESTER_COUNTS: passed=131 failed=0 skipped=0
```

PESTER_PASSED_COUNT: 131
PESTER_FAILED_COUNT: 0
PESTER_SKIPPED_COUNT: 0

### Executed container list, verbatim

```
CONTAINER| Install-RepoDotNetSdk.Tests.ps1
CONTAINER| Invoke-MSTest.AssemblyDiscovery.Tests.ps1
CONTAINER| Invoke-MSTest.Main.Tests.ps1
CONTAINER| Invoke-MSTest.ResultsDirectory.Tests.ps1
CONTAINER| Invoke-MSTest.RunSettings.Tests.ps1
CONTAINER| Invoke-MSTest.TrxSummary.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.Helpers.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.Merge.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.Projection.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
CONTAINER| Invoke-MSTestWithCoverage.Threshold.Tests.ps1
CONTAINER| Invoke-VSBuild.Tests.ps1
```

CONTAINER_COUNT: 16

### The seven test files this delivery creates or repairs, each located in that list

Four created:

| File | Present in container list |
|---|---|
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` | yes |
| `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` | yes |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` | yes |
| `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` | yes |

Three repaired:

| File | Present in container list |
|---|---|
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | yes |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | yes |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | yes |

All seven are named in the enumerated container list.

### Passed-count comparison against the P0-T12 baseline

| Figure | P0-T12 baseline | P7-T3 final |
|---|---|---|
| Passed | 103 | 131 |
| Failed | 0 | 0 |
| Skipped | 0 | 0 |

131 is greater than 103, so the passed count rose by 28 over the same test folder.

### Incidental console output, recorded so it is not mistaken for a defect

The run printed five `Using vstest.console: C:\repo\vstest.console.exe` / `Discovered 1 test
assemblies.` pairs, four `WARNING: Test-result summary was not written:` lines, one `Using MSBuild:`
line naming the Visual Studio installation, one `Sync-PackageReferences: All HintPaths are up to
date` line, and one deprecation warning about an `-EnableNullable` switch.

The `C:\repo\...` value is a test fixture value, not a real path. The four summary warnings are the
non-fatal branch this delivery specifies, exercised deliberately by tests that mock a path-existence
test to true without mocking a content reader, or that answer the read with a document carrying no
result-summary node; one names the missing-path case and three name the no-result-summary case. Each
confirms that the non-fatal branch suppresses the summary write rather than throwing, which is the
behaviour P3-T3 and P4-T2 specify and which
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` asserts directly.

## POST_PESTER_PORCELAIN

Command: git status --porcelain -uall

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-batch-open.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-final-format.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t2-final-analyze.md
```

CSPROJ_PATHS_NEWLY_DIRTIED_BY_THIS_RUN: 0

The POST listing is byte-identical to the PRE listing. The hint-path synchroniser that
`Invoke-VSBuild.Tests.ps1` executes reported `All HintPaths are up to date` and therefore rewrote no
project file. No `git checkout --` was run over any path, because no path met the condition that
would have authorised one.

## Pass 2 — re-run after the P7-T7 toolchain restart

Timestamp: 2026-09-13T07-11

The loop restarted from P7-T1 because P7-T7's coverage gate failed on its first measurement at 82.5
percent for `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` against a floor of 90. The
remediation added two tests to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`,
which is inside this delivery's Write Set, so this task was re-run over the amended tree.

### PRE_PESTER_PORCELAIN, pass 2

Command: git status --porcelain -uall

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-batch-open.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-final-format.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t2-final-analyze.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t3-final-test.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t4-final-csharpier-check.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t5-final-msbuild-analyzer.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t6-final-msbuild-nullable.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml
```

No `.csproj` path is reported.

### Counts line, pass 2, verbatim

```
PESTER_COUNTS: passed=133 failed=0 skipped=0
```

PESTER_PASSED_COUNT: 133
PESTER_FAILED_COUNT: 0
PESTER_SKIPPED_COUNT: 0

The count rose from 131 to 133, which is the two tests the remediation added, both passing. The
container list was byte-identical to the pass-1 list: the same 16 containers, so all seven test files
this delivery creates or repairs are named on this pass as well.

### POST_PESTER_PORCELAIN, pass 2

Command: git status --porcelain -uall

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-batch-open.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t1-final-format.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t2-final-analyze.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t3-final-test.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t4-final-csharpier-check.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t5-final-msbuild-analyzer.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t6-final-msbuild-nullable.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml
```

CSPROJ_PATHS_NEWLY_DIRTIED_BY_THIS_RUN: 0

Byte-identical to the pass-2 PRE listing. The hint-path synchroniser again reported `All HintPaths are
up to date` and rewrote no project file. No `git checkout --` was run over any path on either pass.

## Output Summary

MCP_RESULT_OK_FLAG: true. Direct run EXIT_CODE: 0 with 131 passed, 0 failed, 0 skipped over
`tests/scripts/vscode`. The passed count rose from the P0-T12 baseline of 103 to 131. Sixteen
containers executed and all seven test files this delivery creates or repairs are named among them.
The pre- and post-run porcelain listings are identical and neither reports a `.csproj` path, so the
whole-folder run modified no project file.

The operative result is pass 2: EXIT_CODE: 0 with 133 passed, 0 failed, 0 skipped, the same 16
containers, and pre- and post-run porcelain listings that are again identical and again free of any
`.csproj` path. 133 is greater than the P0-T12 baseline of 103.
