# PoshQC Format Drift Probe ([P0-T5], [P0-T6])

Timestamp: 2026-09-03T11-52

Command:
1. `mcp__drm-copilot__run_poshqc_format` with `workspace_root` of `<repo-root>` and `scan_folders` of `scripts/vscode` and `tests/scripts/vscode`
2. `git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode`
3. `git -C <repo-root> diff -- scripts/vscode tests/scripts/vscode`

EXIT_CODE: 0

## Tool payload

Recorded as returned, with one class-level substitution applied at capture time: the tool echoes the `workspace_root` argument, which is this item's absolute worktree root, and the Host-path hygiene rule in this plan's Plan Conventions prohibits a committed artifact from carrying an absolute host path. That one value is written as `<repo-root>`; every other character of the payload is as returned.

```
{"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
```

## Porcelain output (command 2), verbatim

```
```

(zero lines)

## Diff output (command 3), verbatim

```
```

(zero lines)

PRE-EXISTING FORMAT DRIFT FILES: NONE

DRIFT-IN-PRODUCTION-FILE: NONE

Output Summary: The formatter rewrote no file under `scripts/vscode` or `tests/scripts/vscode` on the unmodified tree. Both the porcelain and the diff capture are empty. Branch A is therefore selected for the rest of this plan: every later `run_poshqc_format` invocation uses `scan_folders` of `scripts/vscode` and `tests/scripts/vscode`.

## POST-RESTORE PORCELAIN ([P0-T6])

Command: `git -C <repo-root> checkout -- scripts/vscode tests/scripts/vscode`, then `git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode`

```
```

(zero lines; the analyzer and test baselines that follow are taken against the committed tree)
