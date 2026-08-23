# Prerequisite Nullable / Type-Check Build — Solution Gate After SVGControl.Test Joins (Issue #418, task P1-T7)

Timestamp: 2026-08-04T18-15

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P1-T7]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `296eac953c5ac3f69c429c7554ab47218e64e852`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)
MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
Working directory: repository root (`C:\Users\DanMoisan\repos\TaskMaster`)

## Command (plan-commanded, authoritative)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
```

EXIT_CODE: 0

## Output Summary

Build succeeded. **0 errors, 5 warnings.**

- **New errors vs baseline: 0**
- **Baseline artifact compared: `evidence/baseline/nullable-build.2026-08-04T21-04.md`**
- **Files edited for remediation: none**

### Diagnostic inventory — plan-commanded incremental run

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) `System.Reactive.PackagesConfigCheck.targets(31,5)` | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` |
| — | error | none | — |

Zero `CS86xx` nullable diagnostics were emitted. Zero `CS0234`, zero `MSB3245`, zero `MSB3277`.
This is **identical** to the plan-commanded run recorded in
`evidence/baseline/nullable-build.2026-08-04T21-04.md` (`EXIT_CODE: 0`, 0 errors, 5 warnings, same
single code-less warning).

### Incrementality caveat — recorded, deliberately not "corrected"

This run executed **0** `CoreCompile` targets and invoked `csc.exe` zero times: every project was
already up to date from the immediately preceding `[P1-T6]` analyzer build. Legacy non-SDK
up-to-date checks are timestamp-based rather than `/p:`-property-based, so adding
`/p:Nullable=enable /p:TreatWarningsAsErrors=true` does not by itself force a recompile. The
`EXIT_CODE: 0` above is a true record of what the plan-commanded command returns in this tree
state, but it is not an independent statement about solution-wide nullable cleanliness.

Per plan Design Decision 11, this vacuity is deliberate and **must not** be resolved by forcing a
rebuild here. A full-recompile nullable + `TreatWarningsAsErrors` solution build cannot reach
`EXIT_CODE: 0` at baseline for reasons wholly outside this feature, and forcing one would make
`[P2-T5]` unreachable. The baseline artifact already carries the full-recompile inventory that
serves as this task's comparison basis:

- `UtilitiesCS.csproj` — **195 pre-existing nullable errors** (`CS8766` x130, `CS8618` x23,
  `CS8625` x12, `CS8600` x9, `CS8601` x8, `CS8604` x7, `CS8602` x3, `CS8603` x2, `CS8714` x1).
  Pre-existing repository nullable debt tracked outside issue #418 and outside its Scope Lock.
- `SVGControl.Test.csproj` — **1 pre-existing error**
  `CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3. Please use language version '8.0' or greater.`
  This is a property of the project's C# language version, not of any Phase 1 edit.
  `SVGControl.Test` was already a solution member at that baseline (commit `0162567d`,
  `[P1-T1]`), so `CS8630` cannot be attributed to this task's changes.
- `TaskMaster.csproj` — 2 `MSB3061` `CoreClean` warnings, environmental (Microsoft Outlook holding
  two native Tesseract DLLs open). Outlook was confirmed not running for this session, and
  `MSB3061` did not appear in the `[P1-T6]` supplementary full-recompile run.

Both `UtilitiesCS`'s 195 errors and `SVGControl.Test`'s `CS8630` are present in the baseline
artifact's inventory and are therefore, by this task's own `- Definition:` clause, not new
diagnostics.

### Verdict

`EXIT_CODE: 0`, zero errors, and zero diagnostic codes absent from
`evidence/baseline/nullable-build.2026-08-04T21-04.md`. Bringing `SVGControl.Test` into the
solution introduces no new nullable or type-check error. No remediation edit was required, so no
`#nullable disable` / `#nullable restore` pair was needed in any `*.Designer.cs` file, the task's
20-edit `SCOPE_EXCEEDED` ceiling was not approached, and no file outside the Scope Lock was
touched.
