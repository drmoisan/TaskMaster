# [P15-T4] Final QA loop, step 3 — nullable / type check

Timestamp: 2026-08-26T16-46

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`

Emitted MSBuild command line (host paths replaced with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /m
```

This is the `CLAUDE.md` §C#1.3 policy command, character-equivalent to the
`.github/workflows/ci.yml` step "Build with nullable warnings treated as errors" modulo the wrapper's
`/m` placement. **`/p:Nullable=enable` is deliberately absent**: no project carries a `<Nullable>`
element and there is no `Directory.Build.props`, so forcing it would conscript every file that has
never adopted the pragma. CI omits it for the same reason.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

| Metric | P0-T13 baseline | P15-T4 (this run) | Delta |
|---|---|---|---|
| Exit code | 0 | **0** | 0 |
| **Error count** | 0 | **0** | 0 |
| Warnings | 5 | **5** | 0 |
| `CS86xx` nullable-flow diagnostics anywhere in the log | 0 | **0** | 0 |
| Distinct projects that executed `CoreCompile` | 18 | **18** | 0 |
| `Skipping target "CoreCompile"` occurrences | 0 | **0** | 0 |
| Wall time | 00:00:29.27 | 00:00:13.68 | — |

**No new error relative to the P0-T13 baseline.** The error count is 0 on both sides and no `CS86xx`
diagnostic appears anywhere in either log.

## Non-vacuity proof

`/t:Rebuild` is load-bearing: a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every
project, so the gate could not fail. Three measurements over the build log:

1. **`Skipping target "CoreCompile"` occurrences: 0.**
2. **18 distinct `/out:` targets** across the log's `csc.exe` command lines — the same eighteen
   assemblies enumerated in `p15-t3-analyzers.2026-08-26T16-45.md`. Every project in
   `TaskMaster.sln` was genuinely recompiled.
3. **`/warnaserror+` is present on the `csc.exe` command lines.** `/p:TreatWarningsAsErrors=true`
   reached the compiler rather than being silently dropped, so any `CS86xx` diagnostic in an
   opted-in file would have become a build error and driven the exit code non-zero.

## Nullable enforcement in this repository is per-file opt-in

A file participates in nullable flow analysis only when it carries a `#nullable enable` directive;
`/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to errors.

None of the ten owned files carries `#nullable enable`:

`SearchScope:` `QuickFiler/Controllers/QfcCollectionController.cs`,
`QuickFiler/Interfaces/IQfcCollectionController.cs`, and every
`QuickFiler.Test/Controllers/QfcCollectionController*.cs`.
`SearchPatterns:` `#nullable enable`.
`SearchResult:` none.

This is stated plainly rather than presented as a pass: **this feature adds no nullable-annotated
file, so the gate's nullable half is vacuous with respect to this feature's own code**. What the gate
does establish for this feature is the type-check half — all ten owned files compile cleanly under
`/warnaserror+` with 0 errors — and that no file elsewhere in the solution regressed under the
merged tree.

Adopting the pragma in `QfcCollectionController.cs` was not attempted. It is a 2,437-line file with no
prior annotation, and annotating it would be a change far larger than the seven defect fixes it
accompanies, in direct conflict with CLAUDE.md's Bugfix Workflow.

## The five warnings

Identical to P15-T3: the same System.Reactive `packages.config` diagnostic on the same five projects
(`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`). Pre-existing repository
debt tracked by open issue #570. It is emitted by an MSBuild target, not by the compiler, which is
why `/p:TreatWarningsAsErrors=true` does not promote it to an error and the build still exits 0.

## Acceptance verification

| Clause | Status |
|---|---|
| `EXIT_CODE: 0` | met |
| a non-zero `CoreCompile` project count | met — **18**, with **0** `Skipping target "CoreCompile"` occurrences |
| no new error relative to the P0-T13 baseline | met — 0 errors on both sides, 0 `CS86xx` on both sides |
