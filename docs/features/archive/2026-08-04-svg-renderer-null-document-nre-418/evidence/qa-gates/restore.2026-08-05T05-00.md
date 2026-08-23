# Final QC Stage 1c — Package Restore

- Task: `[P2-T3]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-08

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"
```

Run from the repository root.

```
EXIT_CODE: 0
```

Summary lines:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.55
```

## The new `ExCSS` entry resolved without adding or modifying any file under `packages/`

This is the specific claim `[P2-T3]` requires, and it was measured on both sides of the restore rather
than assumed.

### Directory count unchanged

```
Command: find packages -maxdepth 1 -type d | wc -l
Before restore: 262
After restore:  262
```

**No package directory was added or removed.** A restore that had needed to fetch `ExCSS` would have
created a new directory and moved this count.

### The `ExCSS` package directory is the pre-existing one

```
Command: ls -1d packages/ExCSS.*
Output:  packages/ExCSS.4.3.2/
```

Exactly one `ExCSS` directory, `packages/ExCSS.4.3.2/` — the same single directory `[P0-T9]` § (e)
measured **before** any edit in this cycle. No `4.3.2` re-fetch and no additional version.

### No tracked change under `packages/`

```
Command: git status --porcelain -- packages/
Output:  (empty)
```

Zero modified, added, or deleted tracked paths under `packages/`.

### Why this was the expected outcome

`SVGControl.Test/packages.config` now names `<package id="ExCSS" version="4.3.2" targetFramework="net481" />`,
and `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` was already present on disk — measured at `[P0-T9]` § (e)
at 368128 bytes, because three production projects (`SVGControl`, `UtilitiesCS`, `QuickFiler`) already
depend on the identical package version. The restore therefore had nothing to fetch: the requested
package was already satisfied. `0 Warning(s) 0 Error(s)` in 0.55 s is consistent with a fully satisfied
restore.

This also confirms the version choice at `[P1-T2]` was correct. Had the entry named a version absent from
`packages/`, the restore would either have downloaded a new directory (moving the count from 262) or
failed.

## Output Summary

`EXIT_CODE: 0` with `0 Warning(s)` and `0 Error(s)` in 0.55 s. The new `ExCSS` entry in
`SVGControl.Test/packages.config` resolved **without adding or modifying any file under `packages/`**:
the top-level directory count is 262 both before and after, `packages/ExCSS.4.3.2/` remains the sole
`ExCSS` directory and is the same one that existed before this cycle's edits, and
`git status --porcelain -- packages/` is empty. Stage 1c of toolchain pass 1 is clean; the loop proceeds
to `[P2-T4]` without restart.
