# Phase 0 — Baseline PowerShell formatter state (P0-T13)

Timestamp: 2026-09-14T18-14

## Porcelain before the formatter call

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim: empty.

## Formatter call

Tool: `mcp__drm-copilot__run_poshqc_format`
Workspace root: the item worktree root.
Scan folders passed, explicitly: `scripts/vscode` and `tests/scripts/vscode`.

Returned payload:

```
ok: true
summary: Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s).
```

## Porcelain after the formatter call

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim: empty.

## Verdict

The formatter rewrote no file. That verdict is derived from the difference between the two porcelain outputs, not from the exit code: a formatter exits the same way whether or not it rewrote files, so the exit code alone is not a sufficient observation. Both outputs are empty and therefore identical, so no file under either scan folder changed.

Because no file was rewritten, this task names no file for the P2-T8 commit to carry, and the exoneration clause in P2-T8's acceptance is not exercised by this task.

Output Summary: PoshQC format ran successfully over both scan folders and rewrote nothing. The porcelain output over `scripts/vscode` and `tests/scripts/vscode` was empty before the call and empty after it.
