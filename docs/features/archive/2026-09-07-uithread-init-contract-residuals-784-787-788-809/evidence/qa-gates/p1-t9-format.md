# [P1-T9] Phase 1 formatting

Timestamp: 2026-09-08T01-02

Command, in this order, after the SDK preamble: `git status --porcelain --untracked-files=all` (before-image); `dotnet tool run csharpier format .`; `git status --porcelain --untracked-files=all` (after-image); `dotnet tool run csharpier check .`

EXIT_CODE: 0

## Output Summary

`csharpier format` is write-mode and exits 0 whether or not it rewrote a file, so the exit code alone decides nothing. The before-and-after tree comparison and the read-only check run are the observations that decide this gate.

Before-image:

```
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/Threading/SyncContextForm.cs
 M UtilitiesCS/Threading/UiThread.cs
 M UtilitiesCS/UtilitiesCS.csproj
?? UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
?? UtilitiesCS/Threading/IUiCaptureSource.cs
```

Formatter output, verbatim:

```
Formatted 1610 files in 6698ms.
```

After-image:

```
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/Threading/SyncContextForm.cs
 M UtilitiesCS/Threading/UiThread.cs
 M UtilitiesCS/UtilitiesCS.csproj
?? UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
?? UtilitiesCS/Threading/IUiCaptureSource.cs
```

Comparison: the two images are identical. **No path appears in the after-image that does not appear in the before-image**, so this task is not re-run. Every path in both images is a Phase 1 Write Set file.

The `Formatted 1610 files` figure is CSharpier's processed-file count, not its rewritten-file count; it rose from the 1608 of the [P0-T9] baseline by exactly the two `.cs` files Phase 1 creates. The formatter did rewrite whitespace inside two already-listed files (`UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` collapsed two wrapped expression bodies onto single lines, moving it from 211 to 209 physical lines), which porcelain cannot show because those files were already listed as modified or untracked before the run. The read-only check below is what establishes that no drift remains.

`csharpier check .` exited 0 with:

```
Checked 1610 files in 6770ms.
```

Post-format physical line counts of the three files Phase 1 touched or created, taken with the pinned idiom `(Get-Content -LiteralPath <path>).Count`:

| Path | Lines |
|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 234 |
| `UtilitiesCS/Threading/IUiCaptureSource.cs` | 50 |
| `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` | 209 |
