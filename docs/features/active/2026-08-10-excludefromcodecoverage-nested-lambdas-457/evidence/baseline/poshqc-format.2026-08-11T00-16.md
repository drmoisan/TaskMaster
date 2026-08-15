# [P0-T6] PoshQC format baseline

Timestamp: 2026-08-11T00-16
Command: `mcp__drm-copilot__run_poshqc_format` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
EXIT_CODE: not emitted by the MCP surface (see below); MCP `ok:true`

MCP Result (verbatim):

```json
{"ok":true,"tool":"run_poshqc_format","workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a","summary":"Ran bundled PoshQC format against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a' with 2 selected scan folder(s)."}
```

`run_poshqc_format` returns only `{ok, tool, workspace_root, summary}`; it carries no process exit
code. `EXIT_CODE:` is therefore recorded as the MCP completion signal `ok:true`, and the substantive
result is measured from the git working tree immediately after the run, below.

## Scan set

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is deliberately excluded, per the plan: it is not
modified by this feature, `run_poshqc_format` rewrites in place, and formatting churn in that file
would be indistinguishable from a feature edit in the `[P2-T11]` changed-file audit.

## Files rewritten (attribution)

no file rewritten

## baseline format diff

`baseline format diff: empty`

`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` taken immediately after the format
run returned no output. This is the branch the plan records as expected (preflight measured
`Invoke-Formatter` as a byte-for-byte no-op on both scan-set files under default settings). `[P2-T11]`
and `[P3-T10]` therefore have a recorded referent: there are zero baseline hunks to exclude from
their "exactly two added lines" measurement.

## Scan granularity

FORMAT_SCAN_GRANULARITY: file-honored

`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode` taken immediately after the
format run (verbatim, empty result):

```
```

Derivation of the `file-honored` verdict, stated as an inference from a measured prior rather than as
an assumption: the plan records that a folder-level scan of `scripts/vscode` rewrites
`scripts/vscode/Sync-PackageReferences.ps1` (188 differing lines),
`scripts/vscode/Invoke-MSTest.ps1` (2) and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (5 differing
lines, 349 -> 350, a `} finally {` split and one indentation change) under `Invoke-Formatter`. None of
those three files appears as modified after this run. Had the tool coerced the scan to the containing
folder, all three would have been rewritten. The restricted listing is empty, so no file under
`scripts/vscode` or `tests/scripts/vscode` was rewritten and the folder-coerced restore branch is not
taken.

Restore branch: NOT TAKEN. No `git checkout -- <path>` was required. The restore branch remains
mandatory after every later format run (`[P3-T2]`) and is re-evaluated there.

## Output Summary

`run_poshqc_format` completed with `ok:true` over the two-file scan set. No file was rewritten;
`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode` is empty and
`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is empty. `baseline format diff:
empty`. Scan granularity measured as `file-honored`. No restore was required.
