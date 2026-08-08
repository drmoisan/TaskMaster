# Baseline Nullable Build (toolchain step 3)

Timestamp: 2026-08-08T16-19

Task: [P0-T9]

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true /m`

EXIT_CODE: 0

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.20
```

## Vacuousness disclosure (material — read before interpreting the EXIT_CODE)

The planned command returned EXIT_CODE 0, but it did **not** compile anything. Two signals prove
this:

1. Elapsed time was 1.20s versus 16.90s for the P0-T8 analyzer build over the same solution.
2. The `CS2002` duplicate-`Compile` warning, which is emitted by the `CoreCompile` target, is
   present in the P0-T8 log and **absent** here. Warning count dropped 6 -> 5 for exactly that
   reason.

MSBuild's `/t:Build` up-to-date check compares source and output timestamps and does not consider
`/p:` property changes. Because P0-T8 had just built every project, every project was considered
up to date and `CoreCompile` never ran, so no nullable diagnostic could be enumerated.

## Forced-rebuild probe (micro-action, to obtain a non-vacuous figure)

To establish what the nullable gate actually measures in this checkout, the identical property set
was rerun with `/t:Rebuild`:

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true /m`
EXIT_CODE: 1

```
    0 Warning(s)
    195 Error(s)

Time Elapsed 00:00:03.51
```

Diagnostic breakdown across the whole solution (all pre-existing at merge-base `003c5715`, with a
completely clean scoped source tree — see `repo-state.2026-08-08T16-11.md`):

| Count | ID | Meaning |
|---|---|---|
| 260 | CS8766 | Nullability of reference types in return type doesn't match implicitly implemented member |
| 46 | CS8618 | Non-nullable field/property uninitialized on exit from constructor |
| 24 | CS8625 | Cannot convert null literal to non-nullable reference type |
| 18 | CS8600 | Converting null literal or possible null value to non-nullable type |
| 16 | CS8601 | Possible null reference assignment |
| 14 | CS8604 | Possible null reference argument |
| 6 | CS8602 | Dereference of a possibly null reference |
| 4 | CS8603 | Possible null return |
| 2 | CS8714 | Type cannot be used as type parameter (notnull constraint) |

(The 195 reported errors are the deduplicated/error-capped total; the per-ID counts above are raw
log-line matches, which include repeated project contexts under `/m`.)

This is pre-existing repository-wide nullable debt, not a product of this change.
`/p:Nullable=enable` forces nullable analysis onto every project including the many that have not
opted in, so the debt it reveals is the whole untouched legacy surface.

**Zero of these diagnostics is attributed to `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`.**
The only two occurrences of `WpfDispatcherYield` in the rebuild log are `csc.exe` command lines
listing the file as a compilation input; neither is a diagnostic line.

## Consequence for the P2-T4 comparison (like-for-like)

P2-T4 runs the identical `/t:Build` command immediately after P2-T3's analyzer `/t:Build`. That is
structurally identical to the baseline sequence here (P0-T9 immediately after P0-T8), so the
baseline and the gate measure the same thing by the same method. The comparison is like-for-like.

Because `WpfDispatcherYield.cs` carries a file-scoped `#nullable enable` on line 1 (see
`source-under-test.2026-08-08T16-12.md`), nullable analysis of that file happens in the **ordinary**
analyzer build too. Any CS86xx defect introduced by Phase 1 in that file would therefore raise the
P2-T3 analyzer warning count above the baseline's 6. That is the effective, non-vacuous nullable
check on the changed file, and P1-T6 is verified against it.

## Build-state restoration

The `/t:Rebuild` probe deleted all build outputs and then failed at `UtilitiesCS`, leaving
`UtilitiesCS\bin\Debug\UtilitiesCS.dll` and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
absent. The P0-T8 analyzer build command was rerun to restore a valid build state:

```
MSBUILD_EXIT_CODE=0
    6 Warning(s)
    0 Error(s)
Time Elapsed 00:00:17.02
```

This reproduces the P0-T8 result exactly (6/0, CS2002 present, ~17s), confirming the tree is back to
a fully-built state before P0-T10 coverage capture and the P0-T12 probe.

Output Summary: The planned nullable command returned EXIT_CODE 0 with 5 warnings and 0 errors, but
was an incremental no-op (1.20s, no CoreCompile) because P0-T8 had just built the solution. A
forced `/t:Rebuild` with the same properties exposes 195 pre-existing repository-wide nullable
errors that predate this change; none is attributed to `WpfDispatcherYield.cs`. P2-T4 uses the same
command in the same position, so the gate is like-for-like with this baseline; the effective
non-vacuous nullable check on the changed file is the P2-T3 analyzer warning count versus the
baseline 6. Build outputs destroyed by the probe were restored (6 warnings / 0 errors, matching
P0-T8).
