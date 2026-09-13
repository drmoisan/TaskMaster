# P0-T10 — PowerShell Format Baseline

Timestamp: 2026-09-13T04-59
Task: [P0-T10]

Scan scope: the two script folders, passed explicitly as `scan_folders`:
`scripts/vscode` and `tests/scripts/vscode`.

## PRE_FORMAT_TREE_STATE

Precondition required before the first formatter invocation, so that any later
formatter-introduced modification at a path this delivery does not own is provably self-authored and
the restore is auditable.

Command: git status --porcelain --untracked-files=all
Run at the start of this delegation, before any command that writes:

```
```

The output was empty: the working tree carried no modification and no untracked file at that point.

Command re-run immediately before the formatter invocation, limited to the two script folders:
git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode

```
```

Empty. No path under `scripts/vscode` or `tests/scripts/vscode` carried a pre-existing uncommitted
modification, so the narrow restore exception applies and a restore at those paths could not destroy
work authored by anyone else. For completeness, the whole-tree status at that moment listed one
modified file and nine untracked files, all of them inside this feature folder and all authored by
this delegation: the plan file (checkbox updates) and the P0-T1 through P0-T9 evidence artifacts.

## Step 1 — Formatter invocation

Tool: mcp__drm-copilot__run_poshqc_format
Command: mcp__drm-copilot__run_poshqc_format with workspace_root set to this worktree and
scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
MCP result summary: `Ran bundled PoshQC format against '<worktree>' with 2 selected scan folder(s).`

## Step 2 — First porcelain status, the recorded pre-existing formatting drift

Command: git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode
Verbatim output:

```
```

The listing is empty. Recorded observation: there was no pre-existing formatting drift in either
script folder, and the formatter rewrote no file. This is the tree observation that discriminates a
clean run from a repairing one; the exit code alone is not accepted for this write-mode step,
because the formatter exits zero in both cases.

Consequence for the formatter restore rule: no reported path existed, so no path required
`git checkout --` to undo a formatter rewrite. The mandated restore command was nonetheless run over
both folders as the rule states, and was a no-op.

## POST_FORMAT_BASELINE_LINE_COUNTS:

Measured while the tree was in its formatted state, before the restore. Each figure is the element
count of the file's content lines.

scripts/vscode/Invoke-MSTest.ps1 202
scripts/vscode/Invoke-MSTestWithCoverage.ps1 351
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 470
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 496
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 144
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 99

These are the figures the 500-line ceiling and the helpers growth bound apply to, because every
later measurement in this plan is taken after a format step. The helpers file measures 470, leaving
30 lines of headroom against the ceiling; the shared argument-builder test file measures 496,
leaving 4.

## Step 3 — Restore and second porcelain status

Command: git checkout -- scripts/vscode tests/scripts/vscode
EXIT_CODE: 0
Output: (no output)

Command: git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode
Verbatim output:

```
```

Empty, as required.

## Output Summary

MCP_RESULT_OK_FLAG: true. The first porcelain listing over the two script folders was empty, so no
pre-existing formatting drift exists and the formatter changed nothing. The second porcelain listing
after the mandated restore is also empty. Six baseline line counts are recorded above under
`POST_FORMAT_BASELINE_LINE_COUNTS:`, every one of them at most 500.

EXIT_CODE: 0
