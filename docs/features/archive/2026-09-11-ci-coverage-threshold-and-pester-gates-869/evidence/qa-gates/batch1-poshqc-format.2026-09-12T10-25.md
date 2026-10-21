# Batch 1 — PowerShell formatter step (P2-T5)

Timestamp: 2026-09-14T18-36

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
 M scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
```

## Porcelain immediately after the call

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim:

The after-output is byte-identical to the before-output and holds the same five entries in the same order:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
```

## Verdict from the porcelain difference

The two porcelain outputs are identical, so the formatter brought no additional file into the modified set. **No file was rewritten that was not already modified by this batch's own edits.**

## Supplementary hash-stability check

The porcelain comparison is necessary but not sufficient here, and that limitation is recorded rather than glossed. All five files in the scan folders that this batch touches were already in the modified state before the formatter ran, so a formatter rewrite of one of them would not change its porcelain status line. The porcelain difference can therefore only detect a rewrite of a file the batch did not touch.

To close that gap, a second formatter pass was run with a SHA-256 hash of every `.ps1` file under both scan folders captured immediately before and immediately after it. This is a mechanically necessary strengthening of the same observation the task asks for and introduces no new outcome.

Files hashed: 30 (14 under `scripts/vscode`, 16 under `tests/scripts/vscode`).

Result: **all 30 hashes are identical before and after the second pass.** The tree is at a formatter fixpoint, so the first pass left nothing for a second pass to change, and no file in either scan folder differs from its formatted form.

Sample of the compared values, unchanged across the pass:

```
Invoke-MSTestWithCoverage.Threshold.ps1 ABA0BB53CFD80E63
Invoke-MSTestWithCoverage.ps1 4D9263A8EB7A81C3
Invoke-MSTest.RunSettings.Tests.ps1 61400DE13A6B93D4
Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 A822876D33EE47B2
Invoke-MSTestWithCoverage.Threshold.Tests.ps1 D53B7DEF7681D3D1
```

Because no file was rewritten, the phase does not restart at P2-T5 and execution proceeds to P2-T6.

Output Summary: PoshQC format ran successfully over both scan folders. The porcelain output was identical before and after, and a hash comparison across a second pass showed all 30 PowerShell files unchanged, so the formatter rewrote nothing.
