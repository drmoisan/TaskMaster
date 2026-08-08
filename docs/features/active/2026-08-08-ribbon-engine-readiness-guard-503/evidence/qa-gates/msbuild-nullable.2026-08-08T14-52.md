# Phase 3 QC Step 5 — Nullable / Type-Check Gate (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T5]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true"`
EXIT_CODE: 0

## Output Summary

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.58
```

- **Errors: 0.** The policy-mandated type-check gate passes.
- **Warnings: 5**, all instances of the pre-existing `System.Reactive` packages.config advisory recorded in the P0-T9 and P0-T10 baselines. Identical to the P0-T10 baseline.

## Known limitation of this gate, restated (issue #512, out of scope)

Exit 0 from this command **alone does not prove the tree is nullable-clean**. MSBuild's up-to-date check skips the `CoreCompile` target when only `/p:` property values change relative to the previous build, so the compiler is never re-invoked with `Nullable=enable` in force. The elapsed time of **1.58 seconds** for a full-solution build — against roughly 8-19 seconds for every build in this cycle that did compile — is direct evidence that no compilation occurred in this invocation. The figure is within 0.03 seconds of the identical P0-T10 baseline measurement of 1.55 seconds.

`code-review.2026-08-08T14-15.md` independently established the substance behind the gate: a forced `/t:Rebuild` of `TaskMaster.csproj` surfaces 195 errors, 64 of them `CS86xx`, all in files untouched by this feature branch, with **zero** attributable to any of the six new production files.

This gate defect and the underlying repository-wide nullable debt are tracked as issue **#512** and are **explicitly out of scope for this remediation cycle**. They are restated here, not remediated.

## Why the limitation does not weaken this cycle's result

This cycle's only source change is `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, which introduces no new nullable surface. The one nullable-flow construct it adds, the null-forgiving `getEnabled!` at line 209, is a suppression rather than a new warning source, and the file **was** genuinely compiled during this cycle: the P3-T4 analyzer build (19 seconds elapsed, `CoreCompile` executed) and the P1-T3 build (which rewrote `TaskMaster.Test.dll`) both compiled it with zero errors and zero diagnostics.

Binary outcome satisfied: `EXIT_CODE: 0`.
