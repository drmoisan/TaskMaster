# Phase 5 Stage 1 — Formatting Verification (Repo-Wide, Read-Only) (Issue #445, AC21 stage 1)

Timestamp: 2026-08-22T09-53

Command:
```
& $DOTNET tool run csharpier check .
```
with `DOTNET` = `C:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk\dotnet.exe`. Run from `WS`. Read-only; no file was modified.

EXIT_CODE: 0

## Verbatim output

```
Checked 1517 files in 6574ms.
```

## Numeric results

| Measurement | Value |
|---|---|
| **Files checked** | **1517** |
| **Files needing formatting** | **0** |
| Exit code | **0** |

CSharpier prints one line per non-conforming file ahead of its summary line. The output contains only the summary line and the exit code is 0, so the count of files needing formatting is 0.

## Comparison against the P0-T11 baseline

| Measurement | Baseline (P0-T11) | Now (P5-T2) | Delta |
|---|---|---|---|
| Files checked | 1517 | 1517 | 0 |
| Files needing formatting | 0 | 0 | 0 |

The files-checked count is unchanged at 1517, which is consistent with this change adding no new file and deleting no file: all five edits are modifications to existing tracked files.

## Scope of the guarantee

This is a repository-wide check, so it covers every one of the 1517 files CSharpier considers, not only the five this plan edits. It therefore also proves the P5-T1 scoping decision was safe: restricting the mutating pass to five files did not leave formatting drift anywhere else in the tree.

This is the CI-parity invocation. It uses `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used, which is the same version `.github/workflows/ci.yml` runs after its `dotnet tool restore`. A globally installed CSharpier of a different version could produce a different verdict; that path was not used.

Output Summary: `csharpier check .` exited **0** with the single line `Checked 1517 files in 6574ms.` — **1517 files checked, 0 files needing formatting**, repository-wide and read-only. This matches the P0-T11 baseline of 1517/0 exactly, with the files-checked count unchanged because this change adds and deletes no file. Stage 1 of the AC21 final toolchain pass is green. The check is repo-wide, so it independently confirms the P5-T1 scoped mutating pass left no formatting drift elsewhere in the tree. The manifest-pinned 1.2.6 was used via `dotnet tool run`, matching CI.
