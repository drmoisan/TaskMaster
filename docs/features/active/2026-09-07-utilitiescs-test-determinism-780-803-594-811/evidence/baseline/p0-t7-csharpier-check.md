# P0-T7 — Baseline toolchain step 1 (CSharpier check)

Timestamp: 2026-09-08T09-21
Task: [P0-T7]
Command: dotnet tool run csharpier check .
EXIT_CODE: 0

Invoked as `& $dotnet tool run csharpier check .` from the worktree root, where `$dotnet` is the
repo-local `.dotnet-sdk\dotnet.exe` installed by P0-T3. Going through `dotnet tool run` binds the
manifest-pinned CSharpier 1.2.6, matching `.github/workflows/_format-check.yml`.

PRE-EXISTING DRIFT: none

Exit code 0 and no path was reported as unformatted, so the tree carries no pre-existing formatting
drift. Every rewrite the P7-T1 format pass produces will therefore be attributable to this change.

## Output Summary

Summary line, verbatim:

```
Checked 1613 files in 7197ms.
```

The line begins with `Checked ` and ends with `ms.`, as the acceptance condition requires. The
processed-file count is 1613 on this tree, 26 higher than the 1587 recorded by #798 one day
earlier; the difference is files added by the 31 origin/main commits merged as `bb1c7d4b`, not a
change made by this item. The count is a processed-file count, not a rewrite count, so it is
recorded for provenance and is not the pass signal; exit 0 with no reported path is the pass
signal.
