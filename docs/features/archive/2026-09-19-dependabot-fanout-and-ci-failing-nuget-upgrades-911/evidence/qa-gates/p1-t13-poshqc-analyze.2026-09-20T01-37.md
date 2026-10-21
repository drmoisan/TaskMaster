# PoshQC Analyze After Phase 1 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-47-05
- Task: [P1-T13]
- Command: CMD-POSHQC-ANALYZE
- EXIT_CODE: 1
- ExpectedExitCode: 1

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

## Integer Finding Total

**13.**

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

A direct `Invoke-ScriptAnalyzer` run over the same four folders also totals 13, which is what
licenses reading the tuple list from that run.

## The First Run Failed This Gate

This task was run twice and the first run is recorded rather than discarded.

**First run: 22 findings.** Nine more than the [P0-T7] baseline of 13. All nine sat in
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, the one file Phase 1 modified, and all
nine were `PSReviewUnusedParameter`:

| Rule | Parameter | Count |
|---|---|---|
| `PSReviewUnusedParameter` | `LibraryDirectory` | 2 |
| `PSReviewUnusedParameter` | `Directory` | 3 |
| `PSReviewUnusedParameter` | `Path` | 3 |
| `PSReviewUnusedParameter` | `Text` | 1 |

Each was a seam delegate that declared a `param()` block matching the production call signature
and then ignored the parameter. Production invokes the delegates positionally, so the parameters
are required for signature parity and could not simply be deleted.

**The correction.** Each delegate was changed to **answer by its parameter** rather than
unconditionally:

- `ListAssetFolder` returns an empty set for an empty library directory, matching the idiom the
  file's pre-existing `Get-AssetSeam` already used;
- `ListProjectPath` in [P1-T7] returns the empty project list for `C:\fake\Proj` specifically and
  a non-empty list for any other directory, so the empty result is a property of the fixture
  rather than of an enumerator that returns nothing whatever it is asked;
- `ListProjectPath` in [P1-T8] and [P1-T9] returns an empty set for an empty directory;
- `ReadText` in [P1-T7] records the path it was asked for alongside its call count;
- `ReadText` in [P1-T8] answers by path, returning the conflicted text for the project file;
- `WriteText` in [P1-T9] records the path and the text alongside its call count.

No assertion in the eight tests changed. Two fixtures became strictly more discriminating: the
[P1-T7] project enumerator can now distinguish a directory that holds no project from an
enumerator that always returns nothing, which is a property the original fixture could not
express.

Per the toolchain rule in `CLAUDE.md`, the loop then restarted at the formatter: [P1-T12] re-ran
with 0 rewrites, [P1-T10] re-ran with identical figures, and this second analyzer run reports 13.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `scan_folders` recorded exactly | yes | recorded above | PASS |
| Total equals the `N` [P0-T7] recorded | 13 | **13** | PASS |
| Finding count for `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | exactly 0 | **0** | PASS |
| Every remaining finding a member of the [P0-T7] tuple set, element by element | yes | 13 of 13 | PASS |

The equality with `N` is the non-vacuity guard. A run that resolved no files reports a total of 0,
an owned count of 0 and a vacuously true subset relation over the empty set, so **a total of 0 is
a failure** unless `N` is 0, which [P0-T7] already forbids. It reported 13.

## Full Finding List — 13 Tuples

| # | File path | Rule name | Severity | Line | In the [P0-T7] set |
|---|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 26 | yes |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 36 | yes |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 39 | yes |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 59 | yes |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 79 | yes |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 106 | yes |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | Warning | 210 | yes |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | Warning | 211 | yes |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `PSUseSingularNouns` | Warning | 139 | yes |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | `PSAvoidUsingWriteHost` | Warning | 101 | yes |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | Warning | 52 | yes |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | Warning | 87 | yes |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSAvoidUsingWriteHost` | Warning | 245 | yes |

Thirteen of thirteen match a [P0-T7] row on file path, rule name, severity and line. Zero findings
sit in any file this phase touched.

## Output Summary

13 findings, equal to the [P0-T7] baseline `N`. Zero in the one file this phase modified. The
first run of this gate reported 22 and failed; the nine surplus findings were corrected in the
test file and the toolchain loop restarted at the formatter. `ok:false` is the expected tool state
while the 13 pre-existing findings remain and is not asserted.
