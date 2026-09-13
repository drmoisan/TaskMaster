# P1-T7 — Phase 1 PowerShell Toolchain

Timestamp: 2026-09-13T05-36
Task: [P1-T7]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders` to every
MCP invocation.

Toolchain order: format, then analyze, then test. No step failed, so the loop was not restarted.

---

## Step 1 — Format

PRE_FORMAT_TREE_STATE:

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t8-msbuild-analyzer-baseline.md
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t9-msbuild-nullable-baseline.md
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t1-part-file-structure.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t2-helpers-dot-source.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t3-projection-shape-tests.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t4-distinct-wording-set.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t5-reconciliation-tests.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t6-package-set-test.md
?? scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
```

Every path this listing reports under either script folder was created or edited by this task's own
phase: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (edited by P1-T2),
`scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` (created by P1-T1) and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` (created by P1-T3). No foreign
path was present before the format invocation, so the precondition on the formatter restore rule held.

Tool: mcp__drm-copilot__run_poshqc_format
Command: mcp__drm-copilot__run_poshqc_format with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC format against '<worktree>' with 2 selected scan folder(s).`

FIRST_POST_FORMAT_PORCELAIN_STATUS (limited to the two script folders):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
```

Restore clause: zero applicable paths. Every one of the three reported paths is named as a
backticked repository-relative path in this plan, so `git checkout --` was run on no path. Nothing was
restored, and in particular nothing belonging to a concurrent item was touched.

SECOND_POST_FORMAT_PORCELAIN_STATUS (re-read after the restore clause completed with no action):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
```

It lists no path this plan does not own, which is this step's acceptance.

Observation beyond the exit code, recorded because the formatter exits zero both when it rewrites
nothing and when it repairs drift: the formatter rewrote
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` and in doing so dropped that
file's UTF-8 byte-order mark, while leaving
`scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` and its byte-order mark untouched. The mark
required by this plan's new-file convention was restored and the formatter was then invoked a second
time; on that invocation the mark survived, which establishes that the file is now formatter-clean
rather than merely mark-carrying. Encoding observed after the second invocation:

```
scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 lines=197 bom=True
tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 lines=451 bom=True
```

A format invocation that rewrites a path this plan owns does not by itself restart the toolchain
loop, because the analyze and test steps below ran against the formatted tree.

---

## Step 2 — Analyze

Tool: mcp__drm-copilot__run_poshqc_analyze
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt: `Exception: PSScriptAnalyzer reported 16 issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

The exit code of 1 is not this gate. The MCP analyzer exits 1 on any Warning and both script folders
already carried thirteen Warning-severity diagnostics at the Phase 0 baseline. The gate is the
diagnostic-set comparison below.

Paired direct run, unconditional:

Command: pwsh -NoProfile -Command '<Invoke-ScriptAnalyzer -Recurse over scripts/vscode and over tests/scripts/vscode, each diagnostic printed as severity, rule name, file leaf name and line number, ending with a labelled total>'
EXIT_CODE: 0

```
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 185
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 186
DIAG| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 139
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 52
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 87
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 147
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157
POWERSHELL_ANALYZER_TOTAL: 16
```

Comparison against the P0-T11 baseline set:

- Total unchanged at 16. Composition unchanged at 13 Warning and 3 Information across 6 files.
- Exactly one line differs from the baseline text: the `PSUseSingularNouns` diagnostic on
  `Invoke-MSTestWithCoverage.Helpers.ps1` moved from line 138 to line 139. That is the same
  pre-existing diagnostic displaced by one line, because P1-T2 inserted one dot-source line above it.
  It is not a new entry.
- NO_ENTRY_FOR_NEW_PART_FILE: true. No diagnostic carries the file leaf name
  `Invoke-MSTestWithCoverage.Projection.ps1`.
- NO_ENTRY_FOR_NEW_TEST_FILE: true. No diagnostic carries the file leaf name
  `Invoke-MSTestWithCoverage.Projection.Tests.ps1`.
- No diagnostic is reported against any file under `tests/scripts/vscode`, as at baseline.

---

## Step 3 — Test

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

