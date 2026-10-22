# [P7-T4] Nullable Gate

Timestamp: 2026-09-08T10-21
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (the [P0-T10] command, verbatim; no `/p:Nullable=enable` added)
EXIT_CODE: 0
Output Summary: The full-solution rebuild with warnings treated as errors succeeded with zero warnings and zero errors after every source change this plan makes. Both counts equal the [P0-T10] baseline of zero.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.14
```

NULLABLE-WARNINGS: 0
NULLABLE-ERRORS: 0

## Comparison against the baseline

| Counter | [P0-T10] baseline | This build | Relation |
| --- | --- | --- | --- |
| Warnings | 0 | 0 | equal, so less than or equal holds |
| Errors | 0 | 0 | equal, so less than or equal holds |

## Which files this gate actually exercises

Nullable enforcement in this repository is per-file opt-in: a file participates when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors. Three files in this plan's Write Set carry the directive and are therefore genuinely under the gate:

- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, edited by [P4-T4] and reduced by the [P4-T8] relocation;
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, edited by [P4-T5] and grown by the same relocation, which moved two members that dereference `DropDown` and `_cancelSelection` into it;
- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`, created by [P6-T4] with `#nullable enable` on its first line.

The build output was searched for the pattern `CS86` followed by two digits. Match count: 0, so no nullable-flow diagnostic was raised in any of the three, promoted or otherwise.

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire and no process was terminated.

## D4 note

`/p:Nullable=enable` was not added. No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so that property is a solution-wide opt-in which would conscript every file that has never adopted the pragma; CI omits it deliberately. `/t:Rebuild` was used rather than `/t:Build`, because a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and the gate could not fail.
