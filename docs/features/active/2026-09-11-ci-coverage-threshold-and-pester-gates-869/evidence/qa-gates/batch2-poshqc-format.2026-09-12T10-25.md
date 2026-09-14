# Batch 2 — PowerShell formatter step (P4-T3)

Timestamp: 2026-09-14T19-12

Tool: `mcp__drm-copilot__run_poshqc_format`
Workspace root: the item worktree root.
Scan folders passed, explicitly: `scripts/vscode` and `tests/scripts/vscode`.

Returned payload:

```
ok: true
summary: Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s).
```

## Porcelain immediately before the call

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim:

```
 M scripts/vscode/Invoke-Restore.ps1
 M scripts/vscode/Invoke-VSBuild.ps1
 M tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
?? tests/scripts/vscode/Invoke-Restore.Tests.ps1
```

## Porcelain immediately after the call

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim: byte-identical to the before-output, the same four entries in the same order:

```
 M scripts/vscode/Invoke-Restore.ps1
 M scripts/vscode/Invoke-VSBuild.ps1
 M tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
?? tests/scripts/vscode/Invoke-Restore.Tests.ps1
```

## Verdict from the porcelain difference

The two porcelain outputs are identical, so the formatter brought no additional file into the modified set. **No file was rewritten.**

## Supplementary hash comparison

As in P2-T5, the porcelain comparison alone cannot detect a rewrite of a file this batch had already modified, so a SHA-256 hash of every `.ps1` file under both scan folders was captured immediately before and immediately after the call.

Files hashed: 31 (14 under `scripts/vscode`, 17 under `tests/scripts/vscode`; the test directory gained `Invoke-Restore.Tests.ps1` in P3-T2).

Result: **all 31 hashes are identical before and after the call.** The four files this batch touches are among them:

```
Invoke-Restore.ps1 BA3A1A2FEA7F
Invoke-VSBuild.ps1 239D1D930DF9
Invoke-VSBuild.Tests.ps1 E8C07C5DCE8D
Invoke-Restore.Tests.ps1 581FB51CA0BB
```

Because no file was rewritten, the phase does not restart at P4-T3 and execution proceeds to P4-T4.

Output Summary: PoshQC format ran successfully over both scan folders and rewrote nothing. The porcelain output was identical before and after, and all 31 PowerShell file hashes were unchanged across the call.
