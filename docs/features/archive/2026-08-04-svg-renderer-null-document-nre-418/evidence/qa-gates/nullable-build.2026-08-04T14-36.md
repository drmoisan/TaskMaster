# [P2-T6] Solution Nullable / Type-Check Build — Final QC Pass 1

Timestamp: 2026-08-04T19-59

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`

EXIT_CODE: 0

Output Summary: 0 errors

- `Build succeeded. 5 Warning(s) 0 Error(s)`. Elapsed 00:00:00.94. 18 `CoreCompile` targets.
- The 5 warnings are the code-less `System.Reactive.PackagesConfigCheck.targets(31,5)` packages.config
  notices from `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and `UtilitiesCS.Test`.
- **`CS86xx` count: 0.** No nullable-flow diagnostic of any code was emitted, in `SVGControl`,
  `SVGControl.Test`, or any other project.

## Comparison against the `[P0-T8]` baseline

Baseline of record: `evidence/baseline/nullable-build.2026-08-04T21-04.md` — `EXIT_CODE: 0`,
**0 errors, 5 warnings**, elapsed 00:00:00.92.

| Metric | Baseline `2026-08-04T21-04` | This run | Verdict |
|---|---|---|---|
| EXIT_CODE | 0 | 0 | no worse |
| Errors | 0 | 0 | no worse |
| Warnings | 5 | 5 | no worse |
| `CS86xx` diagnostics | 0 | 0 | identical |

**New diagnostics versus baseline: none.**

Two pre-existing conditions correctly did not surface, and neither is attributable to this change:

- `SVGControl.Test`'s `CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3` — present at baseline,
  surfaces only under a full recompile of that project.
- The 195 pre-existing `UtilitiesCS` nullable errors (`CS8766` x130, `CS8618` x23, `CS8625` x12,
  `CS8600` x9, `CS8601` x8, `CS8604` x7, `CS8602` x3, `CS8603` x2, `CS8714` x1) — repository nullable
  debt tracked outside issue #418 and outside the Scope Lock.

Both surface only under `/t:Rebuild`. `Invoke-VSBuild.ps1` hardcodes `/t:Build`, and legacy non-SDK
up-to-date checks are timestamp-based rather than `/p:`-property-based. **No `/t:Rebuild` was run by
this task**, per the plan's Open Questions note and Design Decision 11.

## Where the nullable guarantee for this change actually rests

Design Decision 11 identifies `[P2-T5]` as the load-bearing enforcement point rather than this task:
`SVGControl/SvgRenderer.cs:1` is `#nullable enable`, so any new `CS86xx` in that file surfaces as a
warning in the ordinary analyzer build regardless of the `/p:Nullable` switch. `[P2-T5]` recompiled
`SVGControl` (36 `CoreCompile` targets) and recorded **zero warnings and zero errors** from
`SVGControl` and `SVGControl.Test`, which is the substantive proof that the new members' nullable
annotations are correct. This task's `EXIT_CODE: 0` is recorded as the plan-commanded gate result.
