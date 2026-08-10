# Phase 0 — Nullable / Type-Check Build Baseline (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T10]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true"`
EXIT_CODE: 0

## Output Summary

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.55
```

- **Errors: 0.** The policy-mandated type-check gate passes.
- **Warnings: 5**, all instances of the pre-existing `System.Reactive` packages.config advisory recorded in the P0-T9 baseline. The `CS2002` warning does not reappear here because `CoreCompile` did not run (see below).

## Known limitation of this gate (recorded, out of scope)

`code-review.2026-08-08T14-15.md` records, and this run corroborates, that **exit 0 from this command alone does not prove the tree is nullable-clean**. MSBuild's up-to-date check skips the `CoreCompile` target when only `/p:` property values change relative to the previous build, so the compiler is never re-invoked with `Nullable=enable` in force. The elapsed time of **1.55 seconds** for a full-solution build — against 19.32 seconds for the P0-T9 analyzer build that did compile — is direct evidence that no compilation occurred in this invocation.

The review independently established the substance behind the gate: a forced `/t:Rebuild` of `TaskMaster.csproj` surfaces 195 errors, 64 of them `CS86xx`, all in files untouched by this feature branch (`OutlookItemTry.cs` 35, `OutlookItemFlaggableTry.cs` 30, `ItemInfo.cs` 20, `PropertyStore.cs` 17), with **zero** attributable to any of the six new production files.

This gate defect and the underlying repository-wide nullable debt are tracked as issue **#512** and are **explicitly out of scope for this remediation cycle**. They are recorded here, not remediated. This cycle changes one C# test file and one XML resource; it introduces no new nullable surface.

P3-T5 runs the identical command and restates this limitation.

Binary outcome satisfied: `EXIT_CODE: 0`.
