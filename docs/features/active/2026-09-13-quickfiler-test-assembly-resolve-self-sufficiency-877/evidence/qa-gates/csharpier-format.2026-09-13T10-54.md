# Final QC step 1: csharpier format — issue #877

Timestamp: 2026-09-13T10-54
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; dotnet tool run csharpier format . 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; $out | Select-Object -Last 20'`
EXIT_CODE: 0
Output Summary: Exit code 0. The tool's own summary line, quoted verbatim: `Formatted 1627 files in 5444ms.` N here is the count of files the pinned CSharpier 1.2.6 processed, not the count it rewrote; it is one higher than the 1626 recorded at the [P0-T10] baseline because `TestSupport/TestAssemblyResolver.cs` is new. N is not treated as a defect signal and the loop is not restarted on it. Immediately after the command returned, `git -C <repo-root> status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport` printed ZERO lines, which is the observation that the format pass rewrote nothing in the write set. Run under an acquired build lock, released immediately after the command returned.

## Why the exit code alone is not the observation

`csharpier format .` is a write-mode command. It exits 0 whether or not it rewrote any file, so its exit code is identical on a clean run and on a repairing run. The two observations recorded above are the tool's own summary line and the post-command porcelain status, taken in that order.

## Post-command porcelain status, verbatim

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport`

Output: zero lines.

The five write-set paths were committed at [P1-T13] and the format pass did not modify any of them. The new file was written with CRLF line endings before the commit, matching the `end_of_line = crlf` setting that `.editorconfig` applies to `[*.{cs,vb}]`, so CSharpier had no line-ending normalization to perform on it.
