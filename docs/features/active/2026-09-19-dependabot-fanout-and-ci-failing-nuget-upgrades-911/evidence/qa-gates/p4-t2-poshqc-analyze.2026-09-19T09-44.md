# P4-T2 — PowerShell analyzer, Batch B close-out

Timestamp: 2026-09-20T01-02

Command: CMD-POSHQC-ANALYZE — MCP tool `mcp__drm-copilot__run_poshqc_analyze`, `workspace_root`
passed as the execution worktree root.

Exact `scan_folders` argument value passed:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

EXIT_CODE: 1

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <repo-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

`MCP Result: ok:true` is not asserted and is expected to be `false` while the residual baseline
findings stand. Exit 1 is the tool's response to a non-empty diagnostic set.

## A transient tool fault on the first invocation, recorded rather than absorbed

The first invocation returned `ok:false` with a different `stderr_excerpt`:

```
Exception: Invoke-ScriptAnalyzer failed for
C:\...\scripts\dependencies\PackageGraph.psm1
(System.InvalidOperationException): You cannot have more than one dynamic module in each dynamic
assembly in this version of the runtime.
```

That is a PSScriptAnalyzer runtime fault, not a diagnostic: it reports no rule, no line and no
count, and it names a file this task did not touch. An identical class of fault was observed once
during Phase 3 from a direct `Invoke-ScriptAnalyzer` call and cleared on re-invocation. The tool
was re-invoked with the identical argument value and returned the 13-issue result above. The fault
is recorded here so a reader does not mistake the retry for a re-run against changed state: no
file changed between the two invocations.

## Integer total finding count

**13.**

## How the tuple set was obtained

The MCP tool reports a count only, so the tuple set was reconciled against a direct run of the
same analyzer over the same four folders:

```
Invoke-ScriptAnalyzer -Path "scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse
```

The direct run totals **13**, equal to the MCP tool's reported 13, which establishes that the
direct invocation reproduces the tool's effective rule set and that the tuples below are the
tool's own finding set.

## Full finding list — 13 `(file path, rule name, line)` tuples

| # | File path | Rule name | Severity | Line |
|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 26 |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 36 |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 39 |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 59 |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 79 |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 106 |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 210 |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 211 |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | PSUseSingularNouns | Warning | 139 |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | PSAvoidUsingWriteHost | Warning | 101 |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 52 |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 87 |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | PSAvoidUsingWriteHost | Warning | 245 |

By severity: 10 Warning, 3 Information.

## Element-by-element comparison against the P0-T17 16-tuple baseline

| Baseline # | Tuple | Present now |
|---|---|---|
| 1 | `Install-RepoDotNetSdk.ps1`, PSUseOutputTypeCorrectly, 26 | yes |
| 2 | `Install-RepoDotNetSdk.ps1`, PSUseOutputTypeCorrectly, 36 | yes |
| 3 | `Install-RepoDotNetSdk.ps1`, PSUseOutputTypeCorrectly, 39 | yes |
| 4 | `Install-RepoDotNetSdk.ps1`, PSAvoidUsingWriteHost, 59 | yes |
| 5 | `Install-RepoDotNetSdk.ps1`, PSAvoidUsingWriteHost, 79 | yes |
| 6 | `Install-RepoDotNetSdk.ps1`, PSAvoidUsingWriteHost, 106 | yes |
| 7 | `Invoke-MSTest.ps1`, PSAvoidUsingWriteHost, 210 | yes |
| 8 | `Invoke-MSTest.ps1`, PSAvoidUsingWriteHost, 211 | yes |
| 9 | `Invoke-MSTestWithCoverage.Helpers.ps1`, PSUseSingularNouns, 139 | yes |
| 10 | `Invoke-Restore.ps1`, PSAvoidUsingWriteHost, 101 | yes |
| 11 | `Invoke-VSBuild.ps1`, PSUseSingularNouns, 52 | yes |
| 12 | `Invoke-VSBuild.ps1`, PSUseSingularNouns, 87 | yes |
| 13 | `Invoke-VSBuild.ps1`, PSAvoidUsingWriteHost, 245 | yes |
| 14 | `Sync-PackageReferences.ps1`, PSAvoidUsingWriteHost, 150 | **removed by the P3-T4 rewrite** |
| 15 | `Sync-PackageReferences.ps1`, PSAvoidUsingWriteHost, 154 | **removed by the P3-T4 rewrite** |
| 16 | `Sync-PackageReferences.ps1`, PSAvoidUsingWriteHost, 157 | **removed by the P3-T4 rewrite** |