Paired direct Pester run over this phase's new test file:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing ConvertTo-JacocoPackageProjection
  [+] emits the exact projection document for a multi-package fixture 84ms
  [+] derives missed as valid minus covered for lines and branches independently 37ms
  [+] emits zero line counters for a package with no class elements 3ms
  [+] emits a zero BRANCH counter rather than omitting it when no branch data is present 10ms
  [+] emits one package element per source package in document order 6ms
  [+] throws the existing missing-packages wording without introducing a second wording 10ms

Describing Assert-JacocoProjectionReconciliation
  [+] returns without throwing when the projection totals equal the source root attributes 12ms
  [+] throws naming the expected and the observed totals when the projection disagrees 7ms

Describing Invoke-MSTestWithCoverage.Projection.ps1 counting-rule delegation
  [+] delegates the counting rule to the per-package helper and re-derives nothing 37ms
Tests completed in 629ms
Tests Passed: 9, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_COUNTS passed=9 failed=0 skipped=0
```

PASSED: 9
FAILED: 0
SKIPPED: 0

Whole-folder direct run, recorded so the phase's effect on the pre-existing population is visible:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode with Run.PassThru and an explicit exit>'
EXIT_CODE: 0

```
PESTER_FOLDER_COUNTS passed=112 failed=0 skipped=0
```

112 is the Phase 0 baseline of 103 plus this phase's 9 new tests, with zero failed and zero skipped,
so no pre-existing test regressed.

## Per-test criterion mapping used by P1-T8 through P1-T13

| Test name | Recorded | Criterion |
|---|---|---|
| `emits the exact projection document for a multi-package fixture` | passed | AC1 |
| `derives missed as valid minus covered for lines and branches independently` | passed | AC2 |
| `delegates the counting rule to the per-package helper and re-derives nothing` | passed | AC3 |
| `throws the existing missing-packages wording without introducing a second wording` | passed | AC6 |
| `emits a zero BRANCH counter rather than omitting it when no branch data is present` | passed | AC7 |
| `emits zero line counters for a package with no class elements` | passed | AC8 |

## Files created, written or deleted by the tests

None. Every fixture in the new test file is a here-string assigned to a script-scoped variable inside
its `It` block, and no test invokes a filesystem write, create or delete. The whole-worktree porcelain
status taken after the test step lists no path other than this delivery's own edits and evidence
artifacts, which confirms no test wrote to disk and no project file was rewritten.

## Addendum — third format invocation and line-ending convergence

The first format invocation rewrote the new test file and emitted line-feed line endings, which
diverge from the carriage-return line-feed endings every pre-existing file in both script folders
carries. The file was converted to carriage-return line-feed endings with its byte-order mark
retained, and the formatter was invoked a third time to establish whether that state is stable.

PRE_FORMAT_TREE_STATE for the third invocation (limited to the two script folders; the index entries
reflect an interim staging of the same three paths):

```
M  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
A  scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
AM tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
```

Every reported path is one this plan owns, so the restore clause again had zero applicable paths and
`git checkout --` was run on no path.

Tool: mcp__drm-copilot__run_poshqc_format
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0

Observation after the third invocation:

```
after_format crlf=451 lfonly=0 bom=True lines=451
```

The formatter preserved both the carriage-return line-feed endings and the byte-order mark, so the
file is formatter-stable in the repository's own line-ending convention. Because that format pass
changed a file, the analyze and test steps were re-run over the changed tree:

```
POWERSHELL_ANALYZER_TOTAL: 16
PESTER_FOLDER_COUNTS passed=112 failed=0 skipped=0
```

No diagnostic carries a file leaf name containing `Projection`, and the analyzer total and the Pester
counts are identical to the figures recorded above, so the loop converged on this pass.

## Output Summary

Format: MCP ok true, exit 0; two porcelain listings recorded, the second listing no path this plan
does not own; formatter restore clause had zero applicable paths.
Analyze: MCP ok false, exit 1, 16 issues, which is the legitimate baseline shape; the direct
diagnostic set matches the P0-T11 set with no new entry, and carries no entry for either file this
phase created.
Test: MCP ok true, exit 0; direct run 9 passed, 0 failed, 0 skipped over the new test file, and 112
passed, 0 failed, 0 skipped over the whole test folder.
FAILED: 0. The toolchain was not restarted.
