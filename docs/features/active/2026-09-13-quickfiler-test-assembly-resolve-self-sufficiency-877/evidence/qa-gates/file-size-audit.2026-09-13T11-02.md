# Gate: file-size limit holds for the write set — issue #877

Timestamp: 2026-09-13T11-02
Command: `pwsh -NoProfile -Command 'foreach ($p in "TestSupport/TestAssemblyResolver.cs","QuickFiler.Test/SetupAssemblyInitializer.cs","UtilitiesCS.Test/TestAssemblyInitializer.cs") { $full = Join-Path "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" $p; Write-Host "$p => $((Get-Content -LiteralPath $full).Count)" }'`
EXIT_CODE: 0
Output Summary: All three source files in the write set are strictly under the 500-line limit set by `.claude/rules/general-code-change.md`. Counts were measured after the CSharpier pass at [P2-T2].

## Measured line counts

| File | Lines | Under 500 |
|---|---|---|
| `TestSupport/TestAssemblyResolver.cs` | 131 | yes |
| `QuickFiler.Test/SetupAssemblyInitializer.cs` | 27 | yes |
| `UtilitiesCS.Test/TestAssemblyInitializer.cs` | 20 | yes |

## Measurement point

The counts above were taken after `dotnet tool run csharpier format .` ran at [P2-T2], so they reflect the formatted state that is committed, not the pre-format state. `UtilitiesCS.Test/TestAssemblyInitializer.cs` shrank from 109 lines to 20 because the three moved members were deleted from it; that logic now lives only in the shared file.

The two `.csproj` files in the write set are not measured here. This task names the three source files, and `.csproj` files are build configuration rather than production, test, or reusable script files.
