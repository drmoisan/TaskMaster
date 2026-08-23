# Phase 5 Stage 2 — Linting / Analyzers (Issue #445, AC21 stage 2)

Timestamp: 2026-08-22T09-56

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl '/flp:logfile=msbuild-analyzer-final.log;verbosity=detailed'
(Select-String -SimpleMatch -Pattern 'Skipping target "CoreCompile"' -Path msbuild-analyzer-final.log | Measure-Object).Count
(Select-String -SimpleMatch -Pattern 'CoreCompile:' -Path msbuild-analyzer-final.log | Measure-Object).Count
```
Run from `WS` via `pwsh -NoProfile`. `/t:Rebuild` is used, never `/t:Build`, per Non-negotiable Command Constraint 1.

EXIT_CODE: 0

## Numeric results against the required thresholds

| Measurement | Value | Required | Pass |
|---|---|---|---|
| MSBuild verdict | `Build succeeded.` | success | yes |
| **Error count** | **0** | 0 | yes |
| **Warning count** | **5** | no greater than the P0-T12 baseline of 5 | yes (equal) |
| **`Skipping target "CoreCompile"` count** | **0** | exactly 0 | yes |
| **`CoreCompile:` count** | **100** | at least 9 | yes |

## Non-vacuity proof

The `Skipping target "CoreCompile"` count is exactly **0** and the `CoreCompile:` target-start count is **100**, an order of magnitude above the floor of 9. Compilation genuinely ran on every project in the solution and the analyzers genuinely executed, so this gate was falsifiable and is not vacuous. A warm `/t:Build` would have returned exit 0 with a non-zero skip count and a near-zero `CoreCompile:` count, which is precisely the failure mode Non-negotiable Command Constraint 1 exists to prevent.

The baseline (P0-T12) reported the same two proofs at 0 and 96. The `CoreCompile:` count differing between runs (96 versus 100) is expected: the detailed log's target-start lines vary with build scheduling across the `/m` parallel workers. Both figures are far above the floor, and the load-bearing assertion is the skip count of 0, which is identical in both runs.

## Warning count is exactly at baseline, and every warning is pre-existing

The 5 warnings are the same third-party System.Reactive `packages.config` advisories recorded in P0-T12, one per affected project. The full distinct set:

```
: The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference. ... [QuickFiler\QuickFiler.csproj]
                                              [TaskMaster\TaskMaster.csproj]
                                              [ToDoModel\ToDoModel.csproj]
                                              [UtilitiesCS.Test\UtilitiesCS.Test.csproj]
                                              [UtilitiesCS\UtilitiesCS.csproj]
```

The count is 5 both before and after this change, so the change introduced **zero new warnings**. These carry no diagnostic ID (the text is `warning :` with no `CS` or `MSB` code), are emitted by a third-party `.targets` file rather than by an analyzer, and are pre-existing repository state unrelated to issue #445.

## No diagnostic cites any file this change touched

The detailed log was scanned for any `warning` or `error` diagnostic naming `KaStringAsync.cs`, `KaChar.cs`, `KaKey.cs`, `IKbdAction.cs`, or `KaStringAsyncTests.cs`. The scan returned **no matches**.

This is a stronger statement than the aggregate count. It establishes directly that none of the five edits produced an analyzer diagnostic, and in particular that:

- Deleting the `Update` property **together with** its `_update` backing field in all four places avoided the unused-private-field diagnostic that removing the property alone would have raised.
- Removing `using System.Windows.Forms;` from `KaChar.cs` while retaining it in `KaKey.cs` produced no unused-using diagnostic in the former and no missing-type error in the latter.
- Adding the first XML documentation comments to `KaStringAsync.cs` produced no CS1570/CS1573/CS1591 diagnostic, consistent with the pre-checked fact that `QuickFiler.csproj` enables no documentation-file generation.
- The two-clause guard clause and the branch-3 gate change produced no diagnostic.

## Command-line properties

`/p:EnableNETAnalyzers=true` and `/p:EnforceCodeStyleInBuild=true` were both supplied, as CLAUDE.md CUT3 step 2 requires. No `/p:Nullable=enable` was added. The log file name ends in `.log`, which `.gitignore` already covers, so it does not appear in any scope-lock `git status` gate.

Output Summary: `Build succeeded.` with EXIT_CODE **0**, **0 errors**, and **5 warnings**, which equals the P0-T12 baseline of 5 and therefore introduces zero new warnings. Both non-vacuity proofs hold: the `Skipping target "CoreCompile"` count is exactly **0** and the `CoreCompile:` count is **100**, well above the required floor of 9, so analyzers genuinely ran on every project. All 5 warnings are the same pre-existing codeless third-party System.Reactive `packages.config` advisories, one per affected project. A targeted scan of the detailed log found **no `warning` or `error` diagnostic citing any of the five files this change edits**, which confirms directly that the property-plus-backing-field deletions, the asymmetric `using` handling, the new XML documentation comments, and the guard clause all compile analyzer-clean. Stage 2 of the AC21 final toolchain pass is green.
