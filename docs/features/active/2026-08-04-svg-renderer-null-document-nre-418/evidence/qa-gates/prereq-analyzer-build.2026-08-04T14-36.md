# Prerequisite Analyzer Build — Solution Gate After SVGControl.Test Joins (Issue #418, task P1-T6)

Timestamp: 2026-08-04T18-13

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P1-T6]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `296eac953c5ac3f69c429c7554ab47218e64e852`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)
MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
Working directory: repository root (`C:\Users\DanMoisan\repos\TaskMaster`)

> **This artifact was overwritten in full.** The prior content at this path recorded a
> superseded pre-rebase capture (`EXIT_CODE: 1`, four `CS0234`, one `MSB3277`, and a
> `SCOPE_EXCEEDED` determination) taken on a host that lacked the VSTO runtime assemblies and
> against a pre-package-update dependency graph. That record is obsolete and has been replaced,
> not appended to, so this artifact asserts exactly one outcome. The superseded conditions are
> both resolved: the VSTO assemblies resolve on this host (zero `CS0234`, zero `MSB3245`), and
> the `System.Runtime.CompilerServices.Unsafe` pin divergence that produced the `MSB3277` was
> realigned upstream by PR #419 (plan Design Decision 10).

## Command (plan-commanded, authoritative)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0

## Output Summary

Build succeeded. **0 errors, 6 warnings.** Elapsed 00:00:11.14.

- **New diagnostics vs baseline: 0**
- **Baseline artifact compared: `evidence/baseline/analyzer-build.2026-08-04T21-04.md`**
- **Files edited for remediation: none**

`MSB3277` count: **0**. `CS0234` count: **0**. `MSB3245` count: **0**. `SVGControl` and
`SVGControl.Test` emitted zero errors and zero warnings of any code.

### Diagnostic inventory — plan-commanded incremental run

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) `System.Reactive.PackagesConfigCheck.targets(31,5)` | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` |
| 1 | warning | `CS2002` | `UtilitiesCS.Test.csproj` |
| — | error | none | — |

Distinct diagnostic codes: `CS2002`, plus one code-less MSBuild warning from
`System.Reactive.PackagesConfigCheck.targets`. This is **identical** to the code set recorded in
the plan-commanded run of `evidence/baseline/analyzer-build.2026-08-04T21-04.md` (0 errors,
6 warnings, same two codes, `MSB3277` count 0). Both diagnostics are pre-existing and out of the
issue #418 Scope Lock: `CS2002` is a duplicate `<Compile>` include for
`UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs`, and the code-less warning
is the `System.Reactive 7.0.0` `packages.config` deprecation notice.

### Supplementary full-recompile inventory (methodology parity with the baseline)

The plan-commanded run used MSBuild target `Build` and executed only **1** `CoreCompile`, because
legacy non-SDK up-to-date checks are timestamp-based rather than `/p:`-property-based. An
incremental run alone cannot enumerate the analyzer diagnostics of skipped projects, so — exactly
as the `2026-08-04T21-04` baseline artifact does — a supplementary run with the **identical
property set** and target `Rebuild` was executed solely to produce a complete inventory. It is
supplementary evidence; it is not the plan command and does not replace the result above.

Supplementary command:
```
MSBuild.exe TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m
```
Supplementary EXIT_CODE: 0 — Build succeeded, **0 errors, 6 warnings**. Genuine full recompile
confirmed: 36 `csc.exe` invocations, 84 `CoreCompile` occurrences, with `SVGControl` and
`SVGControl.Test` both rebuilt (their `bin\Debug` outputs were deleted by `CoreClean` and
regenerated).

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) | five `packages.config` projects (`System.Reactive 7.0.0` notice) |
| 1 | warning | `CS2002` | `UtilitiesCS.Test.csproj` |
| 0 | error | — | — |

Comparison against the baseline's supplementary full-recompile inventory (0 errors, 8 warnings):
the two `MSB3061` `CoreClean` warnings in `TaskMaster.csproj` are **absent** here because
Microsoft Outlook was confirmed not running before the build, so it no longer held
`leptonica-1.82.0.dll` and `tesseract50.dll` open. That is an environmental improvement, not a
code change; the warning count is 6 instead of 8 and no diagnostic code appeared that was absent
from the baseline.

### Verdict

`EXIT_CODE: 0`, zero errors, and zero diagnostic codes absent from
`evidence/baseline/analyzer-build.2026-08-04T21-04.md` at either incremental or full-recompile
scope. Bringing `SVGControl.Test` into the solution introduces no new analyzer diagnostic. No
remediation edit was required, so the task's 20-edit `SCOPE_EXCEEDED` ceiling was not approached
and no file outside the Scope Lock was touched.
