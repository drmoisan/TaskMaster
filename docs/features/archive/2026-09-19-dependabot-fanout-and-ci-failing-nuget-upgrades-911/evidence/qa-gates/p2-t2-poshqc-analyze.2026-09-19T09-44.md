# P2-T2 — PoshQC analyze over the four scan folders

Timestamp: 2026-09-19T14-58

Command: MCP tool `mcp__drm-copilot__run_poshqc_analyze`.

`workspace_root`: `<execution-worktree-root>`

Exact `scan_folders` argument value passed:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

EXIT_CODE: 1

ExpectedExitCode: 1

MCP payload of the accepted run, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

`MCP Result: ok:true` is **not** asserted and is expected to be `false` while the 16 pre-existing
findings remain. Exit 1 is the tool's response to a non-empty diagnostic set, not a tool failure.

## Integer total finding count

**16.**

## The first run reported 17, and the extra finding was this change's own

The task was run twice. The first run reported 17 issues, one more than the baseline. The
additional finding was located by a direct `Invoke-ScriptAnalyzer` enumeration over the same four
folders:

```
scripts/dependencies/PackageGraph.psm1 | PSUseOutputTypeCorrectly | Information | 127
```

That file is one of the two this change created, so the acceptance clause it violated is the
owned-file clause — "the finding count for files this change has created or modified as of this
task ... is exactly 0" — and not the baseline-subset clause. The defect was in this change's own
code and was corrected rather than reported:

- `Get-PackageManifestPath` declared `[OutputType([string])]` while returning
  `@($selected | Sort-Object)`, whose inferred element type the analyzer could not reconcile with
  the scalar declaration.
- The attribute became `[OutputType([string[]])]` and the return became
  `return [string[]]@($selected | Sort-Object)`. The explicit cast is load-bearing: widening the
  attribute alone leaves the pipeline's inferred output at `System.Object` and the rule still
  fires.

The correction modified a source file, so the toolchain loop restarted at formatting. P2-T1 records
that third format pass, which rewrote 0 of 34 files. This run is the re-run of P2-T2 after that
restart. The 17-finding run is recorded here rather than discarded, because an artifact that showed
only the passing run would not show that the owned-file clause was ever exercised.

## How the tuple set was obtained

The MCP tool reports a count only — no rule name, no file, no line — so the tuple set this task
requires cannot be read from it. The count was reconciled against a direct run of the same analyzer
over the same four folders:

```
Invoke-ScriptAnalyzer -Path "scripts/dependencies"       -Recurse
Invoke-ScriptAnalyzer -Path "scripts/vscode"             -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/vscode"       -Recurse
```

The direct run totals **16**, equal to the MCP tool's reported 16, which is what establishes that
the direct invocation reproduces the tool's effective rule set and that the tuples below are the
tool's own finding set. The same reconciliation held on the failing run, where both reported 17.

## Full finding list — 16 `(file path, rule name, line)` tuples

| # | File path | Rule name | Severity | Line | In P0-T17 baseline |
|---|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 26 | yes |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 36 | yes |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 39 | yes |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 59 | yes |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 79 | yes |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 106 | yes |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 210 | yes |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 211 | yes |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | PSUseSingularNouns | Warning | 139 | yes |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | PSAvoidUsingWriteHost | Warning | 101 | yes |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 52 | yes |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 87 | yes |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | PSAvoidUsingWriteHost | Warning | 245 | yes |
| 14 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 150 | yes |
| 15 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 154 | yes |
| 16 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 157 | yes |

By severity: 13 Warning, 3 Information, 16 total.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The exact `scan_folders` argument value passed is recorded | recorded verbatim above | PASS |
| The integer total finding count is recorded | **16** | PASS |
| The full finding list is recorded as `(file path, rule name, line)` tuples | 16 rows, each with all three fields plus severity | PASS |
| The total is **exactly 16**, the full baseline set P0-T17 recorded | **16** | PASS |
| The finding count for files this change created or modified as of this task — `scripts/dependencies/PackageGraph.psm1` and `tests/scripts/dependencies/PackageGraph.Tests.ps1` — is exactly 0 | **0** | PASS |
| Every finding is a member of the 16-tuple baseline set, compared element by element | all 16 match on file, rule and line | PASS |

Element-by-element comparison against
`evidence/baseline/p0-t17-poshqc-analyze.2026-09-19T09-44.md`: the two sets are identical in
membership, in file path, in rule name, in severity and in line number, row for row in the same
sorted order. No baseline finding disappeared and none moved line.

## Non-vacuity

The exact-16 total is the load-bearing guard, and it is exact rather than bounded for the reason
the task states: a run that resolved no files at all reports a total of 0, an owned count of 0 and
a vacuously true subset relation over the empty set, so a total of 0 would be a failure rather than
a clean result. The guard was exercised in both directions in this task —

- **Above 16**: the first run reported 17 and the task did not pass. The owned-file clause fired on
  a genuine defect in this change's own module.
- **Not below 16**: all 16 baseline tuples are present and matched individually, so the set has not
  silently shrunk through a file dropping out of the scan.

The owned-file count of 0 is itself paired with a positive observation rather than standing alone:
the two owned files were enumerated explicitly and both were in the analyzer's scan population, as
the 17-finding run proves — one of them produced a finding, so neither is invisible to the tool.

No finding appears outside the baseline subset in a file this change did not touch, which would
have meant the change perturbed an unrelated file.

Output Summary: CMD-POSHQC-ANALYZE over the four explicitly supplied scan folders returned
EXIT_CODE 1 with `ok:false` and `PSScriptAnalyzer reported 16 issue(s).` The total is **16** — 13
Warning and 3 Information — matching the P0-T17 baseline element by element, and the finding count
for the two files this change owns is **0**. An earlier run of this task reported 17; the extra
finding was `PSUseOutputTypeCorrectly` at `scripts/dependencies/PackageGraph.psm1:127`, which was
corrected by declaring `[OutputType([string[]])]` and casting the return, after which the toolchain
loop restarted at formatting and this run was taken.
