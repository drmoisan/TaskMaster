# P2-T5 — Phase 2 PowerShell Toolchain

Timestamp: 2026-09-13T05-55
Task: [P2-T5]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders` to every
MCP invocation.

Toolchain order: format, then analyze, then test. No step failed and the format step rewrote nothing,
so the loop was not restarted.

---

## Step 1 — Format

PRE_FORMAT_TREE_STATE:

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p2-t1-batch-open.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p2-t2-namespace-tests.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p2-t3-derivation-tests.md
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p2-t4-verdict-and-failed-name-tests.md
?? scripts/vscode/Invoke-MSTest.TrxSummary.ps1
?? tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
```

The only paths under either script folder are `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` (created
by P2-T1) and `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` (created by P2-T2 and extended
by P2-T3 and P2-T4), both created by this executor in this phase. No foreign path was present before
the format invocation, so the precondition on the formatter restore rule held.

Tool: mcp__drm-copilot__run_poshqc_format
Command: mcp__drm-copilot__run_poshqc_format with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC format against '<worktree>' with 2 selected scan folder(s).`

FIRST_POST_FORMAT_PORCELAIN_STATUS (limited to the two script folders):

```
?? scripts/vscode/Invoke-MSTest.TrxSummary.ps1
?? tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
```

Restore clause: zero applicable paths. Both reported paths are named as backticked
repository-relative paths in this plan, so `git checkout --` was run on no path.

SECOND_POST_FORMAT_PORCELAIN_STATUS (re-read after the restore clause completed with no action):

```
?? scripts/vscode/Invoke-MSTest.TrxSummary.ps1
?? tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
```

It lists no path this plan does not own, which is this step's acceptance.

Observation beyond the exit code, recorded because the formatter exits zero both when it rewrites
nothing and when it repairs drift:

```
scripts/vscode/Invoke-MSTest.TrxSummary.ps1 crlf=150 lfonly=0 bom=True lines=150
tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 crlf=193 lfonly=0 bom=True lines=193
```

Both files retained their carriage-return line-feed endings and their UTF-8 byte-order marks. The
formatter emits line-feed endings and no mark whenever it actually rewrites a file, which is how the
same observation was read in Phase 1, so retention of both properties establishes that neither file
was rewritten here: both were already formatter-clean when written.

---

## Step 2 — Analyze

Tool: mcp__drm-copilot__run_poshqc_analyze
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt: `Exception: PSScriptAnalyzer reported 16 issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

The exit code of 1 is not this gate; the diagnostic-set comparison below is.

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
- The only textual difference from the baseline remains the `PSUseSingularNouns` diagnostic on
  `Invoke-MSTestWithCoverage.Helpers.ps1` at line 139 rather than 138, which Phase 1 already accounted
  for: it is the same pre-existing diagnostic displaced by the one dot-source line P1-T2 inserted
  above it.
- NO_ENTRY_FOR_NEW_PART_FILE: true. No diagnostic carries the file leaf name
  `Invoke-MSTest.TrxSummary.ps1`.
- NO_ENTRY_FOR_NEW_TEST_FILE: true. No diagnostic carries the file leaf name
  `Invoke-MSTest.TrxSummary.Tests.ps1`.
- No entry is present that is absent from the baseline set.

---

## Step 3 — Test

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
EXIT_CODE: 0
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

Paired direct Pester run over this phase's new test file:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing Get-TrxRunSummary namespace handling
  [+] reads the counters correctly from a namespaced test-result document 81ms
  [+] an unprefixed XPath over the same fixture selects zero nodes 4ms

Describing Get-TrxRunSummary skipped derivation
  [+] reports skipped as total minus executed and preserves the verbatim platform figures 11ms
  [+] states the skipped derivation in the formatted output 12ms

Describing Get-TrxRunSummary verdict and failed names
  [+] reports the run verdict and the names of the failed results 5ms
  [+] returns an empty failed-name collection for a fixture with no result elements 6ms
  [+] throws a specific message for a document with no result-summary node 17ms
Tests completed in 541ms
Tests Passed: 7, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_COUNTS passed=7 failed=0 skipped=0
```

PASSED: 7
FAILED: 0
SKIPPED: 0

Whole-folder direct run, recorded so the phase's effect on the pre-existing population is visible:

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode with Run.PassThru and an explicit exit>'
EXIT_CODE: 0

```
PESTER_FOLDER_COUNTS passed=119 failed=0 skipped=0
```

119 is the 112 recorded at the end of Phase 1 plus this phase's 7 new tests, with zero failed and zero
skipped, so no pre-existing test regressed.

## Per-test criterion mapping used by P2-T6 through P2-T8

| Test name | Recorded | Criterion |
|---|---|---|
| `reads the counters correctly from a namespaced test-result document` | passed | AC9 |
| `an unprefixed XPath over the same fixture selects zero nodes` | passed | AC9 |
| `reports skipped as total minus executed and preserves the verbatim platform figures` | passed | AC10 |
| `states the skipped derivation in the formatted output` | passed | AC10 |
| `reports the run verdict and the names of the failed results` | passed | AC11 |
| `returns an empty failed-name collection for a fixture with no result elements` | passed | AC11 |
| `throws a specific message for a document with no result-summary node` | passed | AC11 |

## Files created, written or deleted by the tests

None. Every fixture in the new test file is a here-string assigned to a script-scoped variable in the
`BeforeAll` block, and no test invokes a filesystem write, create or delete. The part file itself
invokes only `Set-StrictMode`, so the code under test cannot touch the filesystem either.

## Output Summary

Format: MCP ok true, exit 0; two porcelain listings recorded, the second listing no path this plan
does not own; restore clause had zero applicable paths; both new files retained their carriage-return
line-feed endings and byte-order marks, so the formatter rewrote nothing.
Analyze: MCP ok false, exit 1, 16 issues, which is the legitimate baseline shape; the direct
diagnostic set introduces no entry absent from the P0-T11 set and carries no entry for either file this
phase created.
Test: MCP ok true, exit 0; direct run 7 passed, 0 failed, 0 skipped over the new test file, and 119
passed, 0 failed, 0 skipped over the whole test folder.
FAILED: 0. The toolchain was not restarted.