Thirteen of the sixteen baseline tuples are present, unchanged in file, rule and line. The three
absent ones are exactly the three the task text names: the `PSAvoidUsingWriteHost` findings at
lines 150, 154 and 157 of `scripts/vscode/Sync-PackageReferences.ps1`. The rewrite replaced those
`Write-Host` calls with `Write-Information ... -InformationAction Continue` and `Write-Warning`,
and that file now reports 0 findings.

**Every finding in the current set is a member of the baseline set.** No finding appears in a file
this change did not touch that was not already in the baseline.

The line numbers are still comparable because the PoshQC formatter rewrote none of the five files
carrying them: P4-T1's round-1 hash-difference set was `PackageCompatibility.psm1` and
`DependabotConfig.Tests.ps1` only, and round 2's was empty. This is the condition gate rule 14
attaches to P0-T17's positional citations, and it holds.

## Findings in files this change has created or modified

Enumerated explicitly, as the task requires:

| File | Findings |
|---|---|
| `scripts/dependencies/PackageGraph.psm1` | 0 |
| `scripts/dependencies/PackageCompatibility.psm1` | 0 |
| `scripts/vscode/Sync-PackageReferences.ps1` | 0 |
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 0 |
| `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | 0 |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 0 |
| `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | 0 |
| **Total across the seven owned files** | **0** |

None of the seven appears anywhere in the 13-tuple list above, which is the direct check.

Two owned findings were raised and cleared during Phase 3 before this gate ran, and are recorded
so the zero is not read as a claim that the rule never fires: `PSUseBOMForUnicodeEncodedFile` on
`scripts/dependencies/PackageCompatibility.psm1` and on
`tests/scripts/dependencies/PackageCompatibility.Tests.ps1`, each caused by a single em dash in a
comment written without a byte-order mark. Both were fixed by replacing the character, matching
the `PackageGraph` precedent of pure-ASCII files with no byte-order mark. The zero-owned-findings
clause did the work it was written for, as it did at P2-T2.

## Non-vacuity

The exact-13 total is the guard. A run that resolved no files at all would report a total of 0, an
owned count of 0, and a vacuously true subset relation over the empty set; **a total of 0 would be
a failure, not a clean result**. The observed total is 13, the 13 tuples are enumerated, and each
is matched element by element against a named baseline row.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Exact `scan_folders` value recorded | verbatim | recorded | PASS |
| Integer total finding count | exactly 13 | 13 | PASS |
| Full finding tuple list recorded | 13 tuples | 13 rows, each with file, rule, severity and line | PASS |
| Findings in the seven owned files | exactly 0 | 0 | PASS |
| Every finding a member of the P0-T17 baseline set | all | 13 of 13, matched element by element | PASS |
| `MCP Result: ok:true` | not asserted | `ok:false` observed and recorded | per instruction |

Output Summary: CMD-POSHQC-ANALYZE over the four explicitly supplied `scan_folders` returned
EXIT_CODE 1 with `ok:false` and `PSScriptAnalyzer reported 13 issue(s).`, matching the expected
fall from the 16-finding baseline. The 13 are enumerated as `(file, rule, line)` tuples and every
one matches a P0-T17 baseline row exactly; the three absent rows are precisely the
`PSAvoidUsingWriteHost` findings at lines 150, 154 and 157 of
`scripts/vscode/Sync-PackageReferences.ps1` that the P3-T4 rewrite removed. All seven files this
change has created or modified report **0** findings. A transient
`Invoke-ScriptAnalyzer` runtime fault on the first invocation is recorded above; no file changed
between it and the retry.
