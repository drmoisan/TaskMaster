# Phase 5 Stage 3 — Type Checking / Nullable (Issue #445, AC21 stage 3)

Timestamp: 2026-08-22T09-58

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl '/flp:logfile=msbuild-nullable-final.log;verbosity=detailed'
(Select-String -SimpleMatch -Pattern 'Skipping target "CoreCompile"' -Path msbuild-nullable-final.log | Measure-Object).Count
```
Run from `WS` via `pwsh -NoProfile`. **No `/p:Nullable=enable`** and **no `/t:Build`**, per Non-negotiable Command Constraints 2 and 1.

EXIT_CODE: 0

## Numeric results against the required thresholds

| Measurement | Value | Required | Pass |
|---|---|---|---|
| MSBuild verdict | `Build succeeded.` | success | yes |
| **Error count** | **0** | 0 | yes |
| **`Skipping target "CoreCompile"` count** | **0** | exactly 0 | yes |
| `CoreCompile:` count | 111 | (corroborating) | — |
| Warning count | 5 | (baseline 5) | equal |
| `CS86xx` nullable diagnostics in log | **0** | — | — |

## Non-vacuity proof

The `Skipping target "CoreCompile"` count is exactly **0**, with a corroborating `CoreCompile:` count of **111**. Every project genuinely compiled under `/p:TreatWarningsAsErrors=true`, so the type-check gate is falsifiable and was not vacuous. A warm `/t:Build` would have returned exit 0 having skipped `CoreCompile` on every project, in which case the gate could not have failed regardless of the code's state.

## Why the 5 warnings are not promoted to errors

`/p:TreatWarningsAsErrors=true` promotes **compiler** warnings. The 5 surviving warnings are the third-party System.Reactive `packages.config` advisories emitted by an MSBuild task inside a `.targets` file, and they carry no diagnostic identifier (the log text is `warning :`, with no `CS` or `MSB` code). A codeless MSBuild task warning is not a compiler warning and is not promoted. The count is 5 both at baseline (P0-T13) and here, so this change introduced no new warning of any kind.

## Nullable enforcement model, verified rather than assumed

Nullable enforcement in this repository is **per-file opt-in**: a file participates only when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes that file's `CS86xx` diagnostics to build errors. Solution-wide `/p:Nullable=enable` is deliberately omitted here and in `.github/workflows/ci.yml`, because no project carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property would conscript every file that never adopted the pragma.

The pragma count was measured in each of the five changed files rather than assumed:

| File | `#nullable` occurrences |
|---|---|
| `QuickFiler/Controllers/KaStringAsync.cs` | 0 |
| `QuickFiler/Controllers/KaChar.cs` | 0 |
| `QuickFiler/Controllers/KaKey.cs` | 0 |
| `QuickFiler/Interfaces/IKbdAction.cs` | 0 |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 |

None of the five files opts in, and **this change adds no pragma**. No new nullable surface is therefore created, and the repository-wide `CS86xx` count in the log is **0**. This is consistent with the guard clause's use of `if (other is null)` plus an explicit `throw`, which is a runtime check rather than a nullable-annotation change: it alters no signature and introduces no `?` annotation anywhere.

## No diagnostic cites any file this change touched

The detailed log was scanned for any `warning` or `error` diagnostic naming `KaStringAsync.cs`, `KaChar.cs`, `KaKey.cs`, `IKbdAction.cs`, or `KaStringAsyncTests.cs`. The scan returned **no matches**, so none of the five edits produced a compiler or nullable diagnostic under warnings-as-errors.

## CI parity

This command is character-for-character the one in `.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"), except for the deliberate `/t:Build` to `/t:Rebuild` substitution required for a warm local worktree and the added file logger. Neither substitution weakens the gate; `/t:Rebuild` strengthens it, which the skip count of 0 demonstrates.

Output Summary: `Build succeeded.` with EXIT_CODE **0** and **0 errors**. The non-vacuity proof holds: the `Skipping target "CoreCompile"` count is exactly **0**, with a corroborating `CoreCompile:` count of 111, so every project genuinely type-checked under `/p:TreatWarningsAsErrors=true`. The repository-wide `CS86xx` nullable diagnostic count is **0**. The 5 surviving warnings equal the P0-T13 baseline and are the same codeless third-party System.Reactive advisories, which are MSBuild task warnings rather than compiler warnings and are therefore not promoted to errors. All five changed files were measured to carry **zero** `#nullable` pragmas and this change adds none, so no new nullable surface exists; the guard clause uses a runtime `is null` test and an explicit throw, altering no signature and adding no annotation. A targeted log scan found no `warning` or `error` diagnostic citing any of the five changed files. `/p:Nullable=enable` was not added and `/t:Build` was not used. Stage 3 of the AC21 final toolchain pass is green.
